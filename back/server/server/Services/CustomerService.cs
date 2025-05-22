using System.Text;
using MongoDB.Driver;
using server.Contracts;
using server.Interfaces;
using server.Services.MongoDb;
using MongoDB.Driver.Linq;
using MongoDB.Bson;
using server.Models;

public class CustomerService
{
    private readonly MongoDbService mongoDbService;
    private readonly IEmbeddingProviderFactory embeddingProviderFactory;

    public CustomerService(
        MongoDbService mongoDbService,
        IEmbeddingProviderFactory embeddingProviderFactory)
    {
        this.mongoDbService = mongoDbService;
        this.embeddingProviderFactory = embeddingProviderFactory;
    }

    public async Task<string> ProcessCustomerRequest(AskQuestionRequest input)
    {
        var client = await GetClientByApiKeyAsync(input.ApiKey);
        var userVector = await GenerateUserEmbeddingAsync(client, input.Request);

        var topAnswers = await FindTopRelevantAnswersAsync(client, userVector, topK: 5);
        if (!topAnswers.Any())
            return "Релевантна відповідь не знайдена.";

        // Далі передаємо ці відповіді у LLM/Prompt-модель
        var finalResponse = await GenerateAnswer(client, input.Request, topAnswers);
        return finalResponse;
    }

    private async Task<Client> GetClientByApiKeyAsync(string apiKey)
    {
        var client = await mongoDbService.Clients
            .Find(c => c.ApiKey == apiKey)
            .FirstOrDefaultAsync();

        if (client == null)
            throw new Exception("Клієнта з таким API ключем не знайдено.");

        if (client.Expiration < DateTime.UtcNow)
            throw new Exception("Підписка завершилась. Оновіть доступ.");

        if (string.IsNullOrWhiteSpace(client.EmbeddingProvider) || string.IsNullOrWhiteSpace(client.ProvidedKey))
            throw new Exception("У клієнта не налаштовано embedding-провайдер.");

        return client;
    }

    private async Task<List<double>> GenerateUserEmbeddingAsync(Client client, string userQuestion)
    {
        var provider = embeddingProviderFactory.Create(client.EmbeddingProvider, client.ProvidedKey);
        return (await provider.GenerateEmbeddingsAsync(new List<string> { userQuestion })).First();
    }

    private async Task<List<string>> FindTopRelevantAnswersAsync(Client client, List<double> userVector, int topK)
    {
        var collectionName = mongoDbService.GetQuestionsCollectionName(client.Id, client.EmbeddingProvider);
        var questionsCollection = mongoDbService.GetQuestionsCollection(collectionName);

        var pipeline = new[]
        {
            new BsonDocument
            {
                { "$search", new BsonDocument
                    {
                        { "index", "default" },
                        { "knnBeta", new BsonDocument
                            {
                                { "vector", new BsonArray(userVector) },
                                { "path", "embedding" },
                                { "k", topK }
                            }
                        }
                    }
                }
            },
            new BsonDocument { { "$limit", topK } }
        };

        var results = await questionsCollection.Aggregate<BsonDocument>(pipeline).ToListAsync();

        var answers = results
            .Select(doc => doc.GetValue("answer", null)?.AsString ?? doc.GetValue("Answer", null)?.AsString)
            .Where(ans => !string.IsNullOrWhiteSpace(ans))
            .ToList();

        return answers;
    }
    private async Task<string> GenerateAnswer(Client client, string userQuestion, List<string> topAnswers)
    {
        var provider = embeddingProviderFactory.Create(client.EmbeddingProvider, client.ProvidedKey);

        var contextBlock = string.Join("\n", topAnswers.Select((ans, i) => $"[{i + 1}] {ans}"));
        var prompt = $"Користувач запитав: {userQuestion}\nОсь повʼязані відповіді:\n{contextBlock}\nЗгенеруй точну і стислу відповідь:";

        var response = await provider.GeneratePrompt(prompt);
        return response;
    }
}
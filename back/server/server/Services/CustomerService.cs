using System.Text;
using MongoDB.Driver;
using server.Contracts;
using server.Interfaces;
using server.Services.MongoDb;

namespace server.Services;

public class CustomerService
{
    private readonly MongoDbService mongoDbService;
    private readonly IEmbeddingProviderFactory embeddingProviderFactory;

    public CustomerService(MongoDbService mongoDbService, IEmbeddingProviderFactory embeddingProviderFactory)
    {
        this.mongoDbService = mongoDbService;
        this.embeddingProviderFactory = embeddingProviderFactory;
    }

    public async Task<string> ProcessCustomerRequest(AskQuestionRequest input)
    {
        var apikey = input.ApiKey;
        var userQuestion = input.Request;

        var client = await mongoDbService.Clients
            .Find(c => c.ApiKey == apikey)
            .FirstOrDefaultAsync();

        if (client == null)
            throw new Exception("Клієнта з таким API ключем не знайдено.");

        if (client.Expiration < DateTime.UtcNow)
            throw new Exception("Підписка завершилась. Оновіть доступ.");

        if (string.IsNullOrWhiteSpace(client.EmbeddingProvider) || string.IsNullOrWhiteSpace(client.ProvidedKey))
            throw new Exception("У клієнта не налаштовано embedding-провайдер.");

        var provider = embeddingProviderFactory.Create(client.EmbeddingProvider, client.ProvidedKey);
        var vector = (await provider.GenerateEmbeddingsAsync(new List<string> { userQuestion })).First();

        var questions = await mongoDbService.Questions
            .Find(q => q.ClientId == client.Id && q.Embedding.Count == vector.Count)
            .ToListAsync();

        if (!questions.Any())
            throw new Exception("Немає питань у базі для цього клієнта.");

        var bestMatch = questions
            .Select(q => new
            {
                q.Answer,
                Similarity = CosineSimilarity(q.Embedding, vector)
            })
            .OrderByDescending(r => r.Similarity)
            .FirstOrDefault();

        if (bestMatch == null || bestMatch.Similarity < 0.7)
            return "Релевантна відповідь не знайдена.";

        return bestMatch.Answer;
    }

    private double CosineSimilarity(List<double> v1, List<double> v2)
    {
        if (v1 == null || v2 == null || v1.Count != v2.Count)
            return 0;

        var a = NormalizeCopy(v1);
        var b = NormalizeCopy(v2);

        double dot = 0;
        for (int i = 0; i < a.Count; i++)
        {
            dot += a[i] * b[i];
        }

        return dot;
    }

    private List<double> NormalizeCopy(List<double> vector)
    {
        var magnitude = Math.Sqrt(vector.Sum(x => x * x)) + 1e-8;
        return vector.Select(x => x / magnitude).ToList();
    }
}

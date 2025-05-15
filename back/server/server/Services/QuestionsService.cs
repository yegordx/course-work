using System.Security.Cryptography;
using MongoDB.Driver;
using server.Contracts;
using server.Interfaces;
using server.Models;
using server.Services.Embeddings;
using server.Services.MongoDb;

public class QuestionsService
{
    private readonly MongoDbService mongoDbService;
    private readonly IEmbeddingProviderFactory embeddingProviderFactory;

    public QuestionsService(
        MongoDbService mongoDbService,
        IEmbeddingProviderFactory embeddingProviderFactory)
    {
        this.mongoDbService = mongoDbService;
        this.embeddingProviderFactory = embeddingProviderFactory;
    }


    public async Task<List<Question>> GetAll(string clientId)
    {
        var result = await mongoDbService.Questions
            .Find(q => q.ClientId == clientId)
            .ToListAsync();

        return result ?? new List<Question>();
    }


    public async Task Delete(string clientId, string questionId)
    {
        var result = await mongoDbService.Questions.DeleteOneAsync(
            q => q.Id == questionId && q.ClientId == clientId
        );

        if (result.DeletedCount == 0)
            throw new Exception("Питання не знайдено або вже видалене.");
    }

    public async Task UploadFaq(string clientId, List<QuestionDto> input)
    {
        var client = await mongoDbService.Clients
            .Find(c => c.Id == clientId)
            .FirstOrDefaultAsync();

        if (client == null)
            throw new Exception("Клієнта не знайдено");

        if (string.IsNullOrWhiteSpace(client.EmbeddingProvider) || string.IsNullOrWhiteSpace(client.ProvidedKey))
            throw new Exception("Не вказано embedding-провайдера або ключ");

        var provider = embeddingProviderFactory.Create(client.EmbeddingProvider, client.ProvidedKey);

        var validInput = input
            .Where(dto => !string.IsNullOrWhiteSpace(dto.Question))
            .ToList();

        if (!validInput.Any())
            return;

        await mongoDbService.Questions.DeleteManyAsync(q => q.ClientId == clientId);

        var texts = validInput.Select(dto => dto.Question).ToList();

        var embeddings = await provider.GenerateEmbeddingsAsync(texts);

        if (embeddings.Count != validInput.Count)
            throw new Exception("Кількість ембеддингів не відповідає кількості питань.");

        var questions = validInput
            .Zip(embeddings, (dto, embedding) => new Question
            {
                ClientId = clientId,
                Text = dto.Question,
                Answer = dto.Answer,
                Embedding = embedding,
                CreatedAt = DateTime.UtcNow
            })
            .ToList();

        await mongoDbService.Questions.InsertManyAsync(questions);

        var update = Builders<Client>.Update.Set(c => c.IsActivated, true);

        if (string.IsNullOrEmpty(client.ApiKey))
        {
            var newKey = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
            update = update.Set(c => c.ApiKey, newKey);
        }


        await mongoDbService.Clients.UpdateOneAsync(c => c.Id == clientId, update);
    }
}


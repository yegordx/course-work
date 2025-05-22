using System.Security.Cryptography;
using MongoDB.Driver;
using server.Contracts;
using server.Interfaces;
using server.Models;
using server.Services.MongoDb;
using MongoDB.Bson;


public class QuestionsService
{
    private readonly MongoDbService mongoDbService;
    private readonly IEmbeddingProviderFactory embeddingProviderFactory;
    private readonly AtlasApiSettings atlasApiSettings;

    public QuestionsService(
        MongoDbService mongoDbService,
        IEmbeddingProviderFactory embeddingProviderFactory,
        AtlasApiSettings atlasApiSettings)
    {
        this.mongoDbService = mongoDbService;
        this.embeddingProviderFactory = embeddingProviderFactory;
        this.atlasApiSettings = atlasApiSettings;
    }

    public async Task UploadFaq(string clientId, List<QuestionDto> input)
    {
        var client = await GetAndValidateClientAsync(clientId);
        var validInput = FilterValidQuestions(input);
        if (!validInput.Any()) return;

        var collectionName = mongoDbService.GetQuestionsCollectionName(clientId, client.EmbeddingProvider);
        var questionsCollection = mongoDbService.GetQuestionsCollection(collectionName);

        await DeleteExistingQuestionsAsync(questionsCollection, clientId);

        var embeddings = await GenerateEmbeddingsAsync(validInput, client);

        await InsertQuestionsAsync(questionsCollection, validInput, embeddings, clientId);
        await UpdateClientActivationAsync(client);

        await CreateVectorSearchIndexAsync(
            questionsCollection,
            "default",
            embeddings[0].Count
        );
    }

    private async Task<Client> GetAndValidateClientAsync(string clientId)
    {
        var client = await mongoDbService.Clients
            .Find(c => c.Id == clientId)
            .FirstOrDefaultAsync();

        if (client == null)
            throw new Exception("Клієнта не знайдено");

        if (string.IsNullOrWhiteSpace(client.EmbeddingProvider) || string.IsNullOrWhiteSpace(client.ProvidedKey))
            throw new Exception("Не вказано embedding-провайдера або ключ");

        return client;
    }

    private List<QuestionDto> FilterValidQuestions(List<QuestionDto> input) =>
        input.Where(dto => !string.IsNullOrWhiteSpace(dto.Question)).ToList();

    private async Task DeleteExistingQuestionsAsync(IMongoCollection<Question> questionsCollection, string clientId)
    {
        await questionsCollection.DeleteManyAsync(q => q.ClientId == clientId);
    }

    private async Task<List<List<double>>> GenerateEmbeddingsAsync(List<QuestionDto> validInput, Client client)
    {
        var provider = embeddingProviderFactory.Create(client.EmbeddingProvider, client.ProvidedKey);
        var texts = validInput.Select(dto => dto.Question).ToList();
        var embeddings = await provider.GenerateEmbeddingsAsync(texts);

        if (embeddings.Count != validInput.Count)
            throw new Exception("Кількість ембеддингів не відповідає кількості питань.");

        return embeddings;
    }

    private async Task InsertQuestionsAsync(
        IMongoCollection<Question> questionsCollection,
        List<QuestionDto> validInput,
        List<List<double>> embeddings,
        string clientId)
    {
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

        await questionsCollection.InsertManyAsync(questions);
    }

    private async Task UpdateClientActivationAsync(Client client)
    {
        var update = Builders<Client>.Update.Set(c => c.IsActivated, true);

        if (string.IsNullOrEmpty(client.ApiKey))
        {
            var newKey = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
            update = update.Set(c => c.ApiKey, newKey);
        }

        await mongoDbService.Clients.UpdateOneAsync(c => c.Id == client.Id, update);
    }

    private async Task CreateVectorSearchIndexAsync(
    IMongoCollection<Question> questionsCollection,
    string indexName,
    int dimensions)
    {
        var definition = new BsonDocument
    {
        { "mappings", new BsonDocument
            {
                { "dynamic", false },
                { "fields", new BsonDocument
                    {
                        { "embedding", new BsonDocument
                            {
                                { "type", "knnVector" },
                                { "dimensions", dimensions },
                                { "similarity", "cosine" }
                            }
                        }
                    }
                }
            }
        }
    };

        var model = new CreateSearchIndexModel(indexName, definition);
        await questionsCollection.SearchIndexes.CreateOneAsync(model);
    }



    //public async Task<List<Question>> GetAll(string clientId, string embeddingProvider)
    //{
    //    var collectionName = mongoDbService.GetQuestionsCollectionName(clientId, embeddingProvider);
    //    var questionsCollection = mongoDbService.GetQuestionsCollection(collectionName);
    //    var result = await questionsCollection.Find(q => q.ClientId == clientId).ToListAsync();
    //    return result ?? new List<Question>();
    //}

    //public async Task Delete(string clientId, string questionId, string embeddingProvider)
    //{
    //    var collectionName = mongoDbService.GetQuestionsCollectionName(clientId, embeddingProvider);
    //    var questionsCollection = mongoDbService.GetQuestionsCollection(collectionName);
    //    var result = await questionsCollection.DeleteOneAsync(
    //        q => q.Id == questionId && q.ClientId == clientId
    //    );

    //    if (result.DeletedCount == 0)
    //        throw new Exception("Питання не знайдено або вже видалене.");
    //}
}


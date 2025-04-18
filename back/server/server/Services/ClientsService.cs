using MongoDB.Bson;
using MongoDB.Driver;
using server.Models;
using server.Interfaces;
using server.Services.Authorization;
using server.Services.MongoDb;
using server.Contracts;
using server.Services.Embeddings;
using Microsoft.AspNetCore.Identity;


namespace server.Services;

public class ClientsService
{
    private readonly IPasswordHasher passwordHasher;
    private readonly IJwtProvider jwtProvider;
    private readonly MongoDbService mongoDbService;
    private readonly IEmbeddingProviderFactory embeddingProviderFactory;
    public ClientsService(
       IPasswordHasher passwordHasher,
       IJwtProvider jwtProvider,
       MongoDbService mongoDbService,
       IEmbeddingProviderFactory embeddingProviderFactory)
    {
        this.passwordHasher = passwordHasher;
        this.jwtProvider = jwtProvider;
        this.mongoDbService = mongoDbService;
        this.embeddingProviderFactory = embeddingProviderFactory;
    }

    public async Task<string> Register(string email, string password)
    {
        var existingUser = await mongoDbService.Clients
            .Find(u => u.Email == email)
            .FirstOrDefaultAsync();

        if (existingUser != null)
        {
            throw new Exception("Користувач з таким email вже існує.");
        }

        var hashedPassword = passwordHasher.Generate(password);

        var newUser = new Client
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Email = email,
            Password = hashedPassword,
            ApiKey = "",
            EmbeddingProvider = "",
            ProvidedKey = "",
            CreatedAt = DateTime.UtcNow,
            Expiration = default
        };

        await mongoDbService.Clients.InsertOneAsync(newUser);

        return jwtProvider.GenerateUserToken(newUser);
    }

    public async Task<string> Login(string email, string password)
    {
        var client = await mongoDbService.Clients
            .Find(c => c.Email == email)
            .FirstOrDefaultAsync();


        var result = passwordHasher.Verify(password, client.Password);

        if (result == false)
        {
            throw new Exception("Failed to Login");
        }

        var token = jwtProvider.GenerateUserToken(client);
        return token;
    }

    public async Task SelectPlan(string clientId, int months)
    {
        if (months <= 0 || months > 12) throw new ArgumentException("Неправильна тривалість тарифу");

        var filter = Builders<Client>.Filter.Eq(c => c.Id, clientId);
        var update = Builders<Client>.Update
            .Set(c => c.Expiration, DateTime.UtcNow.AddMonths(months));

        var result = await mongoDbService.Clients.UpdateOneAsync(filter, update);

        if (result.MatchedCount == 0) throw new Exception("Клієнта не знайдено");
    }

    public async Task SetEmbeddingProvider(string clientId, string providerName, string providedKey)
    {
        if (string.IsNullOrWhiteSpace(providerName) || string.IsNullOrWhiteSpace(providedKey))
            throw new ArgumentException("Провайдер або ключ не можуть бути порожніми");

        var filter = Builders<Client>.Filter.Eq(c => c.Id, clientId);

        var update = Builders<Client>.Update
            .Set(c => c.EmbeddingProvider, providerName)
            .Set(c => c.ProvidedKey, providedKey);

        var result = await mongoDbService.Clients.UpdateOneAsync(filter, update);

        if (result.MatchedCount == 0)
            throw new Exception("Клієнта не знайдено");
    }

    public async Task<ClientProfileDto> GetProfile(string clientId)
    {
        var client = await mongoDbService.Clients
            .Find(c => c.Id == clientId)
            .FirstOrDefaultAsync();

        if (client == null)
            throw new Exception("Клієнта не знайдено");

        return new ClientProfileDto
        {
            Email = client.Email,
            EmbeddingProvider = client.EmbeddingProvider,
            Expiration = client.Expiration,
            IsActivated = client.IsActivated,
            CreatedAt = client.CreatedAt,
            ApiKey = client.ApiKey
        };
    }

    public async Task UploadFaq(string clientId, List<FaqItemDto> items)
    {
        var client = await mongoDbService.Clients
            .Find(c => c.Id == clientId)
            .FirstOrDefaultAsync();

        if (client == null)
            throw new Exception("Клієнта не знайдено");

        if (string.IsNullOrEmpty(client.EmbeddingProvider) || string.IsNullOrEmpty(client.ProvidedKey))
            throw new Exception("Не вказано embedding-провайдера або ключ");

        var provider = embeddingProviderFactory.Create(client.EmbeddingProvider, client.ProvidedKey);

        var questions = items.Select(i => i.Question).ToList();

        var embeddings = await provider.GenerateEmbeddingsAsync(questions);

        var documents = items.Select((item, i) => new Question
        {
            ClientId = clientId,
            Text = item.Question,
            Answer = item.Answer,
            Embedding = embeddings[i],
            CreatedAt = DateTime.UtcNow
        });

        await mongoDbService.Questions.InsertManyAsync(documents);

        var update = Builders<Client>.Update.Set(c => c.IsActivated, true);
        await mongoDbService.Clients.UpdateOneAsync(c => c.Id == clientId, update);
    }
}

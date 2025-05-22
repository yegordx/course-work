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

        var provider = embeddingProviderFactory.Create(providerName, providedKey);

        try
        {
            var embeddings = await provider.GenerateEmbeddingsAsync(new List<string> { "Hello World" });

            if (embeddings == null || embeddings.Count != 1 || embeddings[0].Count == 0)
                throw new Exception("Некоректна відповідь від провайдера.");
        }
        catch (Exception ex)
        {
            throw new Exception("Помилка під час перевірки провайдера: " + ex.Message);
        }

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

    public async Task<string> GetApiKey(string clientId)
    {
        var client = await mongoDbService.Clients
            .Find(c => c.Id == clientId)
            .FirstOrDefaultAsync();

        if (client == null || string.IsNullOrEmpty(client.ApiKey))
            throw new Exception("API ключ не знайдено або клієнта не існує");

        return client.ApiKey;
    }
}

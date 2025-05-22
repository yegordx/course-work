using MongoDB.Driver;
using server.Models;

namespace server.Services.MongoDb;
public class MongoDbService
{
    private readonly IMongoDatabase _database;
    public string DatabaseName { get; }

    public MongoDbService(MongoDbSettings settings)
    {
        var client = new MongoClient(settings.ConnectionString);
        _database = client.GetDatabase(settings.DatabaseName);
        DatabaseName = settings.DatabaseName;
    }

    public IMongoCollection<Client> Clients =>
        _database.GetCollection<Client>("Clients");

    public IMongoCollection<Question> GetQuestionsCollection(string collectionName)
        => _database.GetCollection<Question>(collectionName);

    public string GetQuestionsCollectionName(string clientId, string embeddingProvider)
    {
        var sanitizedProvider = embeddingProvider
            .Replace(" ", "_")
            .Replace("-", "_")
            .ToLowerInvariant();
        return $"Questions_{sanitizedProvider}_{clientId}";
    }
}

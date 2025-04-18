using MongoDB.Driver;
using server.Models;

namespace server.Services.MongoDb;
public class MongoDbService
{
    private readonly IMongoDatabase _database;

    public MongoDbService(MongoDbSettings settings)
    {
        var client = new MongoClient(settings.ConnectionString);
        _database = client.GetDatabase(settings.DatabaseName);
    }

    public IMongoCollection<Client> Clients =>
        _database.GetCollection<Client>("Clients");

    public IMongoCollection<Question> Questions =>
        _database.GetCollection<Question>("FaQ_documents");
}

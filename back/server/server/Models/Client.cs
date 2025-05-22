using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace server.Models;

public class Client
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("email")]
    public string Email { get; set; } = null!;

    [BsonElement("password")]
    public string Password { get; set; } = null!;

    [BsonElement("api_key")]
    public string ApiKey { get; set; } = null!; 

    [BsonElement("embedding_provider")]
    public string EmbeddingProvider { get; set; } = null!; 

    [BsonElement("provided_key")]
    public string ProvidedKey { get; set; } = null!; 

    [BsonElement("created_at")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("expiration")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime Expiration { get; set; }

    [BsonElement("is_activated")]
    public bool IsActivated { get; set; } = false;
}

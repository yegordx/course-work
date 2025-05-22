using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace server.Models;

public class Question
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("client_id")]
    public string ClientId { get; set; } = null!;

    [BsonElement("text")]
    public string Text { get; set; } = null!;

    [BsonElement("answer")]
    public string Answer { get; set; } = null!;

    [BsonElement("embedding")]
    public List<double> Embedding { get; set; } = new();

    [BsonElement("created_at")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
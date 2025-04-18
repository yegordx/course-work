namespace server.Contracts;

public class ClientProfileDto
{
    public string Email { get; set; } = null!;

    public string EmbeddingProvider { get; set; } = string.Empty;

    public DateTime Expiration { get; set; }

    public bool IsActivated { get; set; }

    public DateTime CreatedAt { get; set; }

    public string ApiKey { get; set; } = string.Empty;
}

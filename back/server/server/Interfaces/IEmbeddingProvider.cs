namespace server.Interfaces;

public interface IEmbeddingProvider
{
    Task<List<List<float>>> GenerateEmbeddingsAsync(List<string> inputs);
}

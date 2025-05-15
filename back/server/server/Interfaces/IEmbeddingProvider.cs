namespace server.Interfaces;

public interface IEmbeddingProvider
{
    Task<List<List<double>>> GenerateEmbeddingsAsync(List<string> inputs);
}

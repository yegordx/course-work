namespace server.Interfaces;

public interface IEmbeddingProviderFactory
{
    IEmbeddingProvider Create(string providerName, string apiKey);
}

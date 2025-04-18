using server.Interfaces;

namespace server.Services.Embeddings;

public class EmbeddingProviderFactory : IEmbeddingProviderFactory
{
    private readonly IServiceProvider _serviceProvider;

    public EmbeddingProviderFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IEmbeddingProvider Create(string providerName, string apiKey)
    {
        return providerName.ToLower() switch
        {
            "openai" => ActivatorUtilities.CreateInstance<OpenAiProvider>(_serviceProvider, apiKey),
            "cohere" => ActivatorUtilities.CreateInstance<CohereProvider>(_serviceProvider, apiKey),
            "huggingface" => ActivatorUtilities.CreateInstance<HuggingFaceProvider>(_serviceProvider, apiKey),
            _ => throw new ArgumentException("Невідомий провайдер", nameof(providerName))
        };
    }
}

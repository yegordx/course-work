using server.Interfaces;

namespace server.Services.Embeddings;

public class CohereProvider : IEmbeddingProvider
{
    private readonly string apiKey;
    private readonly HttpClient httpClient;

    public CohereProvider(string apiKey)
    {
        this.apiKey = apiKey;
        httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
    }

    public async Task<List<List<double>>> GenerateEmbeddingsAsync(List<string> inputs)
    {
        var requestBody = new
        {
            texts = inputs,
            model = "embed-english-v3.0"
        };

        var response = await httpClient.PostAsJsonAsync("https://api.cohere.ai/v1/embed", requestBody);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<CohereEmbeddingResponse>();
        return json?.Embeddings ?? new List<List<double>>();
    }

    private class CohereEmbeddingResponse
    {
        public List<List<double>> Embeddings { get; set; } = new();
    }

    public async Task<string> GeneratePrompt(string prompt)
    {
        return "Hello World!";
    }
}
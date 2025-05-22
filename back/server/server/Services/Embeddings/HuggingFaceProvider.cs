using server.Interfaces;
using System.Net.Http.Headers;

namespace server.Services.Embeddings;

public class HuggingFaceProvider : IEmbeddingProvider
{
    private readonly string apiKey;
    private readonly HttpClient httpClient;

    public HuggingFaceProvider(string apiKey)
    {
        this.apiKey = apiKey;
        httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
    }

    public async Task<List<List<double>>> GenerateEmbeddingsAsync(List<string> inputs)
    {
        var results = new List<List<double>>();

        foreach (var input in inputs)
        {
            var content = JsonContent.Create(new { inputs = input });
            var response = await httpClient.PostAsync("https://api-inference.huggingface.co/pipeline/feature-extraction/sentence-transformers/all-MiniLM-L6-v2", content);
            response.EnsureSuccessStatusCode();

            var embedding = await response.Content.ReadFromJsonAsync<List<List<double>>>();
            results.Add(embedding?[0] ?? new List<double>());
        }

        return results;
    }

    public async Task<string> GeneratePrompt(string prompt)
    {
        return "Hello World!";
    }
}
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using server.Interfaces;

public class OpenAiProvider : IEmbeddingProvider
{
    private readonly string apiKey;
    private readonly HttpClient httpClient;

    public OpenAiProvider(string apiKey)
    {
        this.apiKey = apiKey;

        this.httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://api.openai.com/v1/")
        };
        this.httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
    }

    public async Task<List<List<float>>> GenerateEmbeddingsAsync(List<string> inputs)
    {
        var requestBody = new
        {
            input = inputs,
            model = "text-embedding-3-small"
        };

        var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync("embeddings", content);
        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadAsStringAsync();

        var json = JsonDocument.Parse(responseBody);
        var data = json.RootElement.GetProperty("data");

        var result = new List<List<float>>();

        foreach (var item in data.EnumerateArray())
        {
            var vector = item.GetProperty("embedding").EnumerateArray().Select(v => v.GetSingle()).ToList();
            result.Add(vector);
        }

        return result;
    }
}

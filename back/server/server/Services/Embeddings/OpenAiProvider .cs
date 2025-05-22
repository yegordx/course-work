using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using server.Interfaces;

public class OpenAiProvider : IEmbeddingProvider
{
    private readonly HttpClient httpClient;

    public OpenAiProvider(string apiKey)
    {
        httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://api.openai.com/v1/")
        };
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
    }

    public async Task<List<List<double>>> GenerateEmbeddingsAsync(List<string> inputs)
    {
        var requestBody = new
        {
            input = inputs,
            model = "text-embedding-ada-002"
        };

        var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync("embeddings", content);
        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(responseBody);

        var indexToEmbedding = new Dictionary<int, List<double>>();

        foreach (var item in json.RootElement.GetProperty("data").EnumerateArray())
        {
            var index = item.GetProperty("index").GetInt32();
            var rawVector = item.GetProperty("embedding").EnumerateArray().Select(v => v.GetDouble()).ToList();

            indexToEmbedding[index] = rawVector;
        }

        return Enumerable.Range(0, inputs.Count)
                         .Select(i => indexToEmbedding[i])
                         .ToList();
    }

    public async Task<string> GeneratePrompt(string prompt)
    {
        var requestBody = new
        {
            model = "gpt-3.5-turbo",
            messages = new[]
            {
            new { role = "system", content = "Ти корисний асистент, який відповідає на основі наданого контексту." },
            new { role = "user", content = prompt }
        }
        };

        var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync("chat/completions", content);
        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(responseBody);

        var message = json.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        return message ?? "Не вдалося згенерувати відповідь.";
    }
}
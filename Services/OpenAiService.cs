using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

public class OpenAIService
{
    private readonly HttpClient _http;
    private readonly string _apiKey;

    public OpenAIService(IConfiguration config)
    {
        _http = new HttpClient();
        _apiKey = config["OpenAI:ApiKey"] ?? throw new Exception("Missing OpenAI key");
    }


    public async Task<string> GenerateCaptionAsync(string description)
    {
        var requestBody = new
        {
            model = "gpt-4o-mini",
            messages = new[]
            {
                new { role = "system", content = "You are a marketing assistant that writes short, fun, engaging captions for Instagram posts in the same language the user writes." },
                new { role = "user", content = description }
            }
        };


        var requestJson = JsonSerializer.Serialize(requestBody);
        using var req = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
        req.Headers.Add("Authorization", $"Bearer {_apiKey}");
        req.Content = new StringContent(requestJson, Encoding.UTF8, "application/json");

        var resp = await _http.SendAsync(req);
        resp.EnsureSuccessStatusCode();

        var respJson = await resp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(respJson);

        // Navigate JSON like: choices[0].message.content
        var caption = doc
            .RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        return caption ?? "Não consegui gerar a legenda.";
    }

    public async Task<object> GenerateImageAsync(string description)
    {
        var system = "You are a creative visual assistant that generates vivid, photorealistic images based on user descriptions. Always keep the same language as the user.";
        var userPrompt = description;
        var prompt = $"{system}\n\nUser description: {userPrompt}";

        using var req = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/images/generations");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        req.Content = new StringContent(JsonSerializer.Serialize(new
        {
            model = "dall-e-3",
            prompt,
            size = "1024x1024"
        }), Encoding.UTF8, "application/json");

        HttpResponseMessage resp = null!;
        string json = string.Empty;

        for (int i = 0; i < 3; i++)
        {
            resp = await _http.SendAsync(req);
            json = await resp.Content.ReadAsStringAsync();

            if (resp.IsSuccessStatusCode)
                break;

            Console.WriteLine($"Image API attempt {i + 1} failed: {(int)resp.StatusCode} - {json}");
            await Task.Delay(2000); // wait 2s before retry
        }

        if (!resp.IsSuccessStatusCode)
            throw new HttpRequestException($"OpenAI returned {(int)resp.StatusCode}: {resp.ReasonPhrase}\n{json}");

        using var doc = JsonDocument.Parse(json);
        var imageUrl = doc.RootElement
            .GetProperty("data")[0]
            .GetProperty("url")
            .GetString();

        return new { description, imageUrl };
    }


}

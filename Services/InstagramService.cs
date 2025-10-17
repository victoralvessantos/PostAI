using System.Net.Http.Headers;
using System.Text.Json;

namespace PostAI.Api.Services;

public class InstagramService
{
    private readonly IConfiguration _config;
    private readonly HttpClient _http;
    private readonly StorageService _storage;

    public InstagramService(IConfiguration config, StorageService storage)
    {
        _config = config;
        _storage = storage;
        _http = new HttpClient();
    }

    public string GetLoginUrl()
    {
        var appId = _config["Instagram:AppId"];
        var redirect = _config["Instagram:RedirectUri"];

        // ✅ Business / Creator permissions
        var scope = string.Join(",",
            new[]
            {
                "instagram_business_basic",
                "instagram_business_manage_messages",
                "instagram_business_manage_comments",
                "instagram_business_content_publish",
                "instagram_business_manage_insights"
            });

        // ✅ Correct OAuth URL for Instagram Graph API
        var url =
            $"https://www.instagram.com/oauth/authorize" +
            $"?force_reauth=true" +
            $"&client_id={appId}" +
            $"&redirect_uri={Uri.EscapeDataString(redirect!)}" +
            $"&response_type=code" +
            $"&scope={Uri.EscapeDataString(scope)}";

        return url;
    }


    public async Task<string> ExchangeCodeAsync(string code)
    {
        var appId = _config["Instagram:AppId"];
        var secret = _config["Instagram:AppSecret"];
        var redirect = _config["Instagram:RedirectUri"];

        var resp = await _http.PostAsync($"https://api.instagram.com/oauth/access_token",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["client_id"] = appId!,
                ["client_secret"] = secret!,
                ["grant_type"] = "authorization_code",
                ["redirect_uri"] = redirect!,
                ["code"] = code
            }));

        var json = await resp.Content.ReadAsStringAsync();
        Console.WriteLine(json);
        using var doc = JsonDocument.Parse(json);

        return doc.RootElement.GetProperty("access_token").GetString()!;
    }
    
    // public async Task<string> ExchangeCodeAsync(string code)
    // {
    //     var appId = _config["Instagram:AppId"];
    //     var secret = _config["Instagram:AppSecret"];
    //     var redirect = _config["Instagram:RedirectUri"];

    //     // Step 1: short-lived token
    //     var tokenResp = await _http.PostAsync(
    //         "https://graph.facebook.com/v20.0/oauth/access_token",
    //         new FormUrlEncodedContent(new Dictionary<string, string>
    //         {
    //             ["client_id"] = appId!,
    //             ["client_secret"] = secret!,
    //             ["redirect_uri"] = redirect!,
    //             ["code"] = code
    //         }));

    //     var json = await tokenResp.Content.ReadAsStringAsync();
    //     Console.WriteLine(json);
    //     using var doc = JsonDocument.Parse(json);
    //     var shortLived = doc.RootElement.GetProperty("access_token").GetString()!;

    //     // Step 2: long-lived token (~60 days)
    //     var longResp = await _http.GetAsync(
    //         $"https://graph.facebook.com/v20.0/oauth/access_token" +
    //         $"?grant_type=fb_exchange_token" +
    //         $"&client_id={appId}" +
    //         $"&client_secret={secret}" +
    //         $"&fb_exchange_token={shortLived}");

    //     var json2 = await longResp.Content.ReadAsStringAsync();
    //     using var doc2 = JsonDocument.Parse(json2);
    //     var longLived = doc2.RootElement.GetProperty("access_token").GetString()!;

    //     return longLived;
    // }


    public async Task<string> PublishPostAsync(string accessToken, string imageUrl, string caption)
    {
        var userId = await GetUserIdAsync(accessToken);

        // Step 1: Create media object
        var createResp = await _http.PostAsync(
            $"https://graph.facebook.com/v20.0/{userId}/media",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["image_url"] = imageUrl,
                ["caption"] = caption,
                ["access_token"] = accessToken
            }));

        var json1 = await createResp.Content.ReadAsStringAsync();
        using var doc1 = JsonDocument.Parse(json1);
        var creationId = doc1.RootElement.GetProperty("id").GetString();

        // Step 2: Publish it
        var publishResp = await _http.PostAsync(
            $"https://graph.facebook.com/v20.0/{userId}/media_publish?creation_id={creationId}&access_token={accessToken}", null);

        publishResp.EnsureSuccessStatusCode();
        return "Post published successfully!";
    }

    private async Task<string> GetUserIdAsync(string accessToken)
    {
        var json = await _http.GetStringAsync($"https://graph.facebook.com/v20.0/me?fields=id&access_token={accessToken}");
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("id").GetString()!;
    }
}

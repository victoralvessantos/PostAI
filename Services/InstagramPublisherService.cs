using System.Net.Http.Headers;
using System.Text.Json;

public class InstagramPublisherService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;

    public InstagramPublisherService(HttpClient http, IConfiguration config)
    {
        _http = http;
        _config = config;
    }

    public async Task<string> PublishAsync(string accessToken, string imageUrl, string caption)
    {
        // 1️⃣ Get user's connected Facebook Page(s)
        var pagesResp = await _http.GetAsync($"https://graph.facebook.com/v20.0/me/accounts?access_token={accessToken}");
        var pagesJson = await pagesResp.Content.ReadAsStringAsync();
        using var pagesDoc = JsonDocument.Parse(pagesJson);

        var pageId = pagesDoc.RootElement.GetProperty("data")[0].GetProperty("id").GetString();

        // 2️⃣ Get Instagram Business Account ID linked to that Page
        var igResp = await _http.GetAsync($"https://graph.facebook.com/v20.0/{pageId}?fields=instagram_business_account&access_token={accessToken}");
        var igJson = await igResp.Content.ReadAsStringAsync();
        using var igDoc = JsonDocument.Parse(igJson);

        var igUserId = igDoc.RootElement
            .GetProperty("instagram_business_account")
            .GetProperty("id")
            .GetString();

        // 3️⃣ Create a media container (image + caption)
        var mediaResp = await _http.PostAsync(
            $"https://graph.facebook.com/v20.0/{igUserId}/media",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["image_url"] = imageUrl,
                ["caption"] = caption,
                ["access_token"] = accessToken
            }));

        var mediaJson = await mediaResp.Content.ReadAsStringAsync();
        using var mediaDoc = JsonDocument.Parse(mediaJson);
        var creationId = mediaDoc.RootElement.GetProperty("id").GetString();

        // 4️⃣ Publish the media container
        var publishResp = await _http.PostAsync(
            $"https://graph.facebook.com/v20.0/{igUserId}/media_publish",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["creation_id"] = creationId!,
                ["access_token"] = accessToken
            }));

        var publishJson = await publishResp.Content.ReadAsStringAsync();
        return publishJson; // should include the post id
    }
}

using System.Text.Json;

public static class InstagramPublishEndpoints
{
    public static void MapInstagramPublishEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/instagram/publish", async (
            InstagramPublisherService publisher,
            HttpRequest request) =>
        {
            var body = await JsonSerializer.DeserializeAsync<JsonElement>(request.Body);
            var token = body.GetProperty("accessToken").GetString();
            var imageUrl = body.GetProperty("imageUrl").GetString();
            var caption = body.GetProperty("caption").GetString();

            var result = await publisher.PublishAsync(token!, imageUrl!, caption!);
            return Results.Ok(JsonDocument.Parse(result));
        });
    }
}

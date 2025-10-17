using System.Text.Json;
using PostAI.Api.Services;

namespace PostAI.Api.Endpoints;

public static class PromoEndpoints
{
    public static void MapEndpoints(IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/promo");

        group.MapPost("/generate",
            async (PromoRequest req, OpenAIService ai, StorageService storage, UsageTrackerService tracker) =>
            {
                var caption = await ai.GenerateCaptionAsync(req.Description);
                await tracker.LogAsync("anonymous", "caption", 0.002m);

                dynamic image = await ai.GenerateImageAsync(req.Description);
                await tracker.LogAsync("anonymous", "image", 0.04m);

                // Save permanent copy
                var blobUrl = await storage.SaveImageAsync(
                    (string)image.imageUrl,
                    $"{Guid.NewGuid()}.png");

                await storage.SavePostAsync("anonymous", req.Description, caption, blobUrl);

                return Results.Ok(new { req.Description, caption, imageUrl = blobUrl });
            })
        .WithName("GeneratePromo")
        .WithOpenApi(op =>
        {
            op.Summary = "Generates image and caption to an instagram post";
            op.Description = "Creates an image and caption based on a description";
            return op;
        });
    }
}

public record PromoRequest(string Description);

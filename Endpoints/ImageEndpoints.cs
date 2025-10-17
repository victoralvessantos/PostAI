using PostAI.Api.Services;

namespace PostAI.Api.Endpoints;

public static class ImageEndpoints
{
    public static void MapEndpoints(IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/image");

        group.MapPost("/generate",
            async (ImageRequest req, OpenAIService ai, UsageTrackerService tracker) =>
            {
                var result = await ai.GenerateImageAsync(req.Description);

                await tracker.LogAsync("anonymous", "image", 0.04m); // rough DALL·E 3 cost

                return Results.Ok(result);
            })
        .WithName("GenerateImage")
        .WithOpenApi(op =>
        {
            op.Summary = "Generates an image based on a description";
            op.Description = "Uses DALL·E 3 to create image according to a given text";
            return op;
        });
    }
}

public record ImageRequest(string Description);

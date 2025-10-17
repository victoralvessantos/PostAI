using PostAI.Api.Services;

namespace PostAI.Api.Endpoints;

public static class CaptionEndpoints
{
    public static void MapEndpoints(IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/caption");

        group.MapPost("/generate",
            async (CaptionRequest req, OpenAIService ai, UsageTrackerService tracker) =>
            {
                var caption = await ai.GenerateCaptionAsync(req.Description);

                await tracker.LogAsync("anonymous", "caption", 0.002m);

                return Results.Ok(new { caption });
            })
        .WithName("GenerateCaption")
        .WithOpenApi(operation =>
        {
            operation.Summary = "Generates a fun caption to be used on instagram";
            operation.Description = "Receives a description and returns a small caption with emojis and hastags";
            return operation;
        });
    }
}

public record CaptionRequest(string Description);
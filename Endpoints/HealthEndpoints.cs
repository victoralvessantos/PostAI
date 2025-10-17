namespace PostAI.Api.Endpoints;

public static class HealthEndpoints
{
    public static void MapEndpoints(IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/health");

        group.MapGet("/", () =>
            Results.Ok(new
            {
                status = "ok",
                version = "1.0",
                framework = "net9.0"
            }))
        .WithName("HealthCheck")
        .WithOpenApi(operation =>
        {
            operation.Summary = "Verify API status.";
            return operation;
        });
    }
}

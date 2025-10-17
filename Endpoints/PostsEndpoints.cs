using PostAI.Api.Services;

namespace PostAI.Api.Endpoints;

public static class PostsEndpoints
{
    public static void MapEndpoints(IEndpointRouteBuilder routes)
    {
        var g = routes.MapGroup("/api/posts");
        g.MapGet("/", async (StorageService storage) =>
        {
            var posts = await storage.GetPostsAsync("anonymous");
            return Results.Ok(posts);
        })
        .WithName("GetPosts")
        .WithOpenApi(op => { op.Summary = "Lists generated posts"; return op; });
    }
}

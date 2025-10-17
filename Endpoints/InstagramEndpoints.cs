using PostAI.Api.Services;

namespace PostAI.Api.Endpoints;

public static class InstagramEndpoints
{
    public static void MapEndpoints(IEndpointRouteBuilder routes)
    {
        var g = routes.MapGroup("/api/instagram");

        g.MapGet("/login", (InstagramService ig) =>
        {
            var url = ig.GetLoginUrl();
            return Results.Redirect(url);
        })
        .WithOpenApi(operation =>
        {
            operation.Summary = "Logins to instagram account";
            return operation;
        });

        g.MapGet("/callback", async (string code, InstagramService ig, StorageService storage) =>
        {
            var token = await ig.ExchangeCodeAsync(code);
            await storage.SavePostAsync("anonymous", "token", "instagram", token); // quick store
            return Results.Ok("Instagram connected!");
        });

        g.MapPost("/post",
            async (PostRequest req, InstagramService ig, StorageService storage) =>
            {
                var tokenEntity = (await storage.GetPostsAsync("anonymous"))
                    .FirstOrDefault(x => x["Caption"].ToString() == "instagram");

                if (tokenEntity is null)
                    return Results.BadRequest(new { message = "Instagram not connected. Please login first." });

                var token = tokenEntity["ImageUrl"].ToString();
                var result = await ig.PublishPostAsync(token!, req.ImageUrl, req.Caption);

                return Results.Ok(new { result });
            })
        .WithOpenApi(operation =>
        {
            operation.Summary = "Post the image and the caption generated on the provided instagram account";
            return operation;
        });
    }
}

public record PostRequest(string ImageUrl, string Caption);

using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace PostAI.Api.Services;

public class StorageService
{
    private readonly BlobContainerClient _blob;
    private readonly TableClient _table;

    public StorageService(IConfiguration cfg)
    {
        var conn = cfg["Azure:StorageConnectionString"]!;
        var container = cfg["Azure:BlobContainer"]!;
        var table = cfg["Azure:TableName"]!;

        _blob = new BlobContainerClient(conn, container);
        _blob.CreateIfNotExists(PublicAccessType.Blob);
        _table = new TableClient(conn, table);
        _table.CreateIfNotExists();
    }

    public async Task<string> SaveImageAsync(string imageUrl, string fileName)
    {
        using var http = new HttpClient();
        var bytes = await http.GetByteArrayAsync(imageUrl);
        var blob = _blob.GetBlobClient(fileName);
        await blob.UploadAsync(new MemoryStream(bytes), overwrite: true);
        return blob.Uri.ToString();
    }

    public async Task SavePostAsync(string userId, string description, string caption, string imageUrl)
    {
        var entity = new TableEntity(userId, Guid.NewGuid().ToString())
        {
            ["Description"] = description,
            ["Caption"] = caption,
            ["ImageUrl"] = imageUrl,
            ["CreatedAt"] = DateTime.UtcNow
        };
        await _table.AddEntityAsync(entity);
    }

    public async Task<List<TableEntity>> GetPostsAsync(string userId)
    {
        var query = _table.QueryAsync<TableEntity>(x => x.PartitionKey == userId);
        var list = new List<TableEntity>();
        await foreach (var e in query) list.Add(e);
        return list.OrderByDescending(x => (DateTime)x["CreatedAt"]).ToList();
    }
}

using Azure.Data.Tables;

namespace PostAI.Api.Services;

public class UsageTrackerService
{
    private readonly TableClient _table;

    public UsageTrackerService(TableClient table) => _table = table;

    public async Task LogAsync(string userId, string feature, decimal costUsd)
    {
        var entity = new TableEntity($"user-{userId}", Guid.NewGuid().ToString())
        {
            { "Feature", feature },
            { "ApiCostUsd", costUsd },
            { "Timestamp", DateTime.UtcNow }
        };

        await _table.AddEntityAsync(entity);
    }
}

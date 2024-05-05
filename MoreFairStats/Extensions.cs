using Microsoft.Azure.Cosmos;
using System.Text.Json;

namespace MoreFairStats;

public static class Extensions
{
    public static async Task CreateItem<T>(this Container container, T item, string partitionKey)
    {
        await container.CreateItemAsync(item, new PartitionKey(partitionKey));
    }

    public static async Task CreateItem<T>(this Container container, T item, int partitionKey)
    {
        await container.CreateItemAsync(item, new PartitionKey(partitionKey));
    }

    public static async Task<T> ReadItem<T>(this Container container, string id, int partitionKey)
        => (await container.ReadItemAsync<T>(id, new PartitionKey(partitionKey))).Resource;

    public static async Task<T> ReadItem<T>(this Container container, string id, string partitionKey)
        => (await container.ReadItemAsync<T>(id, new PartitionKey(partitionKey))).Resource;

    public static async Task UpsertItem<T>(this Container container, T item, int partitionKey)
    {
        await container.UpsertItemAsync(item, new PartitionKey(partitionKey));
    }

    public static async Task UpsertItem<T>(this Container container, T item, string partitionKey)
    {
        await container.UpsertItemAsync(item, new PartitionKey(partitionKey));
    }

    public static async Task PatchItem<T>(this Container container, string id, int partitionKey, IReadOnlyList<PatchOperation> patchOperations)
    {
        await container.PatchItemAsync<T>(id, new PartitionKey(partitionKey), patchOperations);
    }

    public static async Task DeleteItem<T>(this Container container, string id, int partitionKey)
    {
        await container.DeleteItemAsync<T>(id, new PartitionKey(partitionKey));
    }

    public static T Deserialize<T>(this string json)
    {
        var jsonSerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        return JsonSerializer.Deserialize<T>(json, jsonSerOptions)!;
    }
}

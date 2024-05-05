using Microsoft.Azure.Cosmos;
using System.Text.Json;

namespace MoreFairStats;

public static class Extensions
{
    public static T Sync<T>(this Task<T> task)
    {
        task.Wait();
        return task.Result;
    }

    public static T ReadItem<T>(this Container container, string id, PartitionKey partitionKey)
        => container.ReadItemAsync<T>(id, partitionKey).Sync().Resource;

    public static T ReadItem<T>(this Container container, string id, int partitionKey)
        => container.ReadItemAsync<T>(id, new PartitionKey(partitionKey)).Sync().Resource;

    public static T ReadItem<T>(this Container container, string id, string partitionKey)
    => container.ReadItemAsync<T>(id, new PartitionKey(partitionKey)).Sync().Resource;

    public static void UpsertItem<T>(this Container container, T item, PartitionKey partitionKey)
    {
        var resp = container.UpsertItemAsync<T>(item, partitionKey).Sync();
        Console.WriteLine(resp.StatusCode);
    }

    public static void UpsertItem<T>(this Container container, T item, int partitionKey)
    {
        var resp = container.UpsertItemAsync<T>(item, new PartitionKey(partitionKey)).Sync();
        Console.WriteLine(resp.StatusCode);
    }

    public static void UpsertItem<T>(this Container container, T item, string partitionKey)
    {
        var resp = container.UpsertItemAsync<T>(item, new PartitionKey(partitionKey)).Sync();
        Console.WriteLine(resp.StatusCode);
    }

    public static HttpResponseMessage Get(this HttpClient client, string requestUrl)
        => client.GetAsync(requestUrl).Sync();

    public static string ReadAsString(this HttpContent content)
        => content.ReadAsStringAsync().Sync();

    public static FeedResponse<T> ReadNext<T>(this FeedIterator<T> iterator)
        => iterator.ReadNextAsync().Sync();

    public static T Deserialize<T>(this string json)
    {
        var jsonSerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        return JsonSerializer.Deserialize<T>(json, jsonSerOptions)!;
    }
}

using MoreFairStats;

namespace MoreFairStatsManagement;
public class APIRoundStats
{
    public Dictionary<string, LadderStats>? Ladders { get; set; }
    public List<string>? RoundTypes { get; set; }
    public string? BasePointsToPromote { get; set; }
    public string? CreatedOn { get; set; }
    public string? ClosedOn { get; set; }
    public int Number { get; set; }

    public RoundStats ToRoundStats()
    {
        return new()
        {
            BasePointsToPromote = BasePointsToPromote,
            ClosedOn = ClosedOn,
            CreatedOn = CreatedOn,
            Number = Number,
            NumberOfLadders = Ladders!.Count,
            RoundTypes = RoundTypes,
            id = Number.ToString()
        };
    }
}

public static class MoreFairAPI
{
    private static readonly string BaseURL = "https://fair.kaliburg.de/api/stats/round/raw?season=2";

    public static async Task<APIRoundStats> GetLatest()  // OBS! This doesn't work post-s2 anymore ()
        => await Get(BaseURL);

    public static async Task<APIRoundStats> GetRound(int round)
        => await Get(BaseURL + $"&round={round}");

    private static async Task<APIRoundStats> Get(string url)
    {
        using var client = new HttpClient();
        using var apiResp = await client.GetAsync(url);
        var bodyString = await apiResp.Content.ReadAsStringAsync();
        var roundData = bodyString.Deserialize<APIRoundStats>();
        return roundData;
    }
}
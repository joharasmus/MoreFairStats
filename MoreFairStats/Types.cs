﻿using Microsoft.Azure.Cosmos;

namespace MoreFairStats;

public class Config
{
    public int id { get; set; }
    public int CurrentMaxRound { get; set; }
}
public class RoundAppearance
{
    public Dictionary<string, LadderAppearance>? Ladders { get; set; }
    public List<string>? RoundTypes { get; set; }
    public string? BasePointsToPromote { get; set; }
    public string? CreatedOn { get; set; }
    public string? ClosedOn { get; set; }
    public int Number { get; set; }
}

public class RoundStats
{
    public string? id { get; set; }
    public List<string>? RoundTypes { get; set; }
    public string? BasePointsToPromote { get; set; }
    public string? CreatedOn { get; set; }
    public string? ClosedOn { get; set; }
    public int Number { get; set; }
    public int NumberOfLadders { get; set; }
}

public class LadderStats
{
    public string? id { get; set; }
    public List<Ranker>? Rankers { get; set; }
    public string? CreatedOn { get; set; }
    public string? BasePointsToPromote { get; set; }
    public List<string>? LadderTypes { get; set; }
    public int Round { get; set; }
    public int Ladder { get; set; }
}

public class LadderAppearance
{
    public string? CreatedOn { get; set; }
    public string? BasePointsToPromote { get; set; }
    public List<string>? LadderTypes { get; set; }
    public int Rank { get; set; }
    public string? Points { get; set; }
    public string? Power { get; set; }
    public int Bias { get; set; }
    public int Multi { get; set; }
    public string? Grapes { get; set; }
    public string? Vinegar { get; set; }
    public bool AutoPromote { get; set; }
    public bool Growing { get; set; }
}

public class Ranker
{
    public int AccountId { get; set; }
    public string? UserName { get; set; }
    public int Rank { get; set; }
    public string? Points { get; set; }
    public string? Power { get; set; }
    public int Bias { get; set; }
    public int Multi { get; set; }
    public int AssholePoints { get; set; }
    public string? Grapes { get; set; }
    public string? Vinegar { get; set; }
    public bool AutoPromote { get; set; }
    public bool Growing { get; set; }
}

public class LadderRanker
{
    public string? id { get; set; }
    public int Round { get; set; }
    public int Ladder { get; set; }
    public int AccountId { get; set; }
    public string? UserName { get; set; }
    public int Rank { get; set; }
    public string? Points { get; set; }
    public string? Power { get; set; }
    public int Bias { get; set; }
    public int Multi { get; set; }
    public int AssholePoints { get; set; }
    public string? Grapes { get; set; }
    public string? Vinegar { get; set; }
    public bool AutoPromote { get; set; }
    public bool Growing { get; set; }
}

public class PlayerStats
{
    public int AccountId { get; set; }
    public string? UserName { get; set; }
    public int AHPoints { get; set; }
    public List<RoundAppearance> RoundAppearances { get; set; } = [];
}

public class MoreFairData
{
    private Container LadderDb { get; init; }
    private Container RoundDb { get; init; }
    private Container ConfigDb { get; init; }
    private Container RankersDb { get; init; }

    public MoreFairData(string connectionString)
    {
        var options = new CosmosClientOptions() { EnableContentResponseOnWrite = false, AllowBulkExecution = true };
        var cosmosClient = new CosmosClient(connectionString, options);
        var cosmosDB = cosmosClient.GetDatabase("mfs-cosmosdb");

        LadderDb = cosmosDB.GetContainer("mfs-ladders");
        RoundDb = cosmosDB.GetContainer("mfs-rounds");
        ConfigDb = cosmosDB.GetContainer("mfs-config");
        RankersDb = cosmosDB.GetContainer("mfs-ladderRankers");
    }

    public async Task Create(LadderRanker ranker)
        => await RankersDb.CreateItem(ranker, ranker.Round);

    public async Task<Config> GetConfig()
        => await ConfigDb.ReadItem<Config>("1", "1");

    public async Task<LadderStats> GetLadder(int round, int ladder)
        => await LadderDb.ReadItem<LadderStats>($"R{round}L{ladder}", round);
    public async Task<LadderRanker> GetLadderRanker(int round, int ladder, int accountId)
        => await RankersDb.ReadItem<LadderRanker>($"R{round}L{ladder}P{accountId}", round);
    public async Task<RoundStats> GetRound(int round)
        => await RoundDb.ReadItem<RoundStats>(round.ToString(), round);

    public async Task Upsert(LadderStats ladderStats)
        => await LadderDb.UpsertItem(ladderStats, ladderStats.Round);

    public async Task Upsert(RoundStats roundStats)
        => await RoundDb.UpsertItem(roundStats, roundStats.Number);

    public async Task Upsert(LadderRanker ladderRanker)
        => await RankersDb.UpsertItem(ladderRanker, ladderRanker.Round);

    public async Task PatchLadder(LadderStats ladderStats, IReadOnlyList<PatchOperation> patchOperations)
        => await LadderDb.PatchItem<LadderStats>(ladderStats.id!, ladderStats.Round, patchOperations);

    public FeedIterator<LadderStats> GetLadderQueryIterator(QueryDefinition queryDefinition)
        => LadderDb.GetItemQueryIterator<LadderStats>(queryDefinition);

    public FeedIterator<LadderRanker> GetLadderRankersQueryIterator(QueryDefinition queryDefinition)
        => RankersDb.GetItemQueryIterator<LadderRanker>(queryDefinition);
}
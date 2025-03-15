
using Microsoft.Azure.Cosmos;
using MoreFairStats;
using Microsoft.Extensions.Configuration;
using MoreFairStatsManagement;


var programConfig = new ConfigurationBuilder().AddUserSecrets<Program>().Build();  //This gets the local secrets

var dbConnStr = programConfig["mfsCosmosDbConnStr"]!;
var moreFairData = new MoreFairData(dbConnStr);
var mfsConfig = await moreFairData.GetConfig();
var currentMaxRound = mfsConfig.CurrentMaxRound;

async Task updateWithNewRoundStats(APIRoundStats apiRoundStats)
{
    var dbRoundStats = apiRoundStats.ToRoundStats();
    await moreFairData.Upsert(dbRoundStats);
    var round = dbRoundStats.Number;
    foreach (var ladder in apiRoundStats.Ladders!)
    {
        var ladderStats = ladder.Value;
        ladderStats.Round = round;
        ladderStats.Ladder = int.Parse(ladder.Key);
        Console.WriteLine(ladderStats.Ladder);
        ladderStats.id = $"R{round}L{ladder.Key}";

        var tasks = new List<Task>();

        foreach (var ranker in ladderStats.Rankers!)
        {
            ranker.id = $"{ladderStats.id}P{ranker.AccountId}";
            ranker.Ladder = ladderStats.Ladder;
            ranker.Round = round;
            tasks.Add(moreFairData.Upsert(ranker));
        }
        await Task.WhenAll(tasks);

        ladderStats.Rankers = [];

        await moreFairData.Upsert(ladderStats);
    }
}

async Task UpdateNewRound(int newRoundNumber)
{
    var latestRoundData = await MoreFairAPI.GetRound(newRoundNumber);
    Console.WriteLine(latestRoundData.BasePointsToPromote);
    await updateWithNewRoundStats(latestRoundData);
    mfsConfig.CurrentMaxRound = newRoundNumber;
    await moreFairData.Upsert(mfsConfig);
}

await UpdateNewRound(300);
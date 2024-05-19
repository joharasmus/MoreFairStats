// See https://aka.ms/new-console-template for more information
using Microsoft.Azure.Cosmos;
using MoreFairStats;
using Microsoft.Extensions.Configuration;
using MoreFairStatsManagement;


var programConfig = new ConfigurationBuilder().AddUserSecrets<Program>().Build();  //This gets the local secrets

var dbConnStr = programConfig["mfsCosmosDbConnStr"]!;
var moreFairData = new MoreFairData(dbConnStr);

var mfsConfig = await moreFairData.GetConfig();
var currentMaxRound = mfsConfig.CurrentMaxRound;


//for (int i = 1; i <= currentMaxRound; i++)
//{
//    var trialRound = await moreFairData.GetRound(i);
//    Console.WriteLine(trialRound.id);
//    for (int j = 1; j <= trialRound.NumberOfLadders; j++)
//    {
//        var trialLadder = await moreFairData.GetLadder(i, j);
//        await moreFairData.Upsert(trialLadder);
//    }
//}

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
            var ladderRanker = new LadderRanker()
            {
                AccountId = ranker.AccountId,
                AutoPromote = ranker.AutoPromote,
                Bias = ranker.Bias,
                Grapes = ranker.Grapes,
                Growing = ranker.Growing,
                id = $"{ladderStats.id}P{ranker.AccountId}",
                Ladder = ladderStats.Ladder,
                Multi = ranker.Multi,
                Points = ranker.Points,
                Power = ranker.Power,
                Rank = ranker.Rank,
                Round = round,
                Vinegar = ranker.Vinegar
            };
            var player = new Player()
            {
                AHPoints = ranker.AssholePoints,
                id = ranker.AccountId.ToString(),
                UserName = ranker.UserName
            };
            tasks.Add(moreFairData.Upsert(ladderRanker));
            tasks.Add(moreFairData.Upsert(player));
        }
        await Task.WhenAll(tasks);

        ladderStats.Rankers = [];

        await moreFairData.Upsert(ladderStats);
    }
}

async Task refreshLadderStatsForRound(APIRoundStats apiRoundStats)
{
    var round = apiRoundStats.Number;
    foreach (var ladder in apiRoundStats.Ladders!)
    {
        var ladderStats = ladder.Value;
        ladderStats.Round = round;
        ladderStats.Ladder = int.Parse(ladder.Key);
        ladderStats.id = $"R{round}L{ladder.Key}";
        await moreFairData.Upsert(ladderStats);
    }
}

async Task UpdateNewRound()
{
    var latestRoundData = await MoreFairAPI.GetRound(263);
    Console.WriteLine(latestRoundData.BasePointsToPromote);
    await updateWithNewRoundStats(latestRoundData);
}

await UpdateNewRound();
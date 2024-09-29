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

async Task<LadderStats> parseLadderStats(int round, int ladder)
{
    var ladderStats = await moreFairData.GetLadder(round, ladder);
    var queryString = $"SELECT TOP 10 c.AccountId FROM c WHERE c.Round = {round} AND c.Ladder = {ladder} AND c.Growing = false ORDER BY c.Rank DESC";
    var queryDef = new QueryDefinition(queryString);
    using var queryIter = moreFairData.GetLadderRankersQueryIterator(queryDef);
    var allResults = new List<FeedResponse<LadderRanker>>();
    var result = await queryIter.ReadNextAsync();
    var championPoints = 10;
    foreach (var ladderRanker in result)
    {
        var ranker = new Ranker()
        {
            AccountId = ladderRanker.AccountId,
            ChampionPoints = championPoints
        };
        ladderStats.Rankers!.Add(ranker);
        championPoints--;
    }
    return ladderStats;
}

//var roundNumber = 276;
//var roundStats = await moreFairData.GetRound(roundNumber);
//var champions = new List<Champion>();

//for (var i = 1; i < roundStats.NumberOfLadders; i++) 
//{
//    var ladderStats = await parseLadderStats(roundNumber, i);
//    foreach (var ranker in ladderStats.Rankers!)
//    {
//        var championMatch = champions.Find(c => c.AccountId == ranker.AccountId);
//        if (championMatch == null)
//        {
//            var newChamp = new Champion() { AccountId = ranker.AccountId, ChampionPoints = [] };
//            newChamp.ChampionPoints.Add(i, ranker.ChampionPoints);
//            champions.Add(newChamp);
//        }
//        else
//        {
//            championMatch.ChampionPoints.Add(i, ranker.ChampionPoints);
//        }
//    }
//}

//var allIds = champions.Select(c => c.AccountId.ToString()).ToList();
//var playersFeedResp = await moreFairData.GetManyPlayers(allIds);
//var players = playersFeedResp.ToList();


//foreach (var champion in champions.OrderByDescending(c => c.ChampionPointsSum))
//{
//    champion.UserName = players.First(p => int.Parse(p.id!) == champion.AccountId).UserName;
//    Console.WriteLine(champion.UserName);
//    Console.WriteLine(champion.ChampionPoints.Values.Sum());
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

async Task UpdateNewRound(int newRoundNumber)
{
    var latestRoundData = await MoreFairAPI.GetRound(newRoundNumber);
    Console.WriteLine(latestRoundData.BasePointsToPromote);
    await updateWithNewRoundStats(latestRoundData);
    mfsConfig.CurrentMaxRound = newRoundNumber;
    await moreFairData.Upsert(mfsConfig);
}

await UpdateNewRound(295);
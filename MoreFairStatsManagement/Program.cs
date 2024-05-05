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

var trialRound = await moreFairData.GetRound(currentMaxRound);
for(int i = 1; i <= trialRound.NumberOfLadders; i++)
{
    var trialLadder = await moreFairData.GetLadder(currentMaxRound, i);
    Console.WriteLine(trialLadder.id);
    foreach (var trialRanker in trialLadder.Rankers!)
    {
        var ladderRanker = new LadderRanker()
        {
            id = $"{trialLadder.id}P{trialRanker.AccountId}",
            Round = trialLadder.Round,
            Ladder = trialLadder.Ladder,
            AccountId = trialRanker.AccountId,
            UserName = trialRanker.UserName,
            Rank = trialRanker.Rank,
            Points = trialRanker.Points,
            Power = trialRanker.Power,
            Bias = trialRanker.Bias,
            Multi = trialRanker.Multi,
            AssholePoints = trialRanker.AssholePoints,
            Grapes = trialRanker.Grapes,
            Vinegar = trialRanker.Vinegar,
            AutoPromote = trialRanker.AutoPromote,
            Growing = trialRanker.Growing
        };
        await moreFairData.Upsert(ladderRanker);
    }
}

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
        ladderStats.id = $"R{round}L{ladder.Key}";
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

async Task UpdatePlayersWithNewNames(int roundToQuery)
{
    var firstLadder = await moreFairData.GetLadder(roundToQuery, 1);
    var firstLadderRankers = firstLadder.Rankers!;
    var queryString = $"SELECT * FROM c WHERE c.Round != {roundToQuery}";
    var queryDef = new QueryDefinition(queryString);
    using var queryIter = moreFairData.GetLadderQueryIterator(queryDef);
    while (queryIter.HasMoreResults)
    {
        var result = await queryIter.ReadNextAsync();
        foreach (var ladder in result)
        {
            Console.WriteLine(ladder.id);
            var ladderRankers = ladder.Rankers!;
            var index = 0;
            foreach (var ranker in ladderRankers)
            {
                var matchRanker = firstLadderRankers.Find(r => r.AccountId == ranker.AccountId);
                if (matchRanker != null && (matchRanker.UserName != ranker.UserName || matchRanker.AssholePoints != ranker.AssholePoints))
                {
                    Console.WriteLine(matchRanker.AccountId);
                    var patchOps = new[]
                    {
                        PatchOperation.Replace($"/Rankers/{index}/UserName", matchRanker.UserName),
                        PatchOperation.Replace($"/Rankers/{index}/AssholePoints", matchRanker.AssholePoints),
                    };
                    await moreFairData.PatchLadder(ladder, patchOps);
                }
                index++;
            }
        }
    }
}

async Task UpdateNewRound()
{
    var latestRoundData = await MoreFairAPI.GetLatest();
    Console.WriteLine(latestRoundData.BasePointsToPromote);
    while (currentMaxRound < latestRoundData.Number - 1)  // Iterate until we're 1 away from the last round
    {
        throw new NotImplementedException("Iterate until we get to equality (there may have been multiple ladder updates)");
    }
    await refreshLadderStatsForRound(latestRoundData);
    await UpdatePlayersWithNewNames(latestRoundData.Number);
}

// private void invertRoundsToPlayers()
// {
//     var allRankers = new SortedDictionary<int, PlayerStats>();
//     for (int i = 1; i <= currentMaxRound; i++)
//     {
//         parseRoundStats(i);
//         var firstLadder = roundStats!.Ladders!["1"];
//         foreach (var ranker in firstLadder.Rankers!)
//         {
//             if (!allRankers.ContainsKey(ranker.AccountId))
//             {
//                 var player = new PlayerStats() { AccountId = ranker.AccountId, AHPoints = ranker.AssholePoints, UserName = ranker.UserName };
//                 allRankers[ranker.AccountId] = player;
//             }
//             var roundApp = new RoundAppearance()
//                 {
//                     Ladders = [],
//                     RoundTypes = roundStats.RoundTypes,
//                     BasePointsToPromote = roundStats.BasePointsToPromote,
//                     CreatedOn = roundStats.CreatedOn,
//                     ClosedOn = roundStats.ClosedOn,
//                     Number = roundStats.Number
//                 };
//             allRankers[ranker.AccountId].RoundAppearances!.Add(roundApp);
//             foreach (var keyVal in roundStats.Ladders)
//             {
//                 var ladderNum = keyVal.Key;
//                 var ladder = keyVal.Value;
//                 var ladderRanker = ladder.Rankers!.Find(r => r.AccountId == ranker.AccountId);
//                 var ladderApp = new LadderAppearance()
//                     {
//                         CreatedOn = ladder.CreatedOn,
//                         BasePointsToPromote = ladder.BasePointsToPromote,
//                         LadderTypes = ladder.LadderTypes,
//                         Rank = ladderRanker!.Rank,
//                         Points = ladderRanker.Points,
//                         Power = ladderRanker.Power,
//                         Multi = ladderRanker.Multi,
//                         Bias = ladderRanker.Bias,
//                         Grapes = ladderRanker.Grapes,
//                         Vinegar = ladderRanker.Vinegar,
//                         AutoPromote = ladderRanker.AutoPromote,
//                         Growing = ladderRanker.Growing
//                     };
//                 roundApp.Ladders[ladderNum] = ladderApp;
//                 if (ladderRanker.Growing) break;
//             }

//         }
//         Console.WriteLine(i);
//     }
//     var tunaStats = allRankers[36780];
//     foreach (var appearance in tunaStats.RoundAppearances)
//     {
//         Console.WriteLine(appearance.Number);
//         Console.WriteLine(appearance.Ladders!.Values.Last().Vinegar);
//     }
//     var jsonString = JsonSerializer.Serialize(allRankers);
//     Console.WriteLine("Serialized!");
//     var fileName = "allRankers.json";
//     var filePath = Environment.CurrentDirectory + $"\\Data\\{fileName}";
//     File.WriteAllText(filePath, jsonString);
// }

//UpdateNewRound();
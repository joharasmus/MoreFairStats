// See https://aka.ms/new-console-template for more information
using Microsoft.Azure.Cosmos;
using MoreFairStats;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using MoreFairStatsManagement;


var programConfig = new ConfigurationBuilder().AddUserSecrets<Program>().Build();  //This gets the local secrets

var dbConnStr = programConfig["mfsCosmosDbConnStr"]!;
var moreFairData = new MoreFairData(dbConnStr);

var mfsConfig = moreFairData.Config;
var currentMaxRound = mfsConfig.CurrentMaxRound;
Console.WriteLine(currentMaxRound);

void updateWithNewRoundStats(int round)
{
    using var client = new HttpClient();
    using var apiResp = client.Get($"https://fair.kaliburg.de/api/stats/round/raw?number={round}");
    var bodyString = apiResp.Content.ReadAsString();
    var roundData = bodyString.Deserialize<APIRoundStats>();
    var dbRoundStats = new RoundStats()
    {
        BasePointsToPromote = roundData.BasePointsToPromote,
        ClosedOn = roundData.ClosedOn,
        CreatedOn = roundData.CreatedOn,
        Number = roundData.Number,
        NumberOfLadders = roundData.Ladders!.Count,
        RoundTypes = roundData.RoundTypes,
        id = roundData.Number.ToString()
    };
    moreFairData.Upsert(dbRoundStats);
    foreach (var ladder in roundData.Ladders)
    {
        var ladderStats = ladder.Value;
        ladderStats.Round = round;
        ladderStats.Ladder = int.Parse(ladder.Key);
        ladderStats.id = $"R{round}L{ladder.Key}";
        moreFairData.Upsert(ladderStats);
    }
}

void UpdatePlayersWithNewNames()
{
    var firstLadder = moreFairData.GetLadder(currentMaxRound + 1, 1);
    var firstLadderRankers = firstLadder.Rankers!;
    var queryString = $"SELECT * FROM c WHERE c.Round != {currentMaxRound + 1}";
    var queryDef = new QueryDefinition(queryString);
    using var queryIter = moreFairData.GetLadderQueryIterator(queryDef);
    while (queryIter.HasMoreResults)
    {
        var result = queryIter.ReadNext();
        foreach (var ladder in result)
        {
            Console.WriteLine(ladder.id);
            var ladderRankers = ladder.Rankers!;
            foreach (var ranker in ladderRankers)
            {
                var matchRanker = firstLadderRankers.Find(r => r.AccountId == ranker.AccountId);
                if (matchRanker != null)
                {
                    ranker.UserName = matchRanker.UserName;
                    ranker.AssholePoints = matchRanker.AssholePoints;
                }
            }
            moreFairData.Upsert(ladder);
        }
    }
}

void UpdateNewRound()
{
    updateWithNewRoundStats(currentMaxRound + 1);
    UpdatePlayersWithNewNames();
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
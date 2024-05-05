// See https://aka.ms/new-console-template for more information
using Microsoft.Azure.Cosmos;
using MoreFairStats;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using MoreFairStatsManagement;


var firstConfig = new ConfigurationBuilder().AddUserSecrets<Program>().Build();  //This gets the local secrets
var finalConfig = new ConfigurationBuilder().AddUserSecrets<Program>().AddAzureAppConfiguration(firstConfig["mfsAppConfigConnStr"]).Build(); // This gets app config values from azure

var dbConnStr = finalConfig["mfsCosmosDbConnStr"];
var cosmosClient = new CosmosClient(dbConnStr);
var cosmosDB = cosmosClient.GetDatabase("mfs-cosmosdb");
var currentMaxLadder = int.Parse(finalConfig["currentMaxRound"]!);

LadderStats parseLadderStats(int round, int ladder)
{
    var container = cosmosDB.GetContainer("mfs-ladders");
    var asyncResp = container.ReadItemAsync<LadderStats>($"R{round}L{ladder}", new PartitionKey(round));
    asyncResp.Wait();
    var respItem = asyncResp.Result;
    return respItem.Resource;
}

void getRoundStats(int round)
{
    var client = new HttpClient();
    var asyncApiResp = client.GetAsync($"https://fair.kaliburg.de/api/stats/round/raw?number={round}");
    asyncApiResp.Wait();
    var response = asyncApiResp.Result;
    var asyncBodyString = response.Content.ReadAsStringAsync();
    asyncBodyString.Wait();
    var bodyString = asyncBodyString.Result;
    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    var roundData = JsonSerializer.Deserialize<APIRoundStats>(bodyString, options);
    var dbRoundStats = new RoundStats()
    {
        BasePointsToPromote = roundData!.BasePointsToPromote,
        ClosedOn = roundData.ClosedOn,
        CreatedOn = roundData.CreatedOn,
        Number = roundData.Number,
        NumberOfLadders = roundData.Ladders!.Count,
        RoundTypes = roundData.RoundTypes,
        id = roundData.Number.ToString()
    };
    var mfsLadders = cosmosDB.GetContainer("mfs-ladders");
    var mfsRounds = cosmosDB.GetContainer("mfs-rounds");
    var partKey = new PartitionKey(round);
    var asyncCreatedItem = mfsRounds.UpsertItemAsync(dbRoundStats, partKey);
    asyncCreatedItem.Wait();
    var createdItem = asyncCreatedItem.Result;
    Console.WriteLine(createdItem.StatusCode);
    Console.WriteLine(createdItem.Resource.id);
    var dataDirectory = Environment.CurrentDirectory + "\\Data";
    foreach (var ladder in roundData.Ladders)
    {
        var ladderStats = ladder.Value;
        ladderStats.Round = round;
        ladderStats.Ladder = int.Parse(ladder.Key);
        ladderStats.id = $"R{round}L{ladder.Key}";
        var asyncUpsertedLadder = mfsLadders.UpsertItemAsync(ladderStats, partKey);  // partkey from round can be reused, since partkey has the round as its value for both ladders and rounds
        asyncUpsertedLadder.Wait();
        var upsertedLadder = asyncUpsertedLadder.Result;
        Console.WriteLine(upsertedLadder.StatusCode);
        Console.WriteLine(upsertedLadder.Resource.id);
    }
}

void UpdatePlayersWithNewNames()
{
    var firstLadder = parseLadderStats(currentMaxLadder + 1, 1);
    var firstLadderRankers = firstLadder.Rankers;
    Console.WriteLine(firstLadderRankers!.Count);
    var queryString = "SELECT * FROM c WHERE c.Round != 260";
    var queryDef = new QueryDefinition(queryString);
    var mfsLadders = cosmosDB.GetContainer("mfs-ladders");
    using var queryIter = mfsLadders.GetItemQueryIterator<LadderStats>(queryDef);
    while (queryIter.HasMoreResults)
    {
        var response = queryIter.ReadNextAsync(); // Everything in the first result
        response.Wait();
        var result = response.Result;
        foreach (var ladder in result)
        {
            Console.WriteLine(ladder.id);
            var ladderRankers = ladder.Rankers;
            foreach (var ranker in ladderRankers!)
            {
                var matchRanker = firstLadderRankers.Find(r => r.AccountId == ranker.AccountId);
                if (matchRanker != null)
                {
                    ranker.UserName = matchRanker.UserName;
                    ranker.AssholePoints = matchRanker.AssholePoints;
                }
            }
            var partKey = new PartitionKey(ladder.Round);
            var asyncUpsertedLadder = mfsLadders.UpsertItemAsync(ladder, partKey);
            asyncUpsertedLadder.Wait();
            var upsertedLadder = asyncUpsertedLadder.Result;
            Console.WriteLine(upsertedLadder.StatusCode);
            Console.WriteLine(upsertedLadder.Resource.id);
        }
    }
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

//getRoundStats(currentMaxLadder + 1);
//UpdatePlayersWithNewNames();
//invertRoundsToPlayers()
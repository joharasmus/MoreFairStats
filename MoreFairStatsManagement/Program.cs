// See https://aka.ms/new-console-template for more information
using Microsoft.Azure.Cosmos;
using MoreFairStats;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using MoreFairStatsManagement;

Console.WriteLine("Hello, World!");

var firstConfig = new ConfigurationBuilder().AddUserSecrets<Program>().Build();  //This gets the local secrets
var finalConfig = new ConfigurationBuilder().AddUserSecrets<Program>().AddAzureAppConfiguration(firstConfig["mfsAppConfigConnStr"]).Build(); // This gets app config values from azure

var dbConnStr = finalConfig["mfsCosmosDbConnStr"];
Console.WriteLine(dbConnStr);
var cosmosClient = new CosmosClient(dbConnStr);
var cosmosDB = cosmosClient.GetDatabase("mfs-cosmosdb");
var currentMaxLadder = int.Parse(finalConfig["currentMaxRound"]!);

async void UploadLaddersToAzure()
{
    var mfsLadders = cosmosDB.GetContainer("mfs-ladders");
    var laddersDir = Environment.CurrentDirectory + "\\Data\\Ladders";
    var dirInfo = new DirectoryInfo(laddersDir);
    foreach (var file in dirInfo.GetFiles())
    {
        var ladderjson = File.ReadAllText(file.FullName);
        var ladderStats = JsonSerializer.Deserialize<LadderStats>(ladderjson);
        var partitionKey = new PartitionKey(ladderStats!.Round);
        var createdItem = await mfsLadders.CreateItemAsync(ladderStats, partitionKey);
        Console.WriteLine(createdItem.StatusCode);
        Console.WriteLine(createdItem.Resource.id);
    }
}

async void UploadRoundsToAzure()
{
    var mfsLadders = cosmosDB.GetContainer("mfs-rounds");
    var roundsDir = Environment.CurrentDirectory + "\\Data\\Rounds";
    var dirInfo = new DirectoryInfo(roundsDir);
    foreach (var file in dirInfo.GetFiles())
    {
        var roundJson = File.ReadAllText(file.FullName);
        var roundStats = JsonSerializer.Deserialize<MoreFairStats.RoundStats>(roundJson);
        var partitionKey = new PartitionKey(roundStats!.Number);
        var createdItem = await mfsLadders.CreateItemAsync(roundStats, partitionKey);
        Console.WriteLine(createdItem.StatusCode);
        //Console.WriteLine(createdItem.Resource.id);
    }
}

async Task<LadderStats> parseLadderStats(int round, int ladder)
{
    var container = cosmosDB.GetContainer("mfs-ladders");
    var respItem = await container.ReadItemAsync<LadderStats>($"R{round}L{ladder}", new PartitionKey(round));
    return respItem.Resource;
}

async Task getRoundStats(int round)
{
    var client = new HttpClient();
    var response = await client.GetAsync($"https://fair.kaliburg.de/api/stats/round/raw?number={round}");
    var bodyString = await response.Content.ReadAsStringAsync();
    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    var roundData = JsonSerializer.Deserialize<APIRoundStats>(bodyString, options);
    var newRoundStats = new RoundStats()
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
    var createdItem = await mfsRounds.CreateItemAsync(newRoundStats, partKey);
    Console.WriteLine(createdItem.StatusCode);
    Console.WriteLine(createdItem.Resource.id);
    var dataDirectory = Environment.CurrentDirectory + "\\Data";
    foreach (var ladder in roundData.Ladders)
    {
        var ladderStats = ladder.Value;
        ladderStats.Round = round;
        ladderStats.Ladder = int.Parse(ladder.Key);
        ladderStats.id = $"R{round}L{ladder.Key}";
        var ladderStatsString = JsonSerializer.Serialize(ladderStats);
        var ladderStatsPath = dataDirectory + $"\\Ladders\\R{round}L{ladder.Key}.json";
        File.WriteAllText(ladderStatsPath, ladderStatsString);
    }
}

async void UpdatePlayersWithNewNames()
{
    var firstLadder = await parseLadderStats(currentMaxLadder, 1);
    var firstLadderRankers = firstLadder.Rankers;
    var dirInfo = new DirectoryInfo("C:\\Users\\admin\\source\\repos\\MoreFairStats\\MoreFairStats\\Data\\Ladders\\");
    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    foreach (var ladderFile in dirInfo.GetFiles())
    {
        var ladderFilePath = ladderFile.FullName;
        Console.WriteLine(ladderFilePath);
        var ladderJson = File.ReadAllText(ladderFilePath);
        var ladder = JsonSerializer.Deserialize<LadderStats>(ladderJson, options);
        foreach (var ranker in ladder!.Rankers!)
        {
            var matchRanker = firstLadderRankers!.Find(r => r.AccountId == ranker.AccountId);
            if (matchRanker == null) continue;
            ranker.UserName = matchRanker.UserName;
            ranker.AssholePoints = matchRanker.AssholePoints;
        }
        var jsonString = JsonSerializer.Serialize(ladder);
        File.WriteAllText(ladderFilePath, jsonString);
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

//await getRoundStats(currentMaxLadder);
//UpdatePlayersWithNewNames();
//invertRoundsToPlayers()
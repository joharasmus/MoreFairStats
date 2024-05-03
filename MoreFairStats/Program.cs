using Microsoft.Azure.Cosmos;
using MoreFairStats;
using MoreFairStats.Components;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddServerSideBlazor().AddCircuitOptions(options => { options.DetailedErrors = true; });

var connString = builder.Configuration["cosmosDbConnstring"];
var cosmosClient = new CosmosClient(connString);
var cosmosDB = cosmosClient.GetDatabase("mfs-cosmosdb");

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
        var roundStats = JsonSerializer.Deserialize<NewRoundStats>(roundJson);
        var partitionKey = new PartitionKey(roundStats!.Number);
        var createdItem = await mfsLadders.CreateItemAsync(roundStats, partitionKey);
        Console.WriteLine(createdItem.StatusCode);
        //Console.WriteLine(createdItem.Resource.id);
    }
}

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

//UploadLaddersToAzure();
//UploadRoundsToAzure();

builder.Services.AddSingleton(cosmosClient);
builder.Services.AddSingleton(cosmosDB);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

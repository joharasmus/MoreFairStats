using Microsoft.Azure.Cosmos;
using MoreFairStats.Components;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddServerSideBlazor().AddCircuitOptions(options => { options.DetailedErrors = true; });

var cosmosDbConnString = builder.Configuration["cosmosDbConnstring"];
var cosmosClient = new CosmosClient(cosmosDbConnString);
var cosmosDB = cosmosClient.GetDatabase("mfs-cosmosdb");

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

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

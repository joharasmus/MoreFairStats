using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using MoreFairStats;
using MoreFairStats.Components;

var builder = WebApplication.CreateBuilder(args);

var mfsCosmosDbConnStr = builder.Configuration["mfsCosmosDbConnStr"]!;
var moreFairData = new MoreFairData(mfsCosmosDbConnStr);
builder.Services.AddSingleton(moreFairData);

builder.Services.AddRazorComponents();

var app = builder.Build();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>();

app.Run();

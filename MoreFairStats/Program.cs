using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MoreFairStats;
using MoreFairStats.Components;

var builder = WebApplication.CreateBuilder(args);

var mfsCosmosDbConnStr = builder.Configuration["mfsCosmosDbConnStr"]!;
var moreFairData = new MoreFairData(mfsCosmosDbConnStr);
builder.Services.AddSingleton(moreFairData);

// Add services to the container.
builder.Services.AddRazorComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>();

app.Run();

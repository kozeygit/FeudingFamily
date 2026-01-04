using System.Data;
using System.Diagnostics;
using System.Net;
using FeudingFamily.Components;
using FeudingFamily.EspBuzzer;
using FeudingFamily.Hubs;
using FeudingFamily.JoinGame;
using FeudingFamily.Logic;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Data.Sqlite;

var builder = WebApplication.CreateBuilder(args);

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
if (builder.Environment.IsDevelopment())
{
    builder.Logging.SetMinimumLevel(LogLevel.Debug);
}

// Add services to the container.
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddSignalR();

builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        ["application/octet-stream"]);
});

builder.Services.AddTransient<IDbConnection>(provider =>
{
    // Configure connection string or inject it here
    var configuration = provider.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("DefaultConnection");
    return new SqliteConnection(connectionString);
});

if (args.Contains("--use-esp-buzzers")) builder.Services.AddSingleton<IEspBuzzer, EspBuzzer>();

builder.Services.AddSingleton<IQuestionService, QuestionService>();
builder.Services.AddSingleton<IGameManager, GameManager>();


//-------------------------------------------------------------------------------\\


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();

    app.UseResponseCompression();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();


app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.MapHub<GameHub>("/gamehub");


// Question Endpoint
app.MapGet("/questions2", async (IQuestionService questionService) =>
{
    var questions = await questionService.GetQuestionsAsync();

    return Results.Ok(questions.Select(q => q.MapToDto().Content));
});

app.MapJoinGameEndpoints();

// build db

// var connection = app.Services.GetRequiredService<IDbConnection>();

// DatabaseBuilder dbBuilder = new(connection);
// await dbBuilder.CreateTableAsync(CreateTableSql.Questions);
// await dbBuilder.CreateTableAsync(CreateTableSql.Answers);
// await dbBuilder.PopulateTablesAsync("Data/dbo/JsonQuestions/ff_questions.json");

var ip = Dns.GetHostEntry(Dns.GetHostName()).AddressList[0];
var url = $"http://{ip}";
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Application started. Open browser at {Url}", url);
Process.Start("explorer", url);

app.Run();

using FeudingFamily.Components;
using FeudingFamily.Hubs;
using FeudingFamily.Logic;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.Sqlite;
using System.Data;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddSingleton<IQuestionService, QuestionService>();
builder.Services.AddSingleton<IGameManager, GameManager>();


//-------------------------------------------------------------------------------\\


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
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
    IQuestionService _questionService = questionService;

    var questions = await _questionService.GetQuestionsAsync();

    return Results.Ok(questions.Select(q => q.MapToDto().Content));

});

app.MapGet("/buzz", () =>
{
    return Results.Ok();
});

// just for my local esp buzzers
app.MapPost("/buzz", async (IGameManager gameManager, IHubContext<GameHub> hubContext, HttpRequest request) =>
{
    var stream = new StreamReader(request.Body);
    var body = await stream.ReadToEndAsync();
    string[] vars = body.Split("&");
    var gameKey = vars[0].Split("=")[1];
    var teamName = vars[1].Split("=")[1];

    Console.WriteLine($"Received buzz from esp, game key: {gameKey}, team name: {teamName}");

    var joinResult = gameManager.GetGame(gameKey);

    if (joinResult.Success is false)
    {
        return Results.Ok(false);
    }

    var game = joinResult.Game!;

    if (game.HasTeam(teamName) is false)
    {
        return Results.Ok(false);
    }

    var team = game.GetTeam(teamName)!;

    if (game.Buzz(team) is false)
    {
        return Results.Ok(false);
    };
        
    var presenterConnections = gameManager.GetPresenterConnections(gameKey).Select(c => c.ConnectionId);
    var controllerConnections = gameManager.GetControllerConnections(gameKey).Select(c => c.ConnectionId);
    var teamConnections = gameManager.GetBuzzerConnections(gameKey, team).Select(c => c.ConnectionId);

    var connections = presenterConnections.Concat(controllerConnections).Concat(teamConnections);


    await hubContext.Clients.Clients(presenterConnections).SendAsync("receivePlaySound", "buzz-in");
    
    await hubContext.Clients.Clients(connections).SendAsync("receiveRound", game.CurrentRound.MapToDto());
    await hubContext.Clients.Clients(connections).SendAsync("receiveTeams", game.Teams.Select(t => t.MapToDto()));

    var teamPlaying = game.TeamPlaying?.MapToDto();
    await hubContext.Clients.Clients(connections).SendAsync("receiveTeamPlaying", team);

    await hubContext.Clients.Clients(connections).SendAsync("receiveBuzz", team.MapToDto());

    return Results.Ok(true);
});

// FUCK FUCK FUCK FUCK, I CANT SEND THE SIGNAL BACK TO OTHER CLIENTS WHEN I BUZZ NOW!!!  nvm i figured it out lol.
// FUCK, NOW I NEED TO ADD THE TEAMS BEFORE I CONNECT THE ESPs!!!
// DOUBLE FUCK, NOW I NEED TO STOP THE TEAMS FROM DISPOSING AUTOMATICALLY UGGGGGHHHH!!!

app.MapGet("/form", (IGameManager gameManager, string gameKey, string teamName, string page) =>
{
    if (string.IsNullOrWhiteSpace(gameKey))
    {
        return Results.Redirect($"/?ErrorCode={(int)JoinErrorCode.KeyEmpty}");
    }

    if (page == "Join")
    {
        if (string.IsNullOrWhiteSpace(teamName))
        {
            return Results.Redirect($"/?ErrorCode={(int)JoinErrorCode.TeamNameEmpty}");
        }
    }

    JoinGameResult joinResult = gameManager.GameKeyValidator(gameKey);

    if (!joinResult.Success)
    {
        return Results.Redirect($"/?ErrorCode={(int)joinResult.ErrorCode!}");
    }

    joinResult = gameManager.GetGame(gameKey);

    if (!joinResult.Success)
    {
        joinResult = gameManager.NewGame(gameKey);

        if (!joinResult.Success)
        {
            return Results.Redirect($"/?ErrorCode={(int)joinResult.ErrorCode!}");
        }
    }

    if (joinResult.Game is null)
    {
        return default;
    }

    switch (page)
    {
        case "Join":

            if (teamName.Contains(' ') || teamName.Length > 10)
            {
                return Results.Redirect($"/?ErrorCode={(int)JoinErrorCode.TeamNameInvalid}");
            }

            if (joinResult.Game.Teams.Select(s => s.Name).Contains(teamName) is false)
            {
                if (joinResult.Game.Teams.Count >= 2)
                {
                    return Results.Redirect($"/?ErrorCode={(int)JoinErrorCode.GameHasTwoTeams}");
                }
            }

            Console.WriteLine("Join");
            return Results.Redirect($"/Buzzer/{gameKey}?TeamName={teamName}");

        case "Presenter":
            Console.WriteLine("Presenter");
            return Results.Redirect($"/Presenter/{gameKey}");

        case "Controller":
            Console.WriteLine("Controller");
            return Results.Redirect($"/Controller/{gameKey}");

        default:
            return Results.Redirect("/");
    }

});

// build db

// var connection = app.Services.GetRequiredService<IDbConnection>();

// DatabaseBuilder dbBuilder = new(connection);
// await dbBuilder.CreateTableAsync(CreateTableSql.Questions);
// await dbBuilder.CreateTableAsync(CreateTableSql.Answers);
// await dbBuilder.PopulateTablesAsync("Data/dbo/JsonQuestions/ff_questions.json");

app.Run();

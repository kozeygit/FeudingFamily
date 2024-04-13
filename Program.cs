using FeudingFamily.Components;
using FeudingFamily.Logic;
using FeudingFamily.Hubs;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Data.Sqlite;
using System.Data;

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




app.MapGet("/form", (IGameManager GameManager, string gameKey, string teamName, string page) =>
{
    Console.WriteLine(gameKey);
    Console.WriteLine(teamName);
    Console.WriteLine(page);

    JoinGameResult joinResult = GameManager.GameKeyValidator(gameKey);
    
    if (!joinResult.Success)
    {
        return Results.Redirect($"/?ErrorCode={(int)joinResult.ErrorCode!}");
    }

    joinResult = GameManager.GetGame(gameKey);

    if (!joinResult.Success)
    {
        joinResult = GameManager.NewGame(gameKey);

        if (!joinResult.Success)
        {
            return Results.Redirect($"/?ErrorCode={(int)joinResult.ErrorCode!}");
        }
    }

    switch(page)
    {
        case "Join":
            Console.WriteLine("Join");
            if (string.IsNullOrWhiteSpace(teamName))
            {
                return Results.Redirect($"/?ErrorCode={(int)JoinErrorCode.TeamNameEmpty}");
            }

            if (!joinResult.Game.HasTeam(teamName))
            {
                if (!joinResult.Game.AddTeam(teamName))
                {
                    //! Change to better redirect. error code is wrong
                    return Results.Redirect($"/?ErrorCode={(int)JoinErrorCode.GameHasTwoTeams}");
                }

            }
            
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
// await dbBuilder.PopulateTablesAsync("dbo/JsonQuestions/ff_questions.json");

app.Run();

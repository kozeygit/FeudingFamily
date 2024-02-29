using Microsoft.AspNetCore.ResponseCompression;
using FeudingFamily.Components;
using FeudingFamily.Hubs;
using Dapper;
using FeudingFamily.Models;
using Microsoft.Data.Sqlite;
using FeudingFamily.dbo.Tables;
using FeudingFamily.Data;
using System.Data;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.VisualBasic;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

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


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseResponseCompression();
app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapBlazorHub();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.MapHub<GameHub>("/gameHub");



// Question Endpoint
app.MapGet("/questions", async (IDbConnection connection) =>
{
    IDbConnection _connection = connection;

    const string sql = "SELECT * FROM Questions";

    var questions = await _connection.QueryAsync<Question>(sql);

    Console.WriteLine(questions);

    return Results.Ok(questions);

});

// build db

var connection = app.Services.GetRequiredService<IDbConnection>();

DatabaseBuilder dbBuilder = new(connection);
await dbBuilder.CreateTableAsync(CreateTableSql.Questions);
await dbBuilder.CreateTableAsync(CreateTableSql.Answers);
// await dbBuilder.PopulateTablesAsync("dbo/JsonQuestions/ff_questions.json");

app.Run();

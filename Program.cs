using Microsoft.AspNetCore.ResponseCompression;
using BlazorServer.Components;
using BlazorServer.Hubs;
using Dapper;
using BlazorServer.Models;
using Microsoft.Data.Sqlite;
using BlazorServer.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddResponseCompression(opts =>
{
   opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
         ["application/octet-stream"]);
});

builder.Services.AddSingleton(serviceProvider =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();

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

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

}

app.UseResponseCompression();
app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapBlazorHub();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
app.MapHub<GameHub>("/gameHub");



// Question Endpoint
app.MapGet("questions", async (IConfiguration configuration) =>
{
    var connectionString = configuration.GetConnectionString("DefaultConnection");
    
    using var connection = new SqliteConnection(connectionString);

    const string sql = "SELECT * FROM Questions";

    var questions = await connection.QueryAsync<Question>(sql);

    return Results.Ok(questions);

})
.WithName("GetQuestions");

//build db
DatabaseBuilder.CreateQuestionsTable();
DatabaseBuilder.CreateAnswersTable();

DatabaseBuilder.Test();

// app.Run();

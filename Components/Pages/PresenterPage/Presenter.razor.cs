
using Microsoft.AspNetCore.Components;
using FeudingFamily.Logic;
using FeudingFamily.Models;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;


namespace FeudingFamily;

public class PresenterPageBase : ComponentBase
{
    [Inject]
    IJSRuntime JS { get; set; }

    [Inject]
    NavigationManager Navigation { get; set; }

    [Inject]
    IGameManager GameManager { get; set; }

    [Parameter]
    public string? GameKey { get; set; }

    public Game? Game { get; set; }

    public Question? DefaultQuestion { get; set; }

    protected override void OnParametersSet()
    {

        DefaultQuestion = new Question
        {
            Content = "Default Question",
            Answers = {
                new Answer { Ranking = 1, Content = "Answer1", Points = 100},
                new Answer { Ranking = 2, Content = "Answer2", Points = 80},
                new Answer { Ranking = 3, Content = "Answer3", Points = 60},
                new Answer { Ranking = 4, Content = "Answer4", Points = 40},
                new Answer { Ranking = 5, Content = "Answer5", Points = 20},
            }
        };

        if (GameKey is not null)
            Game = GameManager.GetGame(GameKey).Game;
    }

    public bool IsBuzzerModalShown { get; set; }
    public string BuzzingTeam { get; set; } = string.Empty;

    public async Task ShowBuzzerModalAsync(string teamName)
    {
        try
        {
            BuzzingTeam = teamName;
            IsBuzzerModalShown = true;
            await InvokeAsync(StateHasChanged);

            // non blocking wait for 2 seconds
            await Task.Delay(2000);

            IsBuzzerModalShown = false;

            await InvokeAsync(StateHasChanged);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error has occurred: {ex}");
        }
    }

    private HubConnection? hubConnection;

    protected override async Task OnInitializedAsync()
    {
        hubConnection = new HubConnectionBuilder()
            .WithUrl(Navigation.ToAbsoluteUri("/chathub"))
            .Build();

        hubConnection.On<string>("ReceiveBuzz", async (teamName) =>
        {
            await ShowBuzzerModalAsync(teamName);
            await InvokeAsync(StateHasChanged);
        });

        await hubConnection.StartAsync();
    }

    public bool IsConnected =>
        hubConnection?.State == HubConnectionState.Connected;

    public async ValueTask DisposeAsync()
    {
        if (hubConnection is not null)
        {
            await hubConnection.DisposeAsync();
        }
    }

}
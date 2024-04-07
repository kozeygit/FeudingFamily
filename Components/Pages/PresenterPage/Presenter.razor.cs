
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

    protected override void OnInitialized()
    {
        DefaultQuestion = QuestionService.GetDefaultQuestion();
    }

    protected override void OnParametersSet()
    {
        if (GameKey is not null)
            Game = GameManager.GetGame(GameKey).Game;
    }

    public bool IsBuzzerModalShown { get; set; }
    public bool IsWrongAnswerModalShown { get; set; }
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
            .WithUrl(Navigation.ToAbsoluteUri("/gamehub"))
            .WithAutomaticReconnect()
            .Build();

        hubConnection.On<string>("ReceiveBuzz", async (teamName) =>
        {
            Console.WriteLine("Presenter Send Buzz", teamName);
            await ShowBuzzerModalAsync(teamName);
        });

        await hubConnection.StartAsync();

        await hubConnection.SendAsync("AddToPresenterGroup");
        await hubConnection.SendAsync("AddToGameGroup", GameKey);
    }

    public bool IsConnected =>
        hubConnection?.State == HubConnectionState.Connected;

    public async ValueTask DisposeAsync()
    {
        if (hubConnection is not null)
        {
            await hubConnection.SendAsync("RemoveFromGroups", GameKey);
            await hubConnection.DisposeAsync();
        }
    }

}
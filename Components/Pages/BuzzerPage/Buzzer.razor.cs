
using Microsoft.AspNetCore.Components;
using FeudingFamily.Logic;
using Microsoft.AspNetCore.SignalR.Client;


namespace FeudingFamily;

public class BuzzerPageBase : ComponentBase
{
    [Inject]
    NavigationManager Navigation { get; set; }

    [Inject]
    IGameManager GameManager { get; set; }

    [Parameter]
    public string GameKey { get; set; }

    [SupplyParameterFromQuery]
    public string TeamName { get; set; }

    public Game? Game { get; set; }
    public Team? Team { get; set; }

    public bool IsModalShown { get; set; }
    

    private HubConnection? hubConnection;

    protected override void OnParametersSet()
    {
        Game = GameManager.GetGame(GameKey).Game;
        Team = Game?.GetTeam(TeamName);
        if (Team is null)
        {
            Navigation.NavigateTo("/");
        }
    }

    public bool IsConnected =>
        hubConnection?.State == HubConnectionState.Connected;

    protected override async Task OnInitializedAsync()
    {
        hubConnection = new HubConnectionBuilder()
            .WithUrl(Navigation.ToAbsoluteUri("/gamehub"))
            .Build();

        await hubConnection.StartAsync();
    }

    protected async Task SendBuzz()
    {
        if (hubConnection is not null)
        {
            await hubConnection.SendAsync("SendBuzz", TeamName);
        }

        IsModalShown = true;

        // non blocking wait for 2 seconds
        await Task.Delay(2000).ContinueWith(_ => IsModalShown = false);

        Console.WriteLine("Buzzer Send Buzz");
    }

    public async ValueTask DisposeAsync()
    {
        if (hubConnection is not null)
        {
            await hubConnection.DisposeAsync();
        }
    }

}
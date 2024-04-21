using Microsoft.AspNetCore.Components;
using FeudingFamily.Logic;
using Microsoft.AspNetCore.SignalR.Client;


namespace FeudingFamily.Components;

public class BuzzerPageBase : ComponentBase, IDisposable
{
    [Inject]
    NavigationManager Navigation { get; set; }

    [Parameter]
    public string GameKey { get; set; } = string.Empty;

    [SupplyParameterFromQuery]
    public string TeamName { get; set; } = string.Empty;

    
    public TeamDto? Team { get; set; } = new TeamDto();
    
    public bool IsGameConnected { get; set; }
    public bool IsModalShown { get; set; }
    protected HubConnection? hubConnection;


    protected override async Task OnInitializedAsync()
    {
        if (string.IsNullOrWhiteSpace(GameKey))
        {
            Navigation.NavigateTo($"/?ErrorCode={(int)JoinErrorCode.KeyEmpty}");
        }

        if (string.IsNullOrWhiteSpace(TeamName))
        {
            Navigation.NavigateTo($"/?ErrorCode={(int)JoinErrorCode.TeamNameEmpty}");
        }

        hubConnection = new HubConnectionBuilder()
            .WithUrl(Navigation.ToAbsoluteUri("/gamehub"))
            .Build();

        hubConnection.On<TeamDto>("receiveTeam", async (team) =>
        {
            Console.WriteLine($"Received team");
            Team = team;
            await InvokeAsync(StateHasChanged);
        });

        hubConnection.On<string>("receiveBuzz", async (teamName) =>
        {
            IsModalShown = true;
            await InvokeAsync(StateHasChanged);
            await Task.Delay(2000);
            IsModalShown = false;
            await InvokeAsync(StateHasChanged);
            
        });

        hubConnection.On<bool>("receiveGameConnected", async (isConnected) =>
        {
            if (isConnected is false)
            {
                Navigation.NavigateTo($"/?ErrorCode={(int)JoinErrorCode.GameNotFound}");
            }

            await hubConnection.SendAsync("SendGetTeam", GameKey);
            IsGameConnected = isConnected;
            await InvokeAsync(StateHasChanged);
        });

        await hubConnection.StartAsync();
        await hubConnection.SendAsync("SendJoinGame", GameKey, ConnectionType.Buzzer, TeamName);


    }

    public bool IsHubConnected =>
        hubConnection?.State == HubConnectionState.Connected;

    protected async Task SendBuzz()
    {
        if (hubConnection is not null)
        {
            Console.WriteLine($"--BuzzerPage-- SendBuzz - TeamName: {TeamName}");
            await hubConnection.SendAsync("SendBuzz", GameKey);
        }
    }

    protected async Task NewRound()
    {
        if (hubConnection is not null)
        {
            await hubConnection.SendAsync("SendNewRound", GameKey);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (hubConnection is not null)
        {
            await hubConnection.DisposeAsync();
        }
    }

    void IDisposable.Dispose()
    {
        hubConnection.SendAsync("LeaveGame", GameKey);
        Console.WriteLine("Disposing");
    }
}
using FeudingFamily.Logic;
using FeudingFamily.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

namespace FeudingFamily.Components.Pages.BuzzerPage;

public class BuzzerPageBase : ComponentBase, IAsyncDisposable
{
    protected HubConnection? hubConnection;

    [Inject] private NavigationManager Navigation { get; set; } = default!;

    [Parameter] public string GameKey { get; set; } = string.Empty;

    [SupplyParameterFromQuery(Name = "TeamID")]
    public string TeamGuid { get; set; } = string.Empty;

    public Guid TeamID { get; set; }


    public TeamDto? Team { get; set; } = new();

    public bool IsGameConnected { get; set; }
    public bool IsModalShown { get; set; }

    public bool IsHubConnected =>
        hubConnection?.State == HubConnectionState.Connected;


    public async ValueTask DisposeAsync()
    {
        if (hubConnection is not null)
        {
            // await hubConnection.SendAsync("SendLeaveGame", GameKey);
            Console.WriteLine("Disposing");
            await hubConnection.DisposeAsync();
        }
        else
        {
            Console.WriteLine("Error disposing; hubConnection is null");
        }

        GC.SuppressFinalize(this);
    }


    protected override async Task OnInitializedAsync()
    {
        if (string.IsNullOrWhiteSpace(GameKey)) Navigation.NavigateTo($"/?ErrorCode={(int)JoinErrorCode.KeyEmpty}");

        if (string.IsNullOrWhiteSpace(TeamGuid))
        {
            Console.WriteLine("\n\n !! 1 !!\n\n");
            Navigation.NavigateTo($"/?ErrorCode={(int)JoinErrorCode.TeamNameEmpty}");
        }

        TeamID = Guid.Parse(TeamGuid);

        hubConnection = new HubConnectionBuilder()
            .WithUrl(Navigation.ToAbsoluteUri("/gamehub"))
            .Build();

        hubConnection.On<TeamDto>("receiveTeam", async team =>
        {
            Team = team;
            await InvokeAsync(StateHasChanged);
        });

        hubConnection.On<TeamDto>("receiveBuzz", async team =>
        {
            Team = team;

            IsModalShown = true;
            await InvokeAsync(StateHasChanged);
            await Task.Delay(2000);
            IsModalShown = false;
            await InvokeAsync(StateHasChanged);
        });

        hubConnection.On<bool>("receiveGameConnected", async isConnected =>
        {
            if (isConnected is false) Navigation.NavigateTo("/");

            await hubConnection.SendAsync("SendGetTeam", GameKey);
            IsGameConnected = isConnected;
            await InvokeAsync(StateHasChanged);
        });

        await hubConnection.StartAsync();
        await hubConnection.SendAsync("SendJoinGame", GameKey, ConnectionType.Buzzer, TeamID);
    }

    protected async Task SendBuzz()
    {
        if (hubConnection is not null) await hubConnection.SendAsync("SendBuzz", GameKey);
    }
}
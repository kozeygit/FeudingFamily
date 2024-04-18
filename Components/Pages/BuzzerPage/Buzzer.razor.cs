using Microsoft.AspNetCore.Components;
using FeudingFamily.Logic;
using Microsoft.AspNetCore.SignalR.Client;
using System.Text.Json;


namespace FeudingFamily;

public struct BuzzerModel
{
    public string? GameKey { get; set; }
    public string? TeamName { get; set; }
    public int Points { get; set; }
}

public class BuzzerPageBase : ComponentBase
{
    [Inject]
    NavigationManager Navigation { get; set; }

    [Parameter]
    public string GameKey { get; set; }

    [SupplyParameterFromQuery]
    public string TeamName { get; set; }

    public bool IsGameConnected { get; set; }

    public bool IsModalShown { get; set; }
    
    protected HubConnection? hubConnection;


    protected override async Task OnInitializedAsync()
    {
        if (GameKey is null)
        {
            Navigation.NavigateTo($"/ErrorCode={(int)JoinErrorCode.KeyEmpty}");
        }

        if (TeamName is null)
        {
            Navigation.NavigateTo($"/ErrorCode={(int)JoinErrorCode.TeamNameEmpty}");
        }

        hubConnection = new HubConnectionBuilder()
            .WithUrl(Navigation.ToAbsoluteUri("/gamehub"))
            .Build();

        hubConnection.On<bool>("ReceiveGameConnected", (isConnected) =>
        {
            IsGameConnected = isConnected;
            InvokeAsync(StateHasChanged);
        });

        await hubConnection.StartAsync();
        
        await hubConnection.SendAsync("JoinGame", GameKey, ConnectionType.Buzzer, TeamName);


    }

    public bool IsHubConnected =>
        hubConnection?.State == HubConnectionState.Connected;

    protected async Task SendBuzz()
    {
        if (hubConnection is not null)
        {
            await hubConnection.SendAsync("SendBuzz", TeamName);
        }

        IsModalShown = true;
        // non blocking wait for 2 seconds
        await Task.Delay(2000);

        IsModalShown = false;

        Console.WriteLine($"--BuzzerPage-- SendBuzz - TeamName: {TeamName}");
    }

    public async ValueTask DisposeAsync()
    {
        if (hubConnection is not null)
        {
            await hubConnection.DisposeAsync();
        }
    }

}
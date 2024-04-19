using Microsoft.AspNetCore.Components;
using FeudingFamily.Logic;
using FeudingFamily.Models;
using Microsoft.AspNetCore.SignalR.Client;

namespace FeudingFamily.Components;

public class ControllerPageBase : ComponentBase
{
    [Inject]
    NavigationManager Navigation { get; set; }

    [Parameter]
    public string? GameKey { get; set; }

    public RoundDto Round { get; set; }
    public QuestionDto Question { get; set; }
    public List<TeamDto> Teams { get; set; }

    protected bool IsBuzzerModalShown { get; set; }
    protected string BuzzingTeam { get; set; } = string.Empty;
    protected bool IsWrongModalShown { get; set; }

    public bool IsGameConnected { get; set; }
    protected HubConnection? hubConnection;
    protected override async Task OnInitializedAsync()
    {
        Question = QuestionService.GetDefaultQuestion().MapToDto();

        if (GameKey is null)
        {
            Navigation.NavigateTo($"/ErrorCode={(int)JoinErrorCode.KeyEmpty}");
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

        await hubConnection.SendAsync("SendJoinGame", GameKey, ConnectionType.Controller, null);
    }

    public bool IsHubConnected =>
        hubConnection?.State == HubConnectionState.Connected;

    public async ValueTask DisposeAsync()
    {
        if (hubConnection is not null)
        {
            await hubConnection.DisposeAsync();
        }
    }

}
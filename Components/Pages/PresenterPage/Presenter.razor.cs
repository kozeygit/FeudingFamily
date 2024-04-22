using Microsoft.AspNetCore.Components;
using FeudingFamily.Logic;
using FeudingFamily.Models;
using Microsoft.AspNetCore.SignalR.Client;

namespace FeudingFamily.Components;


public class PresenterPageBase : ComponentBase
{
    [Inject]
    NavigationManager Navigation { get; set; }

    [Parameter]
    public string? GameKey { get; set; }

    public RoundDto Round { get; set; } = new RoundDto();
    public QuestionDto Question { get; set; } = new QuestionDto();
    public List<TeamDto> Teams { get; set; } = [new TeamDto(), new TeamDto()];

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
            Navigation.NavigateTo($"/?ErrorCode={(int)JoinErrorCode.KeyEmpty}");
        }

        hubConnection = new HubConnectionBuilder()
            .WithUrl(Navigation.ToAbsoluteUri("/gamehub"))
            .Build();

        hubConnection.On<string>("receiveBuzz", ShowBuzzerModal);

        hubConnection.On("receiveWrong", ShowWrongModal);

        hubConnection.On<QuestionDto>("receiveQuestion", async (question) =>
        {
            Console.WriteLine($"Received question");
            Console.WriteLine(question);
            Question = question;
            await InvokeAsync(StateHasChanged);
        });

        hubConnection.On<RoundDto>("receiveRound", async (round) =>
        {
            Console.WriteLine($"Received round");
            Round = round;
            await InvokeAsync(StateHasChanged);
        });

        hubConnection.On<List<TeamDto>>("receiveTeams", async (teams) =>
        {
            Console.WriteLine($"Received teams");
            Teams = teams;
            await InvokeAsync(StateHasChanged);
        });

        hubConnection.On<bool>("receiveGameConnected", async (isConnected) =>
        {
            Console.WriteLine($"Game connected: {isConnected}");

            if (isConnected is false)
            {
                Navigation.NavigateTo($"/?ErrorCode={(int)JoinErrorCode.GameNotFound}");
            }
            
            IsGameConnected = isConnected;

            await hubConnection.SendAsync("SendGetQuestion", GameKey);
            await hubConnection.SendAsync("SendGetRound", GameKey);
            await hubConnection.SendAsync("SendGetTeams", GameKey);

            await InvokeAsync(StateHasChanged);
        });

        await hubConnection.StartAsync();

        await hubConnection.SendAsync("SendJoinGame", GameKey, ConnectionType.Presenter, null);
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

    public async Task RevealQuestion()
    {
        if (hubConnection is not null)
        {
            await hubConnection.SendAsync("SendRevealQuestion", GameKey);
        }
    }

    public async Task ShowBuzzerModal(string teamName)
    {
        try
        {
            BuzzingTeam = teamName;

            IsBuzzerModalShown = true;
            await InvokeAsync(StateHasChanged);

            await Task.Delay(2000);
            IsBuzzerModalShown = false;
            await InvokeAsync(StateHasChanged);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error has occurred: {ex}");
        }
    }
    public async Task ShowWrongModal()
    {
        try
        {
            IsWrongModalShown = true;

            await InvokeAsync(StateHasChanged);

            // non blocking wait for 2 seconds
            await Task.Delay(2000);

            IsWrongModalShown = false;

            await InvokeAsync(StateHasChanged);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error has occurred: {ex}");
        }
    }


}
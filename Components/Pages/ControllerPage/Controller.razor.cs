using FeudingFamily.Logic;
using FeudingFamily.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

namespace FeudingFamily.Components;

public class ControllerPageBase : ComponentBase, IAsyncDisposable
{
    [Inject]
    NavigationManager Navigation { get; set; } = default!;

    [Parameter]
    public string? GameKey { get; set; }

    public RoundDto Round { get; set; } = new RoundDto();
    public QuestionDto Question { get; set; } = new QuestionDto();
    public List<TeamDto> Teams { get; set; } = [new TeamDto(), new TeamDto()];
    public Dictionary<TeamDto, bool> IsTeamPlaying = [];

    public bool IsQuestionPickerOpen { get; set; } = false;
    public int NextQuestionId { get; set; }

    public bool IsGameConnected { get; set; }
    protected HubConnection? hubConnection;
    protected override async Task OnInitializedAsync()
    {
        Question = QuestionService.GetDefaultQuestion().MapToDto();

        foreach (var team in Teams)
        {
            IsTeamPlaying.TryAdd(team, false);
        }

        if (GameKey is null)
        {
            Navigation.NavigateTo($"/?ErrorCode={(int)JoinErrorCode.KeyEmpty}");
        }

        hubConnection = new HubConnectionBuilder()
            .WithUrl(Navigation.ToAbsoluteUri("/gamehub"))
            .Build();

        // hubConnection.On<TeamDto>("receiveBuzz", BuzzIn);

        hubConnection.On<QuestionDto>("receiveQuestion", async (question) =>
        {
            Question = question;
            await InvokeAsync(StateHasChanged);
        });

        hubConnection.On<RoundDto>("receiveRound", async (round) =>
        {
            Round = round;
            await InvokeAsync(StateHasChanged);
        });

        hubConnection.On<List<TeamDto>>("receiveTeams", async (teams) =>
        {
            Teams = teams;
            foreach (var team in Teams)
            {
                IsTeamPlaying.TryAdd(team, false);
            }

            await InvokeAsync(StateHasChanged);
        });

        hubConnection.On<TeamDto?>("receiveTeamPlaying", async (teamPlaying) =>
        {
            foreach (var team in IsTeamPlaying.Keys)
            {
                IsTeamPlaying[team] = false;
            }

            if (teamPlaying is not null)
            {
                IsTeamPlaying[teamPlaying] = true;
            }

            await InvokeAsync(StateHasChanged);

        });

        hubConnection.On<bool>("receiveGameConnected", async (isConnected) =>
        {
            Console.WriteLine($"Game connected: {isConnected}");

            if (isConnected is false)
            {
                Navigation.NavigateTo($"/");
            }

            IsGameConnected = isConnected;

            await hubConnection.SendAsync("SendGetQuestion", GameKey);
            await hubConnection.SendAsync("SendGetRound", GameKey);
            await hubConnection.SendAsync("SendGetTeams", GameKey);

            await InvokeAsync(StateHasChanged);
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
            await hubConnection.SendAsync("SendLeaveGame", GameKey);
            Console.WriteLine("Disposing");
            await hubConnection.DisposeAsync();
        }
        else
        {
            Console.WriteLine("Error disposing; hubConnection is null");
        }

        GC.SuppressFinalize(this);
    }

    public async Task EnableBuzzers()
    {
        if (hubConnection is not null)
        {
            await hubConnection.SendAsync("SendEnableBuzzers", GameKey);
        }
    }

    public async Task DisableBuzzers()
    {
        if (hubConnection is not null)
        {
            await hubConnection.SendAsync("SendDisableBuzzers", GameKey);
        }
    }

    public void ShowQuestionPicker()
    {
        IsQuestionPickerOpen = true;
    }

    public async Task SetQuestion(int id)
    {
        Console.WriteLine($"Set question to this: {id}");
        
        NextQuestionId = id;
        if (hubConnection is not null)
        {
            await hubConnection.SendAsync("SendSetQuestion", GameKey, id);
        }
    }

    public async Task SwapTeamPlaying()
    {
        if (hubConnection is not null)
        {
            await hubConnection.SendAsync("SendSwapTeamPlaying", GameKey);
        }
    }

    public async Task RevealQuestion()
    {
        if (hubConnection is not null)
        {
            await hubConnection.SendAsync("SendRevealQuestion", GameKey);
        }
    }

    public async Task RevealAnswer(int answerRanking)
    {
        if (hubConnection is not null)
        {
            await hubConnection.SendAsync("SendRevealAnswer", GameKey, answerRanking);
        }
    }

    public async Task WrongAnswer(bool onlyShow = false)
    {
        if (hubConnection is not null)
        {
            await hubConnection.SendAsync("SendWrongAnswer", GameKey, onlyShow);
        }
    }

    public async Task NewRound()
    {
        if (hubConnection is not null)
        {
            await hubConnection.SendAsync("SendNewRound", GameKey);
        }
    }

    public async Task PlaySound(string soundName)
    {
        Console.Write("Client: ");
        Console.WriteLine(DateTime.UtcNow.ToLocalTime());

        if (hubConnection is not null)
        {
            await hubConnection.SendAsync("SendPlaySound", GameKey, soundName);
        }
    }
    
}
using System.Net;
using FeudingFamily.Logic;
using FeudingFamily.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

namespace FeudingFamily.Components.Pages.PresenterPage;

public class PresenterPageBase : ComponentBase, IAsyncDisposable
{
    protected HubConnection? hubConnection;
    public Dictionary<TeamDto, bool> IsTeamPlaying = [];

    protected PresenterAudio? presenterAudio;

    [Inject] private NavigationManager Navigation { get; set; } = default!;

    [Parameter] public string? GameKey { get; set; }

    public RoundDto Round { get; set; } = new();
    public QuestionDto Question { get; set; } = new();
    public List<TeamDto> Teams { get; set; } = [new(), new()];

    protected bool IsBuzzerModalShown { get; set; }
    protected string BuzzingTeam { get; set; } = string.Empty;
    protected bool IsWrongModalShown { get; set; }

    public bool IsGameConnected { get; set; }
    public string IP { get; set; } = string.Empty;

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

    protected override async Task OnInitializedAsync()
    {
        var ip = Dns.GetHostEntry(Dns.GetHostName()).AddressList[0];
        var url = $"http://{ip}";

        IP = url;

        Question = QuestionService.GetDefaultQuestion().MapToDto();

        foreach (var team in Teams) IsTeamPlaying.TryAdd(team, false);

        if (GameKey is null) Navigation.NavigateTo($"/?ErrorCode={(int)JoinErrorCode.KeyEmpty}");

        hubConnection = new HubConnectionBuilder()
            .WithUrl(Navigation.ToAbsoluteUri("/gamehub"))
            .Build();

        hubConnection.On<TeamDto>("receiveBuzz", ShowBuzzerModal);

        hubConnection.On("receiveWrong", ShowWrongModal);

        hubConnection.On<string>("receivePlaySound", PlaySound);

        hubConnection.On<QuestionDto>("receiveQuestion", async question =>
        {
            Question = question;
            await InvokeAsync(StateHasChanged);
        });

        hubConnection.On<RoundDto>("receiveRound", async round =>
        {
            Round = round;
            await InvokeAsync(StateHasChanged);
        });

        hubConnection.On<List<TeamDto>>("receiveTeams", async teams =>
        {
            Teams = teams;
            foreach (var team in Teams) IsTeamPlaying.TryAdd(team, false);

            await InvokeAsync(StateHasChanged);
        });

        hubConnection.On<TeamDto?>("receiveTeamPlaying", async teamPlaying =>
        {
            foreach (var team in IsTeamPlaying.Keys) IsTeamPlaying[team] = false;

            if (teamPlaying is not null) IsTeamPlaying[teamPlaying] = true;

            await InvokeAsync(StateHasChanged);
        });

        hubConnection.On<bool>("receiveGameConnected", async isConnected =>
        {
            Console.WriteLine($"Game connected: {isConnected}");

            if (isConnected is false) Navigation.NavigateTo("/");

            IsGameConnected = isConnected;

            await hubConnection.SendAsync("SendGetRound", GameKey);
            await hubConnection.SendAsync("SendGetTeams", GameKey);
            await hubConnection.SendAsync("SendGetQuestion", GameKey);

            await InvokeAsync(StateHasChanged);
        });

        await hubConnection.StartAsync();

        await hubConnection.SendAsync("SendJoinGame", GameKey, ConnectionType.Presenter, null);
    }

    public async Task RevealQuestion()
    {
        if (hubConnection is not null) await hubConnection.SendAsync("SendRevealQuestion", GameKey);
    }

    public async Task RevealAnswer(int answerRanking)
    {
        if (hubConnection is not null) await hubConnection.SendAsync("SendRevealAnswer", GameKey, answerRanking);
    }

    public async Task ShowBuzzerModal(TeamDto team)
    {
        if (Teams.Contains(team) is false) return;

        if (team is not null) BuzzingTeam = team.Name;

        IsBuzzerModalShown = true;
        await InvokeAsync(StateHasChanged);

        await Task.Delay(2000).ConfigureAwait(false);
        IsBuzzerModalShown = false;
        await InvokeAsync(StateHasChanged);
    }

    public async Task ShowWrongModal()
    {
        IsWrongModalShown = true;
        await InvokeAsync(StateHasChanged);

        await Task.Delay(2000).ConfigureAwait(false);
        IsWrongModalShown = false;
        await InvokeAsync(StateHasChanged);
    }

    public async Task PlaySound(string soundName)
    {
        var overdub = false;

        if (soundName is "correct-answer" or "wrong-answer") overdub = true;

        if (presenterAudio is not null) await presenterAudio.PlaySound(soundName, overdub);
    }
}
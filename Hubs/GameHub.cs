using FeudingFamily.Logic;
using Microsoft.AspNetCore.SignalR;

namespace FeudingFamily.Hubs;

public class GameHub : Hub
{
    private readonly IGameManager _gameManager;

    public GameHub(IGameManager gameManager)
    {
        _gameManager = gameManager;
        // gameManager.TeamBuzzed += OnBuzzHandler;
        // The reason it keeps crashing is that I think
        // I cannot store this on the hub because the hubs are transient and are created for each call??
        // so I cant do the line above, instead I need to move it to a different class and use the
        // hub context to call the method I want.
        // https://stackoverflow.com/questions/55795669/cannot-access-a-disposed-object-crash-in-signalr 
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var gameKey = _gameManager.GetGameKey(Context.ConnectionId);

        _gameManager.LeaveGame(gameKey, Context.ConnectionId);

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, gameKey);

        await base.OnDisconnectedAsync(exception);
    }


    // TODO: Move the logic for joining the game into the form endpoint, only send a simple join game, and receive the detail via signalR, this is so i can use the team id in the url query instead of the name when joining...
    public async Task SendJoinGame(string gameKey, ConnectionType connectionType, Guid? teamId)
    {
        JoinGameResult joinGame;

        Console.WriteLine(
            $"--Hub-- JoinGame - gameKey: {gameKey}, connectionType: {connectionType}, caller: {Context.ConnectionId}");

        joinGame = await _gameManager.JoinGame(gameKey, Context.ConnectionId, connectionType, teamId);

        if (joinGame.Success is false)
        {
            Console.WriteLine(joinGame.ErrorCode);
            await Clients.Caller.SendAsync("receiveGameConnected", false);
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, gameKey);
        await Clients.Caller.SendAsync("receiveGameConnected", true);

        if (teamId is not null)
        {
            var teams = joinGame.Game!.Teams;
            var teamsDto = teams.Select(t => t.MapToDto());

            var connections = _gameManager.GetConnections(gameKey).Select(c => c.ConnectionId);

            Console.WriteLine($"--Hub-- JoinTeam - gameKey: {gameKey}, team: {teamId}, sender: {Context.ConnectionId}");

            if (connectionType == ConnectionType.Buzzer)
            {
                var team = joinGame.Game.GetTeam((Guid)teamId) ?? throw new Exception("Team does not exist???");
                await Clients.Caller.SendAsync("receiveTeam", team.MapToDto());
            }

            await Clients.Clients(connections).SendAsync("receiveTeams", teamsDto);

            await SendTeamPlaying(gameKey);
        }
    }


    public async Task SendLeaveGame(string gameKey)
    {
        _gameManager.LeaveGame(gameKey, Context.ConnectionId);

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, gameKey);
    }

    public async Task SendGetQuestion(string gameKey)
    {
        var (game, connection) = _gameManager.ValidateGameConnection(gameKey, Context.ConnectionId);

        if (game is null || connection is null) return;

        var question = game.CurrentQuestion.MapToDto();

        await Clients.Caller.SendAsync("receiveQuestion", question);
    }

    public async Task SendSwapTeamPlaying(string gameKey)
    {
        var (game, connection) = _gameManager.ValidateGameConnection(gameKey, Context.ConnectionId);

        if (game is null || connection is null) return;

        game.SwapTeamPlaying();

        var team = game.TeamPlaying?.MapToDto();

        var presenterConnections = _gameManager.GetPresenterConnections(gameKey).Select(c => c.ConnectionId);
        var controllerConnections = _gameManager.GetControllerConnections(gameKey).Select(c => c.ConnectionId);
        var connections = presenterConnections.Concat(controllerConnections);

        await Clients.Clients(connections).SendAsync("receiveTeamPlaying", team);
    }

    public async Task SendTeamPlaying(string gameKey)
    {
        var (game, connection) = _gameManager.ValidateGameConnection(gameKey, Context.ConnectionId);

        if (game is null || connection is null) return;

        var team = game.TeamPlaying?.MapToDto();

        var presenterConnections = _gameManager.GetPresenterConnections(gameKey).Select(c => c.ConnectionId);
        var controllerConnections = _gameManager.GetControllerConnections(gameKey).Select(c => c.ConnectionId);
        var connections = presenterConnections.Concat(controllerConnections);

        await Clients.Clients(connections).SendAsync("receiveTeamPlaying", team);
    }

    public async Task SendGetTeams(string gameKey)
    {
        var (game, connection) = _gameManager.ValidateGameConnection(gameKey, Context.ConnectionId);

        if (game is null || connection is null) return;

        var teams = game.Teams.Select(t => t.MapToDto()).ToList();

        await Clients.Caller.SendAsync("receiveTeams", teams);
        await SendTeamPlaying(gameKey);
    }

    public async Task SendGetRound(string gameKey)
    {
        var (game, connection) = _gameManager.ValidateGameConnection(gameKey, Context.ConnectionId);

        if (game is null || connection is null) return;

        var round = game.CurrentRound.MapToDto();

        await Clients.Caller.SendAsync("receiveRound", round);
    }

    public async Task SendNewRound(string gameKey)
    {
        var (game, connection) = _gameManager.ValidateGameConnection(gameKey, Context.ConnectionId);

        if (game is null || connection is null) return;

        await game.NewRoundAsync();

        var connections = _gameManager.GetConnections(gameKey).Select(c => c.ConnectionId).ToList();

        await Clients.Clients(connections).SendAsync("receiveRound", game.CurrentRound.MapToDto());
        await Clients.Clients(connections).SendAsync("receiveQuestion", game.CurrentQuestion.MapToDto());
        await Clients.Clients(connections).SendAsync("receiveTeams", game.Teams.Select(t => t.MapToDto()));
        await SendTeamPlaying(gameKey);

        foreach (var team in game.Teams)
        {
            var teamConnections = _gameManager.GetBuzzerConnections(gameKey, team).Select(c => c.ConnectionId);
            await Clients.Clients(teamConnections).SendAsync("receiveTeam", team.MapToDto());
        }
    }

    public async Task SendBuzz(string gameKey)
    {
        var (game, connection) = _gameManager.ValidateGameConnection(gameKey, Context.ConnectionId);

        if (game is null || connection is null) return;

        var team = game.GetTeam(connection);

        if (team is null) return;

        game.Buzz(team);
    }

    public async Task SendEnableBuzzers(string gameKey)
    {
        var (game, connection) = _gameManager.ValidateGameConnection(gameKey, Context.ConnectionId);

        if (game is null || connection is null) return;

        game.CurrentRound.IsBuzzersEnabled = true;

        var connections = _gameManager.GetConnections(gameKey).Select(c => c.ConnectionId);

        await Clients.Clients(connections).SendAsync("receiveRound", game.CurrentRound.MapToDto());
    }

    public async Task SendDisableBuzzers(string gameKey)
    {
        var (game, connection) = _gameManager.ValidateGameConnection(gameKey, Context.ConnectionId);

        if (game is null || connection is null) return;

        game.CurrentRound.IsBuzzersEnabled = false;

        var connections = _gameManager.GetConnections(gameKey).Select(c => c.ConnectionId);

        await Clients.Clients(connections).SendAsync("receiveRound", game.CurrentRound.MapToDto());
    }

    public async Task SendRevealQuestion(string gameKey)
    {
        var (game, connection) = _gameManager.ValidateGameConnection(gameKey, Context.ConnectionId);

        if (game is null || connection is null) return;

        var round = game.CurrentRound;

        var playSound = false;

        if (round.IsQuestionRevealed == false)
        {
            round.IsQuestionRevealed = true;
            playSound = true;
        }

        var presenterConnections = _gameManager.GetPresenterConnections(gameKey).Select(c => c.ConnectionId).ToList();
        var controllerConnections = _gameManager.GetControllerConnections(gameKey).Select(c => c.ConnectionId).ToList();
        var connections = presenterConnections.Concat(controllerConnections).ToList();

        Console.WriteLine($"--Hub-- SendRevealQuestion - gameKey: {gameKey}, sender: {Context.ConnectionId}");

        if (playSound) await Clients.Clients(presenterConnections).SendAsync("receivePlaySound", "reveal-question");

        await Clients.Clients(connections).SendAsync("receiveRound", round.MapToDto());
    }

    public async Task SendRevealAnswer(string gameKey, int answerRanking)
    {
        var (game, connection) = _gameManager.ValidateGameConnection(gameKey, Context.ConnectionId);

        if (game is null || connection is null) return;

        var playSound = game.GiveCorrectAnswer(answerRanking);

        var round = game.CurrentRound;

        var presenterConnections = _gameManager.GetPresenterConnections(gameKey).Select(c => c.ConnectionId).ToList();
        var controllerConnections = _gameManager.GetControllerConnections(gameKey).Select(c => c.ConnectionId).ToList();
        var connections = presenterConnections.Concat(controllerConnections).ToList();

        Console.WriteLine(
            $"--Hub-- SendRevealAnswer - gameKey: {gameKey}, answer: {answerRanking}, sender: {Context.ConnectionId}");

        if (playSound)
        {
            if (answerRanking == 1)
                await Clients.Clients(presenterConnections).SendAsync("receivePlaySound", "top-answer");
            else
                await Clients.Clients(presenterConnections).SendAsync("receivePlaySound", "correct-answer");
        }

        await Clients.Clients(connections).SendAsync("receiveRound", round.MapToDto());

        await Clients.Clients(connections).SendAsync("receiveTeams", game.Teams.Select(t => t.MapToDto()));
        await SendTeamPlaying(gameKey);
    }

    public async Task SendWrongAnswer(string gameKey, bool onlyShow)
    {
        var (game, connection) = _gameManager.ValidateGameConnection(gameKey, Context.ConnectionId);

        if (game is null || connection is null) return;

        if (onlyShow is false) game.GiveIncorrectAnswer();

        var round = game.CurrentRound;

        var presenterConnections = _gameManager.GetPresenterConnections(gameKey).Select(c => c.ConnectionId).ToList();
        var controllerConnections = _gameManager.GetControllerConnections(gameKey).Select(c => c.ConnectionId).ToList();
        var connections = presenterConnections.Concat(controllerConnections).ToList();

        Console.WriteLine($"--Hub-- SendWrongAnswer - gameKey: {gameKey}, sender: {Context.ConnectionId}");

        await Clients.Clients(presenterConnections).SendAsync("receivePlaySound", "wrong-answer");

        await Clients.Clients(connections).SendAsync("receiveRound", round.MapToDto());
        await Clients.Clients(presenterConnections).SendAsync("receiveWrong");

        await Clients.Clients(connections).SendAsync("receiveTeams", game.Teams.Select(t => t.MapToDto()));
        await SendTeamPlaying(gameKey);
    }

    public async Task SendPlaySound(string gameKey, string soundName)
    {
        var (game, connection) = _gameManager.ValidateGameConnection(gameKey, Context.ConnectionId);

        if (game is null || connection is null) return;

        var presenterConnections = _gameManager.GetPresenterConnections(gameKey).Select(c => c.ConnectionId);

        Console.Write("Hub: ");
        Console.WriteLine(DateTime.UtcNow.ToLocalTime());

        Console.WriteLine(
            $"--Hub-- SendPlaySound - gameKey: {gameKey}, soundName: {soundName}, sender: {Context.ConnectionId}");

        await Clients.Clients(presenterConnections).SendAsync("receivePlaySound", soundName);
    }

    public async Task SendSetQuestion(string gameKey, int questionId)
    {
        var (game, connection) = _gameManager.ValidateGameConnection(gameKey, Context.ConnectionId);

        if (game is null || connection is null) return;

        await game.SetQuestionAsync(questionId);
        await SendNewRound(gameKey);
    }

    public async Task SendEditTeamName(string gameKey, string oldTeamName, string newTeamName)
    {
        var (game, connection) = _gameManager.ValidateGameConnection(gameKey, Context.ConnectionId);

        if (game is null || connection is null) return;

        var changedTeam = game.GetTeam(oldTeamName) ?? throw new Exception("Team not found");

        if (game.EditTeamName(oldTeamName, newTeamName))
            changedTeam = game.GetTeam(newTeamName) ?? throw new Exception("new team name didnt work");

        var teams = game.Teams.Select(t => t.MapToDto()).ToList();
        var changedTeamConnections =
            _gameManager.GetBuzzerConnections(gameKey, changedTeam).Select(c => c.ConnectionId);
        var presenterConnections = _gameManager.GetPresenterConnections(gameKey).Select(c => c.ConnectionId);
        var controllerConnections = _gameManager.GetControllerConnections(gameKey).Select(c => c.ConnectionId);
        var connections = presenterConnections.Concat(controllerConnections);

        Console.WriteLine(
            $"--Hub-- EditTeamName - gameKey: {gameKey}, team: {oldTeamName}, newTeamName: {newTeamName}, sender: {Context.ConnectionId}");

        await Clients.Clients(connections).SendAsync("receiveTeams", teams);
        await Clients.Clients(changedTeamConnections).SendAsync("receiveTeam", changedTeam.MapToDto());
    }
}
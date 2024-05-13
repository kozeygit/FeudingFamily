using FeudingFamily.Logic;
using Microsoft.AspNetCore.SignalR;

namespace FeudingFamily.Hubs;

public class GameHub : Hub
{
    private readonly IGameManager _gameManager;

    public GameHub(IGameManager gameManager)
    {
        _gameManager = gameManager;
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var gameKey = _gameManager.GetGameKey(Context.ConnectionId);

        _gameManager.LeaveGame(gameKey, Context.ConnectionId);

        if (gameKey is not null)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, gameKey);
        }
        
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendJoinGame(string gameKey, ConnectionType connectionType, string? teamName)
    {
        JoinGameResult joinGame;

        Console.WriteLine($"--Hub-- JoinGame - gameKey: {gameKey}, connectionType: {connectionType}, teamName: {teamName}, caller: {Context.ConnectionId}");

        if (teamName is null)
            joinGame = _gameManager.JoinGame(gameKey, Context.ConnectionId, connectionType);

        else
            joinGame = _gameManager.JoinGame(gameKey, Context.ConnectionId, teamName);

        if (joinGame.Success is false)
        {
            Console.WriteLine(joinGame.ErrorCode);
            await Clients.Caller.SendAsync("receiveGameConnected", false);
            return;
        }

        joinGame = _gameManager.GetGame(gameKey);

        if (joinGame.Success is false)
        {
            await Clients.Caller.SendAsync("receiveGameConnected", false);
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, gameKey);
        await Clients.Caller.SendAsync("receiveGameConnected", true);

        if (teamName is not null)
        {
            var teams = joinGame.Game!.Teams;
            var teamsDto = teams.Select(t => t.MapToDto());

            var presenterConnections = _gameManager.GetPresenterConnections(gameKey).Select(c => c.ConnectionId);
            var controllerConnections = _gameManager.GetControllerConnections(gameKey).Select(c => c.ConnectionId);
            var connections = presenterConnections.Concat(controllerConnections);

            Console.WriteLine($"--Hub-- SendWrongAnswer - gameKey: {gameKey}, sender: {Context.ConnectionId}");

            await Clients.Clients(connections).SendAsync("receiveTeams", teamsDto);
        }
    }

    public async Task SendLeaveGame(string gameKey)
    {
        _gameManager.LeaveGame(gameKey, Context.ConnectionId);

        if (gameKey is not null)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, gameKey);
        }
    }

    public async Task SendGetQuestion(string gameKey)
    {
        var (game, connection) = _gameManager.ValidateGameConnection(gameKey, Context.ConnectionId);

        if (game is null || connection is null)
        {
            return;
        }

        var question = game.CurrentQuestion.MapToDto();

        await Clients.Caller.SendAsync("receiveQuestion", question);
    }

    public async Task SendSwapTeamPlaying(string gameKey)
    {
        var (game, connection) = _gameManager.ValidateGameConnection(gameKey, Context.ConnectionId);

        if (game is null || connection is null)
        {
            return;
        }

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

        if (game is null || connection is null)
        {
            return;
        }

        var team = game.TeamPlaying?.MapToDto();

        var presenterConnections = _gameManager.GetPresenterConnections(gameKey).Select(c => c.ConnectionId);
        var controllerConnections = _gameManager.GetControllerConnections(gameKey).Select(c => c.ConnectionId);
        var connections = presenterConnections.Concat(controllerConnections);

        await Clients.Clients(connections).SendAsync("receiveTeamPlaying", team);
    }
    public async Task SendGetTeams(string gameKey)
    {
        var (game, connection) = _gameManager.ValidateGameConnection(gameKey, Context.ConnectionId);

        if (game is null || connection is null)
        {
            return;
        }

        var teams = game.Teams.Select(t => t.MapToDto()).ToList();

        await Clients.Caller.SendAsync("receiveTeams", teams);
        await SendTeamPlaying(gameKey);
    }

    public async Task SendGetRound(string gameKey)
    {
        var (game, connection) = _gameManager.ValidateGameConnection(gameKey, Context.ConnectionId);

        if (game is null || connection is null)
        {
            return;
        }

        var round = game.CurrentRound.MapToDto();

        await Clients.Caller.SendAsync("receiveRound", round);
    }

    public async Task SendNewRound(string gameKey)
    {

        var (game, connection) = _gameManager.ValidateGameConnection(gameKey, Context.ConnectionId);

        if (game is null || connection is null)
        {
            return;
        }

        await game.NewRound();

        var connections = _gameManager.GetConnections(gameKey).Select(c => c.ConnectionId);

        await Clients.Clients(connections).SendAsync("receiveRound", game.CurrentRound.MapToDto());
        await Clients.Clients(connections).SendAsync("receiveQuestion", game.CurrentQuestion.MapToDto());
        await Clients.Clients(connections).SendAsync("receiveTeams", game.Teams.Select(t => t.MapToDto()));
        await SendTeamPlaying(gameKey);

        foreach (var team in game.Teams)
        {
            var teamconnections = _gameManager.GetBuzzerConnections(gameKey, team).Select(c => c.ConnectionId);
            await Clients.Clients(teamconnections).SendAsync("receiveTeam", team.MapToDto());
        }
    }

    public async Task SendBuzz(string gameKey)
    {
        var (game, connection) = _gameManager.ValidateGameConnection(gameKey, Context.ConnectionId);

        if (game is null || connection is null)
        {
            return;
        }

        var team = game.GetTeam(connection);

        if (team is null)
        {
            return;
        }

        if (game.Buzz(team) is false)
        {
            return;
        };

        var presenterConnections = _gameManager.GetPresenterConnections(gameKey).Select(c => c.ConnectionId);
        var controllerConnections = _gameManager.GetControllerConnections(gameKey).Select(c => c.ConnectionId);
        var teamConnections = _gameManager.GetBuzzerConnections(gameKey, team).Select(c => c.ConnectionId);

        var connections = presenterConnections.Concat(controllerConnections).Concat(teamConnections);

        Console.WriteLine($"--Hub-- SendBuzz - teamName: {team.Name}, gameKey: {gameKey}, sender: {Context.ConnectionId}");

        await Clients.Clients(presenterConnections).SendAsync("receivePlaySound", "buzz-in");
        
        await Clients.Clients(connections).SendAsync("receiveRound", game.CurrentRound.MapToDto());
        await Clients.Clients(connections).SendAsync("receiveTeams", game.Teams.Select(t => t.MapToDto()));
        await SendTeamPlaying(gameKey);


        await Clients.Clients(connections).SendAsync("receiveBuzz", team.MapToDto());
    }

    public async Task SendEnableBuzzers(string gameKey)
    {
        var (game, connection) = _gameManager.ValidateGameConnection(gameKey, Context.ConnectionId);

        if (game is null || connection is null)
        {
            return;
        }

        game.CurrentRound.IsBuzzersEnabled = true;

        var connections = _gameManager.GetConnections(gameKey).Select(c => c.ConnectionId);

        await Clients.Clients(connections).SendAsync("receiveRound", game.CurrentRound.MapToDto());
    }

    public async Task SendDisableBuzzers(string gameKey)
    {
        var (game, connection) = _gameManager.ValidateGameConnection(gameKey, Context.ConnectionId);

        if (game is null || connection is null)
        {
            return;
        }

        game.CurrentRound.IsBuzzersEnabled = false;

        var connections = _gameManager.GetConnections(gameKey).Select(c => c.ConnectionId);

        await Clients.Clients(connections).SendAsync("receiveRound", game.CurrentRound.MapToDto());
    }

    public async Task SendRevealQuestion(string gameKey)
    {
        var (game, connection) = _gameManager.ValidateGameConnection(gameKey, Context.ConnectionId);

        if (game is null || connection is null)
        {
            return;
        }

        var round = game.CurrentRound;

        bool playSound = false;

        if (round.IsQuestionRevealed == false)
        {
            round.IsQuestionRevealed = true;
            playSound = true;
        }

        var presenterConnections = _gameManager.GetPresenterConnections(gameKey).Select(c => c.ConnectionId);
        var controllerConnections = _gameManager.GetControllerConnections(gameKey).Select(c => c.ConnectionId);
        var connections = presenterConnections.Concat(controllerConnections);

        Console.WriteLine($"--Hub-- SendRevealQuestion - gameKey: {gameKey}, sender: {Context.ConnectionId}");

        if (playSound)
        {
            await Clients.Clients(presenterConnections).SendAsync("receivePlaySound", "reveal-question");
        }

        await Clients.Clients(connections).SendAsync("receiveRound", round.MapToDto());
    }

    public async Task SendRevealAnswer(string gameKey, int answerRanking)
    {
        var (game, connection) = _gameManager.ValidateGameConnection(gameKey, Context.ConnectionId);

        if (game is null || connection is null)
        {
            return;
        }

        var playSound = game.GiveCorrectAnswer(answerRanking);

        var round = game.CurrentRound;

        var presenterConnections = _gameManager.GetPresenterConnections(gameKey).Select(c => c.ConnectionId);
        var controllerConnections = _gameManager.GetControllerConnections(gameKey).Select(c => c.ConnectionId);
        var connections = presenterConnections.Concat(controllerConnections);

        Console.WriteLine($"--Hub-- SendRevealAnswer - gameKey: {gameKey}, answer: {answerRanking}, sender: {Context.ConnectionId}");

        if (playSound)
        {
            if (answerRanking == 1)
            {
                await Clients.Clients(presenterConnections).SendAsync("receivePlaySound", "top-answer");
            }
            else
            {
                await Clients.Clients(presenterConnections).SendAsync("receivePlaySound", "correct-answer");
            }
        }

        await Clients.Clients(connections).SendAsync("receiveRound", round.MapToDto());

        await Clients.Clients(connections).SendAsync("receiveTeams", game.Teams.Select(t => t.MapToDto()));
        await SendTeamPlaying(gameKey);
    }

    public async Task SendWrongAnswer(string gameKey, bool onlyShow)
    {
        var (game, connection) = _gameManager.ValidateGameConnection(gameKey, Context.ConnectionId);

        if (game is null || connection is null)
        {
            return;
        }

        if (onlyShow is false)
        {
            game.GiveIncorrectAnswer();
        }

        var round = game.CurrentRound;

        var presenterConnections = _gameManager.GetPresenterConnections(gameKey).Select(c => c.ConnectionId);
        var controllerConnections = _gameManager.GetControllerConnections(gameKey).Select(c => c.ConnectionId);
        var connections = presenterConnections.Concat(controllerConnections);

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

        if (game is null || connection is null)
        {
            return;
        }

        var presenterConnections = _gameManager.GetPresenterConnections(gameKey).Select(c => c.ConnectionId);

        Console.Write("Hub: ");
        Console.WriteLine(DateTime.UtcNow.ToLocalTime());

        Console.WriteLine($"--Hub-- SendPlaySound - gameKey: {gameKey}, soundName: {soundName}, sender: {Context.ConnectionId}");

        await Clients.Clients(presenterConnections).SendAsync("receivePlaySound", soundName);
    }

    //!-----------------------not implemented yet-----------------------!\\

    public async Task SendShowWinner(Team winningTeam)
    {
        await Clients.Groups("Presenters", "Buzzers").SendAsync("receiveShowWinner", winningTeam);
    }

    public async Task SendStartTimer(string gameKey)
    {
        var (game, connection) = _gameManager.ValidateGameConnection(gameKey, Context.ConnectionId);

        if (game is null || connection is null)
        {
            return;
        }

        var presenterConnections = _gameManager.GetPresenterConnections(gameKey).Select(c => c.ConnectionId);

        await Clients.Clients(presenterConnections).SendAsync("receivePlaySound", "timer-long");
        await Clients.Clients(presenterConnections).SendAsync("receiveStartTimer");
    }
}

/*
Needed:

Controller:
TODO: send question (question) -> presenter, controller
TODO: send round over (winner of round)? -> presenter
    * maybe split into show winner and have the page reload for a new round instead
TODO: send countdown () -> presenter
*/


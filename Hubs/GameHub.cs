using System.Runtime.InteropServices.Marshalling;
using FeudingFamily.Logic;
using FeudingFamily.Models;
using Microsoft.AspNetCore.SignalR;

namespace FeudingFamily.Hubs;

public class GameHub : Hub
{
    private readonly IGameManager _gameManager;

    public GameHub(IGameManager gameManager)
    {
        _gameManager = gameManager;
    }

    public async Task RemoveFromGroups(string gameKey)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, gameKey);
        _gameManager.LeaveGame(gameKey, Context.ConnectionId);
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
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var gameKey = _gameManager.GetGameKey(Context.ConnectionId);
        
        if (string.IsNullOrEmpty(gameKey) is false)
            await SendLeaveGame(gameKey);
        
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendLeaveGame(string gameKey)
    {
        if (string.IsNullOrEmpty(gameKey) is true)
            return;

        if (_gameManager.GetGame(gameKey).Success is false)
            return;

        int connCount = _gameManager.GetConnections(gameKey).Count;
        Console.WriteLine($"Connections: {connCount}");
        
        _gameManager.LeaveGame(gameKey, Context.ConnectionId);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, gameKey);
    }

    public async Task SendBuzz(string gameKey)
    {
        var joinGameResult = _gameManager.GetGame(gameKey);

        if (joinGameResult.Success is false)
        {
            Console.WriteLine(joinGameResult.ErrorCode);
            return;
        }

        var connection = _gameManager.GetConnection(gameKey, Context.ConnectionId);

        if (_gameManager.HasConnection(gameKey, connection) is false)
        {
            Console.WriteLine(connection.ConnectionId);
            return;
        }

        var game = joinGameResult.Game;
        var team = game.GetTeam(connection);

        // Game buzz in logic if true do, if false dont yno

        var presenterConnections = _gameManager.GetPresenterConnections(gameKey).Select(c => c.ConnectionId);
        var controllerConnections = _gameManager.GetControllerConnections(gameKey).Select(c => c.ConnectionId);
        var teamConnections = team.Members.Select(c => c.ConnectionId);

        var conns = presenterConnections.Concat(controllerConnections).Concat(teamConnections);

        Console.WriteLine($"--Hub-- SendBuzz - teamName: {team.Name}, gameKey: {gameKey}, sender: {Context.ConnectionId}");
        
        await Clients.Clients(conns).SendAsync("receiveBuzz", team.Name);
    }

    public async Task SendGetQuestion(string gameKey)
    {
        var joinGameResult = _gameManager.GetGame(gameKey);

        if (joinGameResult.Success is false)
        {
            await Clients.Caller.SendAsync("receiveQuestion", null);
            return;
        }

        var connection = _gameManager.GetConnection(gameKey, Context.ConnectionId);

        if (_gameManager.HasConnection(gameKey, connection) is false)
        {
            await Clients.Caller.SendAsync("receiveQuestion", null);
            return;
        }

        var game = joinGameResult.Game;
        var question = game.CurrentQuestion.MapToDto();

        await Clients.Caller.SendAsync("receiveQuestion", question);
    }
    
    public async Task SendGetTeam(string gameKey)
    {
        var joinGameResult = _gameManager.GetGame(gameKey);

        if (joinGameResult.Success is false)
        {
            await Clients.Caller.SendAsync("receiveTeam", null);
            return;
        }

        var connection = _gameManager.GetConnection(gameKey, Context.ConnectionId);

        if (_gameManager.HasConnection(gameKey, connection) is false)
        {
            await Clients.Caller.SendAsync("receiveTeam", null);
            return;
        }

        var game = joinGameResult.Game;
        var team = game.GetTeam(connection);
        
        await Clients.Caller.SendAsync("receiveTeam", team);
    }
    
    public async Task SendGetTeams(string gameKey)
    {
        var joinGameResult = _gameManager.GetGame(gameKey);

        if (joinGameResult.Success is false)
        {
            await Clients.Caller.SendAsync("receiveTeams", null);
            return;
        }

        var connection = _gameManager.GetConnection(gameKey, Context.ConnectionId);

        if (_gameManager.HasConnection(gameKey, connection) is false)
        {
            await Clients.Caller.SendAsync("receiveTeams", null);
            return;
        }

        var game = joinGameResult.Game;
        var teams = game.Teams.Select(t => t.MapToDto()).ToList();
        
        await Clients.Caller.SendAsync("receiveTeams", teams);
    }
    
    public async Task SendGetRound(string gameKey)
    {
        var joinGameResult = _gameManager.GetGame(gameKey);

        if (joinGameResult.Success is false)
        {
            await Clients.Caller.SendAsync("receiveRound", null);
            return;
        }
        
        var connection = _gameManager.GetConnection(gameKey, Context.ConnectionId);

        if (_gameManager.HasConnection(gameKey, connection) is false)
        {
            await Clients.Caller.SendAsync("receiveRound", null);
            return;
        }

        var game = joinGameResult.Game;
        var round = game.CurrentRound.MapToDto();

        await Clients.Caller.SendAsync("receiveRound", round);
    }

    //!----------------------------------------------------------------------------------!\\


    public async Task SendNewRound(string gameKey) // Gets a new question and send to the controller for host to decide if to use
    {
        var joinGameResult = _gameManager.GetGame(gameKey);

        if (joinGameResult.Success is false)
        {
            await Clients.Caller.SendAsync("receiveError", joinGameResult.ErrorCode.ToString());
            return;
        }

        var connection = _gameManager.GetConnection(gameKey, Context.ConnectionId);

        var game = joinGameResult.Game;
        await game.NewRound();

        var conns = _gameManager.GetConnections(gameKey).Select(c => c.ConnectionId);

        await Clients.Clients(conns).SendAsync("receiveQuestion", game.CurrentQuestion.MapToDto());
        await Clients.Clients(conns).SendAsync("receiveRound", game.CurrentRound.MapToDto());

        foreach (var team in game.Teams)
        {
            var teamConns = team.Members.Select(c => c.ConnectionId);
            await Clients.Clients(teamConns).SendAsync("receiveTeam", team.MapToDto());
        }
    }

    public async Task SendRevealQuestion(string gameKey)
    {
        var joinGameResult = _gameManager.GetGame(gameKey);

        if (joinGameResult.Success is false)
        {
            await Clients.Caller.SendAsync("receiveError", joinGameResult.ErrorCode.ToString());
            return;
        }

        var connection = _gameManager.GetConnection(gameKey, Context.ConnectionId);

        var game = joinGameResult.Game;

        var round = game.CurrentRound;

        round.IsQuestionRevealed = true;

        var pConns = _gameManager.GetPresenterConnections(gameKey).Select(c => c.ConnectionId);
        var cConns = _gameManager.GetControllerConnections(gameKey).Select(c => c.ConnectionId);
        var conns = pConns.Concat(cConns);

        Console.WriteLine($"--Hub-- SendRevealQuestion - gameKey: {gameKey}, sender: {Context.ConnectionId}");

        await Clients.Clients(conns).SendAsync("receiveRound", round.MapToDto());
    }


    // * Just send the question model which includes a list of the answers... duh, when do i just need the answers ???
    // public async Task SendAnswers(List<Answer> answers) // Sends current answers to presenter and controller // * Will Probably move this to the view instead and reload the pages
    // {
    //     await Clients.Groups("Presenters", "Controllers").SendAsync("receiveAnswers", answers);
    // }


    public async Task SendRevealAnswer(int answerId)
    {
        await Clients.Group("Presenters").SendAsync("receiveRevealAnswer", answerId);
    }

    public async Task SendWrongAnswer(int wrongAnswersCount)
    {
        await Clients.Group("Presenters").SendAsync("receiveWrongAnswer", wrongAnswersCount);
    }

    public async Task SendShowWinner(Team winningTeam)
    {
        await Clients.Groups("Presenters", "Buzzers").SendAsync("receiveShowWinner", winningTeam);
    }

    public async Task SendPlaySound(string soundName)
    {
        await Clients.Group("Presenters").SendAsync("receivePlaySound", soundName);
    }

    public async Task SendCountdown()
    {
        await Clients.Group("Presenters").SendAsync("receiveCountdown");
    }
}

/*
Needed:

Controller:
TODO: new question -> controller
TODO: send question (question) -> presenter, controller
TODO: send answer (list of answers (answer + point)) -> presenter, controller
TODO: send reveal question () -> presenter
TODO: send reveal answer (index of ans) -> presenter
TODO: send wrong answer (how many wrong/how many Xs to show) -> presenter
TODO: send round over (winner of round)? -> presenter
    * maybe split into show winner and have the page reload for a new round instead
TODO: send countdown () -> presenter
TODO: send sound (sound name) -> presenter
! send close buzzer model () -> presenter
    * Probably will just have the buzzer modal close after a time span instead. Maybe add an indicator on the page to show the answering team.

Buzzer:
send buzzer call (team who buzzed) -> presenter
*/


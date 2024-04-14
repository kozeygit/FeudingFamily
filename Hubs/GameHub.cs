using System.Runtime.CompilerServices;
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

    public async Task JoinGame(string gameKey, ConnectionType connectionType)
    {
        _gameManager.JoinGame(gameKey, Context.ConnectionId, connectionType);
        await Groups.AddToGroupAsync(Context.ConnectionId, gameKey);
    }
    
    public async Task JoinGame(string gameKey, string TeamName)
    {
        _gameManager.JoinGame(gameKey, Context.ConnectionId, TeamName);
        await Groups.AddToGroupAsync(Context.ConnectionId, gameKey);
    }

    public async Task LeaveGame(string gameKey)
    {
        _gameManager.LeaveGame(gameKey, Context.ConnectionId);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, gameKey);
    }

    public async Task SendBuzz(string teamName)
    {
        var gameKey = _gameManager.GetGameKeyFromConnectionId(Context.ConnectionId);
        var pConns = _gameManager.GetPresenterConnections(gameKey);
        var cConns = _gameManager.GetControllerConnections(gameKey);

        List<string> conns = pConns.Concat(cConns).Select(c => c.ConnectionId).ToList() ?? [];

        Console.WriteLine("Hub Send Buzz");
        
        await Clients.Clients(conns).SendAsync("receiveBuzz", teamName);
    }

    //!----------------------------------------------------------------------------------!\\


    public async Task NewQuestion() // Gets a new question and send to the controller for host to decide if to use
    {
        await Clients.Caller.SendAsync("receiveNewQuestion");
    }

    public async Task SendQuestion(Question question) // Sends current question to presenter and controller // * Will Probably move this to the view instead and reload the pages
    {
        await Clients.Groups("Presenters", "Controllers").SendAsync("receiveQuestion", question);
    }

    // * Just send the question model which includes a list of the answers... duh, when do i just need the answers ???
    // public async Task SendAnswers(List<Answer> answers) // Sends current answers to presenter and controller // * Will Probably move this to the view instead and reload the pages
    // {
    //     await Clients.Groups("Presenters", "Controllers").SendAsync("receiveAnswers", answers);
    // }

    public async Task SendRevealQuestion()
    {
        await Clients.Group("Presenters").SendAsync("receiveRevealQuestion");
    }

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


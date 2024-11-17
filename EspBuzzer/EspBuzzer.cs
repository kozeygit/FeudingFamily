using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using FeudingFamily.Hubs;
using FeudingFamily.Logic;

namespace FeudingFamily.EspBuzzer;

public interface IEspBuzzer
{
    bool AllowConnections { get; set; }

}

public class EspBuzzer : IEspBuzzer
{
    private readonly TcpServer _tcpServer;
    private readonly IGameManager _gameManager;
    private HashSet<string> ConnectedBuzzers = [];
    public bool AllowConnections { get; set; }

    public EspBuzzer(IGameManager gameManager)
    {
        Console.WriteLine("Esp Buzzers Enabled");

        _gameManager = gameManager;
        _gameManager.TeamBuzzed += OnBuzzHandler;
        _tcpServer = new TcpServer();
        Task.Run(() => _tcpServer.Start());

        _tcpServer.OnDataReceived += OnClientCommand;
    }

    public Task OnBuzzHandler(object? sender, (string gameKey, Team team) args)
    {
        if (args.gameKey != "1234")
        {
            return Task.CompletedTask;
        }
        
        var connections = _gameManager.GetEspBuzzerConnections(args.gameKey, args.team);

        var buzzers = connections.Select(c => c.ConnectionId);

        foreach (var buzzer in ConnectedBuzzers)
        {
            _tcpServer.SendMessageToClient(buzzer, "LED=0");
        }
        
        foreach (var buzzer in buzzers)
        {
            _tcpServer.SendMessageToClient(buzzer, "LED=1");
        }
        return Task.CompletedTask;

    }
    public void OnClientCommand(object? sender, DataReceivedArgs e)
    {
        var message = e.Message.Trim();
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }
        
        Console.WriteLine(message);

        if (message.Contains("CONNECT"))
        {
            string buzzerId = e.ConnectionId;
            JoinGame(buzzerId, "1234");
            return;
        }

        if (message.Contains("BUZZ") || message == "BUZZ")
        {
            string buzzerId = e.ConnectionId;

            if (!ConnectedBuzzers.Contains(buzzerId))
            {
                JoinGame(buzzerId, "1234");
                return;
            }
            
            if (SendBuzz(buzzerId, "1234"))
            {
                _tcpServer.SendMessageToClient(e.ConnectionId, "LED=1");
            }
            else
            {
                _tcpServer.SendMessageToClient(e.ConnectionId, "LED=0");
            }

            return;
        }

        Console.Write("Unhandled command: ");
        Console.WriteLine(message);
    }

    private void JoinGame(string buzzerId, string gameKey)
    {
        _tcpServer.SendMessageToClient(buzzerId, "CONNECTED");

        var result = _gameManager.JoinGame(gameKey, buzzerId, ConnectionType.EspBuzzer, null);

        if (result.Success is false)
        {
            Console.WriteLine($"Failed to join: {gameKey} because");
            return;
        }

        ConnectedBuzzers.Add(buzzerId);
        Console.WriteLine($"Buzzer connected: {buzzerId}");
    }

    public void OnClientDisconnected(object? sender, DataReceivedArgs e)
    {
        throw new NotImplementedException();
    }


    private void OnClientConnected(object? sender, DataReceivedArgs e)
    {
        throw new NotImplementedException();
    }
    
    public bool SendBuzz(string buzzerId, string gameKey)
    {
        var gameResult = _gameManager.GetGame(gameKey);

        if (gameResult.Success is false)
        {
            return false;
        }

        var game = gameResult.Game!;

        var connection = _gameManager.GetConnection("1234", buzzerId) ?? throw new Exception("ESPBuzzer not in connection manager list???");

        var team = game.GetTeam(connection);

        if (team is null)
        {
            return false;
        }

        return game.Buzz(team);

    }

}








// Idea, Very stupid one, but still... 
// When a buzzer wants to join the game
// Send a noninvasive prompt to all controller pages that lasts 5 seconds.
// The prompt should ask if the buzzer should be allowed to join.
// The prompt should then ask for a number between 1 and 5, the amount of presses the buzzer should do.
// If the buzzer matches the count within a certain time (Maybe 5 seconds), then the buzzer is allowed to join.

// If the buzzer is allowed to join, then the prompt should ask which team the buzzer should join, or if to create a new one for it.

// ! FOR NOW JUST HAVE IT ALWAY JOIN GAME 1234. and prompt for the team name only.
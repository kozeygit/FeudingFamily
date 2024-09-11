using System;
using System.Collections.Concurrent;
using FeudingFamily.Hubs;
using FeudingFamily.Logic;

namespace FeudingFamily.EspBuzzer;

public interface IEspBuzzer
{
    bool AllowConnections { get; set; }

}

public class EspBuzzer : IEspBuzzer
{
    // TODO: Add your code here
    private readonly TcpServer _tcpServer;
    private readonly IGameManager _gameManager;
    public ConcurrentDictionary<string, Channel> ConnectedBuzzers = [];
    public bool AllowConnections { get; set; }

    public EspBuzzer(IGameManager gameManager)
    {
        Console.WriteLine("Esp Buzzers Enabled");

        _gameManager = gameManager;
        _tcpServer = new TcpServer();
        Task.Run(() => _tcpServer.Start());

        _tcpServer.OnDataReceived += OnClientCommand;
    }

    private void BuzzerJoin(string buzzerId, string gameKey)
    {
        _tcpServer.SendMessageToClient(buzzerId, "CONNECTED");
        Console.WriteLine($"Buzzer connected: {buzzerId}");
        // Add buzzer to team
        // If team does not exist, create it.
        // If team exists, add buzzer to it.
    
    }

    public void OnClientDisconnected(object? sender, DataReceivedArgs e)
    {
        throw new NotImplementedException();
    }

    public void OnClientCommand(object? sender, DataReceivedArgs e)
    {
        var message = e.Message.Trim();
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        if (message.Contains("CONNECT"))
        {
            string buzzerId = e.ConnectionId;
            BuzzerJoin(buzzerId, "1234");
            return;
        }
        Console.Write("Unhandled command: ");
        Console.WriteLine(message);
    }

    private void OnClientConnected(object? sender, DataReceivedArgs e)
    {
        throw new NotImplementedException();
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
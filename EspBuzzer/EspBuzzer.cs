using FeudingFamily.Logic;
using FeudingFamily.Models;
using Microsoft.Extensions.Logging;

namespace FeudingFamily.EspBuzzer;

public interface IEspBuzzer
{
    bool AllowConnections { get; set; }
}

public class EspBuzzer : IEspBuzzer
{
    private readonly IGameManager _gameManager;
    private readonly TcpServer _tcpServer;
    private readonly ILogger<EspBuzzer> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly HashSet<string> ConnectedBuzzers = [];

    public EspBuzzer(IGameManager gameManager, ILogger<EspBuzzer> logger, ILoggerFactory loggerFactory)
    {
        _logger = logger;
        _loggerFactory = loggerFactory;
        _logger.LogInformation("ESP buzzers enabled");

        _gameManager = gameManager;
        _gameManager.TeamBuzzed += OnBuzzHandler;
        _tcpServer = new TcpServer(_loggerFactory.CreateLogger<TcpServer>());
        Task.Run(() => _tcpServer.Start());

        _tcpServer.OnDataReceived += OnClientCommand;
    }

    public bool AllowConnections { get; set; }

    public async Task OnTeamChangeHandler(object? sender, (string gameKey, Team? playingTeam) args)
    {
        if (args.gameKey != "1234")
        {
            await Task.CompletedTask;
            return;
        }

        List<GameConnection> connections;

        if (args.playingTeam is null)
        {
            connections = _gameManager.GetEspBuzzerConnections(args.gameKey);
            var buzzers = connections.Select(c => c.ConnectionId).ToList();
            buzzers.ForEach(b => _tcpServer.SendMessageToClient(b, "LED=0"));
            return;
        }


        connections = _gameManager.GetEspBuzzerConnections(args.gameKey, args.playingTeam);
        var playingBuzzers = connections.Select(c => c.ConnectionId);
        var nonPlayingBuzzers = ConnectedBuzzers.Where(b => !playingBuzzers.Contains(b));

        foreach (var buzzer in nonPlayingBuzzers) _tcpServer.SendMessageToClient(buzzer, "LED=0");

        foreach (var buzzer in playingBuzzers) _tcpServer.SendMessageToClient(buzzer, "LED=1");

        await Task.CompletedTask;
    }

    public async Task OnBuzzHandler(object? sender, (string gameKey, Team team) args)
    {
        if (args.gameKey != "1234") await Task.CompletedTask;

        await Task.CompletedTask;
    }

    public void OnClientCommand(object? sender, DataReceivedArgs e)
    {
        var message = e.Message.Trim();

        if (string.IsNullOrWhiteSpace(message)) return;

        var buzzerId = e.ConnectionId;

        if (message.Contains("CONNECT"))
        {
            JoinGame(buzzerId, "1234");
        }

        else if (message.Contains("BUZZ") || message == "BUZZ")
        {
            SendBuzz(buzzerId, "1234");
        }

        else
        {
            _logger.LogWarning("Unhandled command: {Message}", message);
        }
    }

    private async Task JoinGame(string buzzerId, string gameKey)
    {
        var result = await _gameManager.JoinGame(gameKey, buzzerId, ConnectionType.EspBuzzer, null);

        if (result.Success is false)
        {
            _logger.LogError("Buzzer {BuzzerId} failed to join game {GameKey}: {ErrorCode}", buzzerId, gameKey, result.ErrorCode);
            _tcpServer.SendMessageToClient(buzzerId, "ERROR");
            return;
        }

        var game = _gameManager.GetGame(gameKey).Game;

        game.OnTeamChange += OnTeamChangeHandler;
        _tcpServer.SendMessageToClient(buzzerId, "CONNECTED");
        ConnectedBuzzers.Add(buzzerId);
        _logger.LogInformation("Buzzer connected: {BuzzerId}", buzzerId);
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
            _logger.LogWarning("Buzz ignored: game {GameKey} not found for buzzer {BuzzerId}", gameKey, buzzerId);
            return false;
        }

        var game = gameResult.Game!;
        
        GameConnection connection;
        try
        {
            connection = _gameManager.GetConnection("1234", buzzerId);
        }
        catch (NullReferenceException ex)
        {
            _logger.LogWarning("Buzzer {BuzzerId} not found in game. Attempting rejoin", buzzerId);
            _tcpServer.SendMessageToClient(buzzerId, "ERROR");
            JoinGame(buzzerId, gameKey);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error getting connection for buzzer {BuzzerId}", buzzerId);
            return false;
        }

        var team = game.GetTeam(connection);

        if (team is null)
        {
            _logger.LogWarning("Buzz ignored: connection not mapped to team for buzzer {BuzzerId}", buzzerId);
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
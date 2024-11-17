using System.Collections;
using System.ComponentModel;
using FeudingFamily.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;

namespace FeudingFamily.Logic;

public class GameManager : IGameManager
{
    private readonly Dictionary<string, Game> games = [];
    private readonly Dictionary<string, GameRoom> gameRooms = [];
    private readonly IQuestionService _questionService;
    private readonly IHubContext<GameHub> _hubContext;
    public event AsyncEventHandler<(string, Team)>? TeamBuzzed;

    public GameManager(IQuestionService questionService, IHubContext<GameHub> hubContext)
    {
        _questionService = questionService;
        _hubContext = hubContext;
    }


    public JoinGameResult GameKeyValidator(string? gameKey)
    {
        // Valid game key: 4 characters long, alphanumeric, not null/empty/whitespace
        if (string.IsNullOrWhiteSpace(gameKey))
        {
            return new JoinGameResult { ErrorCode = JoinErrorCode.KeyEmpty };
        }

        if (gameKey.Length != 4)
        {
            return new JoinGameResult { ErrorCode = JoinErrorCode.KeyWrongLength };
        }

        if (gameKey.Any(c => !char.IsLetterOrDigit(c)))
        {
            return new JoinGameResult { ErrorCode = JoinErrorCode.KeyNotAlphanumeric };
        }

        return new JoinGameResult { GameKey = gameKey };
    }

    public JoinGameResult NewGame(string gameKey)
    {
        if (!GameKeyValidator(gameKey).Success)
        {
            return new JoinGameResult { ErrorCode = GameKeyValidator(gameKey).ErrorCode };
        }

        if (games.ContainsKey(gameKey))
        {
            return new JoinGameResult { ErrorCode = JoinErrorCode.KeyInUse };
        }

        var game = new Game(gameKey, _questionService);
        games.Add(gameKey, game);
        gameRooms.Add(gameKey, new GameRoom { GameKey = gameKey });

        game.OnBuzz += OnGameBuzz;

        return new JoinGameResult { Game = game };

    }



    public async Task OnGameBuzz(object? sender, (string gameKey, Team team) args)
    {
        Console.WriteLine($"OnGameBuzz, {args.gameKey}, {args.team}");
        string gameKey = args.gameKey;
        Team team = args.team;
        
        var gameResult = GameKeyValidator(gameKey);

        if (gameResult.Success is false)
        {
            return;
        }
        
        var game = GetGame(gameKey).Game;

        var presenterConnections = GetPresenterConnections(gameKey).Select(c => c.ConnectionId);
        var controllerConnections = GetControllerConnections(gameKey).Select(c => c.ConnectionId);
        var teamConnections = GetBuzzerConnections(gameKey, team).Select(c => c.ConnectionId);

        var connections = presenterConnections.Concat(controllerConnections).Concat(teamConnections);


        await _hubContext.Clients.Clients(presenterConnections).SendAsync("receivePlaySound", "buzz-in");

        await _hubContext.Clients.Clients(connections).SendAsync("receiveRound", game.CurrentRound.MapToDto());
        await _hubContext.Clients.Clients(connections).SendAsync("receiveTeams", game.Teams.Select(t => t.MapToDto()));

        var teamPlaying = game.TeamPlaying?.MapToDto();
        await _hubContext.Clients.Clients(connections).SendAsync("receiveTeamPlaying", team);


        await _hubContext.Clients.Clients(connections).SendAsync("receiveBuzz", team.MapToDto());
    }

    public JoinGameResult GetGame(string gameKey)
    {
        if (!GameKeyValidator(gameKey).Success)
        {
            return new JoinGameResult { ErrorCode = GameKeyValidator(gameKey).ErrorCode };
        }

        if (!games.TryGetValue(gameKey, out Game? game))
        {
            return new JoinGameResult { ErrorCode = JoinErrorCode.GameNotFound };
        }

        return new JoinGameResult { Game = game };
    }

    public JoinGameResult JoinGame(string gameKey, string connectionId, ConnectionType connectionType, Guid? teamID)
    {

        if (!games.TryGetValue(gameKey, out Game? game))
        {
            return new JoinGameResult { ErrorCode = JoinErrorCode.GameNotFound };
        }
        

        var connection = new GameConnection { ConnectionId = connectionId, ConnectionType = connectionType };

        if (gameRooms[gameKey].Connections.Contains(connection))
        {
            Console.WriteLine("Already connected");
            return new JoinGameResult { Game = game };
        }

        if (connectionType == ConnectionType.Buzzer && teamID is not null)
        {
            var team = game.GetTeam((Guid)teamID);

            if (team is null)
            {
                return new JoinGameResult { ErrorCode = JoinErrorCode.TeamNameInvalid };
            }

            team.AddMember(connection);
        }

        // if connection type is esp buzzer, check if there is a team with an esp buzzer already, if yay, add to second team; nay, to first
        if (connectionType == ConnectionType.EspBuzzer)
        {
            Team team;

            switch (game.Teams.Count)
            {
                case 0:
                    {
                        game.AddTeam("Team1");
                        team = game.GetTeam("Team1")!;
                        break;
                    }
                case 1:
                    {
                        if (!game.Teams[0].Members.Any(m => m.ConnectionType == ConnectionType.EspBuzzer))
                        {
                            team = game.Teams[0];
                        }

                        else
                        {
                            game.AddTeam("Team2");
                            team = game.GetTeam("Team2")!;
                        }
                        break;
                    }
                case 2:
                    {
                        if (!game.Teams[0].Members.Any(m => m.ConnectionType == ConnectionType.EspBuzzer))
                        {
                            team = game.Teams[0];
                        }

                        else if (!game.Teams[1].Members.Any(m => m.ConnectionType == ConnectionType.EspBuzzer))
                        {
                            team = game.Teams[1];
                        }

                        else
                        {
                            return new JoinGameResult { ErrorCode = JoinErrorCode.GameHasTwoTeams };
                        }
                        break;
                    }
                default:
                    {
                        throw new Exception("OH NO! WHAT HAPPENED? :(");
                    }
            }

            team.AddMember(connection);
        }

        gameRooms[gameKey].AddConnection(connection);

        return new JoinGameResult { Game = game };
    }

    public void LeaveGame(string gameKey, string connectionId)
    {
        Console.WriteLine($"---GameManager---: LeaveGame: {gameKey}, {connectionId}");

        if (gameKey is null || connectionId is null)
        {
            return;
        }

        if (games.TryGetValue(gameKey, out Game? game) is false)
        {
            return;
        }

        if (gameRooms.TryGetValue(gameKey, out GameRoom? gameRoom) is false)
        {
            return;
        }

        var connection = gameRoom.Connections.SingleOrDefault(c => c.ConnectionId == connectionId);

        if (connection is null)
        {
            return;
        }

        game.Teams.ForEach(t => t.Members.Remove(connection));
        gameRoom.RemoveConnection(connection);


        if (gameRoom.Connections.Count == 0 && game.CreatedOn.AddMinutes(1) < DateTime.Now)
        {
            Console.WriteLine($"Removing game: {gameKey}");
            game.Teams.RemoveAll(t => t.Members.Count == 0);
            games.Remove(gameKey);
            gameRooms.Remove(gameKey);
        }
    }

    public string GetGameKey(string connectionId)
    {
        var gameKey = gameRooms.SingleOrDefault(
            gr => gr.Value.Connections.Any(
                c => c.ConnectionId == connectionId)
            ).Key;

        return gameKey;
    }

    public (Game? game, GameConnection? connection) ValidateGameConnection(string gameKey, string connectionId)
    {
        var joinGameResult = GetGame(gameKey);

        if (joinGameResult.Success is false)
        {
            return default;
        }

        var connection = GetConnection(gameKey, connectionId);

        if (HasConnection(gameKey, connection) is false)
        {
            return default;
        }

        return (joinGameResult.Game, connection);
    }

    public bool HasConnection(string gameKey, GameConnection connection)
    {
        var connections = GetConnections(gameKey);
        return connections.Contains(connection);
    }

    public List<GameConnection> GetConnections(string gameKey)
    {
        var connections = gameRooms[gameKey].Connections;
        return connections.ToList();
    }
    public GameConnection GetConnection(string gameKey, string connectionId)
    {
        var connections = GetConnections(gameKey);
        var connection = connections.SingleOrDefault(c => c.ConnectionId == connectionId);
        if (connection is null)
        {
            throw new Exception("Error, no connection found");
        }
        return connection;
    }
    public List<GameConnection> GetPresenterConnections(string gameKey)
    {
        var connections = GetConnections(gameKey);
        var presenterConnections = connections.Where(c => c.ConnectionType == ConnectionType.Presenter).ToList();
        return presenterConnections;
    }
    public List<GameConnection> GetControllerConnections(string gameKey)
    {
        var connections = GetConnections(gameKey);
        var controllerConnections = connections.Where(c => c.ConnectionType == ConnectionType.Controller).ToList();
        return controllerConnections;
    }
    public List<GameConnection> GetBuzzerConnections(string gameKey, Team? team = null)
    {
        if (team is null)
        {
            var connections = GetConnections(gameKey);
            var buzzerConnections = connections.Where(c => c.ConnectionType == ConnectionType.Buzzer).ToList();
            return buzzerConnections;
        }

        return team.Members.ToList();

    }
    public List<GameConnection> GetEspBuzzerConnections(string gameKey, Team? team = null)
    {
        if (team is null)
        {
            var connections = GetConnections(gameKey);
            var espBuzzerConnections = connections.Where(c => c.ConnectionType == ConnectionType.EspBuzzer).ToList();
            return espBuzzerConnections;
        }

        return team.Members.ToList();

    }



    // stolen from the stack lol
    public static string GetErrorMessage(Enum errorCode)
    {
        if (errorCode is null)
            return "";

        var type = errorCode.GetType();
        var field = type.GetField(errorCode.ToString());
        var custAttr = field?.GetCustomAttributes(typeof(DescriptionAttribute), false);
        return custAttr?.SingleOrDefault() is not DescriptionAttribute attribute ? errorCode.ToString() : attribute.Description;
    }

}


public interface IGameManager
{
    event AsyncEventHandler<(string, Team)> TeamBuzzed;
    Task OnGameBuzz(object? sender, (string gameKey, Team team) args);
    JoinGameResult NewGame(string gameKey);
    JoinGameResult JoinGame(string gameKey, string connectionId, ConnectionType connectionType, Guid? teamID);
    JoinGameResult GetGame(string gameKey);
    void LeaveGame(string gameKey, string connectionId);
    JoinGameResult GameKeyValidator(string? gameKey);
    string GetGameKey(string connectionId);
    bool HasConnection(string gameKey, GameConnection connection);
    GameConnection GetConnection(string gameKey, string connectionId);
    List<GameConnection> GetConnections(string gameKey);
    List<GameConnection> GetPresenterConnections(string gameKey);
    List<GameConnection> GetControllerConnections(string gameKey);
    List<GameConnection> GetBuzzerConnections(string gameKey, Team? team = null);
    List<GameConnection> GetEspBuzzerConnections(string gameKey, Team? team = null);
    (Game? game, GameConnection? connection) ValidateGameConnection(string gameKey, string connectionId);
}

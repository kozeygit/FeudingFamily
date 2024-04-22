using System.ComponentModel;

namespace FeudingFamily.Logic;

public class GameManager : IGameManager
{
    private readonly Dictionary<string, Game> games = [];
    private readonly Dictionary<string, GameRoom> gameRooms = [];
    private readonly IQuestionService _questionService;
    public GameManager(IQuestionService questionService)
    {
        _questionService = questionService;
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

        var game = new Game(_questionService);
        games.Add(gameKey, game);
        gameRooms.Add(gameKey, new GameRoom { GameId = gameKey });

        return new JoinGameResult { Game = game };

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

    public JoinGameResult JoinGame(string gameKey, string connectionId, ConnectionType connectionType)
    {
        if (!GameKeyValidator(gameKey).Success)
        {
            return new JoinGameResult { ErrorCode = GameKeyValidator(gameKey).ErrorCode };
        }

        if (!games.TryGetValue(gameKey, out Game? game))
        {
            return new JoinGameResult { ErrorCode = JoinErrorCode.GameNotFound };
        }

        var connection = new GameConnection { ConnectionId = connectionId, ConnectionType = connectionType };

        gameRooms[gameKey].AddConnection(connection);

        return new JoinGameResult { Game = game };
    }

    public JoinGameResult JoinGame(string gameKey, string connectionId, string TeamName)
    {
        Team team;

        if (!GameKeyValidator(gameKey).Success)
        {
            return new JoinGameResult { ErrorCode = GameKeyValidator(gameKey).ErrorCode };
        }

        if (!games.TryGetValue(gameKey, out Game? game))
        {
            return new JoinGameResult { ErrorCode = JoinErrorCode.GameNotFound };
        }

        if (string.IsNullOrWhiteSpace(TeamName))
        {
            return new JoinGameResult { ErrorCode = JoinErrorCode.TeamNameEmpty };
        }

        var connection = new GameConnection { ConnectionId = connectionId, ConnectionType = ConnectionType.Buzzer };

        if (game.HasTeam(TeamName))
        {
            team = game.GetTeam(TeamName)!;
            team.AddMember(connection);
            gameRooms[gameKey].AddConnection(connection);
            return new JoinGameResult { Game = game };
        }

        if (game.Teams.Count >= 2)
        {
            return new JoinGameResult { ErrorCode = JoinErrorCode.GameHasTwoTeams };
        }

        game.AddTeam(TeamName);
        
        team = game.GetTeam(TeamName)!;
        team.AddMember(connection);
        gameRooms[gameKey].AddConnection(connection);

        return new JoinGameResult { Game = game };
    }
    
    public void LeaveGame(string gameKey, string connectionId)
    {
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

        game.Teams.RemoveAll(t => t.Members.Count == 0);

        gameRoom.RemoveConnection(connection);

        Console.WriteLine(gameRoom.Connections.Count);

        if (gameRoom.Connections.Count == 0 && game.CreatedOn.AddMinutes(5) < DateTime.Now)
        {
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


    public bool HasConnection(string gameKey, GameConnection connection)
    {
        var conns = GetConnections(gameKey);
        return conns.Contains(connection);
    }

    public List<GameConnection> GetConnections(string gameKey)
    {
        var connections = gameRooms[gameKey].Connections;
        return connections;
    }
    public GameConnection GetConnection(string gameKey, string connectionId)
    {
        var connections = GetConnections(gameKey);
        var connection = connections.SingleOrDefault(c => c.ConnectionId == connectionId);
        if (connection is null)
        {
            return new GameConnection { ConnectionId = connectionId };
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
    public List<GameConnection> GetBuzzerConnections(string gameKey)
    {
        var connections = GetConnections(gameKey); 
        var buzzerConnections = connections.Where(c => c.ConnectionType == ConnectionType.Buzzer).ToList();
        return buzzerConnections;
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
    JoinGameResult NewGame(string gameKey);
    JoinGameResult JoinGame(string gameKey, string connectionId, ConnectionType connectionType);
    JoinGameResult JoinGame(string gameKey, string connectionId, string TeamName);
    JoinGameResult GetGame(string gameKey);
    void LeaveGame(string gameKey, string connectionId);
    JoinGameResult GameKeyValidator(string? gameKey);
    string GetGameKey(string connectionId);
    bool HasConnection(string gameKey, GameConnection connection);
    GameConnection GetConnection(string gameKey, string connectionId);
    List<GameConnection> GetConnections(string gameKey);
    List<GameConnection> GetPresenterConnections(string gameKey);
    List<GameConnection> GetControllerConnections(string gameKey);
    List<GameConnection> GetBuzzerConnections(string gameKey);
}

using System.ComponentModel;

namespace FeudingFamily.Logic;

public class GameManager : IGameManager
{
    private readonly Dictionary<string, Game> games = [];
    private readonly Dictionary<string, GameRoom> gamesRooms = [];
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

        return new JoinGameResult();
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
        gamesRooms.Add(gameKey, new GameRoom { GameId = gameKey });

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
            return new JoinGameResult { ErrorCode = JoinErrorCode.GameDoesNotExist };
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
            return new JoinGameResult { ErrorCode = JoinErrorCode.GameDoesNotExist };
        }

        var connection = new GameConnection { ConnectionId = connectionId, ConnectionType = connectionType };

        gamesRooms[gameKey].AddConnection(connection);

        return new JoinGameResult { Game = game };
    }

    public JoinGameResult JoinGame(string gameKey, string connectionId, string TeamName)
    {
        if (!GameKeyValidator(gameKey).Success)
        {
            return new JoinGameResult { ErrorCode = GameKeyValidator(gameKey).ErrorCode };
        }

        if (!games.TryGetValue(gameKey, out Game? game))
        {
            return new JoinGameResult { ErrorCode = JoinErrorCode.GameDoesNotExist };
        }

        if (game.Teams.Count == 2)
        {
            return new JoinGameResult { ErrorCode = JoinErrorCode.GameHasTwoTeams };
        }

        if (string.IsNullOrWhiteSpace(TeamName))
        {
            return new JoinGameResult { ErrorCode = JoinErrorCode.TeamNameEmpty };
        }

        var connection = new GameConnection { ConnectionId = connectionId, ConnectionType = ConnectionType.Buzzer };

        if (!game.HasTeam(TeamName))
        {   
            game.AddTeam(TeamName);
        }

        var team = game.Teams.Single(t => t.Name == TeamName);
        team.AddMember(connection);

        gamesRooms[gameKey].AddConnection(connection);

        return new JoinGameResult { Game = game };
    }
    
    public void LeaveGame(string gameKey, string connectionId)
    {
        var connection = gamesRooms[gameKey].Connections.Single(c => c.ConnectionId == connectionId);
        
        foreach (var team in games[gameKey].Teams)
        {
            team.RemoveMember(connection);
        }

        var gameRoom = gamesRooms[gameKey];        
        gameRoom.RemoveConnection(connection);
    }


    public string GetGameKeyFromConnectionId(string connectionId)
    {
        throw new NotImplementedException();
    }

    public List<GameConnection> GetConnections(string gameKey)
    {
        var connections = gamesRooms[gameKey].Connections;
        return connections;
        throw new NotImplementedException();
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
    string GetGameKeyFromConnectionId(string connectionId);
    List<GameConnection> GetConnections(string gameKey);
    List<GameConnection> GetPresenterConnections(string gameKey);
    List<GameConnection> GetControllerConnections(string gameKey);
    List<GameConnection> GetBuzzerConnections(string gameKey);
}

using System.ComponentModel;
using Microsoft.AspNetCore.Mvc.Routing;

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

        if (game.Teams.Length == 2)
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

        var team = game.Teams.Single(t => t.TeamName == TeamName);
        team.AddMember(connection);

        gamesRooms[gameKey].AddConnection(connection);

        return new JoinGameResult { Game = game };
    }

    public bool AddConnection(string gameKey, string connectionId)
    {
        throw new NotImplementedException();
    }

    public bool RemoveConnection(string gameKey, string connectionId)
    {
        throw new NotImplementedException();
    }

    public string GetGameKeyFromConnection(string connectionId)
    {
        throw new NotImplementedException();
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
    JoinGameResult GameKeyValidator(string? gameKey);
    bool AddConnection(string gameKey, string connectionId);
    bool RemoveConnection(string gameKey, string connectionId);
    string GetGameKeyFromConnection(string connectionId);
}

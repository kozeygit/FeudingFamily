using System.ComponentModel;

namespace FeudingFamily.Logic;

public class GameManager : IGameManager
{
    private readonly Dictionary<string, Game> games = [];
    private readonly IQuestionService _questionService;
    public GameManager(IQuestionService questionService)
    {
        _questionService = questionService;
    }

    public JoinGameResult NewGame(string gameKey)
    {
        JoinGameResult validGameKey = GameKeyValidator(gameKey);
        if (validGameKey.Success is false)
            return validGameKey;

        if (games.ContainsKey(gameKey))
            return new JoinGameResult { ErrorCode = JoinErrorCode.KeyInUse };

        var newGame = new Game(_questionService);
        games.Add(gameKey, newGame);
        gameConnections.Add(gameKey, []);

        return new JoinGameResult { GameKey = gameKey, Game = games[gameKey] };
    }

    public JoinGameResult TryJoinGame(string gameKey, string connectionId)
    {
        JoinGameResult validGameKey = GameKeyValidator(gameKey);
        if (validGameKey.Success is false)
        {
            return validGameKey;
        }

        if (games.ContainsKey(gameKey) is false)
            return new JoinGameResult { ErrorCode = JoinErrorCode.GameDoesNotExist };

        if (gameConnections[gameKey].Contains(connectionId))
            return new JoinGameResult { ErrorCode = JoinErrorCode.AlreadyJoined };

        return new JoinGameResult { GameKey = gameKey, Game = games[gameKey] };
    }

    public JoinGameResult GetGame(string gameKey)
    {

        JoinGameResult validGameKey = GameKeyValidator(gameKey);
        if (validGameKey.Success is false)
        {
            return validGameKey;
        }

        if (games.ContainsKey(gameKey) is false)
            return new JoinGameResult { ErrorCode = JoinErrorCode.GameDoesNotExist };

        return new JoinGameResult { GameKey = gameKey, Game = games[gameKey] };
    }

    public JoinGameResult GameKeyValidator(string? gameKey)
    {
        if (gameKey is null)
            return new JoinGameResult { ErrorCode = JoinErrorCode.KeyEmpty };

        if (gameKey.Length != 4)
            return new JoinGameResult { ErrorCode = JoinErrorCode.KeyWrongLength };

        if (gameKey.ToUpper() != gameKey)
            return new JoinGameResult { ErrorCode = JoinErrorCode.KeyNotUpperCase };


        return new JoinGameResult { GameKey = gameKey };
    }

    public bool AddConnectionToGame(string gameKey, string connectionId)
    {
        if (games.ContainsKey(gameKey) is false)
            return false;

        gameConnections[gameKey].Add(connectionId);
        return true;
    }

    public bool RemoveConnectionFromGame(string gameKey, string connectionId)
    {
        if (games.ContainsKey(gameKey) is false)
            return false;

        gameConnections[gameKey].Remove(connectionId);
        return true;
    }

    public string GetGameKeyFromConnectionId(string connectionId)
    {
        foreach (var game in games)
        {
            if (gameConnections[game.Key].Contains(connectionId))
                return game.Key;
        }

        return "";
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
    JoinGameResult GetGame(string gameKey);
    JoinGameResult GameKeyValidator(string? gameKey);
    bool AddConnectionToGame(string gameKey, string connectionId);
    bool RemoveConnectionFromGame(string gameKey, string connectionId);
    string GetGameKeyFromConnectionId(string connectionId);
}

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

    // stolen from the stack lol
    public static string GetErrorMessage(Enum errorCode)
    {
        if (errorCode == null) { return ""; }

        var type = errorCode.GetType();
        var field = type.GetField(errorCode.ToString());
        var custAttr = field?.GetCustomAttributes(typeof(DescriptionAttribute), false);
        return custAttr?.SingleOrDefault() is not DescriptionAttribute attribute ? errorCode.ToString() : attribute.Description;
    }

}

public enum JoinErrorCode
{
    [Description("Key must be provided.")]
    KeyEmpty,

    [Description("Key must be 4 characters long.")]
    KeyWrongLength,

    [Description("Key must be uppercase.")]
    KeyNotUpperCase,
    
    [Description("Key is already in use.")]
    KeyInUse,

    [Description("Game does not exist.")]
    GameDoesNotExist,
    
    [Description("Team name must be provided.")]
    TeamNameEmpty,

    [Description("Team name is already in use.")]
    TeamNameTaken,

    [Description("The requested game already has two teams playing.")]
    GameHasTwoTeams

}

public interface IGameManager
{
    JoinGameResult NewGame(string gameKey);
    JoinGameResult GetGame(string gameKey);
    JoinGameResult GameKeyValidator(string? gameKey);
}
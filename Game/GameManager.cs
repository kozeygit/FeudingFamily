namespace FeudingFamily.Game;

public class GameManager : IGameManager
{
    private readonly Dictionary<string, Game> games = [];
    private readonly IQuestionService _questionService;
    public GameManager(IQuestionService questionService)
    {
        _questionService = questionService;
    }

    public JoinGameResult CreateNewGame(string gameKey)
    {
        JoinGameResult validGameKey = GameKeyValidator(gameKey);

        if (validGameKey.Success is false)
            return validGameKey;

        var newGame = new Game(_questionService);
        games.Add(gameKey, newGame);

        return new JoinGameResult { GameKey = gameKey };
    }

    public JoinGameResult GetGame(string gameKey)
    {

        JoinGameResult validGameKey = GameKeyValidator(gameKey);
        if (validGameKey.Success is false)
        {
            return validGameKey;
        }

        if (games.ContainsKey(gameKey) is false)
            return new JoinGameResult { ErrorMessage = "Game Does Not Exist" };

        return new JoinGameResult { GameKey = gameKey, Game = games[gameKey] };
    }

    private JoinGameResult GameKeyValidator(string? gameKey)
    {
        if (gameKey == null)
            return new JoinGameResult { ErrorMessage = "No Game Key given" };

        if (gameKey.Length != 4)
            return new JoinGameResult { ErrorMessage = "Key must be 4 characters long." };

        if (gameKey.ToUpper() != gameKey)
            return new JoinGameResult { ErrorMessage = "Key must be upper-case" };

        if (games.ContainsKey(gameKey))
            return new JoinGameResult { ErrorMessage = "Key is already in use." };

        return new JoinGameResult { GameKey = gameKey };
    }


}

public interface IGameManager
{
    JoinGameResult CreateNewGame(string gameKey);
    JoinGameResult GetGame(string gameKey);

}
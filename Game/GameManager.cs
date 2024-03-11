namespace FeudingFamily.Game;

public class GameManager : IGameManager
{
    private readonly Dictionary<string, Game> games = [];
    private readonly IQuestionService _questionService;
    public GameManager(IQuestionService questionService)
    {
        _questionService = questionService;
    }

    public JoinGameResult CreateNewGame(string gameId)
    {
        JoinGameResult validGameKey = GameIdValidator(gameId);

        if (validGameKey.Success is false)
            return validGameKey;

        var newGame = new Game(_questionService);
        games.Add(gameId, newGame);

        return new JoinGameResult { GameId = gameId };
    }

    public JoinGameResult GetGame(string gameId)
    {
        if (games.ContainsKey(gameId) is false)
            return new JoinGameResult { ErrorMessage = "Game Does Not Exist" };

        JoinGameResult validGameKey = GameIdValidator(gameId);
        if (validGameKey.Success is false)
        {
            return validGameKey;
        }

        return new JoinGameResult { GameId = gameId, Game = games[gameId] };
    }

    private JoinGameResult GameIdValidator(string? gameId)
    {
        if (gameId == null)
            return new JoinGameResult { ErrorMessage = "No Game Id given" };
        
        if (gameId.Length != 4)
            return new JoinGameResult { ErrorMessage = "Key must be 4 characters long." };

        if (gameId.ToUpper() != gameId)
            return new JoinGameResult { ErrorMessage = "Key must be upper-case" };

        if (games.ContainsKey(gameId))
            return new JoinGameResult { ErrorMessage = "Key is already in use." };

        return new JoinGameResult { GameId = gameId };
    }


}

public interface IGameManager
{
    JoinGameResult CreateNewGame(string gameId);
    JoinGameResult GetGame(string gameId);

}
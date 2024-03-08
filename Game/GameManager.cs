using System.Data;

namespace FeudingFamily.Game;

public class GameManager
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

    public (JoinGameResult, Game?) GetGame(string gameId)
    {
        if (games.ContainsKey(gameId) is false)
            return (new JoinGameResult { ErrorMessage = "Game Does Not Exist" }, null);

        JoinGameResult validGameKey = GameIdValidator(gameId);
        if (validGameKey.Success is false)
        {
            return (validGameKey, null);
        }

        return (new JoinGameResult { GameId = gameId }, games[gameId]);
    }

    private JoinGameResult GameIdValidator(string gameId)
    {
        if (games.ContainsKey(gameId))
            return new JoinGameResult { ErrorMessage = "Key is already in use." };

        if (gameId.Length != 6)
            return new JoinGameResult { ErrorMessage = "Key must be 4 characters long." };

        if (gameId.ToUpper() != gameId)
            return new JoinGameResult { ErrorMessage = "Key must be upper-case" };

        return new JoinGameResult { GameId = gameId };
    }


}
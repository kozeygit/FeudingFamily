namespace FeudingFamily.Game;

public class GameManager
{
    private readonly Dictionary<string, Game> games = [];

    public JoinGameResult CreateNewGame(string gameId)
    {

        var validCreate = GameIdValidator(gameId);
        
        if (validCreate.Success is false)
            return validCreate;

        var newGame = new Game();
        games.Add(gameId, newGame);

        return new JoinGameResult { GameId = gameId };

    }

    public (JoinGameResult, Game?) GetGame(string gameId)
    {
        if (games.ContainsKey(gameId) is false)
            return (new JoinGameResult { ErrorMessage = "Game Does Not Exist" }, null);

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
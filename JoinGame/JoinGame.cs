using FeudingFamily.Logic;

namespace FeudingFamily.JoinGame;

public static class JoinGame
{
    public static void MapJoinGameEndpoints(this WebApplication app)
    {
        app.MapGet("/form", (IGameManager gameManager, string gameKey, string teamName, string page) =>
        {
            teamName = teamName.ToLower();
            gameKey = gameKey.ToLower();

            if (string.IsNullOrWhiteSpace(gameKey))
                return Results.Redirect($"/?ErrorCode={(int)JoinErrorCode.KeyEmpty}");

            var joinResult = gameManager.GameKeyValidator(gameKey);

            if (!joinResult.Success) return Results.Redirect($"/?ErrorCode={(int)joinResult.ErrorCode!}");

            joinResult = gameManager.GetGame(gameKey);

            if (!joinResult.Success)
            {
                joinResult = gameManager.NewGame(gameKey);

                if (!joinResult.Success) return Results.Redirect($"/?ErrorCode={(int)joinResult.ErrorCode!}");
            }

            return page switch
            {
                "Join" => JoinBuzzerPage(gameKey, teamName, joinResult.Game!, gameManager),
                "Presenter" => JoinPresenterPage(gameKey, gameManager),
                "Controller" => JoinControllerPage(gameKey, gameManager),
                _ => Results.Redirect("/")
            };
        });
    }

    private static IResult JoinPresenterPage(string gameKey, IGameManager gameManager)
    {
        return Results.Redirect($"/Presenter/{gameKey}");
    }

    private static IResult JoinControllerPage(string gameKey, IGameManager gameManager)
    {
        return Results.Redirect($"/Controller/{gameKey}");
    }

    public static bool EspJoin(string gameKey, string teamName, IGameManager gameManager)
    {
        teamName = teamName.ToLower();
        gameKey = gameKey.ToLower();

        if (string.IsNullOrWhiteSpace(gameKey)) return false;

        var joinResult = gameManager.GameKeyValidator(gameKey);

        if (!joinResult.Success) return false;

        var gameExists = gameManager.GetGame(gameKey);

        if (!gameExists.Success) return false;

        var game = gameExists.Game!;

        // Team name validation
        if (string.IsNullOrWhiteSpace(teamName))
        {
            Console.WriteLine("\n\n !! 2 !!\n\n");
            return false;
        }

        if (teamName.Length > 10 ||
            teamName.Length < 3)
            return false;

        // Join existing team
        if (game.HasTeamWithName(teamName))
        {
            Console.WriteLine("Joining game: gameKey; Joining team: teamName");
            return false;
        }

        // If game has two teams, return error
        if (game.Teams.Count == 2) return false;

        // Create new team and join
        if (game.AddTeam(teamName) is false) throw new Exception("Error adding team");

        var team = game.GetTeam(teamName)!;
        Console.WriteLine($"Added Team: {team.Name} with id: {team.ID}");


        return true;
    }

    private static IResult JoinBuzzerPage(string gameKey, string teamName, Game game, IGameManager gameManager)
    {
        // Team name validation
        if (string.IsNullOrWhiteSpace(teamName))
        {
            Console.WriteLine("\n\n !! 2 !!\n\n");
            return Results.Redirect($"/?ErrorCode={(int)JoinErrorCode.TeamNameEmpty}");
        }

        if (teamName.Length > 10 ||
            teamName.Length < 3)
            return Results.Redirect($"/?ErrorCode={(int)JoinErrorCode.TeamNameInvalid}");

        // Join existing team
        if (game.HasTeamWithName(teamName))
        {
            Console.WriteLine("Joining game: gameKey; Joining team: teamName");
            return Results.Redirect($"/Buzzer/{gameKey}?TeamID={game.GetTeam(teamName)!.ID}");
        }

        // If game has two teams, return error
        if (game.Teams.Count == 2) return Results.Redirect($"/?ErrorCode={(int)JoinErrorCode.GameHasTwoTeams}");

        // Create new team and join
        if (game.AddTeam(teamName) is false) throw new Exception("Error adding team");

        var team = game.GetTeam(teamName)!;
        Console.WriteLine($"Added Team: {team.Name} with id: {team.ID}");


        return Results.Redirect($"/Buzzer/{gameKey}?TeamID={team.ID}");
    }
}
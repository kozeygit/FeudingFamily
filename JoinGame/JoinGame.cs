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
                "Join" => JoinBuzzerPage(gameKey, teamName, joinResult.Game!),
                "Presenter" => JoinPresenterPage(gameKey),
                "Controller" => JoinControllerPage(gameKey),
                _ => Results.Redirect("/")
            };
        });
    }

    private static IResult JoinPresenterPage(string gameKey)
    {
        return Results.Redirect($"/Presenter/{gameKey}");
    }

    private static IResult JoinControllerPage(string gameKey)
    {
        return Results.Redirect($"/Controller/{gameKey}");
    }
    
    private static IResult JoinBuzzerPage(string gameKey, string teamName, Game game)
    {
        // Team name validation
        if (string.IsNullOrWhiteSpace(teamName))
        {
            // DEBUG: Checkpoint 2
            return Results.Redirect($"/?ErrorCode={(int)JoinErrorCode.TeamNameEmpty}");
        }

        if (teamName.Length >= 12)
            return Results.Redirect($"/?ErrorCode={(int)JoinErrorCode.TeamNameTooLong}");

        if (teamName.Length <= 3)
            return Results.Redirect($"/?ErrorCode={(int)JoinErrorCode.TeamNameTooShort}");

        // Join existing team
        if (game.HasTeamWithName(teamName))
        {
            // TODO: Join game with team name
            return Results.Redirect($"/Buzzer/{gameKey}?TeamID={game.GetTeam(teamName)!.ID}");
        }

        // If game has two teams, return error
        if (game.Teams.Count == 2) return Results.Redirect($"/?ErrorCode={(int)JoinErrorCode.GameHasTwoTeams}");

        // Create new team and join
        if (game.AddTeam(teamName) is false) throw new Exception("Error adding team");

        var team = game.GetTeam(teamName)!;
        // Team added to game


        return Results.Redirect($"/Buzzer/{gameKey}?TeamID={team.ID}");
    }
}
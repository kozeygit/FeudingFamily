using Microsoft.AspNetCore.Components;
using FeudingFamily.Logic;

namespace FeudingFamily;

public class ControllerPageBase : ComponentBase
{
    [Inject]
    public NavigationManager NavigationManager { get; set; }

    [Inject]
    public IGameManager GameManager { get; set; }

    [SupplyParameterFromQuery(Name = "game")]
    public string GameKey { get; set; } = string.Empty;
    public Game Game { get; set; }

    protected override void OnParametersSet()
    {
        if (string.IsNullOrWhiteSpace(GameKey))
        {
            NavigationManager.NavigateTo("/");
        }

        JoinGameResult validKey = GameManager.GameKeyValidator(GameKey);
        if (validKey.Success is false)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n\n ----- {validKey.ErrorMessage} ---- \n\n");
            Console.ResetColor();
        }

        JoinGameResult joinGame = GameManager.GetGame(GameKey);
        if (joinGame.Success)
        {
            Console.WriteLine(joinGame.Success);
        }

        
    }
}
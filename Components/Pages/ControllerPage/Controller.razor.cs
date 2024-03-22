
using Microsoft.AspNetCore.Components;
using FeudingFamily.Logic;


namespace FeudingFamily;

public class ControllerPageBase : ComponentBase
{
    [Inject]
    IGameManager GameManager { get; set; }

    [Parameter]
    public string GameKey { get; set; }

    public Game? Game { get; set; }

    protected override void OnParametersSet()
    {
        Game = GameManager.GetGame(GameKey).Game;
    }
}
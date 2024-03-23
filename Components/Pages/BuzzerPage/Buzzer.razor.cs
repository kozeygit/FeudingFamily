
using Microsoft.AspNetCore.Components;
using FeudingFamily.Logic;


namespace FeudingFamily;

public class BuzzerPageBase : ComponentBase
{
    [Inject]
    IGameManager GameManager { get; set; }

    [Parameter]
    public string GameKey { get; set; }

    [SupplyParameterFromQuery]
    public string TeamName { get; set; }

    public Game? Game { get; set; }

    protected override void OnParametersSet()
    {
        Game = GameManager.GetGame(GameKey).Game;
    }
}
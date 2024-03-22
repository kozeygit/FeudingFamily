
using Microsoft.AspNetCore.Components;
using FeudingFamily.Logic;

namespace FeudingFamily;

public class PresenterPageBase : ComponentBase
{
    [Parameter]
    public JoinGameResult? JoinResult { get; set; }

    public Game Game { get; set; }

    protected override void OnParametersSet()
    {
        Game = JoinResult.Game;
    }
}
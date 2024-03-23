
using Microsoft.AspNetCore.Components;
using FeudingFamily.Logic;
using FeudingFamily.Models;


namespace FeudingFamily;

public class PresenterPageBase : ComponentBase
{
    [Inject]
    IGameManager GameManager { get; set; }

    [Parameter]
    public string? GameKey { get; set; }

    public Game? Game { get; set; }

    public Question? DefaultQuestion { get; set; }

    protected override void OnParametersSet()
    {

        DefaultQuestion = new Question
        {
            Content = "Default Question",
            Answers = {
                new Answer { Ranking = 1, Content = "Answer1", Points = 100},
                new Answer { Ranking = 2, Content = "Answer2", Points = 80},
                new Answer { Ranking = 3, Content = "Answer3", Points = 60},
                new Answer { Ranking = 4, Content = "Answer4", Points = 40},
                new Answer { Ranking = 5, Content = "Answer5", Points = 20},
            }
        };

        if (GameKey is not null)
            Game = GameManager.GetGame(GameKey).Game;
    }
}
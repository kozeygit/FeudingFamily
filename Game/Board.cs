namespace FeudingFamily.Game;

public record Board
{

    public bool QuestionRevealed { get; set; } = false;
    public List<bool> AnswersRevealed { get; set; } = [false, false, false, false, false];
    
}
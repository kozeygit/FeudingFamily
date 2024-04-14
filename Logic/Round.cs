namespace FeudingFamily.Logic;


public class Round
{
    public int Points { get; set; }
    public int WrongAnswers { get; set; }
    public bool IsQuestionRevealed { get; set; }
    public bool[] IsAnswerRevealed { get; set; }
    public Team? RoundWinner { get; set; }

    public Round()
    {
        Points = 0;
        WrongAnswers = 0;
        IsQuestionRevealed = false;
        IsAnswerRevealed = [false, false, false, false, false];
        RoundWinner = null;
    }
}
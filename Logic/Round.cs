namespace FeudingFamily.Logic;


public record RoundDto
{
    public int Points { get; set; } = 0;
    public int WrongAnswers { get; set; } = 0;
    public bool IsQuestionRevealed { get; set; }
    public bool[] IsAnswerRevealed { get; set; } = [false, false, false, false, false];
}

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

    public RoundDto MapToDto()
    {
        return new RoundDto
        {
            Points = Points,
            WrongAnswers = WrongAnswers,
            IsQuestionRevealed = IsQuestionRevealed,
            IsAnswerRevealed = IsAnswerRevealed
        };
    }
}
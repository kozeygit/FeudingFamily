namespace FeudingFamily.Logic;


public record RoundDto
{
    public int Points { get; set; } = 0;
    public int WrongAnswers { get; set; } = 0;
    public bool IsQuestionRevealed { get; set; }
    public bool[] IsAnswerRevealed { get; set; } = [false, false, false, false, false];
    public bool IsBuzzersEnabled { get; set; }
    public string RoundWinner { get; set; } = string.Empty;
}

public class Round
{
    public int Points { get; set; } = 0;
    public int WrongAnswers { get; set; } = 0;
    public bool IsQuestionRevealed { get; set; } = false;
    public bool[] IsAnswerRevealed { get; set; } = [false, false, false, false, false];
    public bool IsBuzzersEnabled { get; set; } = false;
    public Team? RoundWinner { get; set; }

    public RoundDto MapToDto()
    {
        string rw = string.Empty;

        if (RoundWinner is not null)
        {
            rw = RoundWinner.Name;
        }

        return new RoundDto
        {
            Points = Points,
            WrongAnswers = WrongAnswers,
            IsQuestionRevealed = IsQuestionRevealed,
            IsAnswerRevealed = IsAnswerRevealed,
            IsBuzzersEnabled = IsBuzzersEnabled,
            RoundWinner = rw
        };
    }
}
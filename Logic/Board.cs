namespace FeudingFamily.Logic;

public record Board
{
    public bool IsQuestionRevealed { get; set; } = false;
    public bool[] IsAnswerRevealed { get; set; } = [false, false, false, false, false];
    
    //! These probably dont need to be here, could just be handled by the client
    public bool IsBuzzerModalShown { get; set; } = false;
    public bool IsWrongAnswerModalShown { get; set; } = false;

}
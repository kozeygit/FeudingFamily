namespace BlazorServer.Game;

public record Board
{
    // public string Question { get; set; }
    // public List<string> Answers { get; set; }
    // public List<int> AnswerPoints { get; set; }

    // ^^ In a question class? ^^
    public bool QuestionRevealed { get; set; } = false;
    public List<bool> AnswersRevealed { get; set; } = [false, false, false, false, false];
    
}
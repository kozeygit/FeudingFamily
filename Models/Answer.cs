namespace FeudingFamily.Models;
public class Answer
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public int Points { get; set; }
    public int Ranking { get; set; }
    public int QuestionId { get; set; }

    public AnswerDto MapToDto()
    {
        return new AnswerDto
        {
            Content = Content,
            Points = Points,
            Ranking = Ranking
        };
    }
}

public record AnswerDto
{
    public string Content { get; set; } = string.Empty;
    public int Points { get; set; }
    public int Ranking { get; set; }
}
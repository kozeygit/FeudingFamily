namespace FeudingFamily.Models;
public class Question
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public List<Answer> Answers { get; set; } = [];

    public QuestionDto MapToDto()
    {
        return new QuestionDto
        {
            Content = Content,
            Answers = Answers.Select(a => a.MapToDto()).ToList()
        };
    }
}


public record QuestionDto
{
    public string Content { get; set; } = string.Empty;
    public List<AnswerDto> Answers { get; set; } = [];
}
namespace FeudingFamily.Models;
public record Question
{
    public int Id { get; set; }
    public string Content { get; set; } = String.Empty;
    public List<Answer> Answers { get; set; } = [];
}
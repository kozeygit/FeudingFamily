namespace FeudingFamily.Models;
public record Answer
{
    public int Id { get; set; }
    public string Content { get; set; }
    public int Points { get; set; }
    public int Ranking { get; set; }
    public int QuestionId { get; set; }
    public Question Question { get; set; }
    
}
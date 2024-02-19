namespace BlazorServer.Models;
public record Question
{
    public int Id { get; set; }
    public string Content { get; set; }
    public List<Answer> Answers { get; set; }
}
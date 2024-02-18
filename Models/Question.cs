namespace BlazorServer.Game;
public record Question
{
    public int Id { get; set; }
    public string Text { get; set; }
    public List<Answer> Answers { get; set; }
}
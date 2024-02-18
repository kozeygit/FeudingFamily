namespace BlazorServer.Game;
public record Answer
{
    public int Id { get; set; }
    public string Text { get; set; }
    public int Points { get; set; }
}
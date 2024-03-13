namespace FeudingFamily.Game;

public class JoinGameResult
{
    public string? GameKey { get; set; }
    public string? ErrorMessage { get; set; }
    public Game? Game { get; set; }
    public bool Success => ErrorMessage == null;

}
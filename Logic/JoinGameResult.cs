namespace FeudingFamily.Logic;

public class JoinGameResult
{
    public string? GameKey { get; set; }
    public JoinErrorCode? ErrorCode { get; set; }
    public Game? Game { get; set; }
    public bool Success => ErrorCode == null;

}
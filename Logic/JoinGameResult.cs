using System.ComponentModel;
namespace FeudingFamily.Logic;

public class JoinGameResult
{
    public string? GameKey { get; set; }
    public JoinErrorCode? ErrorCode { get; set; }
    public Game? Game { get; set; }
    public bool Success => ErrorCode is null;

}

public enum JoinErrorCode
{
    [Description("Key must be provided.")]
    KeyEmpty,

    [Description("Key must be 4 characters long.")]
    KeyWrongLength,

    [Description("Key must contain only alphanumeric characters.")]
    KeyNotAlphanumeric,
    
    [Description("Key is already in use.")]
    KeyInUse,

    [Description("Game does not exist.")]
    GameDoesNotExist,
    
    [Description("Team name must be provided.")]
    TeamNameEmpty,

    [Description("Team name is already in use.")]
    TeamNameTaken,

    [Description("The requested game already has two teams playing.")]
    GameHasTwoTeams
    

}
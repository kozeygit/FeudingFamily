using System.ComponentModel;

namespace FeudingFamily.Logic;

public class JoinGameResult
{
    public string? GameKey { get; set; }
    public JoinErrorCode? ErrorCode { get; set; }
    public bool Success => ErrorCode is null;
    public Game? Game { get; set; }
}

public enum JoinErrorCode
{
    [Description("Key must be provided.")] KeyEmpty,

    [Description("Key must be 4 characters long.")]
    KeyWrongLength,

    [Description("Key must contain only alphanumeric characters.")]
    KeyNotAlphanumeric,

    [Description("Key is already in use.")]
    KeyInUse,

    [Description("Game does not exist.")] GameNotFound,

    [Description("Team name must be provided.")]
    TeamNameEmpty,

    [Description("Team name is invalid for some reason lol.")]
    TeamNameInvalid,
    [Description("Team name must be less than 12 characters.")]
    TeamNameTooLong,
    [Description("Team name must be more than 3 characters.")]
    TeamNameTooShort,

    [Description("The requested game already has two teams playing.")]
    GameHasTwoTeams
}
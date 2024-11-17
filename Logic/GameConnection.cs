namespace FeudingFamily.Logic;

public enum ConnectionType
{
    Presenter,
    Controller,
    Buzzer,
    EspBuzzer
}

public class GameConnection
{
    public required string ConnectionId { get; set; }
    public ConnectionType ConnectionType { get; set; }
}
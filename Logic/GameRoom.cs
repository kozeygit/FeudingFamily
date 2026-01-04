namespace FeudingFamily.Logic;

public class GameRoom
{
    public string? GameKey { get; init; }

    public HashSet<GameConnection> Connections { get; set; } = [];

    public void AddConnection(GameConnection connection)
    {
        Connections.Add(connection);
    }

    public void RemoveConnection(GameConnection connection)
    {
        Connections.Remove(connection);
    }

    public ConnectionType GetConnectionType(string connectionId)
    {
        var connection = Connections.FirstOrDefault(conn => conn.ConnectionId == connectionId);

        if (connection == null) return ConnectionType.Presenter;

        return connection.ConnectionType;
    }
}
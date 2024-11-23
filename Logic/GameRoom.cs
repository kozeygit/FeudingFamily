namespace FeudingFamily.Logic;

public class GameRoom
{
    public string? GameKey { get; init; }

    public HashSet<GameConnection> Connections { get; set; } = [];

    public void AddConnection(GameConnection connection)
    {
        Connections.Add(connection);
        Console.WriteLine($"Adding Connection, Count: {Connections.Count}");
    }

    public void RemoveConnection(GameConnection connection)
    {
        Connections.Remove(connection);
        Console.WriteLine($"Removing Connection, Count: {Connections.Count}");
    }

    public ConnectionType GetConnectionType(string connectionId)
    {
        var connection = Connections.FirstOrDefault(conn => conn.ConnectionId == connectionId);

        if (connection == null) return ConnectionType.Presenter;

        return connection.ConnectionType;
    }
}
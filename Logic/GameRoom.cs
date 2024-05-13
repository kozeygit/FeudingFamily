namespace FeudingFamily.Logic;

public class GameRoom
{
    /// <summary>
    /// Gets the unique identifier for the game.
    /// </summary>
    public string? GameId { get; init; }

    /// <summary>
    /// Gets or sets the list of game connections.
    /// </summary>
    public List<GameConnection> Connections { get; set; } = [];

    /// <summary>
    /// Adds a game connection to the list of connections.
    /// </summary>
    /// <param name="connection">The game connection to add.</param>
    public void AddConnection(GameConnection connection)
    {
        Connections.Add(connection);
        Console.WriteLine($"Adding Connection, Count: {Connections.Count}");
    }

    /// <summary>
    /// Removes a game connection from the list of connections.
    /// </summary>
    /// <param name="connection">The game connection to remove.</param>
    public void RemoveConnection(GameConnection connection)
    {
        Connections.Remove(connection);
        Console.WriteLine($"Removing Connection, Count: {Connections.Count}");
    }

    /// <summary>
    /// Gets the connection type of the game connection with the specified connection ID.
    /// If the connection is not found, returns the default connection type (Presenter).
    /// </summary>
    /// <param name="connectionId">The connection ID of the game connection.</param>
    /// <returns>The connection type of the game connection.</returns>
    public ConnectionType GetConnectionType(string connectionId)
    {
        var connection = Connections.FirstOrDefault(conn => conn.ConnectionId == connectionId);

        if (connection == null)
        {
            return ConnectionType.Presenter;
        }

        return connection.ConnectionType;
    }
}
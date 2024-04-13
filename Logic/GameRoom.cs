using System.ComponentModel.DataAnnotations;

namespace FeudingFamily.Logic;

public class GameRoom
{
    [Required]
    public string? GameId { get; set; }
    public List<GameConnection> Connections { get; set; } = [];

    public void AddConnection(GameConnection connection)
    {
        Connections.Add(connection);
    }

    public void AddConnection(string connectionId, ConnectionType connectionType)
    {
        AddConnection(new GameConnection { ConnectionId = connectionId, ConnectionType = connectionType });
    }

    public void RemoveConnection(string connectionId)
    {
        Connections.RemoveAll(x => x.ConnectionId == connectionId);
    }

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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FeudingFamily.Logic;

public enum ConnectionType
{
    Presenter,
    Controller,
    Buzzer
}
public class GameConnection
{
    public string ConnectionId { get; set; }
    public ConnectionType ConnectionType { get; set; }
}

public class GameGroup
{
    public string GroupId { get; set; }
    public List<GameConnection> Connections { get; set; } = [];
    public Game Game { get; set; }

    public void AddConnection(string connectionId, ConnectionType connectionType)
    {
        Connections.Add(new GameConnection { ConnectionId = connectionId, ConnectionType = connectionType });
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
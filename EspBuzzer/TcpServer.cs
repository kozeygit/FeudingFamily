using System.Net;
using System.Net.Sockets;

// Make this super simple for now.
// Listen for connections on a port.
// If allow connections flag is on, add to connections list/dict.
// If allow connections flag is off, reject connections.
// If a connection is made, send a message to the client to let them know if they are connected.

// When a connected client sends a command, simply print the command to the console for now.

namespace FeudingFamily.EspBuzzer;

public interface ITcpServer
{
    // public bool AllowConnections { get; set; }
    // event Action<string> OnClientConnected;
    // event Action<string> OnClientCommand;
    // public void SendMessageToClient(string clientIP);
}

public class TcpServer
{
    private readonly TcpListener Listener;
    public Channels ConnectedChannels;

    public TcpServer()
    {
        Listener = new TcpListener(IPAddress.Any, 5000);
    }

    public bool Running { get; set; }
    public event EventHandler<DataReceivedArgs> OnDataReceived;
    public event EventHandler<string> OnChannelClosed;

    public async Task Start()
    {
        Listener.Start();
        Running = true;
        ConnectedChannels = new Channels(this);
        while (Running)
        {
            var client = await Listener.AcceptTcpClientAsync();
            _ = Task.Run(() => new Channel(this).Open(client));
        }
    }

    public void SendMessageToClient(string clientID, string message)
    {
        if (ConnectedChannels.OpenChannels.TryGetValue(clientID, out var channel))
            channel.Send(message);
        else
            Console.WriteLine("Client not found.");
    }

    public bool TryRemoveChannel(string clientID, out Channel channel)
    {
        OnChannelClosed.Invoke(this, clientID);
        return ConnectedChannels.OpenChannels.TryRemove(clientID, out channel);
    }

    public void Stop()
    {
        Listener.Stop();
        Running = false;
    }

    public void OnDataIn(DataReceivedArgs e)
    {
        OnDataReceived?.Invoke(this, e);
    }
}
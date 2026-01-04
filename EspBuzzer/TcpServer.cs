using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;

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
    private readonly ILogger<TcpServer> _logger;
    public Channels ConnectedChannels;

    public TcpServer(ILogger<TcpServer> logger)
    {
        _logger = logger;
        Listener = new TcpListener(IPAddress.Any, 5000);
    }

    public bool Running { get; set; }
    public event EventHandler<DataReceivedArgs> OnDataReceived;
    public event EventHandler<string> OnChannelClosed;

    public async Task Start()
    {
        try
        {
            Listener.Start();
            Running = true;
            ConnectedChannels = new Channels(this);
            _logger.LogInformation("TCP server started on port 5000");
            
            while (Running)
            {
                try
                {
                    var client = await Listener.AcceptTcpClientAsync();
                    _logger.LogDebug("New TCP client connection from {RemoteEndPoint}", client.Client.RemoteEndPoint);
                    _ = Task.Run(() => new Channel(this).Open(client));
                }
                catch (ObjectDisposedException)
                {
                    _logger.LogInformation("TCP server listener disposed, stopping");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error accepting TCP client connection");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatal error starting TCP server");
            Running = false;
        }
    }

    public void SendMessageToClient(string clientID, string message)
    {
        if (ConnectedChannels.OpenChannels.TryGetValue(clientID, out var channel))
            channel.Send(message);
        else
            _logger.LogWarning("Client {ClientId} not found when sending message", clientID);
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
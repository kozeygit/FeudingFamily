using System.Net;
using System.Net.Sockets;
using System.Text;

// Make this super simple for now.
// Listen for connections on a port.
// If allow connections flag is on, add to connections list/dict.
// If allow connections flag is off, reject connections.
// If a connection is made, send a message to the client to let them know if they are connected.

// When a connected client sends a command, simply print the command to the console for now.

namespace FeudingFamily.EspBuzzer;

public interface ITcpServer
{
    public bool AllowConnections { get; set; }
    event Action<string> OnClientConnected;
    event Action<string> OnClientCommand;
    public void SendMessageToClient(string clientIP);
}

public class TcpServer
{
    public bool Running { get; set; }
    public event EventHandler<DataReceivedArgs> DataReceived;
    private TcpListener Listener;
    public Channels ConnectedChannels;

    public Server()
    {
        Listener = new TcpListener(IPAddress.Parse(Globals.ServerAddress), Globals.ServerPort);
    }

    public async void Start()
    {
        try
        {
            Listener.Start();
            Running = true;
            ConnectedChannels = new Channels(this);
            while (Running)
            {
                var client = await Listener.AcceptTcpClientAsync();
                Task.Run(() => new Channel(this).Open(client));
            }

        }
        catch (SocketException)
        {
            throw;
        }
    }

    public void Stop()
    {
        Listener.Stop();
        Running = false;
    }

    public void OnDataIn(DataReceivedArgs e)
    {
        DataReceived?.Invoke(this, e);
    }
}
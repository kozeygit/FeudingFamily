using System.Collections.Concurrent;

namespace FeudingFamily.EspBuzzer;

public class Channels
{
    private readonly TcpServer thisServer;
    public ConcurrentDictionary<string, Channel> OpenChannels;

    public Channels(TcpServer myServer)
    {
        OpenChannels = new ConcurrentDictionary<string, Channel>();
        thisServer = myServer;
    }
}
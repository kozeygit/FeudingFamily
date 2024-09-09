
using System.Collections.Concurrent;

namespace FeudingFamily.EspBuzzer;

    public class Channels
    {
        public ConcurrentDictionary<string, Channel> OpenChannels;
        private readonly TcpServer thisServer;
        
        public Channels(TcpServer myServer)
        {
            OpenChannels = new ConcurrentDictionary<string, Channel>();
            thisServer = myServer;
        }
    }

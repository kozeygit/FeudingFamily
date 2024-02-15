using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace BlazorServer.Hubs
{
    public class ControllerHub : Hub
    {
        
        public async Task SendAnswers(string question)
        {
            await Clients.All.SendAsync("ReceiveQuestion", question);
        }
    }
}
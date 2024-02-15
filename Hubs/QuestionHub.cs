using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace BlazorServer.Hubs
{
    public class QuestionHub : Hub
    {
        
        public async Task SendQuestion(string question)
        {
            await Clients.All.SendAsync("ReceiveQuestion", question);
        }

        public async Task SendAnswers(string answers) // List<Answer> answers
        {
            await Clients.All.SendAsync("ReceiveAnswers", answers);
        }
    }
}
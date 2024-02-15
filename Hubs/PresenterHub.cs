using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace BlazorServer.Hubs
{
    public class PresenterHub : Hub
    {
        public async Task SendSound(string sound)
        {
            throw new NotImplementedException();
        }

        public async Task SendSound1(string sound)
        {
            throw new NotImplementedException();
        }
        public async Task SendSound2(string sound)
        {
            throw new NotImplementedException();
        }
        public async Task SendSound3(string sound)
        {
            throw new NotImplementedException();
        }
    }
}

/*
Needed:

send question (question)
send answers (list of: answers + points tuples?)
send reveal question ()
send reveal answer (index of ans)
send wrong answer (how many wrong/how many Xs to show)
send round over (winner of round)?
send countdown ()
send sound (sound name)
send buzzer call (team who buzzed)
send close buzzer model ()
*/
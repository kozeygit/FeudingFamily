using BlazorServer.Game;
using Microsoft.AspNetCore.SignalR;

namespace BlazorServer.Hubs
{
    public class GameHub : Hub
    {
        public async Task SendQuestion(Question question)
        {
            await Clients.All.SendAsync("ReceiveQuestion", question);
        }

        public async Task SendAnswers(List<Answer> answers)
        {
            await Clients.All.SendAsync("ReceiveAnswers", answers);
        }
    }
}

/*
Needed:

Controller:
    new question -> server
    send question (question) -> presenter, controller
    send answer (list of answers (answer + point)) -> presenter, controller 
    send reveal question () -> presenter
    send reveal answer (index of ans) -> presenter
    send wrong answer (how many wrong/how many Xs to show) -> presenter
    send round over (winner of round)? -> presenter
    send countdown () -> presenter
    send sound (sound name) -> presenter
    send close buzzer model () -> presenter

Buzzer:
    send buzzer call (team who buzzed) -> presenter
*/
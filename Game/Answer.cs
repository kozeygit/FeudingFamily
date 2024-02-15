using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorServer.Game
{
    public record Answer
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public int Points { get; set; }
        public int QuestionId { get; set; }
    }
}
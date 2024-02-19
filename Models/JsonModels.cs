using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorServer.Models
{
    public record JsonQuestionModel
    {
        public string Question { get; set; }
        public JsonAnswerModel Answer1 { get; set; }
        public JsonAnswerModel Answer2 { get; set; }
        public JsonAnswerModel Answer3 { get; set; }
        public JsonAnswerModel Answer4 { get; set; }
        public JsonAnswerModel Answer5 { get; set; }
    }
    public record JsonAnswerModel
    {
        public string Text { get; set; }
        public int Points { get; set; }
    }
}
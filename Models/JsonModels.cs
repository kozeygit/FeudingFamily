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

    public record QuestionDto
    {
        public string Content { get; set; }
        public List<AnswerDto> Answers { get; set; }
    }

    public record JsonAnswerModel
    {
        public string Text { get; set; }
        public int Points { get; set; }
    }

    public record AnswerDto
    {
        public string Content { get; set; }
        public int Points { get; set; }
        public int Ranking { get; set; }
    }
}
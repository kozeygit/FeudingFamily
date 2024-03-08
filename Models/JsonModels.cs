namespace FeudingFamily.Models
{
    public record JsonQuestionModel
    {
        public string Question { get; set; } = string.Empty;
        public JsonAnswerModel? Answer1 { get; set; }
        public JsonAnswerModel? Answer2 { get; set; }
        public JsonAnswerModel? Answer3 { get; set; }
        public JsonAnswerModel? Answer4 { get; set; }
        public JsonAnswerModel? Answer5 { get; set; }
    }

    public record JsonAnswerModel
    {
        public string Text { get; set; } = string.Empty;
        public int Points { get; set; }
    }

}
using System.Data;
using Dapper;
using FeudingFamily.Models;

namespace FeudingFamily.Game;

public class QuestionService : IQuestionService
{
    private readonly IDbConnection _connection;
    private int _questionIndex = 0;
    public int QuestionIndex
    {
        get => _questionIndex % Questions.Count;
        set => _questionIndex = value;
    }
    public List<Question> Questions { get; init; } = [];

    public QuestionService(IDbConnection connection)
    {
        _connection = connection;

        Questions = GetQuestionsFromDB();
        ShuffleQuestions();
    }

    public List<Answer> GetAnswersForQuestionFromDB(int questionId)
    {
        var results = _connection
            .Query<Answer>(@"
                SELECT * FROM Answers
                WHERE QuestionId == @questionId
                ORDER BY Ranking;
            ",
            new { questionId }).ToList();

        return results;
    }

    public List<Question> GetQuestionsFromDB()
    {
        var results = _connection.Query<Question>("SELECT * FROM Questions;");
        foreach (var question in results)
        {
            question.Answers = GetAnswersForQuestionFromDB(question.Id);
        }

        return results.ToList();
    }

    public Question GetNextQuestion()
    {
        var question = Questions[QuestionIndex];
        QuestionIndex++;
        return question;
    }

    public void ShuffleQuestions()
    {
        // Fisher-Yates Shuffle Algorithm
        // Assuming we have an array num of n elements:
        // for i from n−1 iterating down to 1 do
        //  k ← random integer that is 0 ≤ j ≤ i
        //  swap num[k] with num[i]

        for (int i = Questions.Count - 1; i > 0; i--)
        {
            int k = Random.Shared.Next(0, i + 1);
            (Questions[k], Questions[i]) = (Questions[i], Questions[k]);
        }
    }
}

public interface IQuestionService
{
    Question GetNextQuestion();
    List<Question> GetQuestionsFromDB();
    List<Answer> GetAnswersForQuestionFromDB(int questionId);
    void ShuffleQuestions();
}


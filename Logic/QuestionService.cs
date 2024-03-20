using Dapper;
using FeudingFamily.Models;
using System.Data;

namespace FeudingFamily.Logic;

public class QuestionService : IQuestionService
{
    private readonly IDbConnection _connection;
    public List<Question> Questions { get; init; } = [];

    public QuestionService(IDbConnection connection)
    {
        _connection = connection;
        Questions = GetQuestionsFromDB();
    }

    private List<Answer> GetAnswersForQuestionFromDB(int questionId)
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

    private List<Question> GetQuestionsFromDB()
    {
        var results = _connection.Query<Question>("SELECT * FROM Questions;");
        foreach (var question in results)
        {
            question.Answers = GetAnswersForQuestionFromDB(question.Id);
        }

        return results.ToList();
    }


    public List<Question> GetQuestions()
    {
        return Questions;
    }

    public List<Question> GetShuffledQuestions()
    {
        // Fisher-Yates Shuffle Algorithm
        // Assuming we have an array num of n elements:
        // for i from n−1 iterating down to 1 do
        //  k ← random integer that is 0 ≤ j ≤ i
        //  swap num[k] with num[i]

        var shuffledQuestions = new List<Question>(Questions);

        for (int i = shuffledQuestions.Count - 1; i > 0; i--)
        {
            int k = Random.Shared.Next(0, i + 1);
            (shuffledQuestions[k], shuffledQuestions[i]) = (shuffledQuestions[i], shuffledQuestions[k]);
        }

        return shuffledQuestions;
    }
}

public interface IQuestionService
{
    List<Question> GetQuestions();
    List<Question> GetShuffledQuestions();
}


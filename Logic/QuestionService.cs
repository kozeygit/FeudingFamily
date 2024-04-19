using Dapper;
using FeudingFamily.Models;
using SQLitePCL;
using System.Data;
using System.Runtime.InteropServices;

namespace FeudingFamily.Logic;

public class QuestionService : IQuestionService
{
    private readonly IDbConnection _connection;

    public QuestionService(IDbConnection connection)
    {
        _connection = connection;
    }

    public async Task<Question> GetQuestionAsync(int questionId)
    {
        const string sql = "SELECT * FROM Questions WHERE Id = @questionId;";

        var result = await _connection.QuerySingleAsync<Question>(sql, new { questionId });

        result.Answers = await GetAnswersForQuestionAsync(questionId);

        return result;
    }
    
    public async Task<List<Question>> GetQuestionsAsync()
    {
        const string sql = "SELECT * FROM Questions;";

        var results = await _connection.QueryAsync<Question>(sql);

        var questions = results.Select(async q => new Question
        {
            Id = q.Id,
            Content = q.Content,
            Answers = await GetAnswersForQuestionAsync(q.Id)
        })
        .ToList();

        return results.ToList();
    }

    public async Task<List<Question>> GetShuffledQuestionsAsync()
    {
        // Fisher-Yates Shuffle Algorithm
        // Assuming we have an array num of n elements:
        // for i from n−1 iterating down to 1 do
        //  k ← random integer that is 0 ≤ j ≤ i
        //  swap num[k] with num[i]

        var shuffledQuestions = await GetQuestionsAsync();

        for (int i = shuffledQuestions.Count - 1; i > 0; i--)
        {
            int k = Random.Shared.Next(0, i + 1);
            (shuffledQuestions[k], shuffledQuestions[i]) = (shuffledQuestions[i], shuffledQuestions[k]);
        }

        return shuffledQuestions;
    }

    public async Task<Question> GetRandomQuestionAsync()
    {
        const string sql = @"
            SELECT * FROM Questions
            ORDER BY RANDOM()
            LIMIT 1;";

        var question = await _connection.QueryFirstOrDefaultAsync<Question>(sql);

        if (question is null)
            throw new Exception("No questions found in the database.");

        question.Answers = await GetAnswersForQuestionAsync(question.Id);

        return question;
    }

    public async Task<List<Answer>> GetAnswersForQuestionAsync(int questionId)
    {
        const string sql = @"
            SELECT * FROM Answers
            WHERE QuestionId == @questionId
            ORDER BY Ranking;";

        var results = await _connection.QueryAsync<Answer>(sql, new { questionId });

        return results.ToList();
    }

    public static Question GetDefaultQuestion()
    {
        return new Question
        {
            Content = "default-question",
            Answers =
            [
                new Answer { Content = "default-answer-1", Points = 100, Ranking = 1 },
                new Answer { Content = "default-answer-2", Points = 80, Ranking = 2 },
                new Answer { Content = "default-answer-3", Points = 60, Ranking = 3 },
                new Answer { Content = "default-answer-4", Points = 40, Ranking = 4 },
                new Answer { Content = "default-answer-5", Points = 20, Ranking = 5 },
            ]
        };
    }

}

/// <summary>
/// Represents a service for managing questions and answers.
/// </summary>
public interface IQuestionService
{
    /// <summary>
    /// Retrieves all questions.
    /// </summary>
    /// <returns>A list of all questions.</returns>
    Task<List<Question>> GetQuestionsAsync();

    /// <summary>
    /// Retrieves a question by its ID.
    /// </summary>
    /// <param name="questionId">The ID of the question to retrieve.</param>
    /// <returns>The question with the specified ID.</returns>
    Task<Question> GetQuestionAsync(int questionId);

    /// <summary>
    /// Retrieves all questions in a shuffled order.
    /// </summary>
    /// <returns>A list of shuffled questions.</returns>
    Task<List<Question>> GetShuffledQuestionsAsync();

    /// <summary>
    /// Retrieves a random question.
    /// </summary>
    /// <returns>A random question.</returns>
    Task<Question> GetRandomQuestionAsync();

    /// <summary>
    /// Retrieves the answers for a question.
    /// </summary>
    /// <param name="questionId">The ID of the question.</param>
    /// <returns>A list of answers for the specified question.</returns>
    /// Task<List<Answer>> GetAnswersForQuestionAsync(int questionId);
}


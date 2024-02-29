using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using FeudingFamily.Data;
using FeudingFamily.Models;
using Microsoft.Data.Sqlite;

namespace FeudingFamily.Game;

public class QuestionService : IQuestionService
{
    private readonly IDbConnection _connection;
    public QuestionService(IDbConnection connection)
    {
        _connection = connection;
    }

    public List<Answer> GetAnswersFromDB()
    {
        throw new NotImplementedException();
    }

    public List<Question> GetQuestionsFromDB()
    {
        throw new NotImplementedException();
    }

    public Task<Question> GetRandomQuestionAsync()
    {
        throw new NotImplementedException();
    }
}

public interface IQuestionService
{
    Task<Question> GetRandomQuestionAsync();
    List<Question> GetQuestionsFromDB();
    List<Answer> GetAnswersFromDB();
}
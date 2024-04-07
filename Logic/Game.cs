using System.ComponentModel;
using FeudingFamily.Models;

namespace FeudingFamily.Logic;

public class Game
{
    private readonly IQuestionService _questionService;
    private int _questionIndex = 0;
    private int roundPoints = 0;
    private int wrongAnswers = 0;
    private bool isQuestionManual = false;

    public Dictionary<string, Team> Teams { get; set; } = [];
    public Team? TeamPlaying { get; set; } = null;
    public Question? CurrentQuestion { get; set; }
    public Board Board { get; set; }


    public Game(IQuestionService questionService)
    {
        _questionService = questionService;
        
        CurrentQuestion = QuestionService.GetDefaultQuestion();

        Board = new Board();
    }

    public void GiveCorrectAnswer(AnswerDto answer, string teamName)
    {
        TeamPlaying = Teams[teamName];
        roundPoints += answer.Points;
        TeamPlaying?.AddRoundWin();
        throw new NotImplementedException();
    }

    public void GiveCorrectAnswer(AnswerDto answer)
    {
        Board.IsAnswerRevealed[answer.Ranking] = true;
        if (Board.IsAnswerRevealed.All(x => x == true))
        {
            // EndRound();
        }
        roundPoints += answer.Points;
        throw new NotImplementedException();
    }

    // I think i cant do this a better way
    // instead of checking if the question is manual
    // i should have a round started boolean
    // if the round has not started, then when new round is called it should just get a new question
    // if the round has started, then when new round is called it should also reset the board and the round points and blah blah

    public async Task NewRound()
    {
        Board.IsAnswerRevealed = Board.IsAnswerRevealed.Select(_ => false).ToArray();
        Board.IsQuestionRevealed = false;
        
        roundPoints = 0;
        wrongAnswers = 0;
        TeamPlaying = null;
        
        
        //roundStarted = false;
        
        if (!isQuestionManual)
            CurrentQuestion = await NewQuestion();

        isQuestionManual = false;
        
        throw new NotImplementedException();
    }

    public void EndRound()
    {
        Board.IsAnswerRevealed = Board.IsAnswerRevealed.Select(_ => true).ToArray();
        TeamPlaying?.AddPoints(roundPoints);
        TeamPlaying?.AddRoundWin();
        throw new NotImplementedException();
    }

    public void GiveIncorrectAnswer(AnswerDto answer)
    {
        Board.IsAnswerRevealed[answer.Ranking] = true;

        wrongAnswers++;
        if (wrongAnswers == 2)
        {
            SwapTeamPlaying();
        }

        if (wrongAnswers == 3)
        {
            SwapTeamPlaying();
            EndRound();
        }
        throw new NotImplementedException();
    }

    public void SetTeamPlaying(string teamName)
    {
        if (!Teams.TryGetValue(teamName, out Team? team))
        {
            throw new InvalidOperationException("Team does not exist");
        }

        TeamPlaying = team;
    }
    public void SwapTeamPlaying()
    {
        if (TeamPlaying is null)
        {
            throw new InvalidOperationException("No team is currently playing");
        }

        TeamPlaying = TeamPlaying == Teams.Values.First() ? Teams.Values.Last() : Teams.Values.First();
    }

    public async Task<Question> NewQuestion(int id)
    {
        var question = await _questionService.GetQuestionAsync(id);

        isQuestionManual = true;

        return question;
    }

    public async Task<Question> NewQuestion(bool isManual = false)
    {
        var question = await _questionService.GetRandomQuestionAsync();

        isQuestionManual = isManual;

        return question;
    }

    public bool HasTeam(string teamName)
    {
        return Teams.ContainsKey(teamName);
    }

    public bool AddTeam(string teamName)
    {
        if (HasTeam(teamName))
        {
            return false;
        }

        if (Teams.Count >= 2)
        {
            return false;
        }

        var newTeam = new Team(teamName);
        Teams.Add(teamName, newTeam);
        return true;
    }

    public Team? GetTeam(string teamName)
    {
        if (!HasTeam(teamName))
        {
            return null;
        }
        return Teams[teamName];
    }

}
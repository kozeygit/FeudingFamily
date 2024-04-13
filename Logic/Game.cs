using System.ComponentModel;
using FeudingFamily.Models;
using Microsoft.AspNetCore.Mvc;

namespace FeudingFamily.Logic;

public class Game
{
    private readonly IQuestionService _questionService;
    private bool isQuestionManual = false;

    public Team[] Teams { get; set; } = new Team[2];
    public Team? TeamPlaying { get; set; } = null;
    public Board Board { get; set; }
    public Question? CurrentQuestion { get; set; }
    public int RoundPoints { get; set; } = 0;
    public int WrongAnswers { get; set; } = 0;


    public Game(IQuestionService questionService)
    {
        _questionService = questionService;
        
        CurrentQuestion = QuestionService.GetDefaultQuestion();

        Board = new Board();
    }

    public void GiveCorrectAnswer(AnswerDto answer, string teamName)
    {
        var team = Teams.SingleOrDefault(x => x.TeamName == teamName);
        if (team is null)
        {
            throw new InvalidOperationException("Team does not exist");
        }

        TeamPlaying = team;
        RoundPoints += answer.Points;
        TeamPlaying.AddRoundWin();
    }

    public void GiveCorrectAnswer(AnswerDto answer)
    {
        Board.IsAnswerRevealed[answer.Ranking] = true;
        if (Board.IsAnswerRevealed.All(x => x == true))
        {
            // EndRound();
        }
        RoundPoints += answer.Points;
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
        
        RoundPoints = 0;
        WrongAnswers = 0;
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
        TeamPlaying?.AddPoints(RoundPoints);
        TeamPlaying?.AddRoundWin();
        throw new NotImplementedException();
    }

    public void GiveIncorrectAnswer(AnswerDto answer)
    {
        Board.IsAnswerRevealed[answer.Ranking] = true;

        WrongAnswers++;
        if (WrongAnswers == 2)
        {
            SwapTeamPlaying();
        }

        if (WrongAnswers == 3)
        {
            SwapTeamPlaying();
            EndRound();
        }
        throw new NotImplementedException();
    }

    public void SetTeamPlaying(string teamName)
    {
        var team = Teams.SingleOrDefault(t => t.TeamName == teamName);
        
        if (team is null)
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

        TeamPlaying = TeamPlaying == Teams.First() ? Teams.Last() : Teams.First();
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
        return Teams.SingleOrDefault(x => x.TeamName == teamName) is not null;
    }

    public bool AddTeam(string teamName)
    {
        if (HasTeam(teamName))
        {
            return false;
        }

        if (Teams.Length >= 2)
        {
            return false;
        }

        var newTeam = new Team(teamName);
        _ = Teams.Append(newTeam);
        
        return true;
    }

    public Team? GetTeam(string teamName)
    {
        return Teams.SingleOrDefault(x => x.TeamName == teamName);
    }

}
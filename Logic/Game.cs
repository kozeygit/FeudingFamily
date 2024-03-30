using FeudingFamily.Models;

namespace FeudingFamily.Logic;

public class Game
{
    private readonly IQuestionService _questionService;
    private int _questionIndex = 0;

    public Dictionary<string, Team> Teams { get; set; } = [];
    public Team? TeamPlaying { get; set; } = null;
    public List<Question> Questions { get; set; }
    public Question? CurrentQuestion { get; set; }
    public Board Board { get; set; }

    public int QuestionIndex
    {
        get => _questionIndex % Questions.Count;
        set => _questionIndex = value;
    }

    public Game(IQuestionService questionService)
    {
        _questionService = questionService;
        Questions = _questionService.GetShuffledQuestions();
        CurrentQuestion = Questions[QuestionIndex];
        Board = new Board();
    }

    public Question GetNextQuestion()
    {
        var question = Questions[QuestionIndex];
        QuestionIndex++;
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

    public bool AddTeamMember(string teamName, string member)
    {
        if (!HasTeam(teamName))
        {
            return false;
        }

        if (Teams[teamName].HasMember(member))
        {
            return false;
        }

        Teams[teamName].AddMember(member);
        return true;
    }

}
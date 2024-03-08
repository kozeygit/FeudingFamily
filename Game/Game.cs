using FeudingFamily.Models;

namespace FeudingFamily.Game;

public class Game
{
    public Team? Team1 { get; set; } = null;
    public Team? Team2 { get; set; } = null;
    public Team? TeamPlaying { get; set; } = null;
    private bool _trackPoints = false;
    public bool TrackPoints
    {
        get => _trackPoints;
        set
        {
            if (Team1 is not null && Team2 is not null)
            {
                _trackPoints = value;
            }
            else
            {
                _trackPoints = false;
            }
        }
    }
    public List<Question> Questions { get; set; }
    public Question? CurrentQuestion { get; set; }
    public Board Board { get; set; }
    private readonly IQuestionService _questionService;
    private int _questionIndex = 0;
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
    
    public bool JoinTeam(string teamName)
    // Make a return object instead idk
    {
        if (Team1 is not null && Team2 is not null)
            return false;
        
        if (Team1 is null)
            Team1 = new Team(teamName);

        else if (Team2 is null)
            Team2 = new Team(teamName);

        return true;
    }


}
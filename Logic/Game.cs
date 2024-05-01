using FeudingFamily.Models;

namespace FeudingFamily.Logic;

public class Game
{
    private readonly IQuestionService _questionService;
    private bool isQuestionManual = false;
    
    public DateTime CreatedOn { get; init; }
    public List<Team> Teams { get; set; } = [];
    public Team? TeamPlaying { get; set; } = null;
    public Round CurrentRound { get; set; }
    public Question CurrentQuestion { get; set; }
    public List<RoundDto> PreviousRounds { get; set; } = [];
    


    public Game(IQuestionService questionService)
    {
        _questionService = questionService;
        
        CurrentQuestion = QuestionService.GetDefaultQuestion();

        CurrentRound = new Round();

        CreatedOn = DateTime.Now;
    }

    public bool Buzz(Team team)
    {
        if (!CurrentRound.IsBuzzersEnabled)
        {
            return false;
        }

        if (!Teams.Contains(team))
        {
            return false;
        }

        CurrentRound.IsBuzzersEnabled = false;
        TeamPlaying = team;
        return true;
    }

    public bool AddTeam(string teamName)
    {
        if (HasTeam(teamName))
        {
            return false;
        }

        if (Teams.Count == 2)
        {
            return false;
        }

        Teams.Add(new Team(teamName));
        return true;
    }

    public bool HasTeam(string teamName)
    {
        return Teams.Any(team => team.Name == teamName);
    }

    public Team? GetTeam(string teamName)
    {
        return Teams.FirstOrDefault(team => team.Name == teamName);
    }
    
    public Team? GetTeam(GameConnection connection)
    {
        return Teams.FirstOrDefault(team => team.Members.Contains(connection));
    }

    // On client, when user holds down new question button
    // Show popup with input field for question id
    // Then call NewQuestion with the id first
    // Then call NewRound
    // Otherwise, just call NewRound directly
    
    public async Task<Question> NewQuestion(int id)
    {
        CurrentQuestion = await _questionService.GetQuestionAsync(id);
        isQuestionManual = true;
        return CurrentQuestion;
    }


    public async Task NewRound()
    {
        PreviousRounds.Add(CurrentRound.MapToDto());
        Console.WriteLine($"Rounds = {PreviousRounds.Count}");

        CurrentRound = new Round
        {
            IsBuzzersEnabled = true
        };

        if (!isQuestionManual)
            CurrentQuestion = await _questionService.GetRandomQuestionAsync();

        isQuestionManual = false;

        TeamPlaying = null;
        
    }


    public void EndRound()
    {
        if (TeamPlaying != null)
        {
            TeamPlaying.AddPoints(CurrentRound.Points);
            TeamPlaying.AddRoundWin();
            CurrentRound.RoundWinner = TeamPlaying;
        }
    }

    public void GiveCorrectAnswer(int answerRanking, string teamName)
    {
        GiveCorrectAnswer(answerRanking);
        SetTeamPlaying(teamName);
    }

    public void GiveCorrectAnswer(int answerRanking)
    {
        CurrentRound.IsBuzzersEnabled = false;

        var answer = CurrentQuestion.Answers[answerRanking-1];
        
        if (CurrentRound.IsAnswerRevealed[answer.Ranking-1])
        {
            return;
        }

        CurrentRound.Points += answer.Points;
        CurrentRound.IsAnswerRevealed[answer.Ranking-1] = true;

        if (CurrentRound.IsAnswerRevealed.All(x => x == true))
        {
            EndRound();
        }
    }

    public void GiveIncorrectAnswer()
    {
        if (CurrentRound.WrongAnswers == 2)
        {
            SwapTeamPlaying();
            CurrentRound.WrongAnswers++;
        }
        
        else if (CurrentRound.WrongAnswers == 3)
        {
            SwapTeamPlaying();
            EndRound();
        }

        else
        {
            CurrentRound.WrongAnswers++;
        }
        
    }

    public void SetTeamPlaying(string teamName)
    {
        TeamPlaying = GetTeam(teamName);
    }

    public void SwapTeamPlaying()
    {
        if (Teams.Count < 2)
        {
            return;
        }

        TeamPlaying = TeamPlaying == Teams[0] ? Teams[1] : Teams[0];
    }
}

/*

when redirected to buzzer/presenter/controller page. 
Send join game signalr method to join the game with the game key.
show loading game screen until the game is loaded.

if the game is not loaded, then show a loading screen until the game is loaded.
if join game signalr method returns false, then show an error message.
and a button to go back to the home page.

then i can encapsulate the whole page in an if block, if the game is not loaded, it wont need to render the things that require a game connection
so there is no null reference exception.

Add signalr method to join game on initialised for each page, if the game key is valid, then join the game and render the usual page.
If not render an error message or a blankish page until the signalr recieves the game key.

if game is null, then show a loading screen 

*/
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
    public List<Round> PreviousRounds { get; set; } = [];
    


    public Game(IQuestionService questionService)
    {
        _questionService = questionService;
        
        CurrentQuestion = QuestionService.GetDefaultQuestion();

        CurrentRound = new Round();

        CreatedOn = DateTime.Now;
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
        //! REMOVE THIS

        Teams.ForEach(team => team.Points+=10);

        //!^^^^^^^^^^^^^^^^^^^^^^^^^

        CurrentRound = new Round();
        
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
            PreviousRounds.Add(CurrentRound);
        }
    }

    public void GiveCorrectAnswer(Answer answer, string teamName)
    {
        GiveCorrectAnswer(answer);
        SetTeamPlaying(teamName);
    }

    public void GiveCorrectAnswer(Answer answer)
    {
        if (CurrentRound.IsAnswerRevealed[answer.Ranking])
        {
            return;
        }

        CurrentRound.Points += answer.Points;
        CurrentRound.IsAnswerRevealed[answer.Ranking] = true;

        if (CurrentRound.IsAnswerRevealed.All(x => x == true))
        {
            EndRound();
        }
    }

    public void GiveIncorrectAnswer()
    {
        CurrentRound.WrongAnswers++;
        
        if (CurrentRound.WrongAnswers == 3)
        {
            SwapTeamPlaying();
        }

        if (CurrentRound.WrongAnswers == 4)
        {
            SwapTeamPlaying();
            EndRound();
        }
    }

    public void SetTeamPlaying(string teamName)
    {
        TeamPlaying = GetTeam(teamName);
    }
    public void SwapTeamPlaying()
    {
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
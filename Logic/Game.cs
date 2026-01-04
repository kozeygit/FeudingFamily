using FeudingFamily.Models;
using Microsoft.Extensions.Logging;

namespace FeudingFamily.Logic;

public class Game
{
    private readonly IQuestionService _questionService;
    private readonly ILogger<Game> _logger;
    private bool _isQuestionManual;
    private bool _isRoundPlaying;
    private Team? teamPlaying;


    public Game(string gameKey, IQuestionService questionService, ILogger<Game> logger)
    {
        GameKey = gameKey;
        _questionService = questionService;
        _logger = logger;

        CurrentQuestion = QuestionService.GetDefaultQuestion();

        CurrentRound = new Round();

        CreatedOn = DateTime.Now;
    }

    public string GameKey { get; set; }
    public DateTime CreatedOn { get; init; }
    public List<Team> Teams { get; set; } = [];

    public Team? TeamPlaying
    {
        get => teamPlaying;
        private set
        {
            teamPlaying = value;
            OnTeamChange?.Invoke(this, (GameKey, teamPlaying));
        }
    }

    public Round CurrentRound { get; set; }
    public Question CurrentQuestion { get; set; }
    public List<RoundDto> PreviousRounds { get; set; } = [];

    public event AsyncEventHandler<(string, Team)> OnBuzz;
    public event AsyncEventHandler<(string, Team?)> OnTeamChange;

    public bool RemoveTeam(Team team)
    {
        if (!Teams.Contains(team)) return false;
        team.Members.Clear();
        Teams.Remove(team);
        TeamPlaying = null;
        OnTeamChange?.Invoke(this, (GameKey, team));
        return true;
    }

    public bool Buzz(Team team)
    {
        if (!CurrentRound.IsBuzzersEnabled)
        {
            _logger.LogDebug("Buzz attempt ignored: buzzers disabled for {TeamName}", team.Name);
            return false;
        }

        if (!Teams.Contains(team)) return false;

        CurrentRound.IsBuzzersEnabled = false;
        TeamPlaying = team;
        _logger.LogInformation("Team {TeamName} buzzed in successfully - GameKey: {GameKey}", team.Name, GameKey);
        OnBuzz?.Invoke(this, (GameKey, team));

        return true;
    }

    public bool AddTeam(string teamName)
    {
        teamName = teamName.ToLower();

        if (HasTeamWithName(teamName)) return false;

        if (Teams.Count == 2) return false;

        Teams.Add(new Team(teamName));
        return true;
    }

    public bool HasTeamWithName(string teamName)
    {
        return Teams.Any(team => team.Name == teamName.ToLower());
    }

    public Team? GetTeam(string teamName)
    {
        return Teams.FirstOrDefault(team => team.Name == teamName.ToLower());
    }

    public Team? GetTeam(Guid ID)
    {
        return Teams.FirstOrDefault(team => team.ID == ID);
    }

    public Team? GetTeam(GameConnection connection)
    {
        return Teams.FirstOrDefault(team => team.Members.Contains(connection));
    }

    public async Task<Question> SetQuestionAsync(int id)
    {
        try
        {
            CurrentQuestion = await _questionService.GetQuestionAsync(id);
            _isQuestionManual = true;
            _logger.LogDebug("Question set - GameKey: {GameKey}, QuestionId: {QuestionId}", GameKey, id);
            return CurrentQuestion;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set question - GameKey: {GameKey}, QuestionId: {QuestionId}", GameKey, id);
            throw;
        }
    }

    public async Task NewRoundAsync()
    {
        _isRoundPlaying = true;

        PreviousRounds.Add(CurrentRound.MapToDto());
        _logger.LogInformation("New round started - GameKey: {GameKey}, RoundNumber: {RoundCount}", GameKey, PreviousRounds.Count + 1);

        // Reset team wrong answer counters for the new round
        foreach (var team in Teams)
        {
            team.WrongAnswers = 0;
        }

        CurrentRound = new Round
        {
            IsBuzzersEnabled = true
        };

        if (!_isQuestionManual)
            CurrentQuestion = await _questionService.GetRandomQuestionAsync();

        _isQuestionManual = false;

        TeamPlaying = null;
    }

    public void EndRound()
    {
        if (_isRoundPlaying is false) return;

        if (TeamPlaying != null)
        {
            TeamPlaying.AddPoints(CurrentRound.Points);
            TeamPlaying.AddRoundWin();
            CurrentRound.RoundWinner = TeamPlaying;
        }

        _isRoundPlaying = false;
    }

    public bool GiveCorrectAnswer(int answerRanking)
    {
        var playSound = true;

        if (_isRoundPlaying is false) playSound = false;

        CurrentRound.IsBuzzersEnabled = false;

        var answer = CurrentQuestion.Answers[answerRanking - 1];

        if (CurrentRound.IsAnswerRevealed[answer.Ranking - 1]) return false;

        AddRoundPoints(answer);
        CurrentRound.IsAnswerRevealed[answer.Ranking - 1] = true;

        if (CurrentRound.IsAnswerRevealed.All(x => x)) EndRound();

        if (CurrentRound.WrongAnswers == 3) EndRound();

        return playSound;
    }

    public void AddRoundPoints(Answer answer)
    {
        if (_isRoundPlaying) CurrentRound.Points += answer.Points;
    }

    /// <summary>
    /// Determines if we're in the Face-Off phase (no answers revealed yet).
    /// Once an answer is revealed, the main round begins.
    /// </summary>
    private bool IsInFaceOff()
    {
        return !CurrentRound.IsAnswerRevealed.Any(x => x);
    }

    public void GiveIncorrectAnswer()
    {
        if (TeamPlaying is null)
        {
            _logger.LogWarning("GiveIncorrectAnswer called but no team is playing - GameKey: {GameKey}", GameKey);
            return;
        }

        // During Face-Off, wrong answers don't count as strikes
        bool isInFaceOff = IsInFaceOff();

        if (!isInFaceOff && !(CurrentRound.WrongAnswers > 3))
        {
            TeamPlaying.WrongAnswers++;
            CurrentRound.WrongAnswers++;  // Keep in sync for backwards compatibility

            _logger.LogInformation("Team {TeamName} given incorrect answer - Team WrongAnswers: {WrongAnswers} - GameKey: {GameKey}", 
                TeamPlaying.Name, TeamPlaying.WrongAnswers, GameKey);

            if (TeamPlaying.WrongAnswers >= 3)
            {
                SwapTeamPlaying();
                _logger.LogInformation("Team reached 3 wrong answers, control passed - GameKey: {GameKey}", GameKey);
            }
        }
        else
        {
            _logger.LogInformation("Face-Off: Team {TeamName} answered incorrectly (no strike counted) - GameKey: {GameKey}", 
                TeamPlaying.Name, GameKey);
        }
    }

    public void SwapTeamPlaying()
    {
        if (Teams.Count == 0) return;

        if (Teams.Count == 1)
            TeamPlaying = Teams[0];

        else
            TeamPlaying = TeamPlaying == Teams[0] ? Teams[1] : Teams[0];
    }
}

/*

when redirected to buzzer/presenter/controller page.
Send join game SignalR method to join the game with the game key.
show loading game screen until the game is loaded.

if the game is not loaded, then show a loading screen until the game is loaded.
if join game SignalR method returns false, then show an error message.
and a button to go back to the home page.

then i can encapsulate the whole page in an if block, if the game is not loaded, it wont need to render the things that require a game connection
so there is no null reference exception.

Add SignalR method to join game on initialized for each page, if the game key is valid, then join the game and render the usual page.
If not render an error message or a blank-ish page until the SignalR receives the game key.

if game is null, then show a loading screen

*/
namespace FeudingFamily.Game;

public class Team
{
    static int Id = 0;
    public readonly int _teamId;
    public int TeamId { get { return _teamId; } }
    public string TeamName { get; set; }
    public int Points { get; set; } = 0;
    public int RoundsWon { get; set; } = 0;
    
    public Team(string teamName)
    {
        _teamId = Id;
        Id++;
        TeamName = teamName;
    }

    public void AddPoints(int points)
    {
        Points += points;
    }

    public void AddRoundWon()
    {
        RoundsWon++;
    }
}    
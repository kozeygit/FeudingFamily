namespace FeudingFamily.Game;

public class Team(string teamName)
{
    public string TeamName { get; set; } = teamName;
    public int Points { get; set; } = 0;
    public int RoundsWon { get; set; } = 0;

    public void AddPoints(int points)
    {
        Points += points;
    }

    public void AddRoundWon()
    {
        RoundsWon++;
    }
}    
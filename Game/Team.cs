namespace FeudingFamily.Game;

public class Team(string teamName)
{
    public string TeamName { get; set; } = teamName;
    public int Points { get; set; } = 0;
    public int RoundsWon { get; set; } = 0;
    public List<string> Members { get; set; } = [];

    public void AddMember(string member)
    {
        if (!Members.Contains(member))
        {
            Members.Add(member);
        }
        else
        {
            Console.WriteLine("Member already part of team.");
        }
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
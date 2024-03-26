namespace FeudingFamily.Logic;

public class Team(string teamName)
{
    public string TeamName { get; set; } = teamName;
    public int Points { get; set; } = 0;
    public int RoundsWon { get; set; } = 0;
    public HashSet<string> Members { get; set; } = [];

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

    public bool HasMember(string member)
    {
        if (member.Contains(member))
        {
            return true;
        }

        return false;
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
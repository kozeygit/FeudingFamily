namespace FeudingFamily.Logic;

public record TeamDto
{
    public string Name { get; set; } = string.Empty;
    public int Points { get; set; }
    public int RoundsWon { get; set; }
}

public class Team(string teamName)
{
    public string Name { get; set; } = teamName;
    public int Points { get; set; } = 0;
    public int RoundsWon { get; set; } = 0;
    public HashSet<GameConnection> Members { get; set; } = [];

    public void AddMember(GameConnection member)
    {
        if (!Members.Add(member))
        {
            Console.WriteLine("Member already part of team.");
        }
    }

    public bool HasMember(GameConnection member)
    {
        if (Members.Contains(member))
        {
            return true;
        }

        return false;
    }

    public bool RemoveMember(GameConnection member)
    {
        // true if member found and removed; otherwise, false
        return Members.Remove(member);
    }
    public void AddPoints(int points)
    {
        Points += points;
    }

    public void AddRoundWin()
    {
        RoundsWon++;
    }

    public TeamDto MapToDto()
    {
        return new TeamDto
        {
            Name = Name,
            Points = Points,
            RoundsWon = RoundsWon
        };
    }
}
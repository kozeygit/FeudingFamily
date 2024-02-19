using BlazorServer.Models;

namespace BlazorServer.Game;

public class Round
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
    public Question CurrentQuestion { get; set; }


}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FeudingFamily.Game;

public class JoinGameResult
{
    public string? GameId { get; set; }
    public string? ErrorMessage { get; set; }
    public bool Success => ErrorMessage == null;
    
}
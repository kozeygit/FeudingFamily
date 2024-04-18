using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FeudingFamily.Logic;

public enum ConnectionType
{
    Presenter,
    Controller,
    Buzzer
}

public class GameConnection
{
    public required string ConnectionId { get; set; }
    public ConnectionType ConnectionType { get; set; }
}
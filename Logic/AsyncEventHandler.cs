namespace FeudingFamily.Logic;

public delegate Task AsyncEventHandler<TEventArgs>(object? sender, TEventArgs e);
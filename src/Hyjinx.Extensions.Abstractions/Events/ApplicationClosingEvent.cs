namespace Hyjinx.Extensions.Abstractions.Events;

/// <summary>
/// Occurs when the application is closing.
/// </summary>
public class ApplicationClosingEvent : Event
{
    /// <summary>
    /// Identifies whether the application is terminating unexpectedly.
    /// </summary>
    public required bool Terminating { get; init; }
}
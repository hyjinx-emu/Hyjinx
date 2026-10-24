namespace Hyjinx.Extensions.Abstractions.Events;

/// <summary>
/// Occurs when a game has been started.
/// </summary>
public class GameStartedEvent : Event
{
    /// <summary>
    /// The application name.
    /// </summary>
    public required string ApplicationName { get; init; }

    /// <summary>
    /// The title id.
    /// </summary>
    public required string TitleId { get; init; }
}
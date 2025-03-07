namespace Hyjinx.UI.Common.Logging;

/// <summary>
/// Describes thread information for logging operations.
/// </summary>
public readonly struct ThreadInformation
{
    /// <summary>
    /// The thread name (if available).
    /// </summary>
    public string? Name { get; init; }
    
    /// <summary>
    /// The thread id.
    /// </summary>
    public int ManagedThreadId { get; init; }

    public override string ToString()
    {
        return Name ?? $"[{ManagedThreadId}]";
    }
}

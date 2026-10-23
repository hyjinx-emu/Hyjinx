namespace Hyjinx.Extensions.Abstractions;

/// <summary>
/// Identifies a service which supports initialization.
/// </summary>
public interface ISupportInitialization
{
    /// <summary>
    /// Initializes the instance.
    /// </summary>
    void Initialize();
}
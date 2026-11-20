namespace Hyjinx.Ava.Configuration;

/// <summary>
/// A mechanism capable of managing which game configuration is applied.
/// </summary>
internal interface IGameConfigurationManager
{
    /// <summary>
    /// Applys the game configuration (if present) for the application id.
    /// </summary>
    /// <param name="applicationId">The application id being loaded.</param>
    /// <exception cref="System.InvalidOperationException">A game configuration is already applied.</exception>
    void ApplyGameConfig(string applicationId);

    /// <summary>
    /// Clears the applied game configuration.
    /// </summary>
    /// <remarks>This method will have no effect if a configuration has not already been applied.</remarks>
    void ClearGameConfig();
}
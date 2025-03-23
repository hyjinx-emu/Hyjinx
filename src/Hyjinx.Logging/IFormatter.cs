using Microsoft.Extensions.Logging;
using System.IO;

namespace Hyjinx.Logging;

/// <summary>
/// A mechanism which formats and writes log entries.
/// </summary>
public interface IFormatter
{
    /// <summary>
    /// The name of the formatter.
    /// </summary>
    string Name { get; }
    
    /// <summary>
    /// Writes the log entry.
    /// </summary>
    /// <param name="logEntry">The log entry.</param>
    /// <param name="scopeProvider">The scope provider, if available.</param>
    /// <param name="textWriter">The text writer.</param>
    /// <typeparam name="TState">The type of entry state.</typeparam>
    void Write<TState>(in LogEntry<TState> logEntry, IExternalScopeProvider? scopeProvider, TextWriter textWriter);
}

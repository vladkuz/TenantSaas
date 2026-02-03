using Microsoft.Extensions.Logging;

namespace TenantSaas.ContractTests.TestUtilities;

/// <summary>
/// Test logger factory for capturing log messages in tests.
/// Provides a thread-safe collection of captured log entries for verification.
/// </summary>
internal sealed class TestLoggerFactory : ILoggerFactory
{
    private readonly List<CapturedLogEntry> capturedEntries;
    private readonly object lockObject = new();

    public TestLoggerFactory(List<CapturedLogEntry> capturedEntries)
    {
        ArgumentNullException.ThrowIfNull(capturedEntries);
        this.capturedEntries = capturedEntries;
    }

    public void AddProvider(ILoggerProvider provider) { }

    public ILogger CreateLogger(string categoryName) => new TestLogger(capturedEntries, categoryName, lockObject);

    public void Dispose() { }
}

/// <summary>
/// Test logger that captures log entries for verification in tests.
/// </summary>
internal sealed class TestLogger : ILogger
{
    private readonly List<CapturedLogEntry> capturedEntries;
    private readonly string categoryName;
    private readonly object lockObject;

    public TestLogger(List<CapturedLogEntry> capturedEntries, string categoryName, object lockObject)
    {
        this.capturedEntries = capturedEntries;
        this.categoryName = categoryName;
        this.lockObject = lockObject;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        var entry = new CapturedLogEntry(
            CategoryName: categoryName,
            LogLevel: logLevel,
            EventId: eventId,
            Message: formatter(state, exception),
            Exception: exception,
            State: state?.ToString());

        lock (lockObject)
        {
            capturedEntries.Add(entry);
        }
    }
}

/// <summary>
/// Represents a captured log entry for test verification.
/// </summary>
/// <param name="CategoryName">Logger category name.</param>
/// <param name="LogLevel">Log severity level.</param>
/// <param name="EventId">Event identifier.</param>
/// <param name="Message">Formatted log message.</param>
/// <param name="Exception">Exception if logged.</param>
/// <param name="State">String representation of the log state.</param>
internal sealed record CapturedLogEntry(
    string CategoryName,
    LogLevel LogLevel,
    EventId EventId,
    string Message,
    Exception? Exception,
    string? State);

/// <summary>
/// Extension methods for asserting on captured log entries.
/// </summary>
internal static class CapturedLogEntryExtensions
{
    /// <summary>
    /// Finds log entries matching the specified event ID.
    /// </summary>
    public static IEnumerable<CapturedLogEntry> WithEventId(this IEnumerable<CapturedLogEntry> entries, int eventId)
        => entries.Where(e => e.EventId.Id == eventId);

    /// <summary>
    /// Finds log entries containing the specified text.
    /// </summary>
    public static IEnumerable<CapturedLogEntry> ContainingMessage(this IEnumerable<CapturedLogEntry> entries, string text)
        => entries.Where(e => e.Message.Contains(text, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Finds log entries at the specified log level.
    /// </summary>
    public static IEnumerable<CapturedLogEntry> AtLevel(this IEnumerable<CapturedLogEntry> entries, LogLevel level)
        => entries.Where(e => e.LogLevel == level);
}

using Microsoft.Extensions.Logging;

namespace TenantSaas.ContractTests.TestUtilities;

/// <summary>
/// Thread-safe collection for capturing log entries in tests.
/// Provides safe enumeration via ToList() snapshot.
/// </summary>
internal sealed class CapturedLogCollection
{
    private readonly List<CapturedLogEntry> entries = [];
    private readonly object syncRoot = new();

    public void Add(CapturedLogEntry entry)
    {
        lock (syncRoot)
        {
            entries.Add(entry);
        }
    }

    public int Count
    {
        get
        {
            lock (syncRoot)
            {
                return entries.Count;
            }
        }
    }

    /// <summary>
    /// Returns a snapshot of all captured entries. Safe for enumeration.
    /// </summary>
    public List<CapturedLogEntry> ToList()
    {
        lock (syncRoot)
        {
            return [.. entries];
        }
    }

    public void Clear()
    {
        lock (syncRoot)
        {
            entries.Clear();
        }
    }
}

/// <summary>
/// Test logger factory for capturing log messages in tests.
/// Provides a thread-safe collection of captured log entries for verification.
/// </summary>
internal sealed class TestLoggerFactory : ILoggerFactory
{
    private readonly CapturedLogCollection capturedEntries;

    public TestLoggerFactory(CapturedLogCollection capturedEntries)
    {
        ArgumentNullException.ThrowIfNull(capturedEntries);
        this.capturedEntries = capturedEntries;
    }

    public void AddProvider(ILoggerProvider provider) { }

    public ILogger CreateLogger(string categoryName) => new TestLogger(capturedEntries, categoryName);

    public ILogger<T> CreateLogger<T>() => new TestLogger<T>(capturedEntries);

    public void Dispose() { }
}

/// <summary>
/// Generic test logger that captures log entries for verification in tests.
/// </summary>
internal sealed class TestLogger<T> : ILogger<T>
{
    private readonly TestLogger _inner;

    public TestLogger(CapturedLogCollection capturedEntries)
    {
        _inner = new TestLogger(capturedEntries, typeof(T).FullName ?? typeof(T).Name);
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => _inner.BeginScope(state);

    public bool IsEnabled(LogLevel logLevel) => _inner.IsEnabled(logLevel);

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter) => _inner.Log(logLevel, eventId, state, exception, formatter);
}

/// <summary>
/// Test logger that captures log entries for verification in tests.
/// </summary>
internal sealed class TestLogger : ILogger
{
    private readonly CapturedLogCollection capturedEntries;
    private readonly string categoryName;

    public TestLogger(CapturedLogCollection capturedEntries, string categoryName)
    {
        this.capturedEntries = capturedEntries;
        this.categoryName = categoryName;
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

        capturedEntries.Add(entry);
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

namespace PigeonHorde.Logging;

/// <summary>
/// FileLoggerProvider
/// </summary>
public class FileLoggerProvider : ILoggerProvider
{
    private readonly FileLoggerOutput _loggerOutput;

    /// <summary>
    /// FileLoggerProvider constructor
    /// </summary>
    public FileLoggerProvider(FileLoggerOutput loggerOutput)
    {
        _loggerOutput = loggerOutput;
    }

    /// <summary>
    /// Create FileLogger instance
    /// </summary>
    /// <param name="categoryName"></param>
    /// <returns></returns>
    public ILogger CreateLogger(string categoryName) => new FileLogger(categoryName, _loggerOutput);

    /// <summary>
    /// Dispose FileLoggerProvider
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    public void Dispose() { }

    private class FileLogger(string categoryName, FileLoggerOutput loggerOutput) : ILogger
    {
        public IDisposable BeginScope<TState>(TState state) => null!;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception exception,
            Func<TState, Exception, string> formatter) => loggerOutput.Log(logLevel, eventId, state, exception, formatter, categoryName);
    }
}
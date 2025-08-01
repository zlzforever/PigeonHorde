using System.Text;

namespace PigeonHorde.Logging;

/// <summary>
/// Output to file for logging
/// </summary>
public class FileLoggerOutput : IDisposable
{
    private DatedWriter _streamWriter;
    private readonly object _lockObj;
    private string _filename;

    private sealed class DatedWriter(DateTime date, StreamWriter writer) : IDisposable
    {
        public StreamWriter Writer { get; } = writer;
        public DateTime Date { get; } = date;

        public void Dispose()
        {
            Writer.Dispose();
        }
    }

    /// <summary>
    /// Create a file logger output
    /// </summary>
    /// <param name="filename"></param>
    /// <param name="flushInterval"></param>
    public FileLoggerOutput(string filename, int flushInterval = 0)
    {
        _filename = filename;
        _lockObj = new object();
    }

    /// <summary>
    /// Dispose FileLoggerOutput
    /// </summary>
    public void Dispose()
    {
        _streamWriter.Dispose();
    }

    /// <summary>
    /// Logs a message.
    /// </summary>
    /// <typeparam name="TState">The type of <paramref name="state"/>.</typeparam>
    /// <param name="logLevel">The log level.</param>
    /// <param name="eventId">The event identifier.</param>
    /// <param name="state">The state.</param>
    /// <param name="exception">The exception.</param>
    /// <param name="formatter">The formatter.</param>
    /// <param name="categoryName">The category.</param>
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
        Func<TState, Exception, string> formatter, string categoryName)
    {
        var msg = string.Format("[{0:D3}.{1}] ({2}) <{3}> {4}",
            eventId.Id,
            LogFormatter.FormatDate(DateTime.UtcNow),
            logLevel,
            categoryName,
            formatter(state, exception));

        if (exception != null)
        {
            msg += Environment.NewLine + exception;
        }

        lock (_lockObj)
        {
            if (_streamWriter == null)
            {
                _streamWriter = CreateDatedWriter();
            }
            else
            {
                var date = DateTime.Now.Date;
                if (_streamWriter.Date != date)
                {
                    _streamWriter.Dispose();
                    _streamWriter = CreateDatedWriter();
                }
            }

            _streamWriter.Writer.WriteLine(msg);
            _streamWriter.Writer.Flush();
        }
    }

    private DatedWriter CreateDatedWriter()
    {
        var name = Path.GetFileNameWithoutExtension(_filename);
        var date = DateTime.Now.Date;
        var fileName = $"{name}_{date:yyyyMMdd}{Path.GetExtension(_filename)}";
        return new DatedWriter(date, new StreamWriter(
            File.Open(fileName, FileMode.Append, FileAccess.Write, FileShare.ReadWrite),
            Encoding.UTF8));
    }
}
using System.Text;

namespace PigeonHorde.Logging;

/// <summary>
    /// Output to file for logging
    /// </summary>
    public class FileLoggerOutput : IDisposable
    {
        private readonly StreamWriter _streamWriter;
        private readonly object _lockObj;

        /// <summary>
        /// Create a file logger output
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="flushInterval"></param>
        public FileLoggerOutput(string filename, int flushInterval = 0)
        {
            _streamWriter = new StreamWriter(File.Open(filename, FileMode.Append, FileAccess.Write, FileShare.ReadWrite), Encoding.UTF8);
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
                msg += Environment.NewLine + exception.ToString();
            }

            lock (_lockObj)
            {
                _streamWriter.WriteLine(msg);
                _streamWriter.Flush();
            }
        }
    }
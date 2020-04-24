using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Aka.Data.TestUtils
{
    public class StringLogger : ILogger
    {
        public Dictionary<LogLevel, string> Logs = new Dictionary<LogLevel, string>();

        public IDisposable BeginScope<TState>(TState state)
        {
            throw new NotImplementedException();
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var logText = formatter(state, exception);
            if (Logs.TryGetValue(logLevel, out var text))
            {
                Logs[logLevel] = text + logText;
            }
            else
            {
                Logs[logLevel] = logText;
            }

        }
    }
}

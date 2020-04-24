using System;
using DbQueryLogging;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace Demo
{
    public class Program
    {
        const string connectionString = "Data Source=InMemorySample;Mode=Memory;Cache=Shared";

        static void Main(string[] args)
        {
            ILogger consoleLogger = new ConsoleLogger();

            using var connection = new LoggedDbConnection(new SqliteConnection(connectionString), consoleLogger);
            connection.Open();

            var queryCommand = connection.CreateCommand();
            queryCommand.CommandText = "SELECT @textParameter, @intParameter, 'constant sample'";
            var param1 = new SqliteParameter("@textParameter", "SomeText");
            var param2 = new SqliteParameter("@intParameter", 28);
            queryCommand.Parameters.AddRange(new[] { param1, param2 });
            var result = queryCommand.ExecuteScalar();
        }

        public class ConsoleLogger : ILogger
        {
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                Console.WriteLine(formatter(state, exception));
            }

            public IDisposable BeginScope<TState>(TState state) => throw new NotImplementedException();
            public bool IsEnabled(LogLevel logLevel) => true;
        }
    }
}

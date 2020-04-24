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
            queryCommand.CommandText = "SELECT @name, @age";
            queryCommand.Parameters.AddRange(new[] {
                    new SqliteParameter("@name", "Bob"),
                    new SqliteParameter("@age", 28)
                });
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
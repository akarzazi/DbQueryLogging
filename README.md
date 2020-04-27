# DbQueryLogging
A simple wrapper on DbConnection and DbCommand that logs SQL statements into an ILogger instance

![.NET Core](https://github.com/akarzazi/DbQueryLogging/workflows/.NET%20Core/badge.svg)

# Nuget Package
.Net Standard 2.1

https://www.nuget.org/packages/DbQueryLogging/


# Usage
Use as the following:


```csharp
var connection = new LoggedDbConnection(DbConnection, ILogger);
```
Where 

DbConnection : ```System.Data.Common.DbConnection```

ILogger : ```Microsoft.Extensions.Logging.ILogger```

# Demo sample

A sample demo project is also available in the sources at :
https://github.com/akarzazi/DbQueryLogging/tree/master/demo


```csharp
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

```

Output

```
Executing
- CommandType Text
- CommandText SELECT @name, @age
- Parameters
        @name = Bob
        @age = 28
```
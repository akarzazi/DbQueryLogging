using System;
using Aka.Data.TestUtils;

using DbQueryLogging;

using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

using Xunit;

namespace DbQueryLogging.Test.Integration
{
    public class LoggedDbConnectionTest
    {
        const string connectionString = "Data Source=InMemorySample;Mode=Memory;Cache=Shared";

        [Fact]
        public void ExecuteScalar_SimpleCommand_ResultOk_LoggedAsDebug()
        {
            var logSpy = new StringLogger();

            // act
            using var connection = new LoggedDbConnection(new SqliteConnection(connectionString), logSpy);
            connection.Open();

            var queryCommand = connection.CreateCommand();
            queryCommand.CommandText = "SELECT 1";
            var result = (long)queryCommand.ExecuteScalar();

            Assert.Equal(1, result);
            Assert.Contains(queryCommand.CommandText, logSpy.Logs[LogLevel.Debug]);
        }

        [Fact]
        public void ExecuteScalar_CommandWithParameter_ResultOk_LoggedAsDebug()
        {
            var logSpy = new StringLogger();

            // act
            using var connection = new LoggedDbConnection(new SqliteConnection(connectionString), logSpy);
            connection.Open();
            var queryCommand = connection.CreateCommand();
            queryCommand.CommandText = "SELECT @myparameter";

            var param = queryCommand.CreateParameter();
            param.DbType = System.Data.DbType.String;
            param.ParameterName = "@myparameter";
            param.Value = "SomeText";
            queryCommand.Parameters.Add(param);

            var result = (string)queryCommand.ExecuteScalar();

            var debugLogs = logSpy.Logs[LogLevel.Debug];
            Assert.Equal("SomeText", result);
            Assert.Contains(queryCommand.CommandText, debugLogs);
            Assert.Contains("myparameter", debugLogs);
            Assert.Contains("SomeText", debugLogs);
        }

        [Fact]
        public void ExecuteReader_SimpleCommand_ResultOk_LoggedAsDebug()
        {
            var logSpy = new StringLogger();

            // act
            using var connection = new LoggedDbConnection(new SqliteConnection(connectionString), logSpy);

            connection.Open();
            var queryCommand = connection.CreateCommand();
            queryCommand.CommandText = @"
SELECT 1
UNION 
SELECT 2";

            using var reader = queryCommand.ExecuteReader();

            Assert.True(reader.Read());
            Assert.Equal(1, reader.GetInt64(0));
            Assert.True(reader.Read());
            Assert.Equal(2, reader.GetInt64(0));
            Assert.False(reader.Read());

            Assert.Contains(queryCommand.CommandText, logSpy.Logs[LogLevel.Debug]);
        }

        [Fact]
        public void ExecuteNonQuery_SimpleCommand_ResultOk_LoggedAsDebug()
        {
            var logSpy = new StringLogger();

            // act
            using var connection = new LoggedDbConnection(new SqliteConnection(connectionString), logSpy);
            connection.Open();

            var queryCommand = connection.CreateCommand();
            queryCommand.CommandText = "DROP TABLE IF EXISTS People";
            queryCommand.ExecuteNonQuery();

            Assert.Contains(queryCommand.CommandText, logSpy.Logs[LogLevel.Debug]);
        }

        [Fact]
        public void ExecuteScalar_SyntaxError_LoggedAsError()
        {
            var logSpy = new StringLogger();

            // act
            using var connection = new LoggedDbConnection(new SqliteConnection(connectionString), logSpy);

            connection.Open();
            var queryCommand = connection.CreateCommand();
            queryCommand.CommandText = "SELEC_error 1";

            try
            {
                var result = (long)queryCommand.ExecuteScalar();
            }
            catch (Exception ex)
            {
                Assert.Contains("syntax error", ex.Message);
            }

            Assert.Contains(queryCommand.CommandText, logSpy.Logs[LogLevel.Debug]);
            Assert.Contains(queryCommand.CommandText, logSpy.Logs[LogLevel.Error]);
        }

        [Fact]
        public void Ensure_Passthrough_Properties()
        {
            var innerCnx = new SqliteConnection(connectionString);
            var loggedCnx = new LoggedDbConnection(innerCnx, new StringLogger());

            Assert.Equal(loggedCnx.ServerVersion, innerCnx.ServerVersion);
            Assert.Equal(loggedCnx.Database, innerCnx.Database);
            Assert.Equal(loggedCnx.State, innerCnx.State);
            Assert.Equal(loggedCnx.DataSource, innerCnx.DataSource);
            Assert.Equal(loggedCnx.Database, innerCnx.Database);
            Assert.Equal(loggedCnx.ConnectionString, innerCnx.ConnectionString);

            loggedCnx.ConnectionString = "Data Source = InMemorySample2; Mode = Memory;";
            Assert.Equal(loggedCnx.ConnectionString, innerCnx.ConnectionString);
        }

        [Fact]
        public void CommandTimeout_IsPassedToInnerCommand()
        {
            var cmdTimeout = 90;

            // act
            using var connection = new LoggedDbConnection(new SqliteConnection(connectionString), new StringLogger(), cmdTimeout);
            var queryCommand = connection.CreateCommand();      

            Assert.Equal(cmdTimeout, queryCommand.CommandTimeout);
        }
    }
}

using System;
using System.Data;
using Aka.Data.TestUtils;

using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

using Xunit;

namespace DbQueryLogging.Test.Integration
{
    public class LoggedDbCommandTest
    {
        const string connectionString = "Data Source=InMemorySample;Mode=Memory;Cache=Shared";

        [Fact]
        public void ExecuteScalar_SimpleCommand_ResultOk_LoggedAsDebug()
        {
            var logSpy = new StringLogger();
            using var connection = new SqliteConnection(connectionString);

            connection.Open();
            var queryCommand = connection.CreateCommand();
            queryCommand.CommandText = "SELECT 1";

            // act
            var loggedCmd = new LoggedDbCommand(queryCommand, logSpy);
            var result = (long)loggedCmd.ExecuteScalar();

            Assert.Equal(1, result);
            Assert.Contains(queryCommand.CommandText, logSpy.Logs[LogLevel.Debug]);
        }

        [Fact]
        public void ExecuteScalar_CommandWithParameter_ResultOk_LoggedAsDebug()
        {
            var logSpy = new StringLogger();
            using var connection = new SqliteConnection(connectionString);

            connection.Open();
            var queryCommand = connection.CreateCommand();
            queryCommand.CommandText = "SELECT @myparameter";
            var param = queryCommand.CreateParameter();
            param.SqliteType = SqliteType.Text;
            param.ParameterName = "@myparameter";
            param.Value = "SomeText";
            queryCommand.Parameters.Add(param);

            // act
            var loggedCmd = new LoggedDbCommand(queryCommand, logSpy);
            var result = (string)loggedCmd.ExecuteScalar();

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
            using var connection = new SqliteConnection(connectionString);

            connection.Open();
            var queryCommand = connection.CreateCommand();
            queryCommand.CommandText = @"
SELECT 1
UNION 
SELECT 2";

            // act
            var loggedCmd = new LoggedDbCommand(queryCommand, logSpy);
            using var reader = loggedCmd.ExecuteReader();

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

            using var connection = new SqliteConnection(connectionString);
            connection.Open();
            var queryCommand = connection.CreateCommand();
            queryCommand.CommandText = "DROP TABLE IF EXISTS People";

            // act
            var loggedCmd = new LoggedDbCommand(queryCommand, logSpy);
            loggedCmd.ExecuteNonQuery();

            Assert.Contains(queryCommand.CommandText, logSpy.Logs[LogLevel.Debug]);
        }

        [Fact]
        public void ExecuteScalar_SyntaxError_LoggedAsError()
        {
            var logSpy = new StringLogger();
            using var connection = new SqliteConnection(connectionString);

            connection.Open();
            var queryCommand = connection.CreateCommand();
            queryCommand.CommandText = "SELEC_error 1";

            // act
            var loggedCmd = new LoggedDbCommand(queryCommand, logSpy);

            try
            {
                var result = (long)loggedCmd.ExecuteScalar();
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
            var innerCommand = new SqliteCommand();
            var loggedCmd = new LoggedDbCommand(innerCommand, new StringLogger());

            loggedCmd.CommandText = "abc";
            Assert.Equal(loggedCmd.CommandText, innerCommand.CommandText);

            loggedCmd.CommandTimeout = 75;
            Assert.Equal(loggedCmd.CommandTimeout, innerCommand.CommandTimeout);

            loggedCmd.CommandType = CommandType.Text;
            Assert.Equal(loggedCmd.CommandType, innerCommand.CommandType);

            loggedCmd.UpdatedRowSource = UpdateRowSource.OutputParameters;
            Assert.Equal(loggedCmd.UpdatedRowSource, innerCommand.UpdatedRowSource);

            loggedCmd.DesignTimeVisible = !loggedCmd.DesignTimeVisible;
            Assert.Equal(loggedCmd.DesignTimeVisible, innerCommand.DesignTimeVisible);

            Assert.Equal(loggedCmd.Parameters, innerCommand.Parameters);

            using var cnx = new SqliteConnection(connectionString);
            cnx.Open();
            loggedCmd.Transaction = cnx.BeginTransaction();
            Assert.Equal(loggedCmd.Transaction, innerCommand.Transaction);
        }
    }
}

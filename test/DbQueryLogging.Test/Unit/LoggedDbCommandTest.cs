using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

using Moq;
using Moq.Protected;
using Xunit;

namespace DbQueryLogging.Test.Unit
{
    public class LoggedDbCommandTest
    {
        [Fact]
        public void Ensure_Passthrough_Properties_And_Methods_Called()
        {
            var logger = new Mock<ILogger>();
            var dbCmdSpy = new Mock<DbCommand>() { CallBase = true, DefaultValue = DefaultValue.Mock };
            dbCmdSpy.SetupAllProperties();
            var loggedCmd = new LoggedDbCommand(dbCmdSpy.Object, logger.Object);

            loggedCmd.Cancel();
            dbCmdSpy.Verify(p => p.Cancel(), Times.Once);

            loggedCmd.Prepare();
            dbCmdSpy.Verify(p => p.Prepare(), Times.Once);

            loggedCmd.ExecuteNonQuery();
            dbCmdSpy.Verify(p => p.ExecuteNonQuery(), Times.Once);

            loggedCmd.ExecuteScalar();
            dbCmdSpy.Verify(p => p.ExecuteScalar(), Times.Once);

            loggedCmd.CommandText = "abc";
            dbCmdSpy.VerifySet(p => p.CommandText = "abc", Times.Once);
            Assert.Equal("abc", loggedCmd.CommandText);

            loggedCmd.CommandTimeout = 75;
            dbCmdSpy.VerifySet(p => p.CommandTimeout = 75, Times.Once);
            Assert.Equal(75, loggedCmd.CommandTimeout);

            loggedCmd.CommandType = CommandType.TableDirect;
            dbCmdSpy.VerifySet(p => p.CommandType = CommandType.TableDirect, Times.Once);
            Assert.Equal(CommandType.TableDirect, loggedCmd.CommandType);

            loggedCmd.UpdatedRowSource = UpdateRowSource.OutputParameters;
            dbCmdSpy.VerifySet(p => p.UpdatedRowSource = UpdateRowSource.OutputParameters, Times.Once);
            Assert.Equal(UpdateRowSource.OutputParameters, loggedCmd.UpdatedRowSource);

            loggedCmd.DesignTimeVisible = true;
            dbCmdSpy.VerifySet(p => p.DesignTimeVisible = true, Times.Once);
            Assert.True(loggedCmd.DesignTimeVisible);

            var protectedMock = dbCmdSpy.Protected().As<DbCommandProtectedMembers>();

            var expectedDbParam = new Mock<DbParameter>().Object;
            protectedMock.Setup(p => p.CreateDbParameter()).Returns(expectedDbParam);
            var resultedDbParam = loggedCmd.CreateParameter();
            Assert.Equal(expectedDbParam, resultedDbParam);
            protectedMock.Verify(p => p.CreateDbParameter(), Times.Once());
        }
    }

    interface DbCommandProtectedMembers
    {
        DbParameter CreateDbParameter();
        DbTransaction DbTransaction { get; set; }
        DbParameterCollection DbParameterCollection { get; }
    }
}

using System;
using System.Data;
using System.Data.Common;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Xunit;

namespace DbQueryLogging.Test.Unit
{
    public class LoggedDbConnectionTest
    {
        [Fact]
        public void Ensure_Passthrough_Properties_And_Methods_Called()
        {
            var logger = new Mock<ILogger>();
            var dbCmdSpy = new Mock<DbConnection>() { CallBase = true, DefaultValue = DefaultValue.Mock };
            var loggedCnx = new LoggedDbConnection(dbCmdSpy.Object, logger.Object);

            loggedCnx.Open();
            dbCmdSpy.Verify(p => p.Open(), Times.Once);

            loggedCnx.Close();
            dbCmdSpy.Verify(p => p.Close(), Times.Once);

            loggedCnx.ChangeDatabase("db");
            dbCmdSpy.Verify(p => p.ChangeDatabase("db"), Times.Once);

            loggedCnx.ConnectionString = "abc";
            dbCmdSpy.VerifySet(p => p.ConnectionString = "abc", Times.Once);

            _ = loggedCnx.Database;
            dbCmdSpy.VerifyGet(p => p.Database, Times.Once);

            _ = loggedCnx.State;
            dbCmdSpy.VerifyGet(p => p.State, Times.Once);

            _ = loggedCnx.DataSource;
            dbCmdSpy.VerifyGet(p => p.DataSource, Times.Once);

            _ = loggedCnx.ServerVersion;
            dbCmdSpy.VerifyGet(p => p.ServerVersion, Times.Once);

            var protectedMock = dbCmdSpy.Protected().As<DbConnectionProtectedMembers>();

            loggedCnx.BeginTransaction();
            protectedMock.Verify(p => p.BeginDbTransaction(IsolationLevel.Unspecified), Times.Once());

            loggedCnx.CreateCommand();
            protectedMock.Verify(p => p.CreateDbCommand(), Times.Once());
        }
    }

    interface DbConnectionProtectedMembers
    {
        DbTransaction BeginDbTransaction(IsolationLevel isolationLevel);
        DbCommand CreateDbCommand();
    }
}

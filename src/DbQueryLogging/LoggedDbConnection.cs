using Microsoft.Extensions.Logging;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace DbQueryLogging
{
    public class LoggedDbConnection : DbConnection
    {
        public DbConnection Inner { get; }
        public int? CommandTimeout { get; set; }

        private readonly ILogger _logger;

        public LoggedDbConnection(DbConnection inner, ILogger logger)
        {
            Inner = inner;
            _logger = logger;
        }

        public LoggedDbConnection(DbConnection inner, ILogger logger, int commandTimeout)
            : this(inner, logger)
        {
            CommandTimeout = commandTimeout;
        }

        protected override DbCommand CreateDbCommand()
        {
            var cmd = Inner.CreateCommand();
            if (CommandTimeout.HasValue)
                cmd.CommandTimeout = CommandTimeout.Value;

            return new LoggedDbCommand(cmd, _logger);
        }

        protected override DbTransaction BeginDbTransaction(System.Data.IsolationLevel isolationLevel)
        {
            return Inner.BeginTransaction(isolationLevel);
        }

        public override void Close() => Inner.Close();

        public override void Open() => Inner.Open();

        public override string ConnectionString
        {
            get => Inner.ConnectionString;
            set => Inner.ConnectionString = value;
        }

        public override string Database => Inner.Database;

        public override ConnectionState State => Inner.State;

        public override string DataSource => Inner.DataSource;

        public override string ServerVersion => Inner.ServerVersion;

        public override void ChangeDatabase(string databaseName)
        {
            Inner.ChangeDatabase(databaseName);
        }        
    }
}

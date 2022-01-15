using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Salix.Dapper.Cqrs.Abstractions;
using Xunit;
using Xunit.Abstractions;

namespace Salix.Dapper.Cqrs.MsSql.Tests
{
    [ExcludeFromCodeCoverage]
    public sealed class DatabaseContextIntegrationTests : IClassFixture<ChinookLightTestsFixture>, IDisposable
    {
        private DatabaseContext _sut;
        private readonly XUnitLogger<DatabaseContext> _log;
        private readonly ITestOutputHelper _output;

        public DatabaseContextIntegrationTests(ITestOutputHelper output)
        {
            _log = new XUnitLogger<DatabaseContext>(output);
            _sut = new DatabaseContext(ChinookLightTestsFixture.SqlConnectionString, _log);
            _output = output;
        }

        public void Dispose() => _sut.Dispose();

        [Fact]
        public void Constructor_Connection_NotYetCreated()
        {
            object conn = ChinookLightTestsFixture.GetInternalConnection(_sut);
            conn.Should().BeNull();
        }

        [Fact]
        public void Constructor_Transaction_NotYetCreated()
        {
            object tran = _sut.Transaction;
            tran.Should().BeNull();
        }

        [Fact]
        public void Constructor_ConnectionString_Stored()
        {
            var connStr = _sut.ConnectionString;
            connStr.Should().NotBeNullOrEmpty();
            connStr.Should().Be(ChinookLightTestsFixture.SqlConnectionString);
        }

        [Fact]
        public void ToString_Used_ShowsConnectionInfo()
        {
            var connHash = _sut.Connection.GetHashCode();
            var tranHash = _sut.Transaction.GetHashCode();
            _ = _sut.ToString().Should().Be($"SqlConnection: {connHash} (OPEN) to ChinookLight on (localdb)\\MSSQLLocalDB; Transaction: {tranHash}; ");
        }

        [Fact]
        public void Using_Disposed_ReopensConnection()
        {
            // This is wrong usage of connection, but bad developers can do weird things. so this functionality test.
            var initialConnectionHash = _sut.Connection.GetHashCode();
            using (var conn = _sut.Connection)
            {
                // No-op
            } // Connection gets disposed here

            ((SqlConnection)ChinookLightTestsFixture.GetInternalConnection(_sut))
                .State.Should().Be(ConnectionState.Closed);

            var sameConnectionHash = _sut.Connection.GetHashCode(); // Should reopen connection as EnsureOpenConnection() is called by prop getter.
            initialConnectionHash.Should().Be(sameConnectionHash);
            _sut.Connection.State.Should().Be(ConnectionState.Open);
        }

        [Fact]
        public void ToString_Unused_NotConnected() => _ = _sut.ToString().Should().Be("Connection not open");

        [Fact]
        public void Context_ConnectionAccess_EntireLifecycle()
        {
            var conn = _sut.Connection; //act - forces insides to do lifting all up.

            conn.Should().NotBeNull();
            conn.Should().BeOfType(typeof(SqlConnection));
            conn.State.Should().Be(ConnectionState.Open);
            object tran = _sut.Transaction;
            tran.Should().NotBeNull();
            tran.Should().BeOfType(typeof(SqlTransaction));
            ((SqlTransaction)tran).IsolationLevel.Should().Be(IsolationLevel.ReadCommitted);
            ((SqlTransaction)tran).Connection.Should().BeSameAs(conn);

            _log.LogDebug("--- Calling DatabaseContext.Dispose() explicitly NOW.");
            _sut.Dispose(); // e.g. Ending business operation (by IoC or using(){ } )


            /*
            *****  Checking logging statements for event order *****
            Created new MS SQL connection with Hash: 1432091
            Opening SQL connection (Hash: 1432091).
            Event: MS SQL Connection StateChange (Closed => Open)
            Connection to ChinookLight opened in 136 ms (Hash: 1432091).
            Created new SQL transaction with Hash: 713423
            --- Calling DatabaseContext.Dispose() explicitly NOW.
            SQL Transaction (Hash: 713423) Commit in ReleaseConnection (business operation complete).
            Event: MS SQL Connection StateChange (Open => Closed)
            Connection closed (ReleaseConnection). (Hash: 1432091)
            Event: MS SQL Connection got disposed
            */
            _log.LoggedMessages.Should().HaveCount(10);
            _log.LoggedMessages[0].Should().Contain("Created new MS SQL connection");
            _log.LoggedMessages[1].Should().Contain("Opening SQL connection");
            _log.LoggedMessages[2].Should().Contain("Event: MS SQL Connection StateChange (Closed => Open)");
            _log.LoggedMessages[3].Should().Contain("Connection to ChinookLight opened in");
            _log.LoggedMessages[4].Should().Contain("Created new SQL transaction");
            _log.LoggedMessages[5].Should().Contain("--- Calling DatabaseContext.Dispose() explicitly NOW");
            _log.LoggedMessages[6].Should().Contain("Commit in ReleaseConnection (business operation complete)");
            _log.LoggedMessages[7].Should().Contain("Event: MS SQL Connection StateChange (Open => Closed)");
            _log.LoggedMessages[8].Should().Contain("Connection closed (ReleaseConnection)");
            _log.LoggedMessages[9].Should().Contain("Event: MS SQL Connection got disposed");
        }

        [Fact]
        public void Dispose_DisposedConnection_CheckLog()
        {
            // Should NOT use this in such manner. Transaction is most likely Rollback-ed and may hold other transactions from doing their job (locked).
            // Here it is to ensure it still [kinda] works even if used incorrectly.
            using (var conn = _sut.Connection)
            {
                conn.Should().NotBeNull();
            }

            _log.LogDebug("--- Calling DatabaseContext.Dispose() explicitly NOW.");
            _sut.Dispose();

            /*
            *****  Checking logging statements for event order *****
            Created new MS SQL connection with Hash: 49508310 to (localdb)\MSSQLLocalDB:ChinookLight
            Opening SQL connection (Hash: 49508310).
            Event: MS SQL Connection StateChange (Closed => Open)
            Connection to ChinookLight opened in 122 ms (Hash: 49508310).
            Created new SQL transaction with Hash: 6590935 with Isolation level ReadCommitted onto connection 49508310.
            Event: MS SQL Connection StateChange (Open => Closed)
            Event: MS SQL Connection got disposed // *** by using statement ***
            --- Calling DatabaseContext.Dispose() explicitly NOW.
            SQL Transaction is already completed (Commit or Rollback) in ReleaseConnection.
            SQL Connection is already Closed in ReleaseConnection operation.
            Event: MS SQL Connection got disposed
            */
            _log.LoggedMessages.Count.Should().Be(11);
            _log.LoggedMessages[5].Should().Contain("Event: MS SQL Connection StateChange (Open => Closed)");
            _log.LoggedMessages[6].Should().Contain("Event: MS SQL Connection got disposed");
            _log.LoggedMessages[7].Should().Contain("--- Calling DatabaseContext.Dispose() explicitly NOW");
            _log.LoggedMessages[8].Should().Contain("SQL Transaction has its associated connection closed");
            _log.LoggedMessages[9].Should().Contain("SQL Connection is already Closed in ReleaseConnection operation");
            _log.LoggedMessages[10].Should().Contain("Event: MS SQL Connection got disposed");
        }

        [Fact]
        public void Connection_ReAccess_SameObjects()
        {
            var conn1 = _sut.Connection;
            var tran1 = _sut.Transaction;
            conn1.Should().NotBeNull();
            tran1.Should().NotBeNull();
            var conn2 = _sut.Connection;
            var tran2 = _sut.Transaction;
            conn2.Should().NotBeNull();
            tran2.Should().NotBeNull();

            conn1.Should().BeSameAs(conn2);
            tran1.Should().BeSameAs(tran2);
            _log.LogDebug("--- Calling DatabaseContext.Dispose() explicitly NOW.");
            _sut.Dispose();

            /*
            *****  Checking logging statements for event order *****
            Created new MS SQL connection with Hash: 66502754 to (localdb)\MSSQLLocalDB:ChinookLight
            Opening SQL connection (Hash: 66502754).
            Event: MS SQL Connection StateChange (Closed => Open)
            Connection to ChinookLight opened in 0,402 ms (Hash: 66502754).
            Created new SQL transaction with Hash: 30136159 with Isolation level ReadCommitted onto connection 66502754.
            --- Calling DatabaseContext.Dispose() explicitly NOW.
            SQL Transaction (Hash: 30136159) Commit in ReleaseConnection (business operation complete).
            Event: MS SQL Connection StateChange (Open => Closed)
            Connection closed (ReleaseConnection). (Hash: 66502754)
            Event: MS SQL Connection got disposed
            */
            _log.LoggedMessages.Count.Should().Be(10);
        }

        [Fact]
        public void Context_Query_ReturnsData()
        {
            var artist = _sut.ExecuteSql(transaction => _sut.Connection.QueryFirstOrDefault<string>("SELECT Name FROM Artist WHERE ArtistId = @Id", new { Id = 1 }, transaction));
            _sut.ExecutionTime.Should().NotBe(TimeSpan.MinValue);
            _log.LogDebug("--- Calling DatabaseContext.Dispose() explicitly NOW.");
            _sut.Dispose();

            artist.Should().Be("AC/DC");

            /*
            *****  Checking logging statements for event order *****
            Created new MS SQL connection with Hash: 49508310 to (localdb)\MSSQLLocalDB:ChinookLight
            Opening SQL connection (Hash: 49508310).
            Event: MS SQL Connection StateChange (Closed => Open)
            Connection to ChinookLight opened in 1 sec 7 ms (Hash: 49508310).
            Created new SQL transaction with Hash: 6590935 with Isolation level ReadCommitted onto connection 49508310.
            Attempting to execute SQL statement (ExecuteQuery).
            SQL statement executed in 82 ms.
            --- Calling DatabaseContext.Dispose() explicitly NOW.
            SQL Transaction (Hash: 6590935) Commit in ReleaseConnection (business operation complete).
            Event: MS SQL Connection StateChange (Open => Closed)
            Connection closed (ReleaseConnection). (Hash: 49508310)
            Event: MS SQL Connection got disposed
            */
            _log.LoggedMessages.Count.Should().Be(12);
            _log.LoggedMessages[5].Should().Contain("Attempting to execute SQL statement");
            _log.LoggedMessages[6].Should().Contain("SQL statement executed in");
        }

        [Fact]
        public async Task Context_AsyncQuery_ReturnsData()
        {
            var artist = await _sut.ExecuteSql(transaction => _sut.Connection.QueryFirstOrDefaultAsync<string>("SELECT Name FROM Artist WHERE ArtistId = @Id", new { Id = 1 }, transaction));
            _sut.ExecutionTime.Should().NotBe(TimeSpan.MinValue);
            _log.LogDebug("--- Calling DatabaseContext.Dispose() explicitly NOW.");
            _sut.Dispose();

            artist.Should().Be("AC/DC");

            /*
            *****  Checking logging statements for event order *****
            Created new MS SQL connection with Hash: 49508310 to (localdb)\MSSQLLocalDB:ChinookLight
            Opening SQL connection (Hash: 49508310).
            Event: MS SQL Connection StateChange (Closed => Open)
            Connection to ChinookLight opened in 1 sec 7 ms (Hash: 49508310).
            Created new SQL transaction with Hash: 6590935 with Isolation level ReadCommitted onto connection 49508310.
            Attempting to execute SQL statement (ExecuteQuery).
            SQL statement executed in 82 ms.
            --- Calling DatabaseContext.Dispose() explicitly NOW.
            SQL Transaction (Hash: 6590935) Commit in ReleaseConnection (business operation complete).
            Event: MS SQL Connection StateChange (Open => Closed)
            Connection closed (ReleaseConnection). (Hash: 49508310)
            Event: MS SQL Connection got disposed
            */
            _log.LoggedMessages.Count.Should().Be(12);
            _log.LoggedMessages[5].Should().Contain("Attempting to execute SQL statement");
            _log.LoggedMessages[6].Should().Contain("SQL statement executed in");
        }

        [Fact]
        public void Context_TwoQueries_SameConnection()
        {
            var connBegin = _sut.Connection.GetHashCode();
            var tranBegin = _sut.Transaction.GetHashCode();
            var zeroTime = _sut.ExecutionTime;

            var artist = _sut.ExecuteSql(transaction => _sut.Connection.QueryFirstOrDefault<string>("SELECT Name FROM Artist WHERE ArtistId = @Id", new { Id = 1 }, transaction));
            _sut.Connection.GetHashCode().Should().Be(connBegin);
            _sut.Transaction.GetHashCode().Should().Be(tranBegin);
            var firstTime = _sut.ExecutionTime;
            firstTime.Should().BeGreaterThan(zeroTime);

            var album = _sut.ExecuteSql(transaction => _sut.Connection.QueryFirstOrDefault<string>("SELECT Title FROM Album WHERE AlbumId = @Id", new { Id = 5 }, transaction));
            album.Should().Be("Big Ones");
            _sut.Connection.GetHashCode().Should().Be(connBegin);
            _sut.Transaction.GetHashCode().Should().Be(tranBegin);
            var secondTime = _sut.ExecutionTime;
            secondTime.Should().NotBe(zeroTime);
            secondTime.Should().NotBe(firstTime);

            _log.LogDebug("--- Calling DatabaseContext.Dispose() explicitly NOW.");
            _sut.Dispose();

            /*
            *****  Checking logging statements for event order *****
            Created new MS SQL connection with Hash: 58801125 to (localdb)\MSSQLLocalDB:ChinookLight
            Opening SQL connection (Hash: 58801125).
            Event: MS SQL Connection StateChange (Closed => Open)
            Connection to ChinookLight opened in 90 ms (Hash: 58801125).
            Created new SQL transaction with Hash: 19964545 with Isolation level ReadCommitted onto connection 58801125.
            Attempting to execute SQL statement (ExecuteQuery). // <-- First Query
            SQL statement executed in 66 ms.
            Attempting to execute SQL statement (ExecuteQuery). // <-- Second Query
            SQL statement executed in 2,255 ms.
            --- Calling DatabaseContext.Dispose() explicitly NOW.
            SQL Transaction (Hash: 19964545) Commit in ReleaseConnection (business operation complete).
            Event: MS SQL Connection StateChange (Open => Closed)
            Connection closed (ReleaseConnection). (Hash: 58801125)
            Event: MS SQL Connection got disposed
            */
            _log.LoggedMessages.Count.Should().Be(14);
            _log.LoggedMessages[5].Should().Contain("Attempting to execute SQL statement");
            _log.LoggedMessages[6].Should().Contain("SQL statement executed in");
            _log.LoggedMessages[7].Should().Contain("Attempting to execute SQL statement");
            _log.LoggedMessages[8].Should().Contain("SQL statement executed in");
        }

        [Fact]
        public async Task ContextAsync_TwoQueries_SameConnection()
        {
            var connBegin = _sut.Connection.GetHashCode();
            var tranBegin = _sut.Transaction.GetHashCode();
            var zeroTime = _sut.ExecutionTime;

            var artist = await _sut.ExecuteSql(transaction => _sut.Connection.QueryFirstOrDefaultAsync<string>("SELECT Name FROM Artist WHERE ArtistId = @Id", new { Id = 1 }, transaction));
            _sut.Connection.GetHashCode().Should().Be(connBegin);
            _sut.Transaction.GetHashCode().Should().Be(tranBegin);
            var firstTime = _sut.ExecutionTime;
            firstTime.Should().BeGreaterThan(zeroTime);

            var album = await _sut.ExecuteSql(transaction => _sut.Connection.QueryFirstOrDefaultAsync<string>("SELECT Title FROM Album WHERE AlbumId = @Id", new { Id = 5 }, transaction));
            album.Should().Be("Big Ones");
            _sut.Connection.GetHashCode().Should().Be(connBegin);
            _sut.Transaction.GetHashCode().Should().Be(tranBegin);
            var secondTime = _sut.ExecutionTime;
            secondTime.Should().NotBe(zeroTime);
            secondTime.Should().NotBe(firstTime);

            _log.LogDebug("--- Calling DatabaseContext.Dispose() explicitly NOW.");
            _sut.Dispose();

            /*
            *****  Checking logging statements for event order *****
            Created new MS SQL connection with Hash: 58801125 to (localdb)\MSSQLLocalDB:ChinookLight
            Opening SQL connection (Hash: 58801125).
            Event: MS SQL Connection StateChange (Closed => Open)
            Connection to ChinookLight opened in 90 ms (Hash: 58801125).
            Created new SQL transaction with Hash: 19964545 with Isolation level ReadCommitted onto connection 58801125.
            Attempting to execute SQL statement (ExecuteQuery). // <-- First Query
            SQL statement executed in 66 ms.
            Attempting to execute SQL statement (ExecuteQuery). // <-- Second Query
            SQL statement executed in 2,255 ms.
            --- Calling DatabaseContext.Dispose() explicitly NOW.
            SQL Transaction (Hash: 19964545) Commit in ReleaseConnection (business operation complete).
            Event: MS SQL Connection StateChange (Open => Closed)
            Connection closed (ReleaseConnection). (Hash: 58801125)
            Event: MS SQL Connection got disposed
            */
            _log.LoggedMessages.Count.Should().Be(14);
            _log.LoggedMessages[5].Should().Contain("Attempting to execute SQL statement");
            _log.LoggedMessages[6].Should().Contain("SQL statement executed in");
            _log.LoggedMessages[7].Should().Contain("Attempting to execute SQL statement");
            _log.LoggedMessages[8].Should().Contain("SQL statement executed in");
        }

        [Fact]
        public void Commit_Forced_NewTransaction()
        {
            var artist = _sut.ExecuteSql(transaction => _sut.Connection.QueryFirstOrDefault<string>("SELECT Name FROM Artist WHERE ArtistId = @Id", new { Id = 1 }, transaction));
            var initialTransaction = _sut.Transaction.GetHashCode();
            var initialConn = _sut.Connection.GetHashCode();
            _sut.CommitTransaction();
            var album = _sut.ExecuteSql(transaction => _sut.Connection.QueryFirstOrDefault<string>("SELECT Title FROM Album WHERE AlbumId = @Id", new { Id = 5 }, transaction));
            var newTransaction = _sut.Transaction.GetHashCode();
            var sameConn = _sut.Connection.GetHashCode();

            _log.LogDebug("--- Calling DatabaseContext.Dispose() explicitly NOW.");
            _sut.Dispose();

            artist.Should().Be("AC/DC");
            album.Should().Be("Big Ones");
            sameConn.Should().Be(initialConn);
            newTransaction.Should().NotBe(initialTransaction);

            /*
            *****  Checking logging statements for event order *****
            Created new MS SQL connection with Hash: 49508310 to (localdb)\MSSQLLocalDB:ChinookLight
            Opening SQL connection (Hash: 49508310).
            Event: MS SQL Connection StateChange (Closed => Open)
            Connection to ChinookLight opened in 1 sec 163 ms (Hash: 49508310).
            Created new SQL transaction with Hash: 6590935 with Isolation level ReadCommitted onto connection 49508310.
            Attempting to execute SQL statement (ExecuteQuery).
            SQL statement executed in 72 ms.
            Explicit Transaction Commit (Hash: 6590935).
            Created new SQL transaction with Hash: 42931033 with Isolation level ReadCommitted onto connection 49508310.
            Attempting to execute SQL statement (ExecuteQuery).
            SQL statement executed in 1,109 ms.
            --- Calling DatabaseContext.Dispose() explicitly NOW.
            SQL Transaction (Hash: 42931033) Commit in ReleaseConnection (business operation complete).
            Event: MS SQL Connection StateChange (Open => Closed)
            Connection closed (ReleaseConnection). (Hash: 49508310)
            Event: MS SQL Connection got disposed
            */
            _log.LoggedMessages.Count.Should().Be(16);
            _log.LoggedMessages[7].Should().Contain("Explicit Transaction Commit");
            _log.LoggedMessages[8].Should().Contain("Created new SQL transaction");
        }

        [Fact]
        public async Task CommitAsync_Forced_NewTransaction()
        {
            var artist = await _sut.ExecuteSql(transaction => _sut.Connection.QueryFirstOrDefaultAsync<string>("SELECT Name FROM Artist WHERE ArtistId = @Id", new { Id = 1 }, transaction));
            var initialTransaction = _sut.Transaction.GetHashCode();
            var initialConn = _sut.Connection.GetHashCode();
            _sut.CommitTransaction();
            var album = await _sut.ExecuteSql(transaction => _sut.Connection.QueryFirstOrDefaultAsync<string>("SELECT Title FROM Album WHERE AlbumId = @Id", new { Id = 5 }, transaction));
            var newTransaction = _sut.Transaction.GetHashCode();
            var sameConn = _sut.Connection.GetHashCode();

            _log.LogDebug("--- Calling DatabaseContext.Dispose() explicitly NOW.");
            _sut.Dispose();

            artist.Should().Be("AC/DC");
            album.Should().Be("Big Ones");
            sameConn.Should().Be(initialConn);
            newTransaction.Should().NotBe(initialTransaction);

            /*
            *****  Checking logging statements for event order *****
            Created new MS SQL connection with Hash: 49508310 to (localdb)\MSSQLLocalDB:ChinookLight
            Opening SQL connection (Hash: 49508310).
            Event: MS SQL Connection StateChange (Closed => Open)
            Connection to ChinookLight opened in 1 sec 163 ms (Hash: 49508310).
            Created new SQL transaction with Hash: 6590935 with Isolation level ReadCommitted onto connection 49508310.
            Attempting to execute SQL statement (ExecuteQuery).
            SQL statement executed in 72 ms.
            Explicit Transaction Commit (Hash: 6590935).
            Created new SQL transaction with Hash: 42931033 with Isolation level ReadCommitted onto connection 49508310.
            Attempting to execute SQL statement (ExecuteQuery).
            SQL statement executed in 1,109 ms.
            --- Calling DatabaseContext.Dispose() explicitly NOW.
            SQL Transaction (Hash: 42931033) Commit in ReleaseConnection (business operation complete).
            Event: MS SQL Connection StateChange (Open => Closed)
            Connection closed (ReleaseConnection). (Hash: 49508310)
            Event: MS SQL Connection got disposed
            */
            _log.LoggedMessages.Count.Should().Be(16);
            _log.LoggedMessages[7].Should().Contain("Explicit Transaction Commit");
            _log.LoggedMessages[8].Should().Contain("Created new SQL transaction");
        }

        [Fact]
        public void Rollback_Forced_NewTransaction()
        {
            var artist = _sut.ExecuteSql(transaction => _sut.Connection.QueryFirstOrDefault<string>("SELECT Name FROM Artist WHERE ArtistId = @Id", new { Id = 1 }, transaction));
            var initialTransaction = _sut.Transaction.GetHashCode();
            var initialConn = _sut.Connection.GetHashCode();
            _sut.RollbackTransaction();
            var album = _sut.ExecuteSql(transaction => _sut.Connection.QueryFirstOrDefault<string>("SELECT Title FROM Album WHERE AlbumId = @Id", new { Id = 5 }, transaction));
            var newTransaction = _sut.Transaction.GetHashCode();
            var sameConn = _sut.Connection.GetHashCode();

            _log.LogDebug("--- Calling DatabaseContext.Dispose() explicitly NOW.");
            _sut.Dispose();

            artist.Should().Be("AC/DC");
            album.Should().Be("Big Ones");
            sameConn.Should().Be(initialConn);
            newTransaction.Should().NotBe(initialTransaction);

            /*
            *****  Checking logging statements for event order *****
            Created new MS SQL connection with Hash: 49508310 to (localdb)\MSSQLLocalDB:ChinookLight
            Opening SQL connection (Hash: 49508310).
            Event: MS SQL Connection StateChange (Closed => Open)
            Connection to ChinookLight opened in 1 sec 163 ms (Hash: 49508310).
            Created new SQL transaction with Hash: 6590935 with Isolation level ReadCommitted onto connection 49508310.
            Attempting to execute SQL statement (ExecuteQuery).
            SQL statement executed in 72 ms.
            Explicit Transaction Rollback (Hash: 6590935).
            Created new SQL transaction with Hash: 42931033 with Isolation level ReadCommitted onto connection 49508310.
            Attempting to execute SQL statement (ExecuteQuery).
            SQL statement executed in 1,109 ms.
            --- Calling DatabaseContext.Dispose() explicitly NOW.
            SQL Transaction (Hash: 42931033) Commit in ReleaseConnection (business operation complete).
            Event: MS SQL Connection StateChange (Open => Closed)
            Connection closed (ReleaseConnection). (Hash: 49508310)
            Event: MS SQL Connection got disposed
            */
            _log.LoggedMessages.Count.Should().Be(16);
            _log.LoggedMessages[7].Should().Contain("Explicit Transaction Rollback");
            _log.LoggedMessages[8].Should().Contain("Created new SQL transaction");
        }

        [Fact]
        public async Task RollbackAsync_Forced_NewTransaction()
        {
            var artist = await _sut.ExecuteSql(transaction => _sut.Connection.QueryFirstOrDefaultAsync<string>("SELECT Name FROM Artist WHERE ArtistId = @Id", new { Id = 1 }, transaction));
            var initialTransaction = _sut.Transaction.GetHashCode();
            var initialConn = _sut.Connection.GetHashCode();
            _sut.RollbackTransaction();
            var album = await _sut.ExecuteSql(transaction => _sut.Connection.QueryFirstOrDefaultAsync<string>("SELECT Title FROM Album WHERE AlbumId = @Id", new { Id = 5 }, transaction));
            var newTransaction = _sut.Transaction.GetHashCode();
            var sameConn = _sut.Connection.GetHashCode();

            _log.LogDebug("--- Calling DatabaseContext.Dispose() explicitly NOW.");
            _sut.Dispose();

            artist.Should().Be("AC/DC");
            album.Should().Be("Big Ones");
            sameConn.Should().Be(initialConn);
            newTransaction.Should().NotBe(initialTransaction);

            /*
            *****  Checking logging statements for event order *****
            Created new MS SQL connection with Hash: 49508310 to (localdb)\MSSQLLocalDB:ChinookLight
            Opening SQL connection (Hash: 49508310).
            Event: MS SQL Connection StateChange (Closed => Open)
            Connection to ChinookLight opened in 1 sec 163 ms (Hash: 49508310).
            Created new SQL transaction with Hash: 6590935 with Isolation level ReadCommitted onto connection 49508310.
            Attempting to execute SQL statement (ExecuteQuery).
            SQL statement executed in 72 ms.
            Explicit Transaction Rollback (Hash: 6590935).
            Created new SQL transaction with Hash: 42931033 with Isolation level ReadCommitted onto connection 49508310.
            Attempting to execute SQL statement (ExecuteQuery).
            SQL statement executed in 1,109 ms.
            --- Calling DatabaseContext.Dispose() explicitly NOW.
            SQL Transaction (Hash: 42931033) Commit in ReleaseConnection (business operation complete).
            Event: MS SQL Connection StateChange (Open => Closed)
            Connection closed (ReleaseConnection). (Hash: 49508310)
            Event: MS SQL Connection got disposed
            */
            _log.LoggedMessages.Count.Should().Be(16);
            _log.LoggedMessages[7].Should().Contain("Explicit Transaction Rollback");
            _log.LoggedMessages[8].Should().Contain("Created new SQL transaction");
        }

        [Fact]
        public void SqlError_Rollback_NewTransaction()
        {
            var initialConn = _sut.Connection.GetHashCode();
            var initialTransaction = _sut.Transaction.GetHashCode();

            // ACT - Wrong select
            SqlException exc = null;
            try
            {
                var artist = _sut.ExecuteSql(transaction => _sut.Connection.QueryFirstOrDefault<string>("SELECT Wow FROM Maw WHERE NoId = @Id", new { Id = 1 }, transaction));
            }
            catch (SqlException ex)
            {
                exc = ex;
            }

            // Just continue with context
            var album = _sut.ExecuteSql(transaction => _sut.Connection.QueryFirstOrDefault<string>("SELECT Title FROM Album WHERE AlbumId = @Id", new { Id = 5 }, transaction));
            var newTransaction = _sut.Transaction.GetHashCode();
            var sameConn = _sut.Connection.GetHashCode();

            _log.LogDebug("--- Calling DatabaseContext.Dispose() explicitly NOW.");
            _sut.Dispose();

            album.Should().Be("Big Ones");
            sameConn.Should().Be(initialConn);
            newTransaction.Should().NotBe(initialTransaction);

            exc.Should().NotBeNull();
            exc.Message.Should().Contain("Invalid");

            /*
            *****  Checking logging statements for event order *****
            Created new MS SQL connection with Hash: 13726014 to (localdb)\MSSQLLocalDB:ChinookLight
            Opening SQL connection (Hash: 13726014).
            Event: MS SQL Connection StateChange (Closed => Open)
            Connection to ChinookLight opened in 117 ms (Hash: 13726014).
            Created new SQL transaction with Hash: 64379541 with Isolation level ReadCommitted onto connection 13726014.
            Attempting to execute SQL statement (ExecuteQuery).
            SQL Transaction Rollback due to exception thrown (ExecuteQuery).
            Created new SQL transaction with Hash: 42253292 with Isolation level ReadCommitted onto connection 13726014.
            Attempting to execute SQL statement (ExecuteQuery).
            SQL statement executed in 15 ms.
            --- Calling DatabaseContext.Dispose() explicitly NOW.
            SQL Transaction (Hash: 42253292) Commit in ReleaseConnection (business operation complete).
            Event: MS SQL Connection StateChange (Open => Closed)
            Connection closed (ReleaseConnection). (Hash: 13726014)
            Event: MS SQL Connection got disposed
            */
            _log.LoggedMessages.Count.Should().Be(15);
            _log.LoggedMessages[6].Should().Contain("SQL Transaction Rollback due to exception thrown");
            _log.LoggedMessages[7].Should().Contain("Created new SQL transaction");
        }

        [Fact]
        public async Task SqlErrorAsync_Rollback_NewTransaction()
        {
            var initialConn = _sut.Connection.GetHashCode();
            var initialTransaction = _sut.Transaction.GetHashCode();

            // ACT - Wrong select
            SqlException exc = null;
            try
            {
                string artist = await _sut.ExecuteSql(transaction => _sut.Connection.QueryFirstOrDefaultAsync<string>("SELECT Wow FROM Maw WHERE NoId = @Id", new { Id = 1 }, transaction));
            }
            catch (SqlException ex)
            {
                exc = ex;
            }

            // Just continue with context
            var album = await _sut.ExecuteSql(transaction => _sut.Connection.QueryFirstOrDefaultAsync<string>("SELECT Title FROM Album WHERE AlbumId = @Id", new { Id = 5 }, transaction));
            var newTransaction = _sut.Transaction.GetHashCode();
            var sameConn = _sut.Connection.GetHashCode();

            _log.LogDebug("--- Calling DatabaseContext.Dispose() explicitly NOW.");
            _sut.Dispose();

            album.Should().Be("Big Ones");
            sameConn.Should().Be(initialConn);
            newTransaction.Should().NotBe(initialTransaction);

            exc.Should().NotBeNull();
            exc.Message.Should().Contain("Invalid");

            /*
            *****  Checking logging statements for event order *****
            Created new MS SQL connection with Hash: 13726014 to (localdb)\MSSQLLocalDB:ChinookLight
            Opening SQL connection (Hash: 13726014).
            Event: MS SQL Connection StateChange (Closed => Open)
            Connection to ChinookLight opened in 117 ms (Hash: 13726014).
            Created new SQL transaction with Hash: 64379541 with Isolation level ReadCommitted onto connection 13726014.
            Attempting to execute SQL statement (ExecuteQuery).
            SQL Transaction Rollback due to exception thrown (ExecuteQuery).
            Created new SQL transaction with Hash: 42253292 with Isolation level ReadCommitted onto connection 13726014.
            Attempting to execute SQL statement (ExecuteQuery).
            SQL statement executed in 15 ms.
            --- Calling DatabaseContext.Dispose() explicitly NOW.
            SQL Transaction (Hash: 42253292) Commit in ReleaseConnection (business operation complete).
            Event: MS SQL Connection StateChange (Open => Closed)
            Connection closed (ReleaseConnection). (Hash: 13726014)
            Event: MS SQL Connection got disposed
            */
            _log.LoggedMessages.Count.Should().Be(15);
            _log.LoggedMessages[6].Should().Contain("SQL Transaction Rollback due to exception thrown");
            _log.LoggedMessages[7].Should().Contain("Created new SQL transaction");
        }

        [Fact]
        public void InsertError_Rollback_NotAdded()
        {
            var initialConn = _sut.Connection.GetHashCode();
            var initialTransaction = _sut.Transaction.GetHashCode();

            // ACT - Wrong insert
            int rowsAffected1 = 0;
            SqlException exc = null;
            try
            {
                rowsAffected1 = _sut.ExecuteSql(transaction => _sut.Connection.Execute("INSERT INTO Artist (FirstName, LastName) VALUES ('Freddie', 'Mercury')", null, transaction));
            }
            catch (SqlException ex)
            {
                exc = ex;
            }

            // Just continue with context
            var rowsAffected2 = _sut.ExecuteSql(transaction => _sut.Connection.Execute("INSERT INTO Artist (Name) VALUES ('Peter Gabriel')", null, transaction));
            var newTransaction = _sut.Transaction.GetHashCode();
            var sameConn = _sut.Connection.GetHashCode();
            rowsAffected1.Should().Be(0);
            rowsAffected2.Should().Be(1);
            sameConn.Should().Be(initialConn);
            newTransaction.Should().NotBe(initialTransaction);

            _log.LogDebug("--- Calling DatabaseContext.Dispose() explicitly NOW.");
            _sut.Dispose();

            // Creating a new Context (with same logger - accumulate both)
            _sut = new DatabaseContext(ChinookLightTestsFixture.SqlConnectionString, _log);
            var artists = _sut.ExecuteSql(transaction => _sut.Connection.Query<string>("SELECT Name FROM Artist", null, transaction));
            var secondConn = _sut.Connection.GetHashCode();
            var secondTransaction = _sut.Transaction.GetHashCode();

            secondConn.Should().NotBe(initialConn);
            secondTransaction.Should().NotBe(initialTransaction);
            secondTransaction.Should().NotBe(newTransaction);
            artists.Should().NotBeNullOrEmpty();
            artists.Should().HaveCount(5);
            artists.Should().Contain("Peter Gabriel");
            _log.LogDebug("--- Calling DatabaseContext.Dispose() explicitly AGAIN.");
            _sut.Dispose();

            /*
            *****  Checking logging statements for event order *****
            Created new MS SQL connection with Hash: 1432091 to (localdb)\MSSQLLocalDB:ChinookLight
            Opening SQL connection (Hash: 1432091).
            Event: MS SQL Connection StateChange (Closed => Open)
            Connection to ChinookLight opened in 1 sec 61 ms (Hash: 1432091).
            Created new SQL transaction with Hash: 713423 with Isolation level ReadCommitted onto connection 1432091.
            Attempting to execute SQL statement (ExecuteSql).
            SQL Transaction Rollback due to exception thrown in DatabaseContext.ExecuteSql. Invalid column name 'FirstName'.
            Invalid column name 'LastName'.
            Created new SQL transaction with Hash: 561929 with Isolation level ReadCommitted onto connection 1432091.
            Attempting to execute SQL statement (ExecuteSql).
            SQL statement executed in 3,627 ms.
            --- Calling DatabaseContext.Dispose() explicitly NOW.
            SQL Transaction (Hash: 561929) Commit in ReleaseConnection (business operation complete).
            Event: MS SQL Connection StateChange (Open => Closed)
            Connection closed (ReleaseConnection). (Hash: 1432091)
            Event: MS SQL Connection got disposed
            ***********************
            Created new MS SQL connection with Hash: 45516262 to (localdb)\MSSQLLocalDB:ChinookLight
            Opening SQL connection (Hash: 45516262).
            Event: MS SQL Connection StateChange (Closed => Open)
            Connection to ChinookLight opened in 1,082 ms (Hash: 45516262).
            Created new SQL transaction with Hash: 62938641 with Isolation level ReadCommitted onto connection 45516262.
            Attempting to execute SQL statement (ExecuteSql).
            SQL statement executed in 29 ms.
            --- Calling DatabaseContext.Dispose() explicitly AGAIN.
            SQL Transaction (Hash: 62938641) Commit in ReleaseConnection (business operation complete).
            Event: MS SQL Connection StateChange (Open => Closed)
            Connection closed (ReleaseConnection). (Hash: 45516262)
            Event: MS SQL Connection got disposed
            */
            _log.LoggedMessages.Count.Should().Be(27);
            _log.LoggedMessages[6].Should().Contain("SQL Transaction Rollback due to exception thrown");
            _log.LoggedMessages[7].Should().Contain("Created new SQL transaction");
            _log.LoggedMessages[8].Should().Contain("Attempting to execute SQL statement");
            _log.LoggedMessages[9].Should().Contain("SQL statement executed");
        }

        [Fact]
        public async Task InsertErrorAsync_Rollback_NotAdded()
        {
            var initialConn = _sut.Connection.GetHashCode();
            var initialTransaction = _sut.Transaction.GetHashCode();

            // ACT - Wrong insert
            int rowsAffected1 = 0;
            SqlException exc = null;
            try
            {
                rowsAffected1 = await _sut.ExecuteSql(transaction => _sut.Connection.ExecuteAsync("INSERT INTO Album (Title, ArtistId) VALUES ('No Artist', 999)", null, transaction));
            }
            catch (SqlException ex)
            {
                exc = ex;
            }

            // Just continue with context
            var rowsAffected2 = await _sut.ExecuteSql(transaction => _sut.Connection.ExecuteAsync("INSERT INTO Album (Title, ArtistId) VALUES ('Swimsuit', 1)", null, transaction));
            var newTransaction = _sut.Transaction.GetHashCode();
            var sameConn = _sut.Connection.GetHashCode();
            rowsAffected1.Should().Be(0);
            rowsAffected2.Should().Be(1);
            sameConn.Should().Be(initialConn);
            newTransaction.Should().NotBe(initialTransaction);

            _log.LogDebug("--- Calling DatabaseContext.Dispose() explicitly NOW.");
            _sut.Dispose();

            // Creating a new Context (with same logger - accumulate both)
            _sut = new DatabaseContext(ChinookLightTestsFixture.SqlConnectionString, _log);
            var albums = await _sut.ExecuteSql(transaction => _sut.Connection.QueryAsync<string>("SELECT Title FROM Album WHERE ArtistId = 1", null, transaction));
            var secondConn = _sut.Connection.GetHashCode();
            var secondTransaction = _sut.Transaction.GetHashCode();

            secondConn.Should().NotBe(initialConn);
            secondTransaction.Should().NotBe(initialTransaction);
            secondTransaction.Should().NotBe(newTransaction);
            albums.Should().NotBeNullOrEmpty();
            albums.Should().HaveCount(3);
            albums.Should().NotContain("No Artist");
            albums.Should().Contain("Swimsuit");
            _log.LogDebug("--- Calling DatabaseContext.Dispose() explicitly AGAIN.");
            _sut.Dispose();

            /*
            *****  Checking logging statements for event order *****
            */
            _log.LoggedMessages.Count.Should().Be(27);
            _log.LoggedMessages[6].Should().Contain("SQL Transaction Rollback due to exception thrown");
            _log.LoggedMessages[7].Should().Contain("Created new SQL transaction");
            _log.LoggedMessages[8].Should().Contain("Attempting to execute SQL statement");
            _log.LoggedMessages[9].Should().Contain("SQL statement executed");
        }

        [Fact]
        public void InfoEvent_Procedure_Logged()
        {
            var artist = _sut.ExecuteSql(transaction => _sut.Connection.Execute("EXEC [dbo].[InfoEventEmitter]", null, transaction));
            _log.LoggedMessages.Count.Should().Be(10);
            _log.LoggedMessages[6].Should().Contain("Event: InfoEvent");
            _log.LoggedMessages[7].Should().Contain("Received message \"Information text\"");
            _log.LoggedMessages[8].Should().Contain("Received message \"This is PRINT event\"");
        }

        [Fact]
        public async Task QueryAsync_CancellationToken_GetsCancelled()
        {
            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(3));
            var sessionLogger = new XUnitLogger<SqlDatabaseSession>(_output);
            var sess = new SqlDatabaseSession(_sut, sessionLogger);
            var cqrs = new CommandQueryContext(sess);

            var resultTask = cqrs.QueryAsync(new LongQuery(), cancellationTokenSource.Token);
            try
            {
                if (!resultTask.Wait(TimeSpan.FromSeconds(5)))
                {
                    throw new TimeoutException("Async Query Cancellation did not happen."); // should have cancelled
                }
            }
            catch (AggregateException agg)
            {
                Assert.True(agg.InnerException.GetType().Name == "SqlException");
            }

            _sut.Dispose();
        }

        [Fact]
        public async Task QueryAsync_NoCancellationToken_Waits()
        {
            var sessionLogger = new XUnitLogger<SqlDatabaseSession>(_output);
            var sess = new SqlDatabaseSession(_sut, sessionLogger);
            var cqrs = new CommandQueryContext(sess);

            var result = await cqrs.QueryAsync(new LongQuery());
            result.Should().Be(1);
            _sut.ExecutionTime.Should().BeGreaterThan(TimeSpan.FromSeconds(9));
            _sut.Dispose();
        }
    }


    [ExcludeFromCodeCoverage]
    public sealed class LongQuery : MsSqlQuerySingleBase<int?>
    {
        public override string SqlStatement => "WAITFOR DELAY '00:00:10';SELECT 1";
    }
}

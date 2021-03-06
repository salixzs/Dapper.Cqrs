using System;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Salix.Dapper.Cqrs.MsSql.Tests
{
    [ExcludeFromCodeCoverage]
    public sealed class ChinookLightTestsFixture : IDisposable
    {
        private const string ConnectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog={dbname};Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;";
        private readonly IMessageSink _messageSink;

        public static string SqlConnectionString => ConnectionString.Replace("{dbname}", "ChinookLight");

        public static object GetInternalConnection(object instance) => GetInstanceField(typeof(DatabaseContext), instance, "_connection");

        /// <summary>
        /// Uses reflection to get the field value from an object.
        /// </summary>
        /// <param name="type">The instance type.</param>
        /// <param name="instance">The instance object.</param>
        /// <param name="fieldName">The field's name which is to be fetched.</param>
        /// <returns>The field value from the object.</returns>
        internal static object GetInstanceField(Type type, object instance, string fieldName)
        {
            BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                | BindingFlags.Static;
            FieldInfo field = type.GetField(fieldName, bindFlags);
            return field.GetValue(instance);
        }

        public ChinookLightTestsFixture(IMessageSink messageSink)
        {
            _messageSink = messageSink;
            _messageSink.OnMessage(new DiagnosticMessage("ChinookLightTestsFixture initialization (should happen once per all database tests)."));
            this.InitializeTestDatabase();
        }

        private void InitializeTestDatabase()
        {
            if (!this.CreateDatabase())
            {
#pragma warning disable CA2201 // No need to create specific exception type here.
                throw new Exception("Could not create test database on LocalDB (SQL Express).");
#pragma warning restore CA2201
            }
            this.CreateDatabaseSchema();
            this.PopulateDatabaseData();
        }

        private bool CreateDatabase()
        {
            try
            {
                using var connection = new SqlConnection(ConnectionString.Replace("{dbname}", "master"));
                connection.Open();
                var alreadyExists = false;
                using (var command = new SqlCommand("SELECT db_id('ChinookLight')", connection))
                {
                    if (command.ExecuteScalar() != DBNull.Value)
                    {
                        _messageSink.OnMessage(new DiagnosticMessage("Database exists."));
                        alreadyExists = true;
                    }
                }

                if (alreadyExists)
                {
                    using var command = new SqlCommand("DROP DATABASE ChinookLight", connection);
                    _messageSink.OnMessage(new DiagnosticMessage("Deleting existing database."));
                    command.ExecuteNonQuery();
                }

                using (var command = new SqlCommand("CREATE DATABASE ChinookLight", connection))
                {
                    _messageSink.OnMessage(new DiagnosticMessage("Creating clean database."));
                    command.ExecuteNonQuery();
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        private void CreateDatabaseSchema()
        {
            using var connection = new SqlConnection(ConnectionString.Replace("{dbname}", "ChinookLight"));
            connection.Open();
            using (var command = new SqlCommand(@"CREATE TABLE [dbo].[Album]
(
    [AlbumId] INT NOT NULL IDENTITY,
    [Title] NVARCHAR(160) NOT NULL,
    [ArtistId] INT NOT NULL,
    CONSTRAINT[PK_Album] PRIMARY KEY CLUSTERED([AlbumId])
)", connection))
            {
                _messageSink.OnMessage(new DiagnosticMessage("Adding Album table."));
                command.ExecuteNonQuery();
            }

            using (var command = new SqlCommand(@"CREATE TABLE [dbo].[Artist]
(
    [ArtistId] INT NOT NULL IDENTITY,
    [Name] NVARCHAR(120),
    CONSTRAINT [PK_Artist] PRIMARY KEY CLUSTERED ([ArtistId])
)", connection))
            {
                _messageSink.OnMessage(new DiagnosticMessage("Adding Artists table."));
                command.ExecuteNonQuery();
            }

            using (var command = new SqlCommand(@"ALTER TABLE [dbo].[Album] ADD CONSTRAINT [FK_AlbumArtistId]
    FOREIGN KEY ([ArtistId]) REFERENCES [dbo].[Artist] ([ArtistId]) ON DELETE NO ACTION ON UPDATE NO ACTION;
", connection))
            {
                _messageSink.OnMessage(new DiagnosticMessage("Adding FK between tables."));
                command.ExecuteNonQuery();
            }

            using (var command = new SqlCommand("CREATE INDEX [IFK_AlbumArtistId] ON [dbo].[Album] ([ArtistId])", connection))
            {
                _messageSink.OnMessage(new DiagnosticMessage("Adding FK Index To Album.Artist."));
                command.ExecuteNonQuery();
            }

            string checkSQLFunction = @"
CREATE FUNCTION [dbo].[CheckSql]
(
    @tsql VARCHAR(4000),
    @parameterInfo VARCHAR(4000) NULL
)
RETURNS VARCHAR(1000)
AS
BEGIN
  DECLARE @Result VARCHAR (1000)

  IF EXISTS (
      SELECT 1
        FROM [sys].[dm_exec_describe_first_result_set] (@tsql, @parameterInfo, 0)
       WHERE [error_message] IS NOT NULL
         AND [error_number] IS NOT NULL
         AND [error_severity] IS NOT NULL
         AND [error_state] IS NOT NULL
         AND [error_type] IS NOT NULL
         AND [error_type_desc] IS NOT NULL
      )
    BEGIN
      SELECT @Result = [error_message]
        FROM [sys].[dm_exec_describe_first_result_set] (@tsql, @parameterInfo, 0)
       WHERE column_ordinal = 0
    END
  ELSE BEGIN
    SET @Result = 'OK'
  END

  RETURN (@Result)
END
";

            using (var command = new SqlCommand(checkSQLFunction, connection))
            {
                _messageSink.OnMessage(new DiagnosticMessage("Adding CheckSQL Function."));
                command.ExecuteNonQuery();
            }

            string raiseEventProc = @"
CREATE PROCEDURE [dbo].[InfoEventEmitter]
AS
BEGIN
    SET NOCOUNT ON;
    RAISERROR('Information text', 3, 3) WITH NOWAIT
    -- RAISERROR('Light error', 11, 14) WITH NOWAIT -- Above 10 are actual errors, which is throwing exceptions in SQL Client
    PRINT 'This is PRINT event';
END
";
            using (var command = new SqlCommand(raiseEventProc, connection))
            {
                _messageSink.OnMessage(new DiagnosticMessage("Adding RaiseEvent Procedure."));
                command.ExecuteNonQuery();
            }
        }

        private void PopulateDatabaseData()
        {
            using var connection = new SqlConnection(ConnectionString.Replace("{dbname}", "ChinookLight"));
            connection.Open();
            using (var command = new SqlCommand(@"INSERT INTO [dbo].[Artist] ([Name]) VALUES (N'AC/DC'),(N'Accept'),(N'Aerosmith'),(N'Alanis Morissette')", connection))
            {
                _messageSink.OnMessage(new DiagnosticMessage("Adding Artists data."));
                command.ExecuteNonQuery();
            }

            using (var command = new SqlCommand(@"INSERT INTO [dbo].[Album] ([Title], [ArtistId]) VALUES (N'For Those About To Rock We Salute You', 1), (N'Balls to the Wall', 2), (N'Restless and Wild', 2), (N'Let There Be Rock', 1), (N'Big Ones', 3), (N'Jagged Little Pill', 4);", connection))
            {
                _messageSink.OnMessage(new DiagnosticMessage("Adding Albums data."));
                command.ExecuteNonQuery();
            }
        }


        public void Dispose()
        {
            using var connection = new SqlConnection(ConnectionString.Replace("{dbname}", "master"));
            connection.Open();
            var alreadyExists = false;
            using (var command = new SqlCommand("SELECT db_id('ChinookLight')", connection))
            {
                if (command.ExecuteScalar() != DBNull.Value)
                {
                    _messageSink.OnMessage(new DiagnosticMessage("Dropping database."));
                    alreadyExists = true;
                }
            }

            if (!alreadyExists)
            {
                return;
            }

            using (var command = new SqlCommand("ALTER DATABASE ChinookLight SET SINGLE_USER WITH ROLLBACK IMMEDIATE", connection))
            {
                command.ExecuteNonQuery();
            }

            using (var command = new SqlCommand("DROP DATABASE ChinookLight", connection))
            {
                command.ExecuteNonQuery();
            }

            _messageSink.OnMessage(new DiagnosticMessage("Test database dropped."));
        }
    }
}

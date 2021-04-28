using System;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Xunit;

namespace Salix.Dapper.Cqrs.Abstractions.Tests
{
    [ExcludeFromCodeCoverage]
    public class LoggerExtensionsTests
    {
        [Theory]
        [InlineData(0, "0")]
        [InlineData(7, "0.001 ms")]
        [InlineData(53, "0.005 ms")]
        [InlineData(423, "0.042 ms")]
        [InlineData(3000, "0.3 ms")]
        [InlineData(3384, "0.338 ms")]
        [InlineData(34545, "3.455 ms")]
        [InlineData(637842, "64 ms")]
        [InlineData(1000000, "100 ms")]
        [InlineData(3829022, "383 ms")]
        [InlineData(9846353, "985 ms")]
        [InlineData(10000000, "1 sec 0 ms")]
        [InlineData(63728923, "6 sec 372 ms")]
        [InlineData(115432433, "11 sec")]
        [InlineData(596563442, "59 sec")]
        [InlineData(600000000, "1 min 0 sec")]
        [InlineData(945435653, "1 min 34 sec")]
        [InlineData(6533453421, "10 min 53 sec")]
        public void ToMinimumString_TimeSpan_CorrectResult(long ticks, string expected)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            var timeSpan = new TimeSpan(ticks);
            _ = timeSpan.ToHumanReadableString().Should().Be(expected);
        }

        [Fact]
        public void ToShortSql_TinySelect_ReturnEntirely()
        {
            string sql = @"
SELECT *
  FROM Table
";
            _ = sql.ToShortSql().Should().Be("SELECT * FROM Table");
        }

        [Fact]
        public void ToShortSql_ShortSelect_ReturnEntirely()
        {
            string sql = @"
SELECT Id,
       Name
  FROM Table
 WHERE Id = @id
";
            _ = sql.ToShortSql().Should().Be("SELECT Id, Name FROM Table WHERE Id = @id");
        }

        [Fact]
        public void ToShortSql_ManyFields_ShortensFields()
        {
            string sql = @"
SELECT Id,
       Name,
       FieldAlpha,
       FieldBeta,
       FieldCharlie,
       FieldDelta,
       FieldEchelon,
       FieldFoxtrott,
       FieldGym,
       FieldHotel,
       FieldIon,
       FieldJerry,
       FieldLand
  FROM Table
 WHERE Id = @id
";
            _ = sql.ToShortSql().Should().Be("SELECT Id, Name, Fie… FROM Table WHERE Id = @id");
        }

        [Fact]
        public void ToShortSql_BigFrom_ShortensFrom()
        {
            string sql = @"
SELECT t.Id,
       a.Name
  FROM Table t
       LEFT JOIN Related r ON r.Id = t.Id
       LEFT JOIN Another a ON a.Id = t.Id
 WHERE Id = @id";
            _ = sql.ToShortSql().Should().Be("SELECT t.Id, a.Name FROM Table t LEFT JOIN Relat… WHERE Id = @id");
        }

        [Fact]
        public void ToShortSql_BigWhere_ShortensWhere()
        {
            string sql = @"
SELECT Id,
       Name
  FROM Table
 WHERE Id = @id
       AND Name = @Name
       AND EXISTS (SELECT Identifier FROM AnotherTable WHERE Id = @Id)
";
            _ = sql.ToShortSql().Should().Be("SELECT Id, Name FROM Table WHERE Id = @id AND Name = @Name AND EXISTS (SEL…");
        }

        [Fact]
        public void ToShortSql_LongSelect_ShortensAll()
        {
            string sql = @"
SELECT t.Id,
       t.Name,
       t.FieldAlpha,
       t.FieldBeta,
       a.FieldCharlie,
       a.FieldDelta,
       a.FieldEchelon,
       a.FieldFoxtrott,
       r.FieldGym,
       r.FieldHotel,
       r.FieldIon,
       r.FieldJerry,
       r.FieldLand
  FROM Table t
       LEFT JOIN Related r ON r.Id = t.Id
       LEFT JOIN Another a ON a.Id = t.Id
 WHERE Id = @id
       AND t.Name = @Name
       AND EXISTS (SELECT Identifier AS Id FROM AnotherTable WHERE Id = @Id)
";
            _ = sql.ToShortSql().Should().Be("SELECT t.Id, t.Name,… FROM Table t LEFT JOIN Relat… WHERE Id = @id AND t.N…");
        }

        [Fact]
        public void ToShortSql_SmsSelect_ShortensAll()
        {
            string sql = @"
SELECT t.[Id],
       t.[Name],
       t.[FieldAlpha],
       t.[FieldBeta],
       a.[FieldCharlie],
       a.[FieldDelta],
       a.[FieldEchelon],
       a.[FieldFoxtrott],
       r.[FieldGym],
       r.[FieldHotel],
       r.[FieldIon],
       r.[FieldJerry],
       r.[FieldLand]
  FROM [dbo].[Table] t
       LEFT JOIN [dbo].[Related] r ON r.[Id] = t.[Id]
       LEFT JOIN [dbo].[Another] a ON a.[Id] = t.[Id]
 WHERE [Id] = @id
       AND t.[Name] = @Name
       AND EXISTS (SELECT [Identifier] AS Id FROM AnotherTable WHERE Id = @Id)
";
            _ = sql.ToShortSql().Should().Be("SELECT t.Id, t.Name,… FROM dbo.Table t LEFT JOIN d… WHERE Id = @id AND t.N…");
        }

        [Fact]
        public void ToShortSql_Subselect_ShortensFields()
        {
            string sql = @"
SELECT Id,
       Name,
       (SELECT Title FROM Titles WHERE Titles.TableId = Id) AS Title
  FROM Table
 WHERE Id = @id
";
            _ = sql.ToShortSql().Should().Be("SELECT Id, Name, (SE… FROM Table WHERE Id = @id");
        }

        [Fact]
        public void ToShortSql_Subselects_ShortensFields()
        {
            string sql = @"
SELECT Id,
       Name,
       (SELECT Title FROM Titles WHERE Titles.TableId = Id) AS Title,
       (SELECT Address FROM Contacts WHERE Contacts.TableId = Id) AS Address
  FROM Table
 WHERE Id = @id
";
            _ = sql.ToShortSql().Should().Be("SELECT Id, Name, (SE… FROM Table WHERE Id = @id");
        }

        [Fact]
        public void ToShortSql_Delete_ShortensStatement()
        {
            string sql = @"
DELETE FROM Table
      WHERE Id = @id
";
            _ = sql.ToShortSql().Should().Be("DELETE FROM Table WHERE Id = @id");
        }

        [Fact]
        public void ToShortSql_Execute_ShortensStatement()
        {
            string sql = @"EXEC [dbo].[SomeFancyStoredProcedure]";
            _ = sql.ToShortSql().Should().Be("EXEC dbo.SomeFancyStoredProcedure");
        }

        [Fact]
        public void ToShortSql_Insert_ShortensStatement()
        {
            string sql = @"
INSERT INTO [Table] (
   Name,
   Title,
   Address,
   Phone
) VALUES (
   @Name,
   @Title,
   @Address,
   @Phone
)
";
            _ = sql.ToShortSql().Should().Be("INSERT INTO Table ( Name, Title, Address, Phone) VALUES ( @Name, @Title, @…");
        }

        [Fact]
        public void ToShortSql_Update_ShortensStatement()
        {
            string sql = @"
UPDATE [Table] SET
   Name = @Name,
   Title = @Title,
   Address = @Address,
   Phone = @Phone
WHERE [Id] = @Id
";
            _ = sql.ToShortSql().Should().Be("UPDATE Table SET Name = @Name, Title = @Title, Address = @Address, Phone =…");
        }

        [Fact]
        public void ToShortSql_Function_ShortensStatement()
        {
            string sql = "SELECT dbo.CheckSql(@tsql, @parameterTypes)";
            _ = sql.ToShortSql().Should().Be("SELECT dbo.CheckSql(@tsql, @parameterTypes)");
        }
    }
}

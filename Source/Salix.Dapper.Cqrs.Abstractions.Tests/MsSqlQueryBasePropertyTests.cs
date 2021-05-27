using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Xunit;

namespace Salix.Dapper.Cqrs.Abstractions.Tests
{
    [ExcludeFromCodeCoverage]
    public class MsSqlQueryBasePropertyTests
    {
        [Fact]
        public void EmptyQuery_Properties_Check()
        {
            var testable = new EmptySingleQuery();
            _ = testable.SqlStatement.Should().Be("SQL Statement is not overridden in inheriting class");
            _ = testable.Parameters.Should().BeNull();
        }

        [Fact]
        public void SimpleQuery_Properties_Check()
        {
            var testable = new SimpleSingleQuery();
            _ = testable.SqlStatement.Should().Be("SELECT Id FROM Table WHERE Id = @Id");
            _ = testable.Parameters.Should().Be(new { Id = 12 });
            _ = RemoveNewLine(testable.RealSqlStatement).Should().Be("DECLARE @Id INT = 12;SELECT Id FROM Table WHERE Id = @Id");
        }

        [Fact]
        public void AllTypesQuery_Properties_Check()
        {
            var testable = new AllParamTypesSingleQuery();
            _ = RemoveNewLine(testable.RealSqlStatement).Should().Be("DECLARE @P01 BIT = 1;DECLARE @P02 TINYINT = 3;DECLARE @P03 SMALLINT = 5;DECLARE @P04 NCHAR(1) = 'z';DECLARE @P05 DECIMAL(29,4) = 3.14;DECLARE @P06 FLOAT = 2.71;DECLARE @P07 REAL = 9.99;DECLARE @P08 INT = 7;DECLARE @P09 BIGINT = 15;DECLARE @P10 IntPtr = '-1';DECLARE @P11 UIntPtr = '8';DECLARE @P12 BIGINT = 1000;DECLARE @P13 DECIMAL(20) = 712;DECLARE @P14 SMALLINT = -6;DECLARE @P15 INT = 21;DECLARE @P16 NVARCHAR(100) = 'wow';DECLARE @P17 DATETIME = '2021-04-03 07:15:28Z';DECLARE @P18 DATETIME = '2021-04-03 05:15:28Z';DECLARE @P19 BIGINT = 762590000000;DECLARE @P20 UNIQUEIDENTIFIER = '73de0f47-a2c9-44e7-82f6-c8c928ec12a0';SQL");
        }

        [Fact]
        public void AllNullableTypesQuery_Properties_Check()
        {
            var testable = new AllParamNullTypesSingleQuery();
            _ = RemoveNewLine(testable.RealSqlStatement).Should().Be("DECLARE @P01 BIT = 1;DECLARE @P02 TINYINT = 3;DECLARE @P03 SMALLINT = 5;DECLARE @P04 NCHAR(1) = 'z';DECLARE @P05 DECIMAL(29,4) = 3.14;DECLARE @P06 FLOAT = 2.71;DECLARE @P07 REAL = 9.99;DECLARE @P08 INT = 7;DECLARE @P09 BIGINT = 15;DECLARE @P10 IntPtr = '-1';DECLARE @P11 UIntPtr = '8';DECLARE @P12 BIGINT = 1000;DECLARE @P13 DECIMAL(20) = 712;DECLARE @P14 SMALLINT = -6;DECLARE @P15 INT = 21;DECLARE @P16 NVARCHAR(100) = 'wow';DECLARE @P17 DATETIME = '2021-04-03 07:15:28Z';DECLARE @P18 DATETIME = '2021-04-03 05:15:28Z';DECLARE @P19 BIGINT = 762590000000;DECLARE @P20 UNIQUEIDENTIFIER = '73de0f47-a2c9-44e7-82f6-c8c928ec12a0';SQL");
        }

        [Fact]
        public void AllNullableTypesQuery_NullProperties_Check()
        {
            var testable = new AllParamNullTypesNullSingleQuery();
            _ = RemoveNewLine(testable.RealSqlStatement).Should().Be("DECLARE @P01 BIT = NULL;DECLARE @P02 TINYINT = NULL;DECLARE @P03 SMALLINT = NULL;DECLARE @P04 NCHAR(1) = NULL;DECLARE @P05 DECIMAL(29,4) = NULL;DECLARE @P06 FLOAT = NULL;DECLARE @P07 REAL = NULL;DECLARE @P08 INT = NULL;DECLARE @P09 BIGINT = NULL;DECLARE @P10 IntPtr = NULL;DECLARE @P11 UIntPtr = NULL;DECLARE @P12 BIGINT = NULL;DECLARE @P13 DECIMAL(20) = NULL;DECLARE @P14 SMALLINT = NULL;DECLARE @P15 INT = NULL;DECLARE @P16 NVARCHAR(100) = NULL;DECLARE @P17 DATETIME = NULL;DECLARE @P18 DATETIME = NULL;DECLARE @P19 BIGINT = NULL;DECLARE @P20 UNIQUEIDENTIFIER = NULL;SQL");
        }

        [Fact]
        public void ToString_Shows_Query()
        {
            var testable = new SimpleSingleQuery();
            testable.ToString().Should().Be("SELECT Id FROM Table WHERE Id = @Id");
        }

        private static string RemoveNewLine(string sql) => sql.Replace("\r", string.Empty).Replace("\n", string.Empty);
    }
}

using System;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Moq;
using Xunit;

namespace Salix.Dapper.Cqrs.Abstractions.Tests
{
    [ExcludeFromCodeCoverage]
    public class MsSqlQueryBaseValidatorTests
    {
        private readonly Mock<IDatabaseSession> _dbSession;
        private string _sql;
        private object _parameterTypes;

        public MsSqlQueryBaseValidatorTests() => _dbSession = new Mock<IDatabaseSession>();

        private void SetupParamterRetrieval() =>
            _ = _dbSession.Setup(s => s.QueryFirstOrDefault<string>(It.IsAny<string>(), It.IsAny<object>()))
                .Callback<string, object>((q, p) =>
                {
                    var anonType = p.GetType();
                    _sql = (string)anonType.GetProperty("tsql").GetValue(p, null);
                    _parameterTypes = (string)anonType.GetProperty("parameterTypes").GetValue(p, null);
                })
                .Returns("OK");

        [Fact]
        public void Validate_OK_Works()
        {
            string query = null;
            object parameters = null;
            _ = _dbSession.Setup(s => s.QueryFirstOrDefault<string>(It.IsAny<string>(), It.IsAny<object>()))
                    .Callback<string, object>((q, p) => { parameters = p; query = q; })
                    .Returns("OK");
            var testable = new EmptyQuery();
            testable.Validate(_dbSession.Object);
            _dbSession.Verify(s => s.QueryFirstOrDefault<string>(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
            _ = query.Should().Be("SELECT dbo.CheckSql(@tsql, @parameterTypes)");
            _ = parameters.Should().NotBeNull();
        }

        [Fact]
        public void Validate_Exception_Works()
        {
            _ = _dbSession.Setup(s => s.QueryFirstOrDefault<string>(It.IsAny<string>(), It.IsAny<object>()))
                    .Returns("Some error information is not OK.");
            var testable = new EmptyQuery();
            Action act = () => testable.Validate(_dbSession.Object);
            act.Should().Throw<DatabaseStatementSyntaxException>();
        }

        [Fact]
        public void Validate_Simple_AllSet()
        {
            this.SetupParamterRetrieval();
            var testable = new SimpleQuery();
            testable.Validate(_dbSession.Object);
            _ = _sql.Should().Be("SELECT Id FROM Table");
            _ = _parameterTypes.Should().Be("@Id INT");
        }

        [Fact]
        public void Validate_AllParameters_AllSet()
        {
            this.SetupParamterRetrieval();
            var testable = new AllParamTypesQuery();
            testable.Validate(_dbSession.Object);
            _ = _parameterTypes.Should().Be("@P01 BIT,@P02 TINYINT,@P03 SMALLINT,@P04 NCHAR(1),@P05 DECIMAL(29,4),@P06 FLOAT,@P07 REAL,@P08 INT,@P09 BIGINT,@P12 BIGINT,@P13 DECIMAL(20),@P14 SMALLINT,@P15 INT,@P16 NVARCHAR(4000),@P17 DATETIME,@P18 DATETIME,@P19 BIGINT,@P20 UNIQUEIDENTIFIER");
        }

        [Fact]
        public void Validate_AllNullableParameters_AllSet()
        {
            this.SetupParamterRetrieval();
            var testable = new AllParamNullTypesQuery();
            testable.Validate(_dbSession.Object);
            _ = _parameterTypes.Should().Be("@P01 BIT,@P02 TINYINT,@P03 SMALLINT,@P04 NCHAR(1),@P05 DECIMAL(29,4),@P06 FLOAT,@P07 REAL,@P08 INT,@P09 BIGINT,@P12 BIGINT,@P13 DECIMAL(20),@P14 SMALLINT,@P15 INT,@P16 NVARCHAR(4000),@P17 DATETIME,@P18 DATETIME,@P19 BIGINT,@P20 UNIQUEIDENTIFIER");
        }

        [Fact]
        public void Validate_AllNullableParametersNulls_AllSet()
        {
            this.SetupParamterRetrieval();
            var testable = new AllParamNullTypesNullQuery();
            testable.Validate(_dbSession.Object);
            _ = _parameterTypes.Should().Be("@P01 BIT,@P02 TINYINT,@P03 SMALLINT,@P04 NCHAR(1),@P05 DECIMAL(29,4),@P06 FLOAT,@P07 REAL,@P08 INT,@P09 BIGINT,@P12 BIGINT,@P13 DECIMAL(20),@P14 SMALLINT,@P15 INT,@P16 NVARCHAR(4000),@P17 DATETIME,@P18 DATETIME,@P19 BIGINT,@P20 UNIQUEIDENTIFIER");
        }
    }
}

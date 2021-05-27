using System.Diagnostics.CodeAnalysis;
using Salix.Dapper.Cqrs.Abstractions;

namespace Sample.AspNet5Api.Database.Tests.Preparation
{
    /// <summary>
    /// Creates a dummy table with almost all possible column types (for functionality checking).
    /// </summary>
    [ExcludeFromCodeCoverage]

    public sealed class TestTableCreateCommand : MsSqlCommandBase
    {
        /// <summary>
        /// Actual SQL Statement to execute against MS SQL database.
        /// </summary>
        public override string SqlStatement => @"
CREATE TABLE dbo.TestColumnTypes (
    IntId INT PRIMARY KEY CLUSTERED NOT NULL,
	NumLong  BIGINT NOT NULL,
	NumSmall SMALLINT NOT NULL,
	NumByte  TINYINT NOT NULL,
	NumBit BIT NOT NULL,
	NumFloat FLOAT(1) NOT NULL,
	NumReal REAL NOT NULL,
	NumMoney MONEY NOT NULL,
	DtDateTime DATETIME NOT NULL,
	DtDateTime2 DATETIME2(1) NOT NULL,
	DtDate DATE NOT NULL,
	DtTime TIME NOT NULL,
	DtSmallDateTime SMALLDATETIME NOT NULL,
	DtDateTimeOffset DATETIMEOFFSET NOT NULL,
	TxtChar CHAR(5) NOT NULL,
	TxtNChar NCHAR(5) NOT NULL,
	TxtVarchar VARCHAR(5) NOT NULL,
	TxtNVarchar NVARCHAR(5) NOT NULL,
	TxtNvarcharMax NVARCHAR(MAX) NOT NULL,
	BinBinary BINARY(64) NOT NULL,
	BinVarbinary VARBINARY(64) NOT NULL,
	BinVarbinaryMax VARBINARY(MAX) NOT NULL,
	GuidGuid UNIQUEIDENTIFIER NOT NULL,
	DataXml XML NOT NULL,
    IntNull INT NULL,
	NumLongNull BIGINT NULL,
	NumSmallNull SMALLINT NULL,
	NumByteNull  TINYINT NULL,
	NumBitNull BIT NULL,
	NumFloatNull FLOAT(3) NULL,
	NumRealNull REAL NULL,
	NumMoneyNull MONEY NULL,
	DtDateTimeNull DATETIME NULL,
	DtDateTime2Null DATETIME2 NULL,
	DtDateNull DATE NULL,
	DtTimeNull TIME NULL,
	DtSmallDateTimeNull SMALLDATETIME NULL,
	DtDateTimeOffsetNull DATETIMEOFFSET NULL,
	TxtCharNull CHAR(5) NULL,
	TxtNCharNull NCHAR(5) NULL,
	TxtVarcharNull VARCHAR(5) NULL,
	TxtNVarcharNull NVARCHAR(5) NULL,
	TxtNvarcharMaxNull NVARCHAR(MAX) NULL,
	BinBinaryNull BINARY(64) NULL,
	BinVarbinaryNull VARBINARY(64) NULL,
	BinVarbinaryMaxNull VARBINARY(MAX) NULL,
	GuidGuidNull UNIQUEIDENTIFIER NULL,
	DataXmlNull XML NULL,
	GuidDefault UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
	TxtDefault NVARCHAR(50) NOT NULL DEFAULT 'Dapper.CQRS',
	NumDefault INT NOT NULL DEFAULT 7,
	AuditTimestamp TIMESTAMP NOT NULL
)
";
    }
}

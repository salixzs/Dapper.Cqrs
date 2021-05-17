using System.Diagnostics.CodeAnalysis;
using Salix.Dapper.Cqrs.Abstractions;

namespace Salix.Dapper.Cqrs.MsSql.Testing.XUnit
{
    /// <summary>
    /// Creates a special CheckSql Scalar function in database to be able to validate SQL statements with it in integration tests.
    /// </summary>
    [ExcludeFromCodeCoverage]

    public sealed class CheckSqlFuctionCreateCommand : MsSqlCommandBase, ICommand
    {
        /// <summary>
        /// Actual SQL Statement to execute against MS SQL database.
        /// </summary>
        public override string SqlStatement => @"
-- =====================================================================================
-- Author:		Anrijs Vitolins (Dapper.CQRS package)
-- Create date: May-2021
-- Description:	Checks SQL statement given in @tsql for its validity against database
-- =====================================================================================
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
    }
}

using System.Diagnostics.CodeAnalysis;

namespace Salix.Dapper.Cqrs.Abstractions.Tests
{

    [ExcludeFromCodeCoverage]
    public sealed class EmptyCommand : MsSqlCommandBase
    {
    }

    [ExcludeFromCodeCoverage]
    public sealed class SimpleCommand : MsSqlCommandBase
    {
        public override string SqlStatement => "UPDATE Table SET Fld = @val";

        public override object Parameters => new { val = 12 };
    }
}

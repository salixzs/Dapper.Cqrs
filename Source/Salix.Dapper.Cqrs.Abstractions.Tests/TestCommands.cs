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

    [ExcludeFromCodeCoverage]
    public sealed class SimpleReturnCommand : MsSqlCommandBase<int>
    {
        public override string SqlStatement => "INSERT INTO Table (Name) VALUES (@Name)";

        public override object Parameters => new { Name = ".Net" };
    }
}

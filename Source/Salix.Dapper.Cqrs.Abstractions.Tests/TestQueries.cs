using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Salix.Dapper.Cqrs.Abstractions.Tests
{

    [ExcludeFromCodeCoverage]
    public sealed class EmptyQuery : MsSqlQueryBase<int>, IQuery<int>
    {
        public async Task<int> ExecuteAsync(IDatabaseSession session) => await session.QueryFirstOrDefaultAsync<int>(this.SqlStatement);
    }

    [ExcludeFromCodeCoverage]
    public sealed class SimpleQuery : MsSqlQueryBase<int>, IQuery<int>
    {
        public override string SqlStatement => "SELECT Id FROM Table";

        public override object Parameters => new { Id = 12 };

        public async Task<int> ExecuteAsync(IDatabaseSession session) => await session.QueryFirstOrDefaultAsync<int>(this.SqlStatement);
    }

    [ExcludeFromCodeCoverage]
    public sealed class AllParamTypesQuery : MsSqlQueryBase<int>, IQuery<int>
    {
        public override string SqlStatement => "SQL";

        public override object Parameters =>
            new AllTypes
            {
                P01 = true,
                P02 = 3,
                P03 = 5,
                P04 = 'z',
                P05 = 3.14M,
                P06 = 2.71,
                P07 = 9.99F,
                P08 = 7,
                P09 = 15,
                P10 = -1,
                P11 = 8,
                P12 = 1000,
                P13 = 712,
                P14 = -6,
                P15 = 21,
                P16 = "wow",
                P17 = new DateTime(2021, 4, 3, 7, 15, 28),
                P18 = new DateTimeOffset(2021, 4, 3, 7, 15, 28, new TimeSpan(2, 0, 0)),
                P19 = new TimeSpan(21, 10, 59),
                P20 = new Guid("73de0f47-a2c9-44e7-82f6-c8c928ec12a0"),
            };

        public async Task<int> ExecuteAsync(IDatabaseSession session) => await session.QueryFirstOrDefaultAsync<int>(this.SqlStatement);
    }

    [ExcludeFromCodeCoverage]
    public sealed class AllParamNullTypesQuery : MsSqlQueryBase<int>, IQuery<int>
    {
        public override string SqlStatement => "SQL";

        public override object Parameters =>
            new AllNullableTypes
            {
                P01 = true,
                P02 = 3,
                P03 = 5,
                P04 = 'z',
                P05 = 3.14M,
                P06 = 2.71,
                P07 = 9.99F,
                P08 = 7,
                P09 = 15,
                P10 = -1,
                P11 = 8,
                P12 = 1000,
                P13 = 712,
                P14 = -6,
                P15 = 21,
                P16 = "wow",
                P17 = new DateTime(2021, 4, 3, 7, 15, 28),
                P18 = new DateTimeOffset(2021, 4, 3, 7, 15, 28, new TimeSpan(2, 0, 0)),
                P19 = new TimeSpan(21, 10, 59),
                P20 = new Guid("73de0f47-a2c9-44e7-82f6-c8c928ec12a0"),
            };

        public async Task<int> ExecuteAsync(IDatabaseSession session) => await session.QueryFirstOrDefaultAsync<int>(this.SqlStatement);
    }

    [ExcludeFromCodeCoverage]
    public sealed class AllParamNullTypesNullQuery : MsSqlQueryBase<int>, IQuery<int>
    {
        public override string SqlStatement => "SQL";

        public override object Parameters => new AllNullableTypes();

        public async Task<int> ExecuteAsync(IDatabaseSession session) => await session.QueryFirstOrDefaultAsync<int>(this.SqlStatement);
    }

    [ExcludeFromCodeCoverage]
    public class AllTypes
    {
        public bool P01 { get; set; }
        public byte P02 { get; set; }
        public sbyte P03 { get; set; }
        public char P04 { get; set; }
        public decimal P05 { get; set; }
        public double P06 { get; set; }
        public float P07 { get; set; }
        public int P08 { get; set; }
        public uint P09 { get; set; }
        public nint P10 { get; set; }
        public nuint P11 { get; set; }
        public long P12 { get; set; }
        public ulong P13 { get; set; }
        public short P14 { get; set; }
        public ushort P15 { get; set; }
        public string P16 { get; set; }
        public DateTime P17 { get; set; }
        public DateTimeOffset P18 { get; set; }
        public TimeSpan P19 { get; set; }
        public Guid P20 { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class AllNullableTypes
    {
        public bool? P01 { get; set; }
        public byte? P02 { get; set; }
        public sbyte? P03 { get; set; }
        public char? P04 { get; set; }
        public decimal? P05 { get; set; }
        public double? P06 { get; set; }
        public float? P07 { get; set; }
        public int? P08 { get; set; }
        public uint? P09 { get; set; }
        public nint? P10 { get; set; }
        public nuint? P11 { get; set; }
        public long? P12 { get; set; }
        public ulong? P13 { get; set; }
        public short? P14 { get; set; }
        public ushort? P15 { get; set; }
        public string P16 { get; set; }
        public DateTime? P17 { get; set; }
        public DateTimeOffset? P18 { get; set; }
        public TimeSpan? P19 { get; set; }
        public Guid? P20 { get; set; }
    }
}

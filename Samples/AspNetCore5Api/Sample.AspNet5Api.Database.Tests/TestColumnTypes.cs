using System;

namespace Sample.AspNet5Api.Database.Tests
{
    /// <summary>
    /// A special DTO POCO for test columns table.
    /// This is only for NuGet package functionality tests. Not necessary in normal application.
    /// </summary>
    public class TestColumnTypes
    {
        public int IntId { get; set; }
        public long NumLong { get; set; }
        public short NumSmall { get; set; }
        public byte NumByte { get; set; }
        public bool NumBit { get; set; }
        public double NumFloat { get; set; }
        public float NumReal { get; set; }
        public decimal NumMoney { get; set; }
        public DateTime DtDateTime { get; set; }
        public DateTime DtDateTime2 { get; set; }
        public DateTime DtDate { get; set; }
        public TimeSpan DtTime { get; set; }
        public DateTime DtSmallDateTime { get; set; }
        public DateTimeOffset DtDateTimeOffset { get; set; }
        public string TxtChar { get; set; }
        public string TxtNChar { get; set; }
        public string TxtVarchar { get; set; }
        public string TxtNVarchar { get; set; }
        public string TxtNvarcharMax { get; set; }
        public byte[] BinBinary { get; set; }
        public byte[] BinVarbinary { get; set; }
        public byte[] BinVarbinaryMax { get; set; }
        public Guid GuidGuid { get; set; }
        public string DataXml { get; set; }


        public int? IntNull { get; set; }
        public long? NumLongNull { get; set; }
        public short? NumSmallNull { get; set; }
        public byte? NumByteNull { get; set; }
        public bool? NumBitNull { get; set; }
        public double? NumFloatNull { get; set; }
        public float? NumRealNull { get; set; }
        public decimal? NumMoneyNull { get; set; }
        public DateTime? DtDateTimeNull { get; set; }
        public DateTime? DtDateTime2Null { get; set; }
        public DateTime? DtDateNull { get; set; }
        public TimeSpan? DtTimeNull { get; set; }
        public DateTime? DtSmallDateTimeNull { get; set; }
        public DateTimeOffset? DtDateTimeOffsetNull { get; set; }
        public string TxtCharNull { get; set; }
        public string TxtNCharNull { get; set; }
        public string TxtVarcharNull { get; set; }
        public string TxtNVarcharNull { get; set; }
        public string TxtNvarcharMaxNull { get; set; }
        public byte[] BinBinaryNull { get; set; }
        public byte[] BinVarbinaryNull { get; set; }
        public byte[] BinVarbinaryMaxNull { get; set; }
        public Guid? GuidGuidNull { get; set; }
        public string DataXmlNull { get; set; }
        public Guid GuidDefault { get; set; }
        public string TxtDefault { get; set; }
        public int NumDefault { get; set; }
        public byte[] AuditTimestamp { get; set; }
    }
}

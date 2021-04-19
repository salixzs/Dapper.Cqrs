using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Salix.Dapper.Cqrs.Abstractions;
using Xunit;

namespace Salix.Dapper.Cqrs.MsSql.Tests
{
    /// <summary>
    /// Testing <see cref="SqlDynamicParameters"></see> class functionalities.
    /// NOTE: In tests WrappedSqlDynamicParameters class is used (defined below) as a wrapper
    /// to get more easy to actual testable class internals.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class DynamicParametersTests
    {
        /// <summary>
        /// Check adding parameter - it appears correctly in public properties.
        /// </summary>
        [Fact]
        public void Add_Normally_AppearsInParameterList()
        {
            var testable = new WrappedSqlDynamicParameters();
            testable.Add("Black", SqlDbType.VarChar);
            testable.ParameterNames.Should().NotBeEmpty();
            testable.ParameterNames.Should().HaveCount(1);
            testable.ParameterNames.First().Should().Be("Black");
        }

        /// <summary>
        /// Check parameter name cleanup, if developer by mistake added it as "@paramname" (with @ in front).
        /// </summary>
        [Fact]
        public void Add_NameWithParmameterSymbol_RemovesParmameterSymbol()
        {
            var testable = new WrappedSqlDynamicParameters();
            testable.Add("@Black", SqlDbType.VarChar);
            testable.ParameterNames.Should().NotBeEmpty();
            testable.ParameterNames.Should().HaveCount(1);
            testable.ParameterNames.First().Should().Be("Black");
        }

        [Fact]
        public void Add_EmptyName_DoesNotAdd()
        {
            var testable = new WrappedSqlDynamicParameters();
            Action action = () => testable.Add(string.Empty, SqlDbType.VarChar);
            action.Should().Throw<ArgumentException>().WithMessage("Cannot create Dynamic parameter with empty name. (Parameter 'name')");
        }

        /// <summary>
        /// Checking parameter validation when ICommand implementation is not passed.
        /// (Just to increase coverage, as it is handled by .Net Framework)
        /// </summary>
        [Fact]
        public void AddParameters_NoCommand_Throws()
        {
            var testable = new WrappedSqlDynamicParameters();
            Action action = () => testable.AddParameters(null);
            action.Should().Throw<ArgumentNullException>();
        }

        /// <summary>
        /// Checking whether String=NULL parameter is handled correctly in Parameters class.
        /// </summary>
        [Fact]
        public void Add_StringNullParameter_StoredCorrectly()
        {
            var testable = new WrappedSqlDynamicParameters();
            testable.Add("Nullable", SqlDbType.VarChar, null);
            testable.AddParameters(new SqlCommand()); // Faking Command execution
            testable.ParameterValues["Nullable"].Should().Be(null);
            testable.ParameterDirections["Nullable"].Should().Be(ParameterDirection.Input);
            testable.SqlDbTypes["Nullable"].Should().Be(SqlDbType.VarChar);
            var internalTestable = (SqlParameter)testable.ParameterDbDataObject["Nullable"];
            internalTestable.ParameterName.Should().Be("Nullable");
            internalTestable.Value.Should().Be(DBNull.Value);
            internalTestable.SqlDbType.Should().Be(SqlDbType.VarChar);
            internalTestable.DbType.Should().Be(DbType.AnsiString);
            internalTestable.Direction.Should().Be(ParameterDirection.Input);
            internalTestable.Size.Should().Be(0);
        }

        /// <summary>
        /// Checking whether String parameter of Varchar2 is handled correctly in Parameters class.
        /// </summary>
        [Fact]
        public void Add_VarcharParameter_StoredCorrectly()
        {
            var testable = new WrappedSqlDynamicParameters();
            testable.Add("Description", SqlDbType.VarChar, "YoKaLeMeNe");
            testable.AddParameters(new SqlCommand()); // Faking Command execution
            testable.ParameterValues["Description"].Should().Be("YoKaLeMeNe");
            testable.SqlDbTypes["Description"].Should().Be(SqlDbType.VarChar);
            testable.ParameterDirections["Description"].Should().Be(ParameterDirection.Input);
            var internalTestable = (SqlParameter)testable.ParameterDbDataObject["Description"];
            internalTestable.ParameterName.Should().Be("Description");
            internalTestable.Value.Should().Be("YoKaLeMeNe");
            internalTestable.DbType.Should().Be(DbType.AnsiString);
            internalTestable.SqlDbType.Should().Be(SqlDbType.VarChar);
            internalTestable.Direction.Should().Be(ParameterDirection.Input);
            internalTestable.Size.Should().Be(4000);
        }

        /// <summary>
        /// Checking whether String parameter of Varchar2 is handled correctly in Parameters class.
        /// </summary>
        [Fact]
        public void Add_NVarcharParameter_StoredCorrectly()
        {
            var testable = new WrappedSqlDynamicParameters();
            testable.Add("Description", SqlDbType.NVarChar, "YoKaLeMeNe");
            testable.AddParameters(new SqlCommand()); // Faking Command execution
            testable.ParameterValues["Description"].Should().Be("YoKaLeMeNe");
            testable.SqlDbTypes["Description"].Should().Be(SqlDbType.NVarChar);
            testable.ParameterDirections["Description"].Should().Be(ParameterDirection.Input);
            var internalTestable = (SqlParameter)testable.ParameterDbDataObject["Description"];
            internalTestable.ParameterName.Should().Be("Description");
            internalTestable.Value.Should().Be("YoKaLeMeNe");
            internalTestable.SqlDbType.Should().Be(SqlDbType.NVarChar);
            internalTestable.DbType.Should().Be(DbType.String);
            internalTestable.Direction.Should().Be(ParameterDirection.Input);
            internalTestable.Size.Should().Be(4000);
        }

        /// <summary>
        /// Checking whether Fixed length string parameter is handled correctly in Parameters class.
        /// </summary>
        [Fact]
        public void Add_NCharParameter_StoredCorrectly()
        {
            var testable = new WrappedSqlDynamicParameters();
            testable.Add("Valsts", SqlDbType.NChar, "Latvijas Republika");
            testable.AddParameters(new SqlCommand()); // Faking Command execution
            testable.ParameterValues["Valsts"].Should().Be("Latvijas Republika");
            testable.SqlDbTypes["Valsts"].Should().Be(SqlDbType.NChar);
            testable.ParameterDirections["Valsts"].Should().Be(ParameterDirection.Input);
            var internalTestable = (SqlParameter)testable.ParameterDbDataObject["Valsts"];
            internalTestable.ParameterName.Should().Be("Valsts");
            internalTestable.Value.Should().Be("Latvijas Republika");
            internalTestable.SqlDbType.Should().Be(SqlDbType.NChar);
            internalTestable.DbType.Should().Be(DbType.StringFixedLength);
            internalTestable.Direction.Should().Be(ParameterDirection.Input);
            internalTestable.Size.Should().Be(4000);
        }

        /// <summary>
        /// Checking whether sized string parameter is handled correctly in Parameters class.
        /// </summary>
        [Fact]
        public void Add_CharParameter_StoredCorrectly()
        {
            var testable = new WrappedSqlDynamicParameters();
            testable.Add("State", SqlDbType.Char, "NO", ParameterDirection.Input, 2);
            testable.AddParameters(new SqlCommand()); // Faking Command execution
            testable.ParameterValues["State"].Should().Be("NO");
            var internalTestable = (SqlParameter)testable.ParameterDbDataObject["State"];
            internalTestable.Value.Should().Be("NO");
            internalTestable.SqlDbType.Should().Be(SqlDbType.Char);
            internalTestable.DbType.Should().Be(DbType.AnsiStringFixedLength);
            internalTestable.Size.Should().Be(2);
        }

        /// <summary>
        /// Checking whether Long parameter is handled correctly in Parameters class.
        /// </summary>
        [Fact]
        public void Add_LongParameter_StoredCorrectly()
        {
            var testable = new WrappedSqlDynamicParameters();
            testable.Add("Identifier", SqlDbType.BigInt, 3145926L);
            testable.AddParameters(new SqlCommand()); // Faking Command execution
            testable.ParameterValues["Identifier"].Should().Be(3145926L);
            testable.SqlDbTypes["Identifier"].Should().Be(SqlDbType.BigInt);
            testable.ParameterDirections["Identifier"].Should().Be(ParameterDirection.Input);
            var internalTestable = (SqlParameter)testable.ParameterDbDataObject["Identifier"];
            internalTestable.ParameterName.Should().Be("Identifier");
            internalTestable.Value.Should().Be(3145926L);
            internalTestable.SqlDbType.Should().Be(SqlDbType.BigInt);
            internalTestable.DbType.Should().Be(DbType.Int64);
            internalTestable.Direction.Should().Be(ParameterDirection.Input);
            internalTestable.Size.Should().Be(0);
        }

        /// <summary>
        /// Checking whether Short Int parameter is handled correctly in Parameters class.
        /// </summary>
        [Fact]
        public void Add_IntParameter_StoredCorrectly()
        {
            var testable = new WrappedSqlDynamicParameters();
            testable.Add("Integer", SqlDbType.Int, 777);
            testable.AddParameters(new SqlCommand()); // Faking Command execution
            testable.ParameterValues["Integer"].Should().Be(777);
            testable.SqlDbTypes["Integer"].Should().Be(SqlDbType.Int);
            testable.ParameterDirections["Integer"].Should().Be(ParameterDirection.Input);
            var internalTestable = (SqlParameter)testable.ParameterDbDataObject["Integer"];
            internalTestable.ParameterName.Should().Be("Integer");
            internalTestable.Value.Should().Be(777);
            internalTestable.SqlDbType.Should().Be(SqlDbType.Int);
            internalTestable.DbType.Should().Be(DbType.Int32);
            internalTestable.Direction.Should().Be(ParameterDirection.Input);
            internalTestable.Size.Should().Be(0);
        }

        /// <summary>
        /// Checking whether DateTime parameter is handled correctly in Parameters class.
        /// </summary>
        [Fact]
        public void Add_DateTimeParameter_StoredCorrectly()
        {
            var testable = new WrappedSqlDynamicParameters();
            DateTime bd = DateTime.Now;
            testable.Add("Birthday", SqlDbType.DateTime, bd);
            testable.AddParameters(new SqlCommand()); // Faking Command execution
            ((DateTime)testable.ParameterValues["Birthday"]).Should().BeCloseTo(bd);
            testable.SqlDbTypes["Birthday"].Should().Be(SqlDbType.DateTime);
            testable.ParameterDirections["Birthday"].Should().Be(ParameterDirection.Input);
            var internalTestable = (SqlParameter)testable.ParameterDbDataObject["Birthday"];
            internalTestable.ParameterName.Should().Be("Birthday");
            ((DateTime)internalTestable.Value).Should().BeCloseTo(bd);
            internalTestable.DbType.Should().Be(DbType.DateTime);
            internalTestable.SqlDbType.Should().Be(SqlDbType.DateTime);
            internalTestable.Direction.Should().Be(ParameterDirection.Input);
            internalTestable.Size.Should().Be(0);
        }

        /// <summary>
        /// Checking whether DateTime parameter is handled correctly in Parameters class.
        /// </summary>
        [Fact]
        public void Add_BooleanParameter_StoredCorrectly()
        {
            var testable = new WrappedSqlDynamicParameters();
            testable.Add("IsTrue", SqlDbType.Bit, true);
            testable.AddParameters(new SqlCommand()); // Faking Command execution
            ((bool)testable.ParameterValues["IsTrue"]).Should().BeTrue();
            testable.SqlDbTypes["IsTrue"].Should().Be(SqlDbType.Bit);
            testable.ParameterDirections["IsTrue"].Should().Be(ParameterDirection.Input);
            var internalTestable = (SqlParameter)testable.ParameterDbDataObject["IsTrue"];
            internalTestable.ParameterName.Should().Be("IsTrue");
            ((bool)internalTestable.Value).Should().BeTrue();
            internalTestable.DbType.Should().Be(DbType.Boolean);
            internalTestable.SqlDbType.Should().Be(SqlDbType.Bit);
            internalTestable.Direction.Should().Be(ParameterDirection.Input);
            internalTestable.Size.Should().Be(0);
        }

        /// <summary>
        /// Checking situation when ICommand already has parameter defined - adding it again in collection just updates it
        /// and does not creates additional with same name.
        /// </summary>
        [Fact]
        public void Add_Existing_IsUpdated()
        {
            var cmd = new SqlCommand();
            cmd.Parameters.Add("Already", SqlDbType.Int, 13);
            var testable = new WrappedSqlDynamicParameters();
            testable.Add("Already", SqlDbType.Int, 12);
            testable.Add("New", SqlDbType.NVarChar, "this is new");
            testable.AddParameters(cmd); // Faking Command execution
            testable.ParameterNames.Should().HaveCount(2, "Existing param with same name should be reused!");
            testable.ParameterValues["Already"].Should().Be(12);
            testable.SqlDbTypes["Already"].Should().Be(SqlDbType.Int);
            testable.ParameterDirections["Already"].Should().Be(ParameterDirection.Input);
            testable.ParameterDbDataObject.Count.Should().Be(2);
            var internalTestable = (SqlParameter)testable.ParameterDbDataObject["Already"];
            internalTestable.Value.Should().Be(12);
            internalTestable.DbType.Should().Be(DbType.Int32);
            internalTestable.Direction.Should().Be(ParameterDirection.Input);
            internalTestable.Size.Should().Be(13);
        }

        /// <summary>
        /// Wrapped class to get out Command parameter binding functionality and internal field.
        /// Such thing can only be due to testing purposes. Do not even think about reusing such thing for production code!
        /// </summary>
        /// <seealso cref="DynamicParameters" />
        private class WrappedSqlDynamicParameters : DynamicParameters
        {
            /// <summary>
            /// Publishing internal protected method, which normally is run by .Net Framework SQL execute procedures to add parameters to ICommand.
            /// </summary>
            /// <param name="cmd">Command for faking parameter adding to it.</param>
            public new void AddParameters(IDbCommand cmd) => base.AddParameters(cmd);

            /// <summary>
            /// Extracting assigned internal parameter values (name, value)
            /// </summary>
            public Dictionary<string, object> ParameterValues => this.ProcessedParameters.ToDictionary(p => p.Key, p => p.Value.Value);

            /// <summary>
            /// Extracting assigned internal parameter SQLDbTypes (name, value)
            /// </summary>
            public Dictionary<string, SqlDbType?> SqlDbTypes => this.ProcessedParameters.ToDictionary(p => p.Key, p => p.Value.SqlDbType);

            /// <summary>
            /// Extracting assigned internal parameter DbTypes (name, value)
            /// </summary>
            public Dictionary<string, ParameterDirection> ParameterDirections => this.ProcessedParameters.ToDictionary(p => p.Key, p => p.Value.ParameterDirection);

            /// <summary>
            /// Extracting assigned internal parameter IDbDataParameter object
            /// </summary>
            public Dictionary<string, IDbDataParameter> ParameterDbDataObject => this.ProcessedParameters.ToDictionary(p => p.Key, p => p.Value.AttachedParam);

            /// <summary>
            /// Reflection to get out internal _parameters field and its contents.
            /// </summary>
            private Dictionary<string, DynamicParameterInfo> ProcessedParameters
            {
                get
                {
                    FieldInfo paramsField = typeof(DynamicParameters)
                             .GetField("_parameters", BindingFlags.Instance | BindingFlags.NonPublic);
                    return (Dictionary<string, DynamicParameterInfo>)paramsField.GetValue(this);
                }
            }
        }
    }
}

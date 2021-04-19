using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using ConfigurationValidation;

namespace Sample.AspNet5Api.Logic
{
    public class DatabaseConfiguration : IValidatableConfiguration
    {
        public string ConnectionString { get; set; }

        public IEnumerable<ConfigurationValidationItem> Validate()
        {
            try
            {
                var connectionObject = new SqlConnectionStringBuilder(this.ConnectionString);
            }
            catch (Exception ex)
            {
                return new List<ConfigurationValidationItem>
                {
                    new ConfigurationValidationItem("Database", nameof(this.ConnectionString), this.ConnectionString, ex.Message)
                };
            }

            return new List<ConfigurationValidationItem>();
        }
    }
}

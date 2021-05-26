using Salix.Dapper.Cqrs.Abstractions;
using Sample.AspNet5Api.Domain;

namespace Sample.AspNet5Api.Queries
{
    /// <summary>
    /// Retrieves an Invoice from database by its ID.
    /// </summary>
    public sealed class InvoiceByIdQuery : MsSqlQuerySingleBase<Invoice>
    {
        private readonly int _objectId;

        /// <summary>
        /// Retrieves an Invoice from database by its ID.
        /// </summary>
        /// <param name="objectId">Invoice ID in database.</param>
        public InvoiceByIdQuery(int objectId) => _objectId = objectId;

        /// <summary>
        /// Actual SQL Statement to execute against MS SQL database.
        /// </summary>
        public override string SqlStatement => @"
SELECT InvoiceId,
       CustomerId,
       InvoiceDate,
       BillingAddress,
       BillingCity,
       BillingState,
       BillingCountry,
       BillingPostalCode,
       Total
  FROM Invoice
 WHERE InvoiceId = @id";

        /// <summary>
        /// Anonymous object of SqlQuery parameter(s).
        /// </summary>
        public override object Parameters => new { id = _objectId };
    }
}

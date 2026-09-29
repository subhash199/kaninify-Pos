using DataHandlerLibrary.Models;
using EntityFrameworkDatabaseLibrary.Data;
using Microsoft.EntityFrameworkCore;

namespace DataHandlerLibrary.Services
{
    public class CardTransactionServices
    {
        private readonly IDbContextFactory<DatabaseInitialization> _dbFactory;

        public CardTransactionServices(IDbContextFactory<DatabaseInitialization> dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task AddAsync(CardTransaction transaction, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(transaction.Transaction_Reference))
            {
                throw new ArgumentException("A sale transaction reference is required.", nameof(transaction));
            }

            using var context = _dbFactory.CreateDbContext();
            context.CardTransactions.Add(transaction);
            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(CardTransaction transaction, CancellationToken cancellationToken = default)
        {
            using var context = _dbFactory.CreateDbContext();
            var existing = await context.CardTransactions.SingleAsync(x => x.Id == transaction.Id, cancellationToken);
            // Status updates must not undo a sale link assigned by checkout.
            var salesTransactionId = existing.SalesTransaction_Id;
            context.Entry(existing).CurrentValues.SetValues(transaction);
            existing.SalesTransaction_Id = salesTransactionId;
            existing.Last_Modified = DateTime.UtcNow;
            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<List<CardTransaction>> GetBySaleAsync(int salesTransactionId)
        {
            using var context = _dbFactory.CreateDbContext();
            return await context.CardTransactions.AsNoTracking()
                .Where(x => x.SalesTransaction_Id == salesTransactionId)
                .OrderBy(x => x.Date_Created)
                .ToListAsync();
        }

        public async Task<List<CardTransaction>> GetByReferenceAsync(string transactionReference)
        {
            using var context = _dbFactory.CreateDbContext();
            return await context.CardTransactions.AsNoTracking()
                .Where(x => x.Transaction_Reference == transactionReference)
                .OrderBy(x => x.Date_Created)
                .ToListAsync();
        }

        public async Task<List<CardTransaction>> GetByGatewayPaymentIdAsync(string gatewayPaymentId)
        {
            using var context = _dbFactory.CreateDbContext();
            return await context.CardTransactions.AsNoTracking()
                .Where(x => x.Gateway_Payment_Id == gatewayPaymentId)
                .OrderBy(x => x.Date_Created)
                .ToListAsync();
        }

        public async Task<(decimal OriginalSaleAmount, string Currency, decimal RefundedAmount)> ComputeRefundBalanceAsync(
            string originalGatewayPaymentId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(originalGatewayPaymentId))
            {
                return (0m, string.Empty, 0m);
            }

            using var context = _dbFactory.CreateDbContext();
            var rows = await context.CardTransactions.AsNoTracking()
                .Where(x => x.Gateway_Payment_Id == originalGatewayPaymentId)
                .OrderBy(x => x.Date_Created)
                .ToListAsync(cancellationToken);

            var sale = rows.FirstOrDefault(x =>
                string.Equals(x.Transaction_Type, "SALE", StringComparison.OrdinalIgnoreCase) &&
                (string.Equals(x.Status, "SUCCESSFUL", StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(x.Status, "SUCCESS", StringComparison.OrdinalIgnoreCase)));
            var currency = sale?.Currency_Code ?? rows.FirstOrDefault()?.Currency_Code ?? string.Empty;
            var saleAmount = sale?.Amount ?? 0m;
            var refunded = rows
                .Where(x => string.Equals(x.Transaction_Type, "REFUND", StringComparison.OrdinalIgnoreCase) &&
                            (string.Equals(x.Status, "SUCCESS", StringComparison.OrdinalIgnoreCase) ||
                             string.Equals(x.Status, "SUCCESSFUL", StringComparison.OrdinalIgnoreCase) ||
                             string.Equals(x.Status, "PENDING", StringComparison.OrdinalIgnoreCase)))
                .Sum(x => x.Amount);
            return (saleAmount, currency, refunded);
        }
    }
}

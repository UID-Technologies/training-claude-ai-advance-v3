using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using LegacyBanking.Data;
using LegacyBanking.Data.Repositories;
using LegacyBanking.Domain.Entities;
using LegacyBanking.Domain.Models;
using LegacyBanking.Integrations;
using Newtonsoft.Json;

namespace LegacyBanking.Business
{
    // INTENTIONAL GOD SERVICE.
    // Contains validation, persistence, business rules,
    // integration, auditing, logging and notification.
    public class BankingService
    {
        public string TransferMoney(TransferRequest request)
        {
            if (request == null)
                throw new Exception("Invalid request.");

            if (string.IsNullOrWhiteSpace(request.FromAccount))
                throw new Exception("Source account is required.");

            if (string.IsNullOrWhiteSpace(request.ToAccount))
                throw new Exception("Target account is required.");

            if (request.FromAccount == request.ToAccount)
                throw new Exception("Source and target cannot match.");

            if (request.Amount <= 0)
                throw new Exception("Amount must be positive.");

            var accountRepository = new AccountRepository();

            var from =
                accountRepository.GetByAccountNumber(request.FromAccount);

            var to =
                accountRepository.GetByAccountNumber(request.ToAccount);

            if (from == null)
                throw new Exception("Source account not found.");

            if (to == null)
                throw new Exception("Target account not found.");

            if (from.Status != "ACTIVE")
                throw new Exception("Source account is not active.");

            if (to.Status != "ACTIVE")
                throw new Exception("Target account is not active.");

            if (from.Balance < request.Amount)
                throw new Exception("Insufficient balance.");

            // CRITICAL LEGACY BUG:
            // updates are performed separately without one DB transaction.
            from.Balance -= request.Amount;
            accountRepository.Update(from);

            to.Balance += request.Amount;
            accountRepository.Update(to);

            var reference =
                "TXN-" + DateTime.Now.Ticks;

            using (var db = new BankingDbContext())
            {
                db.Transactions.Add(new Transaction
                {
                    Reference = reference,
                    AccountId = from.Id,
                    Type = "DEBIT",
                    Amount = request.Amount,
                    Description = request.Description,
                    Status = "POSTED",
                    TransactionDate = DateTime.Now
                });

                db.Transactions.Add(new Transaction
                {
                    Reference = reference,
                    AccountId = to.Id,
                    Type = "CREDIT",
                    Amount = request.Amount,
                    Description = request.Description,
                    Status = "POSTED",
                    TransactionDate = DateTime.Now
                });

                db.SaveChanges();
            }

            LegacyLogger.Info(
                "Transfer performed: " +
                JsonConvert.SerializeObject(request));

            // Legacy fire-and-forget thread.
            new Thread(() =>
            {
                try
                {
                    var auditRepository = new AuditRepository();

                    auditRepository.Add(new AuditLog
                    {
                        UserName = request.RequestedBy,
                        Action = "TRANSFER",
                        EntityName = "Account",
                        EntityId = request.FromAccount,
                        Payload = JsonConvert.SerializeObject(request),
                        CreatedOn = DateTime.Now
                    });

                    if (from.Customer != null)
                    {
                        new LegacyNotificationClient().SendEmail(
                            from.Customer.Email,
                            "Debit Alert",
                            "Amount debited: " + request.Amount);
                    }
                }
                catch
                {
                    // Legacy issue: failure silently swallowed.
                }
            }).Start();

            return reference;
        }

        public decimal CalculateLoanEligibility(
            int customerId,
            decimal requestedAmount)
        {
            var customer =
                new CustomerRepository().GetById(customerId);

            if (customer == null)
                return 0;

            var score =
                new CreditBureauClient()
                    .GetCreditScore(customer.NationalId);

            // Legacy issue: magic business policy.
            if (score < 600)
                return 0;

            if (score < 680)
                return requestedAmount * 0.40m;

            if (score < 720)
                return requestedAmount * 0.60m;

            if (score < 760)
                return requestedAmount * 0.80m;

            return requestedAmount;
        }

        public List<string> GetCustomerDashboard(int customerId)
        {
            var result = new List<string>();

            var customer =
                new CustomerRepository().GetById(customerId);

            if (customer == null)
                return result;

            var accounts =
                new AccountRepository().GetByCustomer(customerId);

            // Legacy N+1 query.
            foreach (var account in accounts)
            {
                using (var db = new BankingDbContext())
                {
                    var transactionCount =
                        db.Transactions.Count(
                            t => t.AccountId == account.Id);

                    result.Add(
                        account.AccountNumber +
                        " | " +
                        account.AccountType +
                        " | Balance=" +
                        account.Balance +
                        " | TxnCount=" +
                        transactionCount);
                }
            }

            return result;
        }
    }
}

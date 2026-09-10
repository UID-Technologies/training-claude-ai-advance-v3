using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using LegacyBanking.Domain.Entities;

namespace LegacyBanking.Data.Repositories
{
    public class TransactionRepository
    {
        public List<Transaction> Search(string accountNumber, string status)
        {
            var result = new List<Transaction>();
            var connectionString =
                ConfigurationManager.ConnectionStrings["BankingDb"].ConnectionString;

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // INTENTIONAL SECURITY BUG:
                // concatenated SQL makes this vulnerable to SQL injection.
                var sql =
                    "SELECT t.Id,t.Reference,t.AccountId,t.Type,t.Amount," +
                    "t.Description,t.TransactionDate,t.Status " +
                    "FROM Transactions t " +
                    "INNER JOIN Accounts a ON a.Id=t.AccountId " +
                    "WHERE a.AccountNumber='" + accountNumber +
                    "' AND t.Status='" + status + "'";

                using (var command = new SqlCommand(sql, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(new Transaction
                        {
                            Id = Convert.ToInt64(reader["Id"]),
                            Reference = Convert.ToString(reader["Reference"]),
                            AccountId = Convert.ToInt32(reader["AccountId"]),
                            Type = Convert.ToString(reader["Type"]),
                            Amount = Convert.ToDecimal(reader["Amount"]),
                            Description = Convert.ToString(reader["Description"]),
                            TransactionDate = Convert.ToDateTime(reader["TransactionDate"]),
                            Status = Convert.ToString(reader["Status"])
                        });
                    }
                }
            }

            return result;
        }
    }
}

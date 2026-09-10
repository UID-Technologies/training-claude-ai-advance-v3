using System;
using System.Text.RegularExpressions;
using LegacyBanking.Data.Repositories;
using LegacyBanking.Domain.Entities;
using LegacyBanking.Integrations;

namespace LegacyBanking.Business
{
    public class CustomerOnboardingService
    {
        public int Register(
            string fullName,
            string email,
            string mobile,
            string nationalId)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                throw new Exception("Customer name is required.");

            if (!Regex.IsMatch(
                    email ?? "",
                    @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                throw new Exception("Invalid email.");

            if (string.IsNullOrWhiteSpace(mobile) ||
                mobile.Length < 8)
                throw new Exception("Invalid phone number.");

            var customer = new Customer
            {
                CustomerNumber = "CUS-" + DateTime.Now.Ticks,
                FullName = fullName.Trim(),
                Email = email,
                Mobile = mobile,
                NationalId = nationalId,
                KycStatus = "PENDING",
                CreatedOn = DateTime.Now
            };

            var repository = new CustomerRepository();
            repository.Save(customer);

            // Legacy issue: external integration directly inside
            // transactional business flow.
            var creditScore =
                new CreditBureauClient().GetCreditScore(nationalId);

            // Intentional questionable business coupling:
            // credit score is incorrectly being used as a proxy for KYC.
            if (creditScore >= 650)
            {
                customer.KycStatus = "APPROVED";
                repository.Save(customer);
            }

            LegacyLogger.Info(
                "Registered customer " +
                customer.CustomerNumber +
                " NationalId=" + customer.NationalId);

            return customer.Id;
        }
    }
}

using XWave.Core.Models;

namespace XWave.Core.Data.DatabaseSeeding.Factories;

public static class TestPaymentAccountFactory
{
    public static List<PaymentAccount> PaymentAccounts()
    {
        var accounts = new List<PaymentAccount>
        {
            new()
            {
                Provider = "mastercard",
                AccountNumber = "12345678",
                ExpiryDate = DateTime.ParseExact("2/5/2023", "d/M/yyyy", null)
            },
            new()
            {
                Provider = "visa",
                AccountNumber = "24681357",
                ExpiryDate = DateTime.ParseExact("1/2/2023", "d/M/yyyy", null)
            },
            new()
            {
                Provider = "visa",
                AccountNumber = "01010101",
                ExpiryDate = DateTime.ParseExact("1/8/2023", "d/M/yyyy", null)
            }
        };

        return accounts;
    }

    public static List<PaymentAccountDetails> PaymentAccountDetails(List<PaymentAccount> paymentAccounts, List<ApplicationUser> users)
    {
        var random = new Random();
        var userIds = users.Select(x => x.Id).ToList();
        var paymentAccountIds = paymentAccounts.Select(x => x.Id).ToList();
        var paymentDetail = new List<PaymentAccountDetails>
        {
            new()
            {
                PaymentAccountId = paymentAccountIds[random.Next(paymentAccountIds.Count)],
                CustomerId = userIds[random.Next(userIds.Count)],
                FirstRegistration = DateTime.Parse("5/1/2020")
            },
            new()
            {
                PaymentAccountId = paymentAccountIds[random.Next(paymentAccountIds.Count)],
                CustomerId = userIds[random.Next(userIds.Count)],
                FirstRegistration = DateTime.Parse("5/1/2020")
            },
            new()
            {
                CustomerId = userIds[random.Next(userIds.Count)],
                PaymentAccountId = paymentAccountIds[random.Next(paymentAccountIds.Count)],
                FirstRegistration = DateTime.Parse("5/7/2019")
            }
        };

        return paymentDetail
            .DistinctBy(x => new { x.PaymentAccountId, x.CustomerId })
            .ToList();
    }
}
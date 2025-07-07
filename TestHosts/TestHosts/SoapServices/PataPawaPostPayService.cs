using Shared.EntityFramework;

namespace TestHosts.SoapServices;

using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Database.PataPawa;
using DataTransferObjects;

public class PataPawaPostPayService : IPataPawaPostPayService
{
    private readonly IDbContextResolver<PataPawaContext> ContextResolver;

    #region Constructors

    public PataPawaPostPayService(IDbContextResolver<PataPawaContext> contextResolver) {
        this.ContextResolver = contextResolver;
    }

    #endregion

    #region Methods

    public LoginResponse Login(String username,
                               String password) {
        // Check if we have an api key
        using ResolvedDbContext<PataPawaContext>? resolvedContext = this.ContextResolver.Resolve("PataPawaReadModel");
        PostPaidAccount account = PataPawaPostPayService.GetPostPaidAccount(username, resolvedContext.Context);

        if (account == null) {
            // this is a first time request
            // Create an account
            account = PataPawaPostPayService.CreatePostPaidAccount(username, password, resolvedContext.Context);

            // return the key in the response
            return new LoginResponse {
                                         APIKey = account.ApiKey,
                                         Balance = account.Balance,
                                         Message = "Success",
                                         Status = 0
                                     };
        }

        // Validate the password
        if (account.Password == password) {
            // All Ok now
            return new LoginResponse {
                                         APIKey = account.ApiKey,
                                         Balance = account.Balance,
                                         Message = "Success",
                                         Status = 0
                                     };
        }

        // Incorrect username and password
        return new LoginResponse {
                                     Message = "Error",
                                     Status = -1
                                 };
    }

    public ProcessBillResponse ProcessBill(String username,
                                           String api_key,
                                           String account_no,
                                           String mobile_no,
                                           String customer_name,
                                           Decimal amount) {
        using ResolvedDbContext<PataPawaContext>? resolvedContext = this.ContextResolver.Resolve("PataPawaReadModel");
        PostPaidAccount account = PataPawaPostPayService.GetAccount(username, api_key, resolvedContext.Context);
        if (account == null) {
            // TODO: this might not be the correct way to respond in this case
            return new ProcessBillResponse {
                                               Status = -1,
                                               Message = "Account not found"
                                           };
        }

        PostPaidBill bill = PataPawaPostPayService.GetBill(account_no, resolvedContext.Context);

        if (bill == null) {
            // Bill not found
            // TODO: this might not be the correct way to respond in this case
            return new ProcessBillResponse {
                                               Status = -1,
                                               Message = $"Bill for account no [{account_no}] not found"
                                           };
        }

        PataPawaPostPayService.MakeBillPayment(amount, bill, resolvedContext.Context);

        // return the response
        return new ProcessBillResponse {
                                           AgentId = "PATAPAWA",
                                           Status = 0,
                                           Message = "Payment successful",
                                           ReceiptNumber = "PCEV143834",
                                           ResultCode = "post000",
                                           SmsId = "345"
                                       };
    }

    public VerifyResponse VerifyAccount(String username,
                                        String api_key,
                                        String account_no) {
        using ResolvedDbContext<PataPawaContext>? resolvedContext = this.ContextResolver.Resolve("PataPawaReadModel");
        PostPaidAccount account = PataPawaPostPayService.GetAccount(username, api_key, resolvedContext.Context);
            if (account == null) {
                // TODO: this might not be the correct way to respond in this case
                return new VerifyResponse {
                                              AccountNumber = null,
                                              AccountBalance = 0,
                                              AccountName = null,
                                              DueDate = DateTime.MinValue
                                          };
            }

            // We have now found an account, lets get the first due bill for the customer account number
            PostPaidBill bill = PataPawaPostPayService.GetBill(account_no, resolvedContext.Context);

            if (bill == null) {
                // Bill not found
                // TODO: this might not be the correct way to respond in this case
                return new VerifyResponse {
                                              AccountNumber = null,
                                              AccountBalance = 0,
                                              AccountName = null,
                                              DueDate = DateTime.MinValue
                                          };
            }

            // We have found a bill now return the info
            return new VerifyResponse {
                                          AccountBalance = bill.Amount,
                                          AccountName = bill.AccountName,
                                          AccountNumber = bill.AccountNumber,
                                          DueDate = bill.DueDate
                                      };
    }

    private static PostPaidAccount CreatePostPaidAccount(String username,
                                                         String password,
                                                         PataPawaContext context) {
        PostPaidAccount account = new PostPaidAccount {
                                                          Password = password,
                                                          UserName = username,
                                                          Balance = 0,
                                                          AccountId = Guid.NewGuid(),
                                                          ApiKey = PataPawaPostPayService.HashApiKey(Guid.NewGuid().ToString())
                                                      };
        context.PostPaidAccounts.Add(account);
        context.SaveChanges();
        return account;
    }

    private static PostPaidAccount GetAccount(String username,
                                              String api_key,
                                              PataPawaContext context) =>
        context.PostPaidAccounts.SingleOrDefault(a => a.UserName == username && a.ApiKey == api_key);

    private static PostPaidBill GetBill(String account_no,
                                        PataPawaContext context) =>
        context.PostPaidBills.OrderBy(p => p.DueDate).FirstOrDefault(p => p.AccountNumber == account_no);

    private static PostPaidAccount GetPostPaidAccount(String username,
                                                      PataPawaContext context) {
        PostPaidAccount account = context.PostPaidAccounts.SingleOrDefault(a => a.UserName == username);
        return account;
    }

    private static String HashApiKey(String input) {
        using(SHA1Managed sha1 = new SHA1Managed()) {
            Byte[] hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
            StringBuilder sb = new StringBuilder(hash.Length * 2);

            foreach (Byte b in hash) {
                // can be "x2" if you want lowercase
                sb.Append(b.ToString("X2"));
            }

            return sb.ToString();
        }
    }

    private static void MakeBillPayment(Decimal amount,
                                        PostPaidBill bill,
                                        PataPawaContext context) {
        // Pay the amount of the bill
        bill.Amount -= amount;

        if (bill.Amount == 0) {
            bill.IsFullyPaid = true;
        }

        context.Attach(bill);
        context.SaveChanges();
    }

    #endregion
}
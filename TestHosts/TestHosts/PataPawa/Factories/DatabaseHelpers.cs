using System;
using System.Collections.Generic;
using TestHosts.PataPawa.Database;

namespace TestHosts.PataPawa.Factories;

public class DatabaseHelpers {
            

    public static Database.Transaction CreatePendingTransaction(PrePayMeter meter) =>
        new()
        {
            Date = DateTime.Now,
            Status = 0,
            Messaage = "transaction still pending",
            IsPending = true,
            MeterNumber = meter.MeterNumber
        };

    public static Database.Transaction CreateTimeBasedFailedTransaction() => new() { Date = DateTime.Now, Status = 2, Messaage = "MTRFE013-M1: There is an insufficient amount (676.77) for the time based \\r\\ncharges (3132). Either tender more money or request less units.", IsPending = false };

    public static Database.Transaction CreateInsufficientFloatFailedTransaction() => new() { Date = DateTime.Now, Status = 2, Messaage = "MTRFE013-M1: There is an insufficient amount (676.77) for the time based \\r\\ncharges (3132). Either tender more money or request less units.", IsPending = false };

    public static Database.Transaction CreateTooSoonFailedTransaction() => new() { Date = DateTime.Now, Status = 1, Messaage = "This transaction has been done too soon from the last one. Please wait for a \\r\\nfew minutes.", IsPending = false };

    public static Database.Transaction CreateSuccessfulTransaction(PrePayMeter meter) =>
        new()
        {
            Status = 0,
            Messaage = Responses.Success,
            Vendor = "support",
            MeterNumber = meter.MeterNumber,
            ResultCode = "elec000",
            StandardTokenAmt = 64,
            StandardTokenTax = 0,
            Units = 6.1m,
            Token = Guid.NewGuid().ToString("N"),
            StandardTokenRctNum = "Ce001OVS3709952",
            Date = DateTime.Now,
            TotalAmount = 400,
            Charges = new List<TransactionCharge> {
                new TransactionCharge {
                    ERCCharge = 3.19m,
                    ForexCharge = 0.47m,
                    FuelIndexCharge = 2.47m,
                    InflationAdjustment = 0,
                    MonthlyFC = 13.27m,
                    REPCharge = 1.39m,
                    TotalTax = 15.21m
                }
            },
            CustomerName = meter.CustomerName,
            Reference = DateTime.Now.ToString("yyyyMMddhhmmsssfff"),
            IsPending = false
        };

    public static Database.Transaction CreateTransactionRecord(string amount, PrePayMeter meter)
    {
        Database.Transaction transaction = amount switch
        {
            "150" => CreatePendingTransaction(meter),
            "999" => CreateTimeBasedFailedTransaction(),
            "888" => CreateInsufficientFloatFailedTransaction(),
            "777" => CreateTooSoonFailedTransaction(),
            _ => CreateSuccessfulTransaction(meter)
        };

        return transaction;
    }
}
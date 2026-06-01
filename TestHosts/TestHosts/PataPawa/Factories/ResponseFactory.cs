using System.Collections.Generic;
using TestHosts.PataPawa.Database;
using TestHosts.PataPawa.DataTransferObjects;
using TestHosts.PataPawa.DataTransferObjects.PrePay;
using Transaction = TestHosts.PataPawa.DataTransferObjects.PrePay.Transaction;

namespace TestHosts.PataPawa.Factories;

public static class ResponseFactory {
    public static VendResponse CreateVendResponse(Database.Transaction transaction)
    {
        VendResponse response = new()
        {
            status = transaction.Status,
            msg = transaction.Messaage,
            transaction = new Transaction
            {
                transactionId = transaction.TransactionId,
                status = transaction.Status,
                vendor = transaction.Vendor,
                meterNo = transaction.MeterNumber,
                rescode = transaction.ResultCode,
                stdTokenAmt = transaction.StandardTokenAmt,
                stdTokenTax = transaction.StandardTokenTax.ToString(),
                units = transaction.Units.ToString(),
                token = transaction.Token,
                stdTokenRctNum = transaction.StandardTokenRctNum,
                date = transaction.Date.ToString("yyyy-MM-dd hh:mm:ss"),
                totalAmount = transaction.TotalAmount.ToString(),
                customerName = transaction.CustomerName,
                reference = transaction.Reference,
                Charges = new List<Fixed>()
            }
        };

        if (transaction.Charges != null)
        {
            foreach (TransactionCharge transactionCharge in transaction.Charges)
            {
                response.transaction.Charges.Add(new Fixed
                {
                    ForexCharge = transactionCharge.ForexCharge,
                    ERCCharge = transactionCharge.ERCCharge,
                    FuelIndexCharge = transactionCharge.FuelIndexCharge,
                    InflationAdjustment = transactionCharge.InflationAdjustment,
                    MonthlyFC = transactionCharge.MonthlyFC,
                    REPCharge = transactionCharge.REPCharge,
                    TotalTax = transactionCharge.TotalTax,
                });
            }
        }

        return response;
    }
}
namespace TestHosts.Database.PataPawa
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using TestHosts.Controllers;

    public class PrePayUser
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public Decimal Balance { get; set; }
        public string Key { get; set; }
    }

    public class PrePayMeter{
        public Guid MeterId { get; set; }
        public string MeterNumber { get; set; }
        public string CustomerName { get; set; }
    }

    public class Transaction
    {
        public string Messaage { get; set; }
        public int Status { get; set; }
        public string Vendor { get; set; }
        public string MeterNumber { get; set; }
        public string ResultCode { get; set; }
        public Decimal StandardTokenTax { get; set; }
        public Decimal StandardTokenAmt { get; set; }
        public Decimal Units { get; set; }
        public string Token { get; set; }
        public string StandardTokenRctNum { get; set; }
        public DateTime Date { get; set; }
        public Decimal TotalAmount { get; set; }
        public int TransactionId { get; set; }
        public virtual List<TransactionCharge> Charges { get; set; }
        public string Reference { get; set; }
        public string CustomerName { get; set; }
        public Boolean IsPending {get; set; }
    }

    public class TransactionCharge{
        
        public int TransactionId { get; set; }
        public int TransactionChargeId { get; set; }
        public Decimal REPCharge { get; set; }
        public Decimal MonthlyFC { get; set; }
        public Decimal ERCCharge { get; set; }
        public Decimal TotalTax { get; set; }
        public Decimal FuelIndexCharge { get; set; }
        public Decimal ForexCharge { get; set; }
        public int InflationAdjustment{ get; set; }
    }
}

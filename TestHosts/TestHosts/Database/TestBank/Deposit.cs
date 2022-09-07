namespace TestHosts.Database.TestBank;

using System;
using System.ComponentModel.DataAnnotations.Schema;

[Table("deposit")]
public class Deposit
{
    public Guid HostIdentifier { get; set; }
    public Guid DepositId { get; set; }
    public String SortCode { get; set; }
    public String AccountNumber { get; set; }
    public Decimal Amount { get; set; }
    public DateTime DateTime { get; set; }
    public String Reference { get; set; }
    public Boolean SentToHost { get; set; }
}
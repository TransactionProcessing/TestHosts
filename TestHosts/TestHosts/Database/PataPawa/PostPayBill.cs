namespace TestHosts.Database.PataPawa;

using System;
using System.ComponentModel.DataAnnotations.Schema;

[Table("postpaidbill")]
public class PostPaidBill
{
    public Guid PostPaidBillId { get; set; }
    public DateTime DueDate { get; set; }
    public Decimal Amount { get; set; }
    public String AccountNumber { get; set; }
    public String AccountName { get; set; }

    public Boolean IsFullyPaid { get; set; }
}
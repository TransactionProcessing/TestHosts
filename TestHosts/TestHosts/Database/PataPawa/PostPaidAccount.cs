namespace TestHosts.Database.PataPawa;

using System;
using System.ComponentModel.DataAnnotations.Schema;

[Table("postpaidaccounts")]
public class PostPaidAccount
{
    public Guid AccountId { get; set; }

    public String ApiKey { get; set; }

    public String UserName { get; set; }

    public String Password { get; set; }

    public Decimal Balance { get; set; }

    
}
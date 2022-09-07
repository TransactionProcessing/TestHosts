namespace TestHosts.Database.TestBank;

using System;
using System.ComponentModel.DataAnnotations.Schema;

[Table("hostconfiguration")]
public class HostConfiguration
{
    public Guid HostIdentifier { get; set; }

    public String CallbackUri { get; set; }

    public String SortCode { get; set; }

    public String AccountNumber { get; set; }
}
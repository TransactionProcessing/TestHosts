using System;
using System.ComponentModel.DataAnnotations;

namespace TestHosts.AgencyBanking.Database.Entities;

public class GoLiveRecord
{
    [Key]
    public long Id { get; set; }

    public string ApprovedBy { get; set; } = "";

    public string Environment { get; set; } = "";

    public DateTime GoLiveDate { get; set; }
}
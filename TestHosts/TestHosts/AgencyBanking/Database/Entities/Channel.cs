using System;
using System.ComponentModel.DataAnnotations;

namespace TestHosts.AgencyBanking.Database.Entities;

public class Channel
{
    [Key]
    public long Id { get; set; }

    public string ChannelCode { get; set; } = "";

    public string ChannelName { get; set; } = "";

    public bool Enabled { get; set; }

    public DateTime CreatedAt { get; set; }
}
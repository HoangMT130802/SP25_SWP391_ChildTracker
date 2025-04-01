﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace DataAccess.Entities;

public partial class UserMembership
{
    public int UserMembershipId { get; set; }

    public int UserId { get; set; }

    public int MembershipId { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public string Status { get; set; }

    public int RemainingConsultations { get; set; }

    public DateTime LastRenewalDate { get; set; }

    public virtual Membership Membership { get; set; }

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    public virtual User User { get; set; }
}
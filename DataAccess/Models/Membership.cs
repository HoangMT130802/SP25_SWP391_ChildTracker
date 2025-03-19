﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace DataAccess.Models;

public partial class Membership
{
    public int MembershipId { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public int Duration { get; set; }

    public decimal Price { get; set; }

    public int MaxChildren { get; set; }

    public int MaxConsultations { get; set; }

    public bool CanAccessConsultation { get; set; }

    public bool Status { get; set; }

    public virtual ICollection<UserMembership> UserMemberships { get; set; } = new List<UserMembership>();
}
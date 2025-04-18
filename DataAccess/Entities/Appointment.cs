﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace DataAccess.Entities;

public partial class Appointment
{
    public int AppointmentId { get; set; }

    public int ScheduleId { get; set; }

    public int UserId { get; set; }

    public int ChildId { get; set; }

    public string SlotTime { get; set; }

    public string Status { get; set; }

    public string MeetingLink { get; set; }

    public string Description { get; set; }

    public string Note { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Child Child { get; set; }

    public virtual ICollection<Rating> Ratings { get; set; } = new List<Rating>();

    public virtual DoctorSchedule Schedule { get; set; }

    public virtual User User { get; set; }
}
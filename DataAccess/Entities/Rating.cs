﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace DataAccess.Entities;

public partial class Rating
{
    public int RatingId { get; set; }

    public int UserId { get; set; }

    public int DoctorId { get; set; }

    public int AppointmentId { get; set; }

    public int Rating1 { get; set; }

    public string Comment { get; set; }

    public bool Status { get; set; }

    public virtual Appointment Appointment { get; set; }

    public virtual User Doctor { get; set; }

    public virtual User User { get; set; }
}
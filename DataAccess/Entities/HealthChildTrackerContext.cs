﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace DataAccess.Entities;

public partial class HealthChildTrackerContext : DbContext
{
    public HealthChildTrackerContext(DbContextOptions<HealthChildTrackerContext> options)
        : base(options)
    {
    }
    public HealthChildTrackerContext() { }
    public virtual DbSet<Appointment> Appointments { get; set; }

    public virtual DbSet<Blog> Blogs { get; set; }

    public virtual DbSet<Child> Children { get; set; }

    public virtual DbSet<ConsultationRequest> ConsultationRequests { get; set; }

    public virtual DbSet<ConsultationResponse> ConsultationResponses { get; set; }

    public virtual DbSet<DailyRecord> DailyRecords { get; set; }

    public virtual DbSet<DoctorProfile> DoctorProfiles { get; set; }

    public virtual DbSet<DoctorSchedule> DoctorSchedules { get; set; }

    public virtual DbSet<GrowthRecord> GrowthRecords { get; set; }

    public virtual DbSet<GrowthStandard> GrowthStandards { get; set; }

    public virtual DbSet<Membership> Memberships { get; set; }

    public virtual DbSet<Rating> Ratings { get; set; }

    public virtual DbSet<Transaction> Transactions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserMembership> UserMemberships { get; set; }

    public static string GetConnectionString(string connectionStringName)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json")
            .Build();

        string connectionString = config.GetConnectionString(connectionStringName);
        return connectionString;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer(GetConnectionString("DefaultConnection"));
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(e => e.AppointmentId).HasName("appointments_appointmentid_primary");

            entity.Property(e => e.ChildId).HasColumnName("child_id");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Description).IsRequired();
            entity.Property(e => e.MeetingLink)
                .IsRequired()
                .HasMaxLength(255);
            entity.Property(e => e.Note).IsRequired();
            entity.Property(e => e.SlotTime)
                .IsRequired()
                .HasMaxLength(50);
            entity.Property(e => e.Status)
                .IsRequired()
                .HasMaxLength(50);
            entity.Property(e => e.UserId).HasColumnName("user_Id");

            entity.HasOne(d => d.Child).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.ChildId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("appointments_child_id_foreign");

            entity.HasOne(d => d.Schedule).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.ScheduleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("appointments_scheduleid_foreign");

            entity.HasOne(d => d.User).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Appointments_Users_UserId");
        });

        modelBuilder.Entity<Blog>(entity =>
        {
            entity.HasKey(e => e.BlogId).HasName("blogs_blog_id_primary");

            entity.Property(e => e.BlogId).HasColumnName("blog_id");
            entity.Property(e => e.AuthorId).HasColumnName("author_id");
            entity.Property(e => e.Content)
                .IsRequired()
                .HasColumnName("content");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.ImageUrl)
                .IsRequired()
                .HasMaxLength(255);
            entity.Property(e => e.Likes).HasColumnName("likes");
            entity.Property(e => e.Status)
                .IsRequired()
                .HasMaxLength(50);
            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("title");
            entity.Property(e => e.Views).HasColumnName("views");

            entity.HasOne(d => d.Author).WithMany(p => p.Blogs)
                .HasForeignKey(d => d.AuthorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Blogs_Users_AuthorId");
        });

        modelBuilder.Entity<Child>(entity =>
        {
            entity.HasKey(e => e.ChildId).HasName("children_child_id_primary");

            entity.Property(e => e.ChildId).HasColumnName("child_id");
            entity.Property(e => e.AllergiesNotes).IsRequired();
            entity.Property(e => e.BirthDate)
                .HasColumnType("datetime")
                .HasColumnName("birth_date");
            entity.Property(e => e.BloodType)
                .IsRequired()
                .HasMaxLength(5);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.FullName)
                .IsRequired()
                .HasMaxLength(255);
            entity.Property(e => e.Gender)
                .IsRequired()
                .HasMaxLength(10)
                .HasColumnName("gender");
            entity.Property(e => e.MedicalHistory).IsRequired();
            entity.Property(e => e.ParentName)
                .IsRequired()
                .HasMaxLength(255);
            entity.Property(e => e.ParentNumber)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.UpdateAt).HasColumnType("datetime");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Children)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Children_Users_UserId");
        });

        modelBuilder.Entity<ConsultationRequest>(entity =>
        {
            entity.HasKey(e => e.RequestId).HasName("consultationrequests_requestid_primary");

            entity.HasIndex(e => e.LastActivityAt, "IX_ConsultationRequests_LastActivityAt");

            entity.HasIndex(e => e.Status, "IX_ConsultationRequests_Status");

            entity.Property(e => e.ClosedAt).HasColumnType("datetime");
            entity.Property(e => e.ClosedReason).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Description).IsRequired();
            entity.Property(e => e.LastActivityAt)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Status)
                .IsRequired()
                .HasMaxLength(50);

            entity.HasOne(d => d.AssignedDoctor).WithMany(p => p.ConsultationRequestAssignedDoctors)
                .HasForeignKey(d => d.AssignedDoctorId)
                .HasConstraintName("FK_ConsultationRequests_Doctor");

            entity.HasOne(d => d.Child).WithMany(p => p.ConsultationRequests)
                .HasForeignKey(d => d.ChildId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("consultationrequests_childid_foreign");

            entity.HasOne(d => d.User).WithMany(p => p.ConsultationRequestUsers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<ConsultationResponse>(entity =>
        {
            entity.HasKey(e => e.ResponseId).HasName("consultationresponses_responseid_primary");

            entity.Property(e => e.Attachments).IsRequired();
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Response).IsRequired();
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Doctor).WithMany(p => p.ConsultationResponses)
                .HasForeignKey(d => d.DoctorId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Request).WithMany(p => p.ConsultationResponses)
                .HasForeignKey(d => d.RequestId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("consultationresponses_requestid_foreign");
        });

        modelBuilder.Entity<DailyRecord>(entity =>
        {
            entity.HasKey(e => e.RecordId).HasName("dailyrecords_recordid_primary");

            entity.Property(e => e.Note).IsRequired();
            entity.Property(e => e.SleepHours).HasColumnType("decimal(8, 2)");

            entity.HasOne(d => d.Child).WithMany(p => p.DailyRecords)
                .HasForeignKey(d => d.ChildId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("dailyrecords_childid_foreign");
        });

        modelBuilder.Entity<DoctorProfile>(entity =>
        {
            entity.HasKey(e => e.DoctorProfileId).HasName("doctorprofiles_doctorprofileid_primary");

            entity.Property(e => e.AverageRating).HasColumnType("decimal(3, 2)");
            entity.Property(e => e.Biography).IsRequired();
            entity.Property(e => e.LicenseNumber)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Qualification)
                .IsRequired()
                .HasMaxLength(255);
            entity.Property(e => e.Specialization)
                .IsRequired()
                .HasMaxLength(255);

            entity.HasOne(d => d.User).WithMany(p => p.DoctorProfiles)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<DoctorSchedule>(entity =>
        {
            entity.HasKey(e => e.ScheduleId).HasName("doctorschedule_scheduleid_primary");

            entity.ToTable("DoctorSchedule");

            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.DoctorId).HasColumnName("doctor_id");
            entity.Property(e => e.SelectedSlots)
                .IsRequired()
                .HasMaxLength(100);
            entity.Property(e => e.Status)
                .IsRequired()
                .HasMaxLength(50);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Doctor).WithMany(p => p.DoctorSchedules)
                .HasForeignKey(d => d.DoctorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DoctorSchedule_Users_DoctorId");
        });

        modelBuilder.Entity<GrowthRecord>(entity =>
        {
            entity.HasKey(e => e.RecordId).HasName("growthrecords_recordid_primary");

            entity.Property(e => e.Bmi)
                .HasColumnType("decimal(8, 2)")
                .HasColumnName("BMI");
            entity.Property(e => e.ChildId).HasColumnName("child_id");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("Created_at");
            entity.Property(e => e.HeadCircumference).HasColumnType("decimal(8, 2)");
            entity.Property(e => e.Height)
                .HasColumnType("decimal(8, 2)")
                .HasColumnName("height");
            entity.Property(e => e.Note).IsRequired();
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("datetime")
                .HasColumnName("Updated_at");
            entity.Property(e => e.Weight)
                .HasColumnType("decimal(8, 2)")
                .HasColumnName("weight");

            entity.HasOne(d => d.Child).WithMany(p => p.GrowthRecords)
                .HasForeignKey(d => d.ChildId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("growthrecords_child_id_foreign");
        });

        modelBuilder.Entity<GrowthStandard>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("growthstandards_id_primary");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Gender)
                .IsRequired()
                .HasMaxLength(10);
            entity.Property(e => e.Measurement)
                .IsRequired()
                .HasMaxLength(50);
            entity.Property(e => e.Median).HasColumnType("decimal(8, 2)");
            entity.Property(e => e.Sd1neg)
                .HasColumnType("decimal(8, 2)")
                .HasColumnName("SD1neg");
            entity.Property(e => e.Sd1pos)
                .HasColumnType("decimal(8, 2)")
                .HasColumnName("SD1pos");
            entity.Property(e => e.Sd2neg)
                .HasColumnType("decimal(8, 2)")
                .HasColumnName("SD2neg");
            entity.Property(e => e.Sd2pos)
                .HasColumnType("decimal(8, 2)")
                .HasColumnName("SD2pos");
            entity.Property(e => e.Sd3neg)
                .HasColumnType("decimal(8, 2)")
                .HasColumnName("SD3neg");
            entity.Property(e => e.Sd3pos)
                .HasColumnType("decimal(8, 2)")
                .HasColumnName("SD3pos");
        });

        modelBuilder.Entity<Membership>(entity =>
        {
            entity.HasKey(e => e.MembershipId).HasName("memberships_membershipid_primary");

            entity.Property(e => e.Description).IsRequired();
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);
            entity.Property(e => e.Price).HasColumnType("decimal(8, 2)");
        });

        modelBuilder.Entity<Rating>(entity =>
        {
            entity.HasKey(e => e.RatingId).HasName("ratings_ratingid_primary");

            entity.Property(e => e.Comment).IsRequired();
            entity.Property(e => e.Rating1).HasColumnName("Rating");

            entity.HasOne(d => d.Appointment).WithMany(p => p.Ratings)
                .HasForeignKey(d => d.AppointmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("ratings_appointmentid_foreign");

            entity.HasOne(d => d.Doctor).WithMany(p => p.RatingDoctors)
                .HasForeignKey(d => d.DoctorId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.User).WithMany(p => p.RatingUsers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.TransactionId).HasName("transactions_transaction_id_primary");

            entity.Property(e => e.TransactionId).HasColumnName("transaction_id");
            entity.Property(e => e.Amount)
                .HasColumnType("decimal(8, 2)")
                .HasColumnName("amount");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).IsRequired();
            entity.Property(e => e.PaymentMethod)
                .IsRequired()
                .HasMaxLength(50);
            entity.Property(e => e.TransactionCode)
                .IsRequired()
                .HasMaxLength(100);
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Transactions_Users_UserId");

            entity.HasOne(d => d.UserMembership).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.UserMembershipId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("transactions_usermembershipid_foreign");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Address)
                .IsRequired()
                .HasMaxLength(255);
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.FullName)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("full_name");
            entity.Property(e => e.Password)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("password");
            entity.Property(e => e.Phone)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("phone");
            entity.Property(e => e.Role)
                .IsRequired()
                .HasMaxLength(50);
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
            entity.Property(e => e.Username)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("username");
        });

        modelBuilder.Entity<UserMembership>(entity =>
        {
            entity.HasKey(e => e.UserMembershipId).HasName("usermemberships_usermembershipid_primary");

            entity.Property(e => e.EndDate).HasColumnType("datetime");
            entity.Property(e => e.LastRenewalDate).HasColumnType("datetime");
            entity.Property(e => e.StartDate).HasColumnType("datetime");
            entity.Property(e => e.Status)
                .IsRequired()
                .HasMaxLength(50);

            entity.HasOne(d => d.Membership).WithMany(p => p.UserMemberships)
                .HasForeignKey(d => d.MembershipId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("usermemberships_membershipid_foreign");

            entity.HasOne(d => d.User).WithMany(p => p.UserMemberships)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
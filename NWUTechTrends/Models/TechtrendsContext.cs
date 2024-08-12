using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace NWUTechTrends.Models;

public partial class TechtrendsContext : DbContext
{
    public TechtrendsContext()
    {
    }

    public TechtrendsContext(DbContextOptions<TechtrendsContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Client> Clients { get; set; }

    public virtual DbSet<JobTelemetry> JobTelemetries { get; set; }

    public virtual DbSet<Process> Processes { get; set; }

    public virtual DbSet<Project> Projects { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=ConnectionStrings:DefaultConnection");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Client>(entity =>
        {
            entity.ToTable("Client", "Config");

            entity.Property(e => e.ClientId)
                .ValueGeneratedNever()
                .HasColumnName("ClientID");
            entity.Property(e => e.DateOnboarded).HasColumnType("datetime");
        });

        modelBuilder.Entity<JobTelemetry>(entity =>
        {
            entity.ToTable("JobTelemetry", "Telemetry");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.AdditionalInfo).IsUnicode(false);
            entity.Property(e => e.BusinessFunction).IsUnicode(false);
            entity.Property(e => e.EntryDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ExcludeFromTimeSaving).HasDefaultValue(false);
            entity.Property(e => e.Geography).IsUnicode(false);
            entity.Property(e => e.JobId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("JobID");
            entity.Property(e => e.ProccesId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("ProccesID");
            entity.Property(e => e.QueueId)
                .IsUnicode(false)
                .HasColumnName("QueueID");
            entity.Property(e => e.StepDescription).IsUnicode(false);
            entity.Property(e => e.UniqueReference).IsUnicode(false);
            entity.Property(e => e.UniqueReferenceType).IsUnicode(false);
        });

        modelBuilder.Entity<Process>(entity =>
        {
            entity.ToTable("Process", "Config");

            entity.Property(e => e.ProcessId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("ProcessID");
            entity.Property(e => e.DateSubmitted)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DefaultBusinessFunction)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasDefaultValue("Unspecified");
            entity.Property(e => e.DefaultGeography)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasDefaultValue("Global");
            entity.Property(e => e.Platform)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.ProcessConfigUrl)
                .IsUnicode(false)
                .HasColumnName("ProcessConfigURL");
            entity.Property(e => e.ProcessName).IsUnicode(false);
            entity.Property(e => e.ProcessType).IsUnicode(false);
            entity.Property(e => e.ProjectId).HasColumnName("ProjectID");
            entity.Property(e => e.ReportUrl)
                .IsUnicode(false)
                .HasColumnName("ReportURL");
            entity.Property(e => e.Submitter).IsUnicode(false);
        });

        modelBuilder.Entity<Project>(entity =>
        {
            entity.ToTable("Project", "Config");

            entity.Property(e => e.ProjectId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("ProjectID");
            entity.Property(e => e.ClientId).HasColumnName("ClientID");
            entity.Property(e => e.ProjectCreationDate)
                .HasDefaultValueSql("(dateadd(hour,(2),getdate()))")
                .HasColumnType("datetime");
            entity.Property(e => e.ProjectDescription)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.ProjectName)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.ProjectStatus)
                .HasMaxLength(255)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

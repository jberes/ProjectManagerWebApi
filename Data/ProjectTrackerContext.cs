#nullable disable
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using ProjectManagerWebApi.Models;

namespace ProjectManagerWebApi.Data
{
    public partial class ProjectTrackerContext : DbContext
    {
        public ProjectTrackerContext()
        {
        }

        public ProjectTrackerContext(DbContextOptions<ProjectTrackerContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Projects> Projects { get; set; }
        public virtual DbSet<Tasks> Tasks { get; set; }
        public virtual DbSet<VwTasksProjects> VwTasksProjects { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Projects>(entity =>
            {
                entity.HasKey(e => e.ProjectId);

                entity.Property(e => e.DateAdded)
                    .HasColumnType("datetime")
                    .HasComputedColumnSql("(getdate())", false);

                entity.Property(e => e.ProjectName)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Tasks>(entity =>
            {
                entity.HasKey(e => e.TaskId);

                entity.Property(e => e.AssignedToEmail)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.DateAdded)
                    .HasColumnType("datetime")
                    .HasComputedColumnSql("(getdate())", false);

                entity.Property(e => e.DateDue).HasColumnType("date");

                entity.Property(e => e.DateUpdated).HasColumnType("date");

                entity.Property(e => e.TaskName)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.HasOne(d => d.Project)
                    .WithMany(p => p.Tasks)
                    .HasForeignKey(d => d.ProjectId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Tasks_Projects");
            });

            modelBuilder.Entity<VwTasksProjects>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("vw_Tasks_Projects");

                entity.Property(e => e.AssignedToEmail)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.DateAdded).HasColumnType("datetime");

                entity.Property(e => e.DateDue).HasColumnType("date");

                entity.Property(e => e.DateUpdated).HasColumnType("date");

                entity.Property(e => e.ProjectName)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.TaskName)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);
            });

            OnModelCreatingGeneratedProcedures(modelBuilder);
            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
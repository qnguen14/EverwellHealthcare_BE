using Microsoft.EntityFrameworkCore;
using System;

namespace Everwell.DAL.Data.Entities
{
    public class EverwellDbContext : DbContext
    {
        public EverwellDbContext(DbContextOptions<EverwellDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("EverWellDB_v1");
            base.OnModelCreating(modelBuilder);

            // Post - User (Staff)
            modelBuilder.Entity<Post>()
                .HasOne(p => p.Staff)
                .WithMany(u => u.Posts)
                .HasForeignKey(p => p.StaffId)
                .OnDelete(DeleteBehavior.Restrict);

            // Appointment relationships
            modelBuilder.Entity<Appointment>(entity =>
            {
                entity.HasOne(a => a.Customer)
                    .WithMany()
                    .HasForeignKey(a => a.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.Consultant)
                    .WithMany()
                    .HasForeignKey(a => a.ConsultantId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.Service)
                    .WithMany()
                    .HasForeignKey(a => a.ServiceId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Feedback relationships
            modelBuilder.Entity<Feedback>(entity =>
            {
                entity.HasOne(f => f.Customer)
                    .WithMany()
                    .HasForeignKey(f => f.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(f => f.Consultant)
                    .WithMany()
                    .HasForeignKey(f => f.ConsultantId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(f => f.Service)
                    .WithMany()
                    .HasForeignKey(f => f.ServiceId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // STITesting relationships
            modelBuilder.Entity<STITesting>(entity =>
            {
                entity.HasOne(s => s.Appointment)
                    .WithMany()
                    .HasForeignKey(s => s.AppointmentId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(s => s.Customer)
                    .WithMany(u => u.STITests)
                    .HasForeignKey(s => s.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // TestResult relationships
            modelBuilder.Entity<TestResult>(entity =>
            {
                entity.HasOne(tr => tr.STITesting)
                    .WithMany(sti => sti.TestResults)
                    .HasForeignKey(tr => tr.STITestingId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(tr => tr.Customer)
                    .WithMany()
                    .HasForeignKey(tr => tr.CustomerId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(tr => tr.Staff)
                    .WithMany()
                    .HasForeignKey(tr => tr.StaffId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // MenstrualCycleTracking relationships
            modelBuilder.Entity<MenstrualCycleTracking>(entity =>
            {
                entity.HasOne(m => m.Customer)
                    .WithMany()
                    .HasForeignKey(m => m.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasMany(m => m.Notifications)
                    .WithOne(n => n.Tracking)
                    .HasForeignKey(n => n.TrackingId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // MenstrualCycleNotification relationships
            modelBuilder.Entity<MenstrualCycleNotification>(entity =>
            {
                entity.HasOne(n => n.Tracking)
                    .WithMany(t => t.Notifications)
                    .HasForeignKey(n => n.TrackingId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Question relationships
            modelBuilder.Entity<Question>(entity =>
            {
                entity.HasOne(q => q.Customer)
                    .WithMany()
                    .HasForeignKey(q => q.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(q => q.Consultant)
                    .WithMany()
                    .HasForeignKey(q => q.ConsultantId)
                    .OnDelete(DeleteBehavior.SetNull);
            });
            
            // ConsultantSchedule relationships and unique constraint
            modelBuilder.Entity<ConsultantSchedule>(entity =>
            {
                entity.HasOne(cs => cs.Consultant)
                    .WithMany()
                    .HasForeignKey(cs => cs.ConsultantId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(cs => new { cs.ConsultantId, cs.WorkDate, cs.ShiftSlot })
                    .IsUnique();
            });
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<STITesting> STITests { get; set; }
        public DbSet<ConsultantSchedule> ConsultantSchedules { get; set; }
    }
}

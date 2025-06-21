using Microsoft.EntityFrameworkCore;
using System;

namespace Everwell.DAL.Data.Entities
{
    public class EverwellDbContext : DbContext
    {
        public EverwellDbContext(DbContextOptions<EverwellDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("EverWellDB_v2.5");
            base.OnModelCreating(modelBuilder);
            
            
            // User - Role relationship
            modelBuilder.Entity<User>(entity =>
            {
                entity
                    .HasOne(u => u.Role)
                    .WithMany()
                    .HasForeignKey(u => u.RoleId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            
            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.Property(r => r.Name)
                    .HasConversion<string>()
                    .IsRequired();
            });
                
            
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasMany(u => u.STITests)
                    .WithOne(s => s.Customer)
                    .HasForeignKey(s => s.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            

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
            });

            // STITesting relationships
            modelBuilder.Entity<STITesting>(entity =>
            {
                entity.HasOne(s => s.Customer)
                      .WithMany(u => u.STITests)
                      .HasForeignKey(s => s.CustomerId)
                      .OnDelete(DeleteBehavior.Restrict);
    
                entity.HasMany(s => s.TestResults)
                    .WithOne(tr => tr.STITesting)
                    .HasForeignKey(tr => tr.STITestingId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.Property(s => s.TotalPrice)
                    .HasColumnType("decimal(18,2)");

                entity.Property(s => s.TestPackage)
                    .HasConversion<string>();
                
                entity.Property(s => s.Status)
                    .HasConversion<string>();

            });

            // TestResult relationships
            modelBuilder.Entity<TestResult>(entity =>
            {
                entity.HasOne(tr => tr.STITesting)
                    .WithMany(sti => sti.TestResults)
                    .HasForeignKey(tr => tr.STITestingId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(tr => tr.Staff)
                    .WithMany(tr => tr.TestResultsExamined)
                    .HasForeignKey(tr => tr.StaffId)
                    .OnDelete(DeleteBehavior.SetNull);
                
                entity.Property(tr => tr.Outcome)
                    .HasConversion<string>();
                
                entity.Property(tr => tr.Parameter)
                    .HasConversion<string>();
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

                entity.HasIndex(cs => new { cs.ConsultantId, cs.WorkDate, cs.Slot })
                    .IsUnique();
            });

            // BlacklistedToken configuration
            modelBuilder.Entity<BlacklistedToken>(entity =>
            {
                entity.HasKey(bt => bt.Id);
                entity.HasIndex(bt => bt.TokenHash).IsUnique();
                entity.HasIndex(bt => bt.ExpiresAt);
            });
            
            // Notification relationships
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasOne(n => n.Customer)
                    .WithMany(u => u.Notifications)
                    .HasForeignKey(n => n.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(n => n.Appointment)
                    .WithMany()
                    .HasForeignKey(n => n.AppointmentId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(n => n.TestResult)
                    .WithMany()
                    .HasForeignKey(n => n.TestResultId)
                    .OnDelete(DeleteBehavior.SetNull);
                
                entity.HasOne(n => n.STITesting)
                    .WithMany()
                    .HasForeignKey(n => n.STITestingId)
                    .OnDelete(DeleteBehavior.SetNull);

                // Create indexes for common query patterns
                entity.HasIndex(n => n.UserId);
                entity.HasIndex(n => n.IsRead);
                entity.HasIndex(n => n.CreatedAt);
            });
            
            
        }

        public DbSet<User> Users { get; set; }
        // public DbSet<Service> Services { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<STITesting> STITests { get; set; }
        public DbSet<TestResult> TestResults { get; set; }
        public DbSet<MenstrualCycleTracking> MenstrualCycleTrackings { get; set; }
        public DbSet<MenstrualCycleNotification> MenstrualCycleNotifications { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<BlacklistedToken> BlacklistedTokens { get; set; }
        public DbSet<ConsultantSchedule> ConsultantSchedules { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Role> Roles { get; set; }
        public virtual DbSet<PaymentTransaction> PaymentTransactions { get; set; }
    }
}
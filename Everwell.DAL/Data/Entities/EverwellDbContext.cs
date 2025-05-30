using Microsoft.EntityFrameworkCore;
using System;

namespace Everwell.DAL.Data.Entities
{
    public class EverwellDbContext : DbContext
    {
        public EverwellDbContext(DbContextOptions<EverwellDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Post>()
                .HasOne(p => p.Staff)
                .WithMany(u => u.Posts) // if you've added ICollection<Post> in User
                .HasForeignKey(p => p.StaffId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Appointment>(entity =>
            {
                // Relationships
                entity.HasOne(a => a.Customer)
                    .WithMany() // Assuming User does not have ICollection<Appointment> CustomerAppointments
                    .HasForeignKey(a => a.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.Service)
                    .WithMany() // Assuming Service does not have ICollection<Appointment>
                    .HasForeignKey(a => a.ServiceId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.Consultant)
                    .WithMany() // Assuming User does not have ICollection<Appointment> ConsultantAppointments
                    .HasForeignKey(a => a.ConsultantId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Feedback>(entity =>
            {
                entity.HasOne(f => f.Customer)
                    .WithMany() // or .WithMany(u => u.Feedbacks) if User has ICollection<Feedback>
                    .HasForeignKey(f => f.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(f => f.Consultant)
                    .WithMany() // or .WithMany(u => u.ConsultantFeedbacks) if User has ICollection<Feedback>
                    .HasForeignKey(f => f.ConsultantId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(f => f.Service)
                    .WithMany() // or .WithMany(s => s.Feedbacks) if Service has ICollection<Feedback>
                    .HasForeignKey(f => f.ServiceId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }


        public DbSet<User> Users { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<STITesting> STITests { get; set; }



    }



}

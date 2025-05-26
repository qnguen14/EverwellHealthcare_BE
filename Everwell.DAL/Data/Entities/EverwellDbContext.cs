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
        }


        public DbSet<User> Users { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Post> Posts { get; set; }


        
    }



}

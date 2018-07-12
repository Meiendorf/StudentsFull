using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Students.Models;

namespace Students.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public DbSet<Student> Students { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Pair> Pairs { get; set; }
        public DbSet<Professor> Professors { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<Student>()
                .HasOne(p => p.Group)
                .WithMany(b => b.Students)
                .HasForeignKey(p => p.GroupId);
            base.OnModelCreating(modelBuilder);
        }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
            
            Database.EnsureCreated();
            
        }
    }
}

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using MeePoint.Models;

namespace MeePoint.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Entity> Entities { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<User> RegisteredUsers { get; set; }
        public DbSet<Meeting> Meetings { get; set; }
        public DbSet<Convocation> Convocations { get; set; }
        public DbSet<GroupMember> GroupMembers { get; set; }
        public DbSet<Document> Documents { get; set; }

        //FluentAPI for table customization
        protected override void OnModelCreating(ModelBuilder builder)
        {

            base.OnModelCreating(builder);

            //GroupMember
            builder.Entity<GroupMember>()
            .HasKey(gp => new { gp.GroupID, gp.UserID });

            builder.Entity<GroupMember>()
            .HasOne(gp => gp.User)
            .WithMany(u => u.Groups)
            .HasForeignKey(gp => gp.UserID)
            .IsRequired();

            builder.Entity<GroupMember>()
            .HasOne(gp => gp.Group)
            .WithMany(g => g.Members)
            .HasForeignKey(gp => gp.GroupID)
            .IsRequired();

            //Convocation
            builder.Entity<Convocation>()
            .HasKey(c => new { c.MeetingID, c.UserID });

            builder.Entity<Convocation>()
            .HasOne(c => c.Meeting)
            .WithMany(m => m.Convocations)
            .HasForeignKey(c => c.MeetingID)
            .IsRequired();

            builder.Entity<Convocation>()
            .HasOne(c => c.User)
            .WithMany(u => u.Convocations)
            .HasForeignKey(c => c.UserID)
            .IsRequired();

            builder.Entity<Convocation>()
            .Property(c => c.Answer)
            .IsRequired();

            //Entity
            builder.Entity<Entity>()
            .HasIndex(e => e.NIF)
            .IsUnique();

            builder.Entity<Entity>()
            .HasIndex(e => e.Name)
            .IsUnique();

            builder.Entity<Entity>()
            .Property(e => e.NIF)
            .IsRequired();

            builder.Entity<Entity>()
            .Property(e => e.Name)
            .IsRequired();

            //Group
            builder.Entity<Group>()
            .HasIndex(g => g.Name)
            .IsUnique();

            builder.Entity<Group>()
            .Property(g => g.Name)
            .IsRequired();

            //User
            builder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

            builder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();

            builder.Entity<User>()
            .HasIndex(u => u.PasswordHash)
            .IsUnique();

            builder.Entity<User>()
            .Property(u => u.Email)
            .IsRequired();

            builder.Entity<User>()
            .Property(u => u.Username)
            .IsRequired();

            builder.Entity<User>()
            .Property(u => u.PasswordHash)
            .IsRequired();

            //Meeting
            builder.Entity<Meeting>()
            .Property(m => m.Quorum)
            .IsRequired();

            builder.Entity<Meeting>()
            .Property(m => m.MeetingDate)
            .IsRequired();

            //Document
            builder.Entity<Document>()
            .Property(d => d.DocumentPath)
            .IsRequired();

        }

    }
}

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
        public DbSet<RegisteredUser> RegisteredUsers { get; set; }
        public DbSet<Meeting> Meetings { get; set; }
        public DbSet<Convocation> Convocations { get; set; }
        public DbSet<GroupMember> GroupMembers { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<ChatMessage> Messages { get; set; }
        public DbSet<SpeakRequest> SpeakRequests { get; set; }

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
            ;

            builder.Entity<Entity>()
            .Property(e => e.NIF)
            .IsRequired();

            builder.Entity<Entity>()
            .Property(e => e.Name)
            .IsRequired();

            //Group
            builder.Entity<Group>()
            .Property(g => g.Name)
            .IsRequired();

            //User
            builder.Entity<RegisteredUser>()
            .HasIndex(u => u.Email)
            .IsUnique();

            builder.Entity<RegisteredUser>()
            .HasIndex(u => u.Username)
            .IsUnique();

            builder.Entity<RegisteredUser>()
            .HasIndex(u => u.PasswordHash)
            .IsUnique();

            builder.Entity<RegisteredUser>()
            .Property(u => u.Email)
            .IsRequired();

            builder.Entity<RegisteredUser>()
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

            //Message
            builder.Entity<ChatMessage>(cm =>
            {
                cm.Property(c => c.MeetingID).IsRequired();
                cm.Property(c => c.Text).IsRequired();
                cm.Property(c => c.Sender).IsRequired();
                cm.Property(c => c.Timestamp).IsRequired();
            });

            builder.Entity<ChatMessage>()
                .HasKey(c => c.MessageID);
                

        }

    }
}

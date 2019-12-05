using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace RescueTinder.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Dog>().HasOne(d => d.Owner)
                .WithMany(o => o.Dogs)
                .HasForeignKey(d => d.OwnerId);

            builder.Entity<Like>()
                .HasKey(k => new
                {
                    k.UserId,
                    k.DogId
                });

            builder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany(s => s.SentMessages)
                .HasForeignKey(m => m.SenderId);

            builder.Entity<Message>()
                .HasOne(m => m.Receiver)
                .WithMany(r => r.ReceivedMessages)
                .HasForeignKey(m => m.ReceiverId);

            builder.Entity<Dog>()
                .HasOne(d => d.Vet)
                .WithMany(v => v.DogPatients)
                .HasForeignKey(d => d.VetId);
        }

        public DbSet<Dog> Dogs { get; set; }

        public DbSet<User> Users { get; set; }

        public DbSet<Message> Messages { get; set; }

        public DbSet<Like> Likes { get; set; }

        public DbSet<VetNote> VetNotes { get; set; }

        public DbSet<Pic> Pics { get; set; }
    }
}

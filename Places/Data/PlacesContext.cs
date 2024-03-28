using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Places.Migrations;
using Places.Models;
using System.Diagnostics;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Places.Data
{
    public class PlacesContext : DbContext
    {
        public PlacesContext(DbContextOptions<PlacesContext> options) : base(options)
        {

        }
        public DbSet<UserProfile> UserProfile { get; set; }
        public DbSet<Location> Location { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<UserProfileEvent> UserProfileEvents { get; set; }

        public DbSet<FriendRequest> FriendsRequest { get; set; }
        public DbSet<Friend> Friends { get; set; }



        public DbSet<Chat> Chats { get; set; }

        public DbSet<Message> Messages { get; set; }

        public DbSet<Feedback> Feedbacks { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<UserProfileEvent>()
        .HasOne(e => e.UserProfile)
        .WithMany()
        .HasForeignKey(e => e.UserProfileId)
        .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<Friend>()
     .HasOne(f => f.User)
     .WithMany(u => u.Friends) // Update to include navigation property
     .HasForeignKey(f => f.UserId)
     .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Friend>()
                .HasOne(f => f.FriendUser)
                .WithMany()
                .HasForeignKey(f => f.FriendId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure FriendRequest
            modelBuilder.Entity<FriendRequest>()
                .HasOne(fr => fr.Sender)
                .WithMany(u => u.SentFriendRequests) // Update to include navigation property
                .HasForeignKey(fr => fr.SenderId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<FriendRequest>()
                .HasOne(fr => fr.Receiver)
                .WithMany(u => u.ReceivedFriendRequests) // Update to include navigation property
                .HasForeignKey(fr => fr.ReceiverId)
                .OnDelete(DeleteBehavior.NoAction);




        }
    }
}

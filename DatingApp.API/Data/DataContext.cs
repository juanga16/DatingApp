using DatingApp.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data
{
    public class DataContext : IdentityDbContext<User, Role, int, IdentityUserClaim<int>, UserRole, IdentityUserLogin<int>, IdentityRoleClaim<int>,
        IdentityUserToken<int>>
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        public DbSet<Value> Values { get; set; }
        public DbSet<Photo> Photos { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<Message> Messages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserRole>(x => 
            {
                x.HasKey(y => new { y.UserId, y.RoleId});
                x.HasOne(y => y.Role)
                    .WithMany(y => y.UserRoles)
                    .HasForeignKey(y => y.RoleId)
                    .IsRequired();                
                x.HasOne(y => y.User)
                    .WithMany(y => y.UserRoles)
                    .HasForeignKey(y => y.UserId)
                    .IsRequired();
            });

            modelBuilder.Entity<Like>()
                        .HasKey(x => new { x.LikerId, x.LikeeId});
            
            modelBuilder.Entity<Like>()
                        .HasOne(x => x.Likee)
                        .WithMany(x => x.Likers)
                        .HasForeignKey(x => x.LikeeId)
                        .OnDelete(DeleteBehavior.Restrict);
            
            modelBuilder.Entity<Like>()
                        .HasOne(x => x.Liker)
                        .WithMany(x => x.Likees)
                        .HasForeignKey(x => x.LikerId)
                        .OnDelete(DeleteBehavior.Restrict);
            
            modelBuilder.Entity<Message>()
                        .HasOne(x => x.Sender)
                        .WithMany(x => x.MessagesSent)
                        .HasForeignKey(x => x.SenderId)
                        .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Message>()
                        .HasOne(x => x.Recipient)
                        .WithMany(x => x.MessagesReceived)
                        .HasForeignKey(x => x.RecipientId)
                        .OnDelete(DeleteBehavior.Restrict);
            
            modelBuilder.Entity<Photo>().HasQueryFilter(x => x.IsApproved);
        }
    }
}
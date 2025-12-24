using Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace Api.Data
{
    public static class DataSeed
    {
        public static void SeedRoles(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<IdentityRole>().HasData(new[]
            {
            new IdentityRole { Id = "1", Name = "Admin", NormalizedName = "ADMIN" },
            new IdentityRole { Id = "2", Name = "User", NormalizedName = "USER" }
        });
        }

        public static void SeedAdmin(this ModelBuilder modelBuilder)
        {
            const string adminId = "1";
            const string adminEmail = "vladusmoroz@gmail.com";
            var hasher = new PasswordHasher<User>();
            var admin = new User
            {
                Id = adminId,
                EmailConfirmed = true,
                UserName = adminEmail,
                NormalizedUserName = adminEmail.ToUpper(),
                FirstName = "Vladyslav",
                LastName = "Moroz",
                Email = adminEmail,
                NormalizedEmail = adminEmail.ToUpper(),
                PhoneNumber = "+380955638293",
                SecurityStamp = Guid.NewGuid().ToString()
            };
            admin.PasswordHash = hasher.HashPassword(admin, "qwerty12345");

            modelBuilder.Entity<User>().HasData(admin);

            modelBuilder.Entity<IdentityUserRole<string>>().HasData(new[]
            {
            new IdentityUserRole<string> { UserId = adminId, RoleId = "1" },
            new IdentityUserRole<string> { UserId = adminId, RoleId = "2" }
            });


             const string userId = "2";
            const string userEmail = "randomuser@example.com";

            var user = new User
            {
                Id = userId,
                EmailConfirmed = true,
                UserName = userEmail,
                NormalizedUserName = userEmail.ToUpper(),
                FirstName = "Random",
                LastName = "User",
                Email = userEmail,
                NormalizedEmail = userEmail.ToUpper(),
                PhoneNumber = "+380955638294",
                SecurityStamp = Guid.NewGuid().ToString()
            };
            user.PasswordHash = hasher.HashPassword(user, "randomPassword123");

            modelBuilder.Entity<User>().HasData(user);

            modelBuilder.Entity<IdentityUserRole<string>>().HasData(new[]
            {
            new IdentityUserRole<string> { UserId = userId, RoleId = "2" }
        });
        }
    }
}

using System;
using System.Linq;
using MediKartX.Infrastructure.Data;

namespace MediKartX.Infrastructure.Data;

public static class DataSeeder
{
    public static void Seed(MediKartXDbContext db, string adminMobile)
    {
        if (!db.Roles.Any(r => r.RoleName == "User"))
        {
            db.Roles.Add(new Role { RoleName = "User", CreatedAt = DateTime.UtcNow });
        }

        if (!db.Roles.Any(r => r.RoleName == "Admin"))
        {
            db.Roles.Add(new Role { RoleName = "Admin", CreatedAt = DateTime.UtcNow });
        }

        db.SaveChanges();

        if (!string.IsNullOrWhiteSpace(adminMobile))
        {
            var existing = db.Users.FirstOrDefault(u => u.MobileNumber == adminMobile);
            if (existing == null)
            {
                existing = new User { MobileNumber = adminMobile, IsActive = true, IsMobileVerified = true, CreatedAt = DateTime.UtcNow };
                db.Users.Add(existing);
                db.SaveChanges();
            }

            var adminRole = db.Roles.First(r => r.RoleName == "Admin");
            if (!db.UserRoles.Any(ur => ur.UserId == existing.UserId && ur.RoleId == adminRole.RoleId))
            {
                db.UserRoles.Add(new UserRole { UserId = existing.UserId, RoleId = adminRole.RoleId, CreatedAt = DateTime.UtcNow });
                db.SaveChanges();
            }
        }
    }
}

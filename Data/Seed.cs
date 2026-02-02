using ManageLife.Base;
using ManageLife.Commons;
using ManageLife.Entities;
using ManageLife.Helpers;
using Microsoft.EntityFrameworkCore;

namespace ManageLife.Data
{
    public static class Seed
    {
        public static async Task SeedData(AppDbContext context)
        {
            using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                var adminRole = await context.Roles.FirstOrDefaultAsync(x => x.Code == RoleConst.Admin);
                if (adminRole == null)
                {
                    adminRole = new RoleEntity
                    {
                        Id = IdHeper.NewId(),
                        Code = RoleConst.Admin,
                        Name = "Admin",
                        Description = "Quản trị viên",
                        CreatedUser = SystemUsers.System,
                        CreatedTime = DateTimeHelper.Now()
                    };
                    context.Roles.Add(adminRole);
                }

                var userRole = await context.Roles.FirstOrDefaultAsync(x => x.Code == RoleConst.User);
                if (userRole == null)
                {
                    userRole = new RoleEntity
                    {
                        Id = IdHeper.NewId(),
                        Code = RoleConst.User,
                        Name = "User",
                        Description = "Người dùng",
                        CreatedUser = SystemUsers.System,
                        CreatedTime = DateTimeHelper.Now()
                    };
                    context.Roles.Add(userRole);
                }

                await context.SaveChangesAsync();

                var adminUser = await context.Users.FirstOrDefaultAsync(x => x.UserName == "admin");
                if (adminUser == null)
                {
                    adminUser = new UserEntity
                    {
                        Id = IdHeper.NewId(),
                        UserName = "admin",
                        Email = "admin@system.local",
                        FullName = "Administrator",
                        HashPassword = PasswordHelper.HashPassword(
                            Environment.GetEnvironmentVariable("ADMIN_DEFAULT_PASSWORD") ?? "D@ngDuong0402"
                        ),
                        IsActive = true,
                        CreatedUser = SystemUsers.System,
                        CreatedTime = DateTimeHelper.Now()
                    };
                    context.Users.Add(adminUser);
                    await context.SaveChangesAsync();
                }

                var hasRole = await context.UserRoles.AnyAsync(x => x.UserId == adminUser.Id && x.RoleId == adminRole.Id);

                if (!hasRole)
                {
                    context.UserRoles.Add(new UserRoleEntity
                    {
                        UserId = adminUser.Id,
                        RoleId = adminRole.Id
                    });
                    await context.SaveChangesAsync();
                }

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}

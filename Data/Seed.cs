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
            if (!await context.Roles.AnyAsync())
            {
                var adminRole = new RoleEntity()
                {
                    Id = IdHeper.NewId(),
                    Name = RoleConst.Admin,
                    Description = "Quản trị viên",
                    CreatedUser = SystemUsers.System,
                    CreatedTime = DateTimeHelper.Now(),
                };
                var userRole = new RoleEntity()
                {
                    Id = IdHeper.NewId(),
                    Name = RoleConst.User,
                    Description = "Người dùng",
                    CreatedUser = SystemUsers.System,
                    CreatedTime = DateTimeHelper.Now(),
                };

                await context.Roles.AddRangeAsync(adminRole, userRole);

                var adminUser = new UserEntity
                {
                    Id = IdHeper.NewId(),
                    UserName = "admin",
                    Email = "duongdangtruong.it@gmail.com",
                    FullName = "Administrator",
                    HashPassword = PasswordHelper.HashPassword("D@ngDuong04022002"),
                    IsActive = true,
                    CreatedUser = SystemUsers.System,
                    CreatedTime = DateTimeHelper.Now(),
                };

                await context.Users.AddAsync(adminUser);

                var adminUserRole = new UserRoleEntity
                {
                    UserId = adminUser.Id,
                    RoleId = adminRole.Id
                };

                await context.UserRoles.AddAsync(adminUserRole);

                await context.SaveChangesAsync();
            }
        }
    }
}

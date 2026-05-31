using ManageLife.Core;
using ManageLife.Commons;
using ManageLife.Entities;
using ManageLife.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ManageLife.Data
{
    public static class Seed
    {
        public static async Task SeedData(AppDbContext context, IConfiguration config)
        {
            using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                var adminRole = await context.Roles.FirstOrDefaultAsync(x => x.Code == RoleConst.Admin);
                if (adminRole == null)
                {
                    adminRole = new RoleEntity
                    {
                        Id = IdHelper.NewId(),
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
                        Id = IdHelper.NewId(),
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
                        Id = IdHelper.NewId(),
                        UserName = "admin",
                        Email = "admin@system.local",
                        FullName = "Administrator",
                        HashPassword = PasswordHelper.HashPassword(
                            config["ADMIN_DEFAULT_PASSWORD"]
                                ?? throw new InvalidOperationException(
                                    "ADMIN_DEFAULT_PASSWORD is required. Set via User Secrets (dev) or environment variable (prod).")
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

                await SeedTelegramBotCommands(context);

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private static async Task SeedTelegramBotCommands(AppDbContext context)
        {
            var hasAny = await context.TelegramBotCommands.AnyAsync(x => !x.IsDeleted);
            if (hasAny) return;

            var defaultCommands = new List<TelegramBotCommandEntity>
            {
                new() { Id = IdHelper.NewId(), Command = "start",  Description = "Bắt đầu sử dụng bot",                           SortOrder = 1, CreatedUser = SystemUsers.System, CreatedTime = DateTimeHelper.Now() },
                new() { Id = IdHelper.NewId(), Command = "info",   Description = "Lấy Chat ID của bạn",                            SortOrder = 2, CreatedUser = SystemUsers.System, CreatedTime = DateTimeHelper.Now() },
                new() { Id = IdHelper.NewId(), Command = "link",   Description = "Liên kết tài khoản ManageLife với Telegram",      SortOrder = 3, CreatedUser = SystemUsers.System, CreatedTime = DateTimeHelper.Now() },
                new() { Id = IdHelper.NewId(), Command = "help",   Description = "Xem hướng dẫn sử dụng",                          SortOrder = 4, CreatedUser = SystemUsers.System, CreatedTime = DateTimeHelper.Now() },
            };

            context.TelegramBotCommands.AddRange(defaultCommands);
            await context.SaveChangesAsync();
        }
    }
}

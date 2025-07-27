using ManageLife.Base;
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
					Name = "Admin",
					Description = "Quản trị viên",
					CreatedUser = "admin",
					CreatedTime = DateTimeHelper.Now(),
				};
				var userRole = new RoleEntity()
				{
					Id = IdHeper.NewId(),
					Name = "User",
					Description = "Người dùng",
					CreatedUser = "admin",
					CreatedTime = DateTimeHelper.Now(),
				};

				await context.Roles.AddRangeAsync(adminRole, userRole);

				var adminUser = new UserEntity()
				{
					Id = IdHeper.NewId(),
					UserName = "admin",
					HashPassword = PasswordHelper.HashPassword("D@ngDuong04022002"),
					RoleId = adminRole.Id
				};

				await context.Users.AddAsync(adminUser);

				await context.SaveChangesAsync();
			}
		}
	}
}

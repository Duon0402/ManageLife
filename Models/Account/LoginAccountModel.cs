using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models
{
	public class LoginAccountModel
	{
		[Required]
		public string UserName { get; set; } = null!;

		[Required]
		public string Password { get; set; } = null!;
	}
}

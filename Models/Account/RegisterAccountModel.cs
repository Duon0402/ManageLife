namespace ManageLife.Models
{
	public class RegisterAccountModel
	{
		public string UserName { get; set; } = null!;
		public string Password { get; set; } = null!;
		public string ConfirmPassword { get; set; } = null!;
	}
}

using System.ComponentModel.DataAnnotations;
namespace ManageLife.Base
{
	public abstract class EntityBase : IEntityBase
	{
		[Key]
		public string Id { get; set; } = IdHeper.NewId();
	}
}
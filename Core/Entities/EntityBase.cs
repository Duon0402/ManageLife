using System.ComponentModel.DataAnnotations;
namespace ManageLife.Core
{
    public abstract class EntityBase : IEntityBase
    {
        [Key]
        public string Id { get; set; } = null!;
    }
}
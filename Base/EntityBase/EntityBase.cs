using System.ComponentModel.DataAnnotations;

namespace ManageLife.Base
{
    public abstract class EntityBase
    {
        [Key]
        public int Id { get; set; }
    }
}
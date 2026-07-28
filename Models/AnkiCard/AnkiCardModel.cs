using ManageLife.Core;
using ManageLife.Entities;
using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models
{
    public class AnkiCardModel
    {
        public string Id { get; set; } = default!;
        public AnkiCardType CardType { get; set; }
        public string FieldFront { get; set; } = default!;
        public string FieldBack { get; set; } = default!;
        public string? FieldExtra { get; set; }
        public string? SourceNote { get; set; }
        public DateTime RecordedDate { get; set; }
    }

    public class CreateAnkiCardRequest : IValidatableRequest
    {
        public AnkiCardType CardType { get; set; }

        [Required(ErrorMessage = "Mặt trước không được để trống")]
        public string FieldFront { get; set; } = default!;

        [Required(ErrorMessage = "Mặt sau không được để trống")]
        public string FieldBack { get; set; } = default!;

        public string? FieldExtra { get; set; }
        public string? SourceNote { get; set; }
    }

    public class UpdateAnkiCardRequest : IValidatableRequest
    {
        [Required]
        public string Id { get; set; } = default!;

        public AnkiCardType CardType { get; set; }

        [Required(ErrorMessage = "Mặt trước không được để trống")]
        public string FieldFront { get; set; } = default!;

        [Required(ErrorMessage = "Mặt sau không được để trống")]
        public string FieldBack { get; set; } = default!;

        public string? FieldExtra { get; set; }
        public string? SourceNote { get; set; }
    }
}

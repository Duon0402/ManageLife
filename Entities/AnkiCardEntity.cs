using ManageLife.Core;

namespace ManageLife.Entities
{
    public class AnkiCardEntity : EntityBase, ICanCreate, ICanUpdate, ISoftDelete
    {
        public string OwnerId { get; set; } = default!;
        public AnkiCardType CardType { get; set; }

        // Ý nghĩa theo CardType:
        //  Basic / BasicReversed / BasicTypeAnswer : FieldFront=Front, FieldBack=Back, FieldExtra=không dùng (null)
        //  BasicOptionalReversed                   : FieldFront=Front, FieldBack=Back, FieldExtra="y" nếu có tạo thẻ đảo chiều, ngược lại null/rỗng
        //  Cloze                                   : FieldFront=ClozeText (chứa "___"), FieldBack=Answer, FieldExtra=nội dung phụ (Extra, hiện ở mặt sau)
        public string FieldFront { get; set; } = default!;
        public string FieldBack { get; set; } = default!;
        public string? FieldExtra { get; set; }

        public string? SourceNote { get; set; }
        public DateTime RecordedDate { get; set; }

        public string CreatedUser { get; set; } = default!;
        public DateTime CreatedTime { get; set; }
        public string? UpdatedUser { get; set; }
        public DateTime? UpdatedTime { get; set; }
        public string? DeletedUser { get; set; }
        public DateTime? DeletedTime { get; set; }
        public bool IsDeleted { get; set; }
    }
}

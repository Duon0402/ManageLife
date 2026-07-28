using System.Text;
using ManageLife.Entities;
using ManageLife.Models;

namespace ManageLife.Helpers
{
    public static class AnkiTextExporter
    {
        private static readonly Dictionary<AnkiCardType, string> NoteTypeName = new()
        {
            [AnkiCardType.Basic] = "Basic",
            [AnkiCardType.BasicReversed] = "Basic (and reversed card)",
            [AnkiCardType.BasicOptionalReversed] = "Basic (optional reversed card)",
            [AnkiCardType.BasicTypeAnswer] = "Basic (type in the answer)",
            [AnkiCardType.Cloze] = "Cloze",
        };

        public static byte[] Build(List<AnkiCardModel> cards)
        {
            var sb = new StringBuilder();
            // Header — cột 1 = tên note type (cho phép trộn nhiều loại/1 file), cột 2 = GUID ổn định,
            // cột 3+ = field theo đúng thứ tự field thật của note type đó.
            sb.Append("#separator:Tab\n");
            sb.Append("#html:false\n");
            sb.Append("#notetype column:1\n");
            sb.Append("#guid column:2\n");
            sb.Append("#deck:ManageLife Cards\n");

            foreach (var card in cards)
            {
                var noteType = NoteTypeName[card.CardType];
                string[] fields = card.CardType switch
                {
                    AnkiCardType.Cloze => new[]
                    {
                        Sanitize(card.FieldFront.Replace("___", "{{c1::" + card.FieldBack + "}}")),
                        Sanitize(card.FieldExtra ?? "")
                    },
                    AnkiCardType.BasicOptionalReversed => new[]
                    {
                        Sanitize(card.FieldFront),
                        Sanitize(card.FieldBack),
                        Sanitize(card.FieldExtra ?? "")
                    },
                    _ => new[] { Sanitize(card.FieldFront), Sanitize(card.FieldBack) }
                };

                // GUID ổn định = card.Id — xuất lại nhiều lần cùng thẻ sẽ CẬP NHẬT thay vì tạo trùng
                // (xác nhận chính thức: "as long as you do not modify the GUID field, you'll be able
                // to import the notes back in to update the existing notes").
                sb.Append(noteType).Append('\t').Append(card.Id);
                foreach (var field in fields)
                    sb.Append('\t').Append(field);
                sb.Append('\n');
            }

            return new UTF8Encoding(false).GetBytes(sb.ToString());
        }

        // Loại bỏ tab/xuống dòng trong nội dung field vì đây là ký tự phân cách của định dạng TSV
        private static string Sanitize(string value) => value.Replace('\t', ' ').Replace('\n', ' ').Replace('\r', ' ');
    }
}

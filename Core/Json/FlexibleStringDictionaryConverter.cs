using System.Text.Json;
using System.Text.Json.Serialization;

namespace ManageLife.Core
{
    /// <summary>
    /// Đọc object JSON thành Dictionary&lt;string, string&gt;, chấp nhận cả mảng rỗng/null (API viết bằng PHP
    /// thường trả <c>[]</c> thay cho <c>{}</c> khi không có phần tử). Giá trị không phải chuỗi được ghi dạng JSON thô.
    /// </summary>
    public class FlexibleStringDictionaryConverter : JsonConverter<Dictionary<string, string>>
    {
        public override Dictionary<string, string> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var result = new Dictionary<string, string>();
            switch (reader.TokenType)
            {
                case JsonTokenType.Null:
                    return result;
                case JsonTokenType.StartArray:
                    //NOTE: [] (hoặc mảng bất kỳ) → coi như không có phần tử
                    reader.Skip();
                    return result;
                case JsonTokenType.StartObject:
                    using (var doc = JsonDocument.ParseValue(ref reader))
                    {
                        foreach (var property in doc.RootElement.EnumerateObject())
                            result[property.Name] = property.Value.ValueKind == JsonValueKind.String
                                ? property.Value.GetString() ?? ""
                                : property.Value.GetRawText();
                    }
                    return result;
                default:
                    throw new JsonException($"Không đọc được {reader.TokenType} thành danh sách khoá–giá trị");
            }
        }

        public override void Write(Utf8JsonWriter writer, Dictionary<string, string> value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            foreach (var (key, item) in value)
                writer.WriteString(key, item);
            writer.WriteEndObject();
        }
    }
}

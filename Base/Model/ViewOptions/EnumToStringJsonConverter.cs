using System.Text.Json;
using System.Text.Json.Serialization;

namespace ManageLife.Base
{
    public abstract class EnumToStringJsonConverter<TEnum> : JsonConverter<TEnum> where TEnum : struct, Enum
    {
        private readonly Dictionary<TEnum, string> _map;
        private readonly Dictionary<string, TEnum> _reverseMap;

        protected EnumToStringJsonConverter(Dictionary<TEnum, string> map)
        {
            _map = map;
            _reverseMap = new Dictionary<string, TEnum>(StringComparer.OrdinalIgnoreCase);
            foreach (var kv in _map)
                _reverseMap[kv.Value] = kv.Key;
        }

        public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string s = reader.GetString() ?? "";
            if (_reverseMap.TryGetValue(s, out var value))
            {
                return value;
            }

            throw new JsonException($"Invalid value '{s}' for enum {typeof(TEnum).Name}");
        }

        public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
        {
            if (_map.TryGetValue(value, out var s))
            {
                writer.WriteStringValue(s);
            }
            else
            {
                throw new JsonException($"Cannot serialize unknown value {value} of enum {typeof(TEnum).Name}");
            }
        }
    }
}

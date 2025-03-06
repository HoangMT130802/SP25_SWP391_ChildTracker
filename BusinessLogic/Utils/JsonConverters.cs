using System.Text.Json;
using System.Text.Json.Serialization;

namespace BusinessLogic.Utils
{
    public class DateTimeConverters
    {
        public class DateOnlyJsonConverter : JsonConverter<DateOnly>
        {
            private const string Format = "yyyy-MM-dd";

            public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType == JsonTokenType.String)
                {
                    var value = reader.GetString();
                    return DateOnly.ParseExact(value!, Format);
                }

                throw new JsonException($"Unexpected token type {reader.TokenType}");
            }

            public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(value.ToString(Format));
            }
        }

        public class TimeOnlyJsonConverter : JsonConverter<TimeOnly>
        {
            private const string Format = "HH:mm";

            public override TimeOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType == JsonTokenType.String)
                {
                    var value = reader.GetString();
                    return TimeOnly.ParseExact(value!, Format);
                }

                throw new JsonException($"Unexpected token type {reader.TokenType}");
            }

            public override void Write(Utf8JsonWriter writer, TimeOnly value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(value.ToString(Format));
            }
        }
    }
} 
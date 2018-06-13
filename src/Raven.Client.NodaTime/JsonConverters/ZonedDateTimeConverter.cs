using System.IO;
using Newtonsoft.Json;
using NodaTime;
using Raven.Imports.NodaTime.Serialization.JsonNet;

namespace Raven.Client.NodaTime.JsonConverters
{
    /// <summary>
    /// Json.NET converter for <see cref="ZonedDateTime"/>.
    /// </summary>   
    internal sealed class ZonedDateTimeConverter : NodaConverterBase<ZonedDateTime>
    {
        protected override ZonedDateTime ReadJsonImpl(JsonReader reader, JsonSerializer serializer)
        {
            var odt = default(OffsetDateTime);
            var zone = default(DateTimeZone);
            var gotOffsetDateTime = false;
            var gotZone = false;
            while (reader.Read())
            {
                if (reader.TokenType != JsonToken.PropertyName)
                    break;

                var propertyName = (string)reader.Value;
                if (!reader.Read())
                    continue;

                if (propertyName == "OffsetDateTime")
                {
                    odt = serializer.Deserialize<OffsetDateTime>(reader);
                    gotOffsetDateTime = true;
                }

                if (propertyName == "Zone")
                {
                    zone = serializer.Deserialize<DateTimeZone>(reader);
                    gotZone = true;
                }
            }

            if (!(gotOffsetDateTime && gotZone))
            {
                throw new InvalidDataException("An ZonedDateTime must contain OffsetDateTime and Zone properties.");
            }

            return new ZonedDateTime(odt.LocalDateTime, zone, odt.Offset);
        }

        protected override void WriteJsonImpl(JsonWriter writer, ZonedDateTime value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("OffsetDateTime");
            serializer.Serialize(writer, value.ToOffsetDateTime());
            writer.WritePropertyName("Zone");
            serializer.Serialize(writer, value.Zone);
            writer.WriteEndObject();
        }
    }
}

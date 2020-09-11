using System.IO;
using Newtonsoft.Json;
using NodaTime;
using Raven.Imports.NodaTime.Serialization.JsonNet;

namespace Raven.Client.NodaTime.JsonConverters
{
    /// <summary>
    /// Reads Start and End properties as LocalDates.
    /// </summary>
    internal class DateIntervalConverter : NodaConverterBase<DateInterval>
    {
        protected override DateInterval ReadJsonImpl(JsonReader reader, JsonSerializer serializer)
        {
            var startLocalDate = default(LocalDate);
            var endLocalDate = default(LocalDate);
            var gotStartLocalDate = false;
            var gotEndLocalDate = false;
            while (reader.Read())
            {
                if (reader.TokenType != JsonToken.PropertyName)
                {
                    break;
                }

                var propertyName = (string)reader.Value;
                if (!reader.Read())
                {
                    continue;
                }

                if (propertyName == "Start")
                {
                    startLocalDate = serializer.Deserialize<LocalDate>(reader);
                    gotStartLocalDate = true;
                }

                if (propertyName == "End")
                {
                    endLocalDate = serializer.Deserialize<LocalDate>(reader);
                    gotEndLocalDate = true;
                }
            }

            if (!(gotStartLocalDate && gotEndLocalDate))
            {
                throw new InvalidDataException("A DateInterval must contain Start and End properties.");
            }

            return new DateInterval(startLocalDate, endLocalDate);
        }

        protected override void WriteJsonImpl(JsonWriter writer, DateInterval value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("Start");
            serializer.Serialize(writer, value.Start);
            writer.WritePropertyName("End");
            serializer.Serialize(writer, value.End);
            writer.WriteEndObject();
        }
    }
}
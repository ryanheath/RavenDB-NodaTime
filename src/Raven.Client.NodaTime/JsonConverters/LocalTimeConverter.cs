using System;
using NodaTime;
using Raven.Imports.Newtonsoft.Json;
using Raven.Imports.Newtonsoft.Json.Linq;
using Raven.Imports.NodaTime.Serialization.JsonNet;

namespace Raven.Client.NodaTime.JsonConverters
{
    /// <summary>
    /// Treats an LocalTime as TimeSpan for json serialization purposes.
    /// </summary>
    internal class LocalTimeConverter : NodaConverterBase<LocalTime>
    {
        protected override LocalTime ReadJsonImpl(JsonReader reader, JsonSerializer serializer)
        {
            var timeSpan = serializer.Deserialize<TimeSpan>(reader);
            return timeSpan.ToLocalTime();
        }

        protected override void WriteJsonImpl(JsonWriter writer, LocalTime value, JsonSerializer serializer)
        {
            var timeSpan = value.ToTimeSpan();
            serializer.Serialize(writer, timeSpan);
        }
    }

    /// <summary>
    /// As LocalTimeConverter but is able to read LocalTimes stored in NodaTime format 
    /// </summary>
    internal class RelaxedLocalTimeConverter : LocalTimeConverter
    {
        protected override LocalTime ReadJsonImpl(JsonReader reader, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.StartObject)
            {
                var o = JObject.Load(reader);
                return o.ToObject<LocalTime>();
            }

            return base.ReadJsonImpl(reader, serializer);
        }
    }
}
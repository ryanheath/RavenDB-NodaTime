using System;
using NodaTime;
using Raven.Imports.Newtonsoft.Json;
using Raven.Imports.NodaTime.Serialization.JsonNet;

namespace Raven.Client.NodaTime.JsonConverters
{
    /// <summary>
    /// Treats an Offset as TimeSpan for json serialization purposes.
    /// </summary>
    internal class OffsetConverter : NodaConverterBase<Offset>
    {
        protected override Offset ReadJsonImpl(JsonReader reader, JsonSerializer serializer)
        {
            var timeSpan = serializer.Deserialize<TimeSpan>(reader);
            return Offset.FromTicks(timeSpan.Ticks);
        }

        protected override void WriteJsonImpl(JsonWriter writer, Offset value, JsonSerializer serializer)
        {
            var timeSpan = value.ToTimeSpan();
            serializer.Serialize(writer, timeSpan);
        }
    }
}
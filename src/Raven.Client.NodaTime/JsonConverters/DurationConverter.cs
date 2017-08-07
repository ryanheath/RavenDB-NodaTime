using System;
using NodaTime;
using Raven.Imports.Newtonsoft.Json;
using Raven.Imports.NodaTime.Serialization.JsonNet;

namespace Raven.Client.NodaTime.JsonConverters
{
    /// <summary>
    /// Treats an Duration as TimeSpan for json serialization purposes.
    /// </summary>
    internal class DurationConverter : NodaConverterBase<Duration>
    {
        protected override Duration ReadJsonImpl(JsonReader reader, JsonSerializer serializer)
        {
            var timeSpan = serializer.Deserialize<TimeSpan>(reader);
            return Duration.FromTimeSpan(timeSpan);
        }

        protected override void WriteJsonImpl(JsonWriter writer, Duration value, JsonSerializer serializer)
        {
            var timeSpan = value.ToTimeSpan();
            serializer.Serialize(writer, timeSpan);
        }
    }
}
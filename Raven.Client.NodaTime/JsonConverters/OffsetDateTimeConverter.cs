using System;
using NodaTime;
using Raven.Imports.Newtonsoft.Json;
using Raven.Imports.NodaTime.Serialization.JsonNet;

namespace Raven.Client.NodaTime.JsonConverters
{
    /// <summary>
    /// Treats an OffsetDateTime as DateTimeOffset for json serialization purposes.
    /// </summary>
    internal class OffsetDateTimeConverter : NodaConverterBase<OffsetDateTime>
    {
        protected override OffsetDateTime ReadJsonImpl(JsonReader reader, JsonSerializer serializer)
        {
            var dto = serializer.Deserialize<DateTimeOffset>(reader);
            return OffsetDateTime.FromDateTimeOffset(dto);
        }

        protected override void WriteJsonImpl(JsonWriter writer, OffsetDateTime value, JsonSerializer serializer)
        {
            var dto = value.ToDateTimeOffset();
            serializer.Serialize(writer, dto);
        }
    }
}
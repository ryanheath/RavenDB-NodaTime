using System;
using NodaTime;
using Raven.Imports.Newtonsoft.Json;
using Raven.Imports.NodaTime.Serialization.JsonNet;

namespace Raven.Client.NodaTime.JsonConverters
{
    internal class CustomPatternBasedJsonConverters
    {
        public static readonly JsonConverter InstantConverter = new NodaPatternConverter<Instant>(NodaUtil.Instant.FullIsoPattern, NodaUtil.Instant.Validate);

        public static readonly JsonConverter LocalDateTimeConverter = new NodaPatternConverter<LocalDateTime>(NodaUtil.LocalDateTime.FullIsoPattern,
                                                                                                              CreateIsoValidator<LocalDateTime>(x => x.Calendar));

        private static Action<T> CreateIsoValidator<T>(Func<T, CalendarSystem> calendarProjection)
        {
            return value =>
            {
                if (calendarProjection(value) == CalendarSystem.Iso) return;
                throw new ArgumentException(string.Format("Values of type {0} must (currently) use the ISO calendar in order to be serialized.", typeof(T).Name));
            };
        }
    }
}

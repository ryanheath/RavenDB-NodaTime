// Copyright 2012 The Noda Time Authors. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using NodaTime;
using Raven.Imports.Newtonsoft.Json;
using NodaTime.Text;

namespace Raven.Imports.NodaTime.Serialization.JsonNet
{
    /// <summary>
    /// Convenience class to expose preconfigured converters for Noda Time types.
    /// </summary>
    internal static class NodaConverters
    {
        /// <summary>
        /// Converter for instants, using the ISO-8601 date/time pattern, extended as required to accommodate milliseconds and ticks, and
        /// specifying 'Z' at the end to show it's effectively in UTC.
        /// </summary>
        public static readonly JsonConverter InstantConverter = new NodaPatternConverter<Instant>(InstantPattern.ExtendedIso);

        /// <summary>
        /// Converter for local dates, using the ISO-8601 date pattern.
        /// </summary>
        // TODO(Post-V1): Consider improving the behaviour with non-ISO calendars. We probably want a pattern which "knows" about a particular calendar, and restricts itself to that calendar.
        public static readonly JsonConverter LocalDateConverter = new RelaxedNodaPatternConverter<LocalDate>(
            LocalDatePattern.Iso, CreateIsoValidator<LocalDate>(x => x.Calendar));

        /// <summary>
        /// Converter for local dates and times, using the ISO-8601 date/time pattern, extended as required to accommodate milliseconds and ticks.
        /// No time zone designator is applied.
        /// </summary>
        public static readonly JsonConverter LocalDateTimeConverter = new NodaPatternConverter<LocalDateTime>(
            LocalDateTimePattern.ExtendedIso, CreateIsoValidator<LocalDateTime>(x => x.Calendar));

        /// <summary>
        /// Converter for local times, using the ISO-8601 time pattern, extended as required to accommodate milliseconds and ticks.
        /// </summary>
        public static readonly JsonConverter LocalTimeConverter = new NodaPatternConverter<LocalTime>(LocalTimePattern.ExtendedIso);

        /// <summary>
        /// Converter for intervals. This must be used in a serializer which also has an instant converter.
        /// </summary>
        public static readonly JsonConverter IntervalConverter = new NodaIntervalConverter();
        
        /// <summary>
        /// Converter for offsets.
        /// </summary>
        public static readonly JsonConverter OffsetConverter = new NodaPatternConverter<Offset>(OffsetPattern.GeneralInvariant);

        /// <summary>
        /// Converter for durations.
        /// </summary>
        public static readonly JsonConverter DurationConverter = new NodaDurationConverter();

        /// <summary>
        /// Round-tripping converter for periods. Use this when you really want to preserve information,
        /// and don't need interoperability with systems expecting ISO.
        /// </summary>
        public static readonly JsonConverter RoundtripPeriodConverter = new NodaPatternConverter<Period>(PeriodPattern.Roundtrip);

        /// <summary>
        /// Normalizing ISO converter for periods. Use this when you want compatibility with systems expecting
        /// ISO durations (~= Noda Time periods). However, note that Noda Time can have negative periods. Note that
        /// this converter losees information - after serialization and deserialization, "90 minutes" will become "an hour and 30 minutes".
        /// </summary>
        public static readonly JsonConverter NormalizingIsoPeriodConverter = new NodaPatternConverter<Period>(PeriodPattern.NormalizingIso);

        /// <summary>
        /// Converter for using instants as dictionary keys.
        /// </summary>
        public static readonly JsonConverter InstantDictionaryKeyConverter = new DictionaryKeyConverter<Instant>();

        /// <summary>
        /// Converter for using local dates as dictionary keys.
        /// </summary>
        public static readonly JsonConverter LocalDateDictionaryKeyConverter = new DictionaryKeyConverter<LocalDate>();

        /// <summary>
        /// Converter for using local dates and times as dictionary keys.
        /// </summary>
        public static readonly JsonConverter LocalDateTimeDictionaryKeyConverter = new DictionaryKeyConverter<LocalDateTime>();

        /// <summary>
        /// Converter for using local times as dictionary keys.
        /// </summary>
        public static readonly JsonConverter LocalTimeDictionaryKeyConverter = new DictionaryKeyConverter<LocalTime>();

        private static Action<T> CreateIsoValidator<T>(Func<T, CalendarSystem> calendarProjection)
        {
            return value => {
                var calendar = calendarProjection(value);
                // We rely on CalendarSystem.Iso being a singleton here.
                if (calendar != CalendarSystem.Iso)
                {
                    throw new ArgumentException(
                        string.Format("Values of type {0} must (currently) use the ISO calendar in order to be serialized.",
                        typeof(T).Name));
                }
            };
        }
    }
}

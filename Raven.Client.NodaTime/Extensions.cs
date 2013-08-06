using System;
using System.Linq;
using NodaTime;
using Raven.Abstractions.Indexing;
using Raven.Client.NodaTime.JsonConverters;
using Raven.Imports.NodaTime.Serialization.JsonNet;

namespace Raven.Client.NodaTime
{
    public static class Extensions
    {
        public static T ConfigureForNodaTime<T>(this T documentStore)
            where T : IDocumentStore
        {
            return documentStore.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
        }

        public static T ConfigureForNodaTime<T>(this T documentStore, IDateTimeZoneProvider zoneProvider)
            where T : IDocumentStore
        {
            var existing = documentStore.Conventions.CustomizeJsonSerializer;
            documentStore.Conventions.CustomizeJsonSerializer = serializer =>
            {
                // Chain any existing serialization conventions
                if (existing != null)
                    existing.Invoke(serializer);

                // Don't do anything if we've already registered them
                if (serializer.Converters.OfType<NodaConverterBase<Instant>>().Any())
                    return;

                // Register standard json converters
                serializer.Converters.Add(CustomPatternBasedJsonConverters.InstantConverter);
                serializer.Converters.Add(NodaConverters.IntervalConverter);
                serializer.Converters.Add(CustomPatternBasedJsonConverters.LocalDateTimeConverter);
                serializer.Converters.Add(NodaConverters.LocalDateConverter);
                serializer.Converters.Add(NodaConverters.RoundtripPeriodConverter);
                serializer.Converters.Add(NodaConverters.InstantDictionaryKeyConverter);
                serializer.Converters.Add(NodaConverters.LocalDateDictionaryKeyConverter);
                serializer.Converters.Add(NodaConverters.LocalDateTimeDictionaryKeyConverter);
                serializer.Converters.Add(NodaConverters.LocalTimeDictionaryKeyConverter);

                // Register custom json converters
                serializer.Converters.Add(new LocalTimeConverter());
                serializer.Converters.Add(new NodaDateTimeZoneConverter(zoneProvider));
                serializer.Converters.Add(new OffsetConverter());
                serializer.Converters.Add(new DurationConverter());
                serializer.Converters.Add(new OffsetDateTimeConverter());
                serializer.Converters.Add(new ZonedDateTimeConverter());
            };

            // Register query value converters
            documentStore.Conventions.RegisterQueryValueConverter<Instant>(CustomQueryValueConverters.InstantConverter);
            documentStore.Conventions.RegisterQueryValueConverter<LocalDateTime>(CustomQueryValueConverters.LocalDateTimeConverter);
            documentStore.Conventions.RegisterQueryValueConverter<LocalDate>(CustomQueryValueConverters.LocalDateConverter);
            documentStore.Conventions.RegisterQueryValueConverter<LocalTime>(CustomQueryValueConverters.LocalTimeConverter, SortOptions.Long, true);
            documentStore.Conventions.RegisterQueryValueConverter<Offset>(CustomQueryValueConverters.OffsetConverter, SortOptions.Long, true);
            documentStore.Conventions.RegisterQueryValueConverter<Duration>(CustomQueryValueConverters.DurationConverter, SortOptions.Long, true);
            documentStore.Conventions.RegisterQueryValueConverter<OffsetDateTime>(CustomQueryValueConverters.OffsetDateTimeConverter);
            documentStore.Conventions.RegisterQueryValueConverter<Period>(CustomQueryValueConverters.PeriodConverter);
            documentStore.Conventions.RegisterQueryValueConverter<ZonedDateTime>(CustomQueryValueConverters.ZonedDateTimeConverter);

            // Register query translators
            documentStore.Conventions.RegisterCustomQueryTranslator<OffsetDateTime>(x => x.ToInstant(), CustomQueryTranslators.OffsetDateTimeToInstantTranslator);
            documentStore.Conventions.RegisterCustomQueryTranslator<OffsetDateTime>(x => x.LocalDateTime, CustomQueryTranslators.OffsetDateTimeLocalDateTimeTranslator);
            documentStore.Conventions.RegisterCustomQueryTranslator<ZonedDateTime>(x => x.ToInstant(), CustomQueryTranslators.ZonedDateTimeTimeToInstantTranslator);

            return documentStore;
        }

        public static TimeSpan ToTimeSpan(this LocalTime localTime)
        {
            return new TimeSpan(localTime.TickOfDay);
        }

        public static LocalTime ToLocalTime(this TimeSpan timeSpan)
        {
            return LocalTime.FromTicksSinceMidnight(timeSpan.Ticks);
        }
    }
}

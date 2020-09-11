using System;
using System.Linq;
using NodaTime;
using Raven.Client.Documents;
using Raven.Client.Documents.Conventions;
using Raven.Client.Documents.Indexes;
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
            documentStore.Conventions.ConfigureForNodaTime(zoneProvider);
            return documentStore;
        }        
        
        public static DocumentConventions ConfigureForNodaTime(this DocumentConventions conventions, IDateTimeZoneProvider zoneProvider)
        {
            var existing = conventions.CustomizeJsonSerializer;
            conventions.CustomizeJsonSerializer = serializer =>
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
                serializer.Converters.Add(new DateIntervalConverter());
            };

            // Register query value converters
            conventions.RegisterQueryValueConverter<Instant>(CustomQueryValueConverters.InstantConverter);
            conventions.RegisterQueryValueConverter<LocalDateTime>(CustomQueryValueConverters.LocalDateTimeConverter);
            conventions.RegisterQueryValueConverter<LocalDate>(CustomQueryValueConverters.LocalDateConverter);
            conventions.RegisterQueryValueConverter<LocalTime>(CustomQueryValueConverters.LocalTimeConverter, RangeType.Long);
            conventions.RegisterQueryValueConverter<Offset>(CustomQueryValueConverters.OffsetConverter, RangeType.Long);
            conventions.RegisterQueryValueConverter<Duration>(CustomQueryValueConverters.DurationConverter, RangeType.Long);
            conventions.RegisterQueryValueConverter<OffsetDateTime>(CustomQueryValueConverters.OffsetDateTimeConverter);
            conventions.RegisterQueryValueConverter<Period>(CustomQueryValueConverters.PeriodConverter);
            conventions.RegisterQueryValueConverter<ZonedDateTime>(CustomQueryValueConverters.ZonedDateTimeConverter);
            conventions.RegisterQueryValueConverter<DateTimeZone>(CustomQueryValueConverters.DateTimeZoneConverter);

            // Register query translators
            conventions.RegisterCustomQueryTranslator<OffsetDateTime>(x => x.ToInstant(), CustomQueryTranslators.OffsetDateTimeToInstantTranslator);
            conventions.RegisterCustomQueryTranslator<OffsetDateTime>(x => x.LocalDateTime, CustomQueryTranslators.OffsetDateTimeLocalDateTimeTranslator);
            conventions.RegisterCustomQueryTranslator<ZonedDateTime>(x => x.ToInstant(), CustomQueryTranslators.ZonedDateTimeTimeToInstantTranslator);

            return conventions;
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

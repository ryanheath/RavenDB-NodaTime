using System;
using System.IO;
using NodaTime;
using NodaTime.Text;

namespace Raven.Bundles.NodaTime
{
    public static class NodaTimeField
    {
        public static Instant AsInstant(DateTime value)
        {
            return Instant.FromDateTimeUtc(value);
        }

        public static LocalDateTime AsLocalDateTime(DateTime value)
        {
            if (value.Kind != DateTimeKind.Unspecified)
                throw new InvalidOperationException();

            return LocalDateTime.FromDateTime(value);
        }

        public static LocalDate AsLocalDate(string value)
        {
            return LocalDatePattern.Iso.Parse(value).Value;
        }

        public static LocalTime AsLocalTime(TimeSpan value)
        {
            if (value < TimeSpan.Zero || value >= TimeSpan.FromHours(24))
                throw new InvalidOperationException();

            return LocalTime.FromTicksSinceMidnight(value.Ticks);
        }

        public static Duration AsDuration(TimeSpan value)
        {
            return Duration.FromTimeSpan(value);
        }

        public static Offset AsOffset(TimeSpan value)
        {
            return Offset.FromTicks(value.Ticks);
        }

        public static OffsetDateTime AsOffsetDateTime(DateTimeOffset value)
        {
            return OffsetDateTime.FromDateTimeOffset(value);
        }

        //public static ZonedDateTime AsZonedDateTime(blit obj)
        //{
        //    var dto = (DateTimeOffset) obj.GetValue("OffsetDateTime");
        //    var zone = (string) obj.GetValue("Zone");

        //    var odt = OffsetDateTime.FromDateTimeOffset(dto);
        //    var tz = DateTimeZoneProviders.Tzdb.GetZoneOrNull(zone);
        //    if (tz == null)
        //        throw new InvalidDataException("Unrecognized Time Zone");

        //    var zdt = new ZonedDateTime(odt.ToInstant(), tz);
        //    return zdt;
        //}

        public static DateTime Resolve(Instant value)
        {
            return value.ToDateTimeUtc();
        }

        public static DateTime Resolve(LocalDateTime value)
        {
            return value.ToDateTimeUnspecified();
        }

        public static string Resolve(LocalDate value)
        {
            return value.ToString(LocalDatePattern.Iso.PatternText, null);
        }

        public static TimeSpan Resolve(LocalTime value)
        {
            return new TimeSpan(value.TickOfDay);
        }

        public static TimeSpan Resolve(Duration value)
        {
            return value.ToTimeSpan();
        }

        public static TimeSpan Resolve(Offset value)
        {
            return value.ToTimeSpan();
        }

        public static DateTimeOffset Resolve(OffsetDateTime value)
        {
            return value.ToDateTimeOffset();
        }

        //public static RavenJObject Resolve(ZonedDateTime value)
        //{
        //    return new RavenJObject
        //           {
        //               { "OffsetDateTime", value.ToDateTimeOffset() },
        //               { "Zone", value.Zone.Id }
        //           };
        //}

        public static int DaysBetween(LocalDate localDate1, LocalDate localDate2)
        {
            return (int) Period.Between(localDate1, localDate2, PeriodUnits.Days).Days;
        }
    }
}

using NodaTime;
using NodaTime.Text;

namespace Raven.Bundles.NodaTime;

public static class NodaTimeField
{
    public static Instant AsInstant(DateTime value) => Instant.FromDateTimeUtc(value);

    public static LocalDateTime AsLocalDateTime(DateTime value)
    {
        if (value.Kind != DateTimeKind.Unspecified)
        {
            throw new InvalidOperationException();
        }

        return LocalDateTime.FromDateTime(value);
    }

    public static LocalDate AsLocalDate(string value) => LocalDatePattern.Iso.Parse(value).Value;

    public static LocalTime AsLocalTime(TimeSpan value)
    {
        if (value < TimeSpan.Zero || value >= TimeSpan.FromHours(24))
        {
            throw new InvalidOperationException();
        }

        return LocalTime.FromTicksSinceMidnight(value.Ticks);
    }

    public static Duration AsDuration(TimeSpan value) => Duration.FromTimeSpan(value);

    public static Offset AsOffset(TimeSpan value) => Offset.FromTicks(value.Ticks);

    public static OffsetDateTime AsOffsetDateTime(DateTimeOffset value) => OffsetDateTime.FromDateTimeOffset(value);

    public static ZonedDateTime AsZonedDateTime(dynamic obj)
    {
        var dto = (DateTimeOffset)obj.OffsetDateTime;
        var zone = (string)obj.Zone;

        var odt = OffsetDateTime.FromDateTimeOffset(dto);
        var tz = DateTimeZoneProviders.Tzdb.GetZoneOrNull(zone) ?? throw new InvalidDataException("Unrecognized Time Zone");
        var zdt = new ZonedDateTime(odt.ToInstant(), tz);
        return zdt;
    }

    public static DateTime Resolve(Instant value) => value.ToDateTimeUtc();

    public static DateTime Resolve(LocalDateTime value) => value.ToDateTimeUnspecified();

    public static string Resolve(LocalDate value) => value.ToString(LocalDatePattern.Iso.PatternText, null);

    public static TimeSpan Resolve(LocalTime value) => new(value.TickOfDay);

    public static TimeSpan Resolve(Duration value) => value.ToTimeSpan();

    public static TimeSpan Resolve(Offset value) => value.ToTimeSpan();

    public static DateTimeOffset Resolve(OffsetDateTime value) => value.ToDateTimeOffset();

    public static int DaysBetween(LocalDate localDate1, LocalDate localDate2) => (int)Period.Between(localDate1, localDate2, PeriodUnits.Days).Days;
}

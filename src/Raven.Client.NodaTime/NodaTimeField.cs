using NodaTime;
using Raven.Client.Documents.Indexes;

namespace Raven.Client.NodaTime;

public static class NodaTimeField
{
    [RavenMethod]
    public static Instant AsInstant(this Instant value) => value;

    [RavenMethod]
    public static LocalDateTime AsLocalDateTime(this LocalDateTime value) => value;

    [RavenMethod]
    public static LocalDate AsLocalDate(this LocalDate value) => value;

    [RavenMethod]
    public static LocalTime AsLocalTime(this LocalTime value) => value;

    [RavenMethod]
    public static Duration AsDuration(this Duration value) => value;

    [RavenMethod]
    public static Offset AsOffset(this Offset value) => value;

    [RavenMethod]
    public static OffsetDateTime AsOffsetDateTime(this OffsetDateTime value) => value;

    [RavenMethod]
    public static ZonedDateTime AsZonedDateTime(this ZonedDateTime value) => value;

    [RavenMethod]
    public static T Resolve<T>(this T value) => value;

    [RavenMethod]
    public static int DaysBetween(this LocalDate localDate1, LocalDate localDate2) => 0;
}

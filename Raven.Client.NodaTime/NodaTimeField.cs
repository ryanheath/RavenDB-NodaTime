using NodaTime;
using Raven.Client.Indexes;

namespace Raven.Client.NodaTime
{
    public static class NodaTimeField
    {
        [RavenMethod]
        public static Instant AsInstant(this Instant value)
        {
            return value;
        }

        [RavenMethod]
        public static LocalDateTime AsLocalDateTime(this LocalDateTime value)
        {
            return value;
        }

        [RavenMethod]
        public static LocalDate AsLocalDate(this LocalDate value)
        {
            return value;
        }

        [RavenMethod]
        public static LocalTime AsLocalTime(this LocalTime value)
        {
            return value;
        }

        [RavenMethod]
        public static Duration AsDuration(this Duration value)
        {
            return value;
        }

        [RavenMethod]
        public static Offset AsOffset(this Offset value)
        {
            return value;
        }

        [RavenMethod]
        public static OffsetDateTime AsOffsetDateTime(this OffsetDateTime value)
        {
            return value;
        }

        [RavenMethod]
        public static ZonedDateTime AsZonedDateTime(this ZonedDateTime value)
        {
            return value;
        }

        [RavenMethod]
        public static T Resolve<T>(this T value)
        {
            return value;
        }

        [RavenMethod]
        public static int DaysBetween(this LocalDate localDate1, LocalDate localDate2)
        {
            return 0;
        }
    }
}

using System;
using NodaTime;

namespace Raven.Client.NodaTime
{
    public static class NodaTimeField
    {
        public static Instant AsInstant(Instant value)
        {
            return value;
        }

        public static LocalDateTime AsLocalDateTime(LocalDateTime value)
        {
            return value;
        }

        public static LocalDate AsLocalDate(LocalDate value)
        {
            return value;
        }

        public static LocalTime AsLocalTime(LocalTime value)
        {
            return value;
        }

        public static Duration AsDuration(Duration value)
        {
            return value;
        }

        public static Offset AsOffset(Offset value)
        {
            return value;
        }

        public static OffsetDateTime AsOffsetDateTime(OffsetDateTime value)
        {
            return value;
        }

        public static ZonedDateTime AsZonedDateTime(ZonedDateTime value)
        {
            return value;
        }

        public static T Resolve<T>(T value)
        {
            return value;
        }
    }
}

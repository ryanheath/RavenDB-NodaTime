using System;
using NodaTime.Text;

namespace Raven.Client.NodaTime
{
    public static class NodaUtil
    {
        public static class Instant
        {
            public static readonly InstantPattern FullIsoPattern = InstantPattern.CreateWithInvariantCulture("yyyy'-'MM'-'dd'T'HH':'mm':'ss;fffffff'Z'");

            public const long MinIsoTicks = -621355968000000000;
            public const long MaxIsoTicks = 2534023007999999999;

            public static global::NodaTime.Instant MinIsoValue
            {
                get { return global::NodaTime.Instant.FromUnixTimeTicks(MinIsoTicks); }
            }

            public static global::NodaTime.Instant MaxIsoValue
            {
                get { return global::NodaTime.Instant.FromUnixTimeTicks(MaxIsoTicks); }
            }

            internal static void Validate(global::NodaTime.Instant instant)
            {
                if (instant.ToUnixTimeTicks() >= MinIsoTicks && instant.ToUnixTimeTicks() <= MaxIsoTicks)
                    return;

                var message = "NodaTime Instant values must fall between UTC years 0001 and 9999 to be compatible with RavenDB.";

                if (instant == global::NodaTime.Instant.MinValue)
                    message += " If you are attempting to use Instant.MinValue, use NodaUtil.Instant.MinIsoValue instead.";

                if (instant == global::NodaTime.Instant.MaxValue)
                    message += " If you are attempting to use Instant.MaxValue, use NodaUtil.Instant.MaxIsoValue instead.";

                throw new ArgumentOutOfRangeException("instant", instant, message);
            }
        }

        public static class OffsetDateTime
        {
            public static global::NodaTime.OffsetDateTime MinIsoValue
            {
                get { return global::NodaTime.OffsetDateTime.FromDateTimeOffset(DateTimeOffset.MinValue); }
            }

            public static global::NodaTime.OffsetDateTime MaxIsoValue
            {
                get { return global::NodaTime.OffsetDateTime.FromDateTimeOffset(DateTimeOffset.MaxValue); }
            }

            public static global::NodaTime.OffsetDateTime Now
            {
                get { return global::NodaTime.OffsetDateTime.FromDateTimeOffset(DateTimeOffset.Now); }
            }

            public static global::NodaTime.OffsetDateTime UtcNow
            {
                get { return global::NodaTime.OffsetDateTime.FromDateTimeOffset(DateTimeOffset.UtcNow); }
            }
        }

        public static class LocalDateTime
        {
            public static readonly LocalDateTimePattern FullIsoPattern = LocalDateTimePattern.CreateWithInvariantCulture("yyyy'-'MM'-'dd'T'HH':'mm':'ss;fffffff");

            public static global::NodaTime.LocalDateTime MinIsoValue
            {
                get { return global::NodaTime.LocalDateTime.FromDateTime(DateTime.MinValue); }
            }

            public static global::NodaTime.LocalDateTime MaxIsoValue
            {
                get { return global::NodaTime.LocalDateTime.FromDateTime(DateTime.MaxValue); }
            }

            public static global::NodaTime.LocalDateTime Now
            {
                get { return global::NodaTime.LocalDateTime.FromDateTime(DateTime.Now); }
            }
        }

        public static class LocalDate
        {
            public static global::NodaTime.LocalDate MinIsoValue
            {
                get { return new global::NodaTime.LocalDate(1, 1, 1); }
            }

            public static global::NodaTime.LocalDate MaxIsoValue
            {
                get { return new global::NodaTime.LocalDate(9999, 12, 31); }
            }

            public static global::NodaTime.LocalDate Today
            {
                get
                {
                    var today = DateTime.Today;
                    return new global::NodaTime.LocalDate(today.Year, today.Month, today.Day);
                }
            }
        }

        public static class LocalTime
        {
            public static global::NodaTime.LocalTime MinIsoValue
            {
                get { return new global::NodaTime.LocalTime(0, 0); }
            }

            public static global::NodaTime.LocalTime MaxIsoValue
            {
                get { return global::NodaTime.LocalTime.FromHourMinuteSecondMillisecondTick(23, 59, 59, 999, 9999); }
            }

            public static global::NodaTime.LocalTime Now
            {
                get { return global::NodaTime.LocalDateTime.FromDateTime(DateTime.Now).TimeOfDay; }
            }

            public static global::NodaTime.LocalTime UtcNow
            {
                get { return global::NodaTime.LocalDateTime.FromDateTime(DateTime.UtcNow).TimeOfDay; }
            }
        }

        public static class Duration
        {
            public static global::NodaTime.Duration MinValue
            {
                get { return global::NodaTime.Duration.FromTicks(long.MinValue); }
            }

            public static global::NodaTime.Duration MaxValue
            {
                get { return global::NodaTime.Duration.FromTicks(long.MaxValue); }
            }
        }

        public static class Period
        {
            public static global::NodaTime.Period MinValue
            {
                get { return global::NodaTime.Period.FromTicks(long.MinValue); }
            }

            public static global::NodaTime.Period MaxValue
            {
                get { return global::NodaTime.Period.FromTicks(long.MaxValue); }
            }
        }

        public static class ZonedDateTime
        {

        }
    }
}

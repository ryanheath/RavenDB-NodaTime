using System;
using System.Globalization;
using NodaTime;
using NodaTime.Text;

namespace Raven.Client.NodaTime
{
    internal class CustomQueryValueConverters
    {
        public static bool InstantConverter(string name, Instant value, bool forRange, out string strValue)
        {
            NodaUtil.Instant.Validate(value);

            strValue = value.ToString(NodaUtil.Instant.FullIsoPattern.PatternText, null);

            return true;
        }

        public static bool LocalDateTimeConverter(string name, LocalDateTime value, bool forRange, out string strValue)
        {
            strValue = value.ToString(NodaUtil.LocalDateTime.FullIsoPattern.PatternText, null);

            return true;
        }

        public static bool LocalDateConverter(string name, LocalDate value, bool forRange, out string strValue)
        {
            strValue = value.ToString(LocalDatePattern.Iso.PatternText, null);

            return true;
        }


        public static bool LocalTimeConverter(string name, LocalTime value, bool forRange, out object objValue)
        {
            var timeSpan = value.ToTimeSpan();

            return TimeSpanConverter(timeSpan, forRange, out objValue);
        }

        public static bool OffsetConverter(string name, Offset value, bool forRange, out object objValue)
        {
            var timeSpan = value.ToTimeSpan();

            return TimeSpanConverter(timeSpan, forRange, out objValue);
        }

        public static bool DurationConverter(string name, Duration value, bool forRange, out object objValue)
        {
            var timeSpan = value.ToTimeSpan();

            return TimeSpanConverter(timeSpan, forRange, out objValue);
        }

        private static bool TimeSpanConverter(TimeSpan timeSpan, bool forRange, out object objValue)
        {
            if (forRange)
            {
                objValue = timeSpan.Ticks;
                return true;
            }

            objValue = timeSpan.ToString("c");

            return true;
        }

        public static bool OffsetDateTimeConverter(string name, OffsetDateTime value, bool forRange, out string strValue)
        {
            var instant = value.ToInstant();
            NodaUtil.Instant.Validate(instant);

            strValue = instant.ToString(NodaUtil.Instant.FullIsoPattern.PatternText, null);

            return true;
        }

        public static bool ZonedDateTimeConverter(string fieldname, ZonedDateTime value, bool forRange, out string strValue)
        {
            var instant = value.ToInstant();
            NodaUtil.Instant.Validate(instant);

            strValue = instant.ToString(NodaUtil.Instant.FullIsoPattern.PatternText, null);

            return true;
        }

        public static bool PeriodConverter(string name, Period value, bool forRange, out string strValue)
        {
            strValue = value.ToString();

            return true;
        }

        public static bool DateTimeZoneConverter(string fieldname, DateTimeZone value, bool forRange, out string strValue)
        {
            strValue = value.ToString();

            return true;
        }
    }

    /// <summary>
    /// Helper function for numeric to indexed string and vice versa
    /// </summary>
    internal static class NumberUtil
    {
        /// <summary>
        /// Translate a number to an indexable string
        /// </summary>
        public static string NumberToString(long number)
        {
            return number.ToString("G", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Translate a number to an indexable string
        /// </summary>
        public static string NumberToString(double number)
        {
            return number.ToString("G", CultureInfo.InvariantCulture);
        }

        public static string NumberToString(float number)
        {
            return number.ToString("G", CultureInfo.InvariantCulture);
        }
    }
}

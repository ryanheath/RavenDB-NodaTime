using NodaTime;
using NodaTime.Text;
using Raven.Abstractions.Indexing;
using Raven.Client.Document;

namespace Raven.Client.NodaTime
{
    internal class CustomQueryValueConverters
    {
        public static bool InstantConverter(string name, Instant value, QueryValueConvertionType type, out string strValue)
        {
            NodaUtil.Instant.Validate(value);

            strValue = value.ToString(NodaUtil.Instant.FullIsoPattern.PatternText, null);

            return true;
        }

        public static bool LocalDateTimeConverter(string name, LocalDateTime value, QueryValueConvertionType type, out string strValue)
        {
            strValue = value.ToString(NodaUtil.LocalDateTime.FullIsoPattern.PatternText, null);

            return true;
        }

        public static bool LocalDateConverter(string name, LocalDate value, QueryValueConvertionType type, out string strValue)
        {
            strValue = value.ToString(LocalDatePattern.IsoPattern.PatternText, null);

            return true;
        }


        public static bool LocalTimeConverter(string name, LocalTime value, QueryValueConvertionType type, out string strValue)
        {
            if (type == QueryValueConvertionType.Range)
            {
                strValue = NumberUtil.NumberToString(value.TickOfDay);

                return true;
            }

            var timeSpan = value.ToTimeSpan();

            strValue = "\"" + timeSpan.ToString("c") + "\"";

            return true;
        }

        public static bool OffsetConverter(string name, Offset value, QueryValueConvertionType type, out string strValue)
        {
            if (type == QueryValueConvertionType.Range)
            {
                strValue = NumberUtil.NumberToString(value.Ticks);

                return true;
            }

            var timeSpan = value.ToTimeSpan();

            strValue = "\"" + timeSpan.ToString("c") + "\"";

            return true;
        }

        public static bool DurationConverter(string name, Duration value, QueryValueConvertionType type, out string strValue)
        {
            if (type == QueryValueConvertionType.Range)
            {
                strValue = NumberUtil.NumberToString(value.Ticks);

                return true;
            }

            var timeSpan = value.ToTimeSpan();

            strValue = "\"" + timeSpan.ToString("c") + "\"";

            return true;
        }

        public static bool OffsetDateTimeConverter(string name, OffsetDateTime value, QueryValueConvertionType type, out string strValue)
        {
            var instant = value.ToInstant();
            NodaUtil.Instant.Validate(instant);

            strValue = instant.ToString(NodaUtil.Instant.FullIsoPattern.PatternText, null);

            return true;
        }

        public static bool ZonedDateTimeConverter(string fieldname, ZonedDateTime value, QueryValueConvertionType type, out string strValue)
        {
            var instant = value.ToInstant();
            NodaUtil.Instant.Validate(instant);

            strValue = instant.ToString(NodaUtil.Instant.FullIsoPattern.PatternText, null);

            return true;
        }

        public static bool PeriodConverter(string name, Period value, QueryValueConvertionType type, out string strValue)
        {
            strValue = value.ToString();

            return true;
        }
    }
}

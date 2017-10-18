// Copyright 2012 The Noda Time Authors. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.IO;
using NodaTime;
using Raven.Imports.Newtonsoft.Json;
using NodaTime.Text;
using Raven.Imports.Newtonsoft.Json.Linq;

namespace Raven.Imports.NodaTime.Serialization.JsonNet
{
    /// <summary>
    /// A JSON converter for types which can be represented by a single string value, parsed or formatted
    /// from an <see cref="IPattern{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type to convert to/from JSON.</typeparam>
    internal class NodaPatternConverter<T> : NodaConverterBase<T>
    {
        private readonly IPattern<T> pattern;
        private readonly Action<T> validator;

        /// <summary>
        /// Creates a new instance with a pattern and no validator.
        /// </summary>
        /// <param name="pattern">The pattern to use for parsing and formatting.</param>
        /// <exception cref="ArgumentNullException"><paramref name="pattern"/> is null</exception>
        public NodaPatternConverter(IPattern<T> pattern) : this(pattern, null)
        {
        }

        /// <summary>
        /// Creates a new instance with a pattern and an optional validator. The validator will be called before each
        /// value is written, and may throw an exception to indicate that the value cannot be serialized.
        /// </summary>
        /// <param name="pattern">The pattern to use for parsing and formatting.</param>
        /// <param name="validator">The validator to call before writing values. May be null, indicating that no validation is required.</param>
        /// <exception cref="ArgumentNullException"><paramref name="pattern"/> is null</exception>
        public NodaPatternConverter(IPattern<T> pattern, Action<T> validator)
        {
            // Note: We could use Preconditions.CheckNotNull, but only if we either made that public in NodaTime
            // or made InternalsVisibleTo this assembly. 
            if (pattern == null)
            {
                throw new ArgumentNullException("pattern");
            }
            this.pattern = pattern;
            this.validator = validator;
        }

        /// <summary>
        /// Implemented by concrete subclasses, this performs the final conversion from a non-null JSON value to
        /// a value of type T.
        /// </summary>
        /// <param name="reader">The JSON reader to pull data from</param>
        /// <param name="serializer">The serializer to use for nested serialization</param>
        protected override T ReadJsonImpl(JsonReader reader, JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.String)
            {
                throw new InvalidDataException(
                    string.Format("Unexpected token parsing {0}. Expected String, got {1}.",
                    typeof(T).Name,
                    reader.TokenType));
            }
            string text = reader.Value.ToString();
            return pattern.Parse(text).Value;
        }

        /// <summary>
        /// Writes the formatted value to the writer.
        /// </summary>
        /// <param name="writer">The writer to write JSON data to</param>
        /// <param name="value">The value to serializer</param>
        /// <param name="serializer">The serializer to use for nested serialization</param>
        protected override void WriteJsonImpl(JsonWriter writer, T value, JsonSerializer serializer)
        {
            if (validator != null)
            {
                validator(value);
            }
            writer.WriteValue(pattern.Format(value));
        }
    }

    /// <summary>
    /// As RelaxedNodaPatternConvert but is able to read T stored in NodaTime format 
    /// </summary>
    internal class RelaxedNodaPatternConverter<T> : NodaPatternConverter<T>
    {
        private readonly Func<JObject, T> parser;

        public RelaxedNodaPatternConverter(IPattern<T> pattern, Func<JObject,T> parser) : base(pattern, null)
        {
            this.parser = parser;
        }

        public RelaxedNodaPatternConverter(IPattern<T> pattern, Func<JObject, T> parser, Action<T> validator) : base(pattern, validator)
        {
            this.parser = parser;
        }

        protected override T ReadJsonImpl(JsonReader reader, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.StartObject)
            {
                var o = JObject.Load(reader);
                return parser(o);
            }

            return base.ReadJsonImpl(reader, serializer);
        }
    }

    public static class ConverterParsers
    {
        public static T ToObject<T>(JObject o)
        {
            return o.ToObject<T>();
        }

        public static LocalDate ToLocalDate(JObject o)
        {
            // we cannot depend on o.ToObject<T> here
            // since for some reason the calendar property will be read as a number while it is not ...
            var year = o.GetValue("year").Value<int>();
            var month = o.GetValue("month").Value<int>();
            var day = o.GetValue("day").Value<int>();
            var id = o.GetValue("calendar").Value<string>();
            var calendar = CalendarSystem.ForId(id == "Iso" ? "ISO" : id); // for some reason ISO is persisted as Iso ...
            return new LocalDate(year, month, day, calendar);
        }

        public static LocalDateTime ToLocalDateTime(JObject o)
        {
            // we cannot depend on o.ToObject<T> here
            // since for some reason the calendar property will be read as a number while it is not ...
            var year = o.GetValue("year").Value<int>();
            var month = o.GetValue("month").Value<int>();
            var day = o.GetValue("day").Value<int>();
            var nanoSecondsOfDay = o.GetValue("nanoOfDay").Value<long>();
            var id = o.GetValue("calendar").Value<string>();
            var calendar = CalendarSystem.ForId(id == "Iso" ? "ISO" : id); // for some reason ISO is persisted as Iso ...
            return new LocalDateTime(year, month, day, 0, 0, calendar).PlusNanoseconds(nanoSecondsOfDay);
        }
    }
}

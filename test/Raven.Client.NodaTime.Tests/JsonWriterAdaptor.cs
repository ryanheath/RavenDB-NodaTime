using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Raven.Client.Documents.Session;
using Raven.Client.Json.Serialization;
using Sparrow.Json;

namespace Raven.Client.NodaTime.Tests
{
	public class JsonWriterAdaptor : IJsonWriter
	{
		private readonly JsonTextWriter _writer;

		public JsonWriterAdaptor(JsonTextWriter writer) => _writer = writer;

		public BlittableJsonReaderObject CreateReader() => throw new NotImplementedException();
		public void Dispose() => _writer.Flush();
		public void FinalizeDocument() { } // noop
		public void WriteMetadata(IMetadataDictionary metadata)
		{
			_writer.WriteStartObject();
			foreach (var kvp in metadata)
			{
				_writer.WritePropertyName(kvp.Key);
				_writer.WriteValue(kvp.Value);
			}
			_writer.WriteEndObject();
		}

		// wrapping
		public void Close() => _writer.Close();
		public void Flush() => _writer.Flush();
		public void WriteComment(string text) => _writer.WriteComment(text);
		public void WriteEnd() => _writer.WriteEnd();
		public void WriteEndArray() => _writer.WriteEndArray();
		public void WriteEndConstructor() => _writer.WriteEndConstructor();
		public void WriteEndObject() => _writer.WriteEndObject();
		public void WriteNull() => _writer.WriteNull();
		public void WritePropertyName(string name) => _writer.WritePropertyName(name);
		public void WritePropertyName(string name, bool escape) => _writer.WritePropertyName(name, escape);
		public void WriteRaw(string json) => _writer.WriteRaw(json);
		public void WriteRawValue(string json) => _writer.WriteRawValue(json);
		public void WriteStartArray() => _writer.WriteStartArray();
		public void WriteStartConstructor(string name) => _writer.WriteStartConstructor(name);
		public void WriteStartObject() => _writer.WriteStartObject();
		public void WriteUndefined() => _writer.WriteUndefined();
		public void WriteValue(bool value) => _writer.WriteValue(value);
		public void WriteValue(bool? value) => _writer.WriteValue(value);
		public void WriteValue(byte value) => _writer.WriteValue(value);
		public void WriteValue(byte? value) => _writer.WriteValue(value);
		public void WriteValue(byte[] value) => _writer.WriteValue(value);
		public void WriteValue(char value) => _writer.WriteValue(value);
		public void WriteValue(char? value) => _writer.WriteValue(value);
		public void WriteValue(DateTime dt) => _writer.WriteValue(dt);
		public void WriteValue(DateTime? value) => _writer.WriteValue(value);
		public void WriteValue(DateTimeOffset dto) => _writer.WriteValue(dto);
		public void WriteValue(DateTimeOffset? value) => _writer.WriteValue(value);
		public void WriteValue(decimal value) => _writer.WriteValue(value);
		public void WriteValue(decimal? value) => _writer.WriteValue(value);
		public void WriteValue(double value) => _writer.WriteValue(value);
		public void WriteValue(double? value) => _writer.WriteValue(value);
		public void WriteValue(float value) => _writer.WriteValue(value);
		public void WriteValue(float? value) => _writer.WriteValue(value);
		public void WriteValue(Guid value) => _writer.WriteValue(value);
		public void WriteValue(Guid? value) => _writer.WriteValue(value);
		public void WriteValue(int value) => _writer.WriteValue(value);
		public void WriteValue(int? value) => _writer.WriteValue(value);
		public void WriteValue(long value) => _writer.WriteValue(value);
		public void WriteValue(long? value) => _writer.WriteValue(value);
		public void WriteValue(object value) => _writer.WriteValue(value);
		public void WriteValue(sbyte value) => _writer.WriteValue(value);
		public void WriteValue(sbyte? value) => _writer.WriteValue(value);
		public void WriteValue(short value) => _writer.WriteValue(value);
		public void WriteValue(short? value) => _writer.WriteValue(value);
		public void WriteValue(string value) => _writer.WriteValue(value);
		public void WriteValue(TimeSpan ts) => _writer.WriteValue(ts);
		public void WriteValue(TimeSpan? value) => _writer.WriteValue(value);
		public void WriteValue(uint value) => _writer.WriteValue(value);
		public void WriteValue(uint? value) => _writer.WriteValue(value);
		public void WriteValue(ulong value) => _writer.WriteValue(value);
		public void WriteValue(ulong? value) => _writer.WriteValue(value);
		public void WriteValue(Uri value) => _writer.WriteValue(value);
		public void WriteValue(ushort value) => _writer.WriteValue(value);
		public void WriteValue(ushort? value) => _writer.WriteValue(value);
		public void WriteWhitespace(string ws) => _writer.WriteWhitespace(ws);
		public void WriteValue(TimeOnly value) => _writer.WriteValue(value);
		public void WriteValue(TimeOnly? value) => _writer.WriteValue(value);
		public void WriteValue(DateOnly value) => _writer.WriteValue(value);
		public void WriteValue(DateOnly? value) => _writer.WriteValue(value);
	}
}

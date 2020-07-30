using System.Diagnostics;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Raven.Client.Documents;
using Raven.Client.Json.Serialization;

namespace Raven.Client.NodaTime.Tests
{
    public static class TestExtensions
    {
        private static IJsonSerializer _serializer;

        public static void DebugWriteJson(this IDocumentStore documentStore, object o)
        {
            if (_serializer == null)
            {
                _serializer = documentStore.Conventions.Serialization.CreateSerializer();
            }

            var sb = new StringBuilder();
            using (var stringWriter = new StringWriter(sb))
            using (var jsonWriter = new JsonTextWriter(stringWriter))
            using (var adaptor = new JsonWriterAdaptor(jsonWriter))
            {
                _serializer.Serialize(adaptor, o);
            }

            Debug.WriteLine(sb);
        }
    }
}
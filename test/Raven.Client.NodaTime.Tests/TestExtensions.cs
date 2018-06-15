using System.Diagnostics;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Raven.Client.Documents;

namespace Raven.Client.NodaTime.Tests
{
    public static class TestExtensions
    {
        private static JsonSerializer _serializer;

        public static void DebugWriteJson(this IDocumentStore documentStore, object o)
        {
            if (_serializer == null)
            {

                _serializer = documentStore.Conventions.CreateSerializer();
            }

            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                _serializer.Serialize(writer, o);
            }

            Debug.WriteLine(sb);
        }
    }
}
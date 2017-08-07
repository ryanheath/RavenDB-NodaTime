using System.Diagnostics;
using System.IO;
using System.Text;
using Raven.Imports.Newtonsoft.Json;

namespace Raven.Client.NodaTime.Tests
{
    public static class TestExtensions
    {
        private static JsonSerializer _serializer;

        public static void DebugWriteJson(this IDocumentStore documentStore, object o)
        {
            if (_serializer == null)
            {
                documentStore.Conventions.CustomizeJsonSerializer += jsonSerializer =>
                {
                    jsonSerializer.Formatting = Formatting.Indented;
                };

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
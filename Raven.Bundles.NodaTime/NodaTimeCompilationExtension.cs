using System.Xml.Serialization;
using NodaTime;
using Raven.Bundles.NodaTime.Indexing;
using Raven.Database.Plugins;

namespace Raven.Bundles.NodaTime
{
    public class NodaTimeCompilationExtension : AbstractDynamicCompilationExtension
    {
        public override string[] GetNamespacesToImport()
        {
            return new[] { typeof(Instant).Namespace, typeof(NodaTimeField).Namespace };
        }

        public override string[] GetAssembliesToReference()
        {
            return new[] { typeof(Instant).Assembly.Location, typeof(NodaTimeField).Assembly.Location, typeof(IXmlSerializable).Assembly.Location };
        }
    }
}

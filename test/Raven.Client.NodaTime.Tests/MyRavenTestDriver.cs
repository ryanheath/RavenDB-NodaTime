using Raven.Client.Documents;
using Raven.TestDriver;

namespace Raven.Client.NodaTime.Tests
{
    public class MyRavenTestDriver : RavenTestDriver
    {
        private static bool _hasBeenConfigured;
        
        protected MyRavenTestDriver()
        {
            if (_hasBeenConfigured) 
                return;
            
            ConfigureServer(new TestServerOptions
            {
                Licensing =
                {
                    ThrowOnInvalidOrMissingLicense = false
                }
            });
            _hasBeenConfigured = true;
        }

        protected override void PreInitialize(IDocumentStore documentStore)
        {
            documentStore.ConfigureForNodaTime();
            base.PreInitialize(documentStore);
        }

        protected IDocumentStore NewDocumentStore() => GetDocumentStore();
    }
}

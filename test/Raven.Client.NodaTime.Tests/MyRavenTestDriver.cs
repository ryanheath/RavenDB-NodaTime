using Raven.Client.Documents;
using Raven.TestDriver;

namespace Raven.Client.NodaTime.Tests;

public class MyRavenTestDriver : RavenTestDriver
{
    static MyRavenTestDriver()
    {
        ConfigureServer(new TestServerOptions
        {
            Licensing =
            {
                ThrowOnInvalidOrMissingLicense = false
            }
        });
    }

    protected override void PreInitialize(IDocumentStore documentStore)
    {
        documentStore.ConfigureForNodaTime();
        base.PreInitialize(documentStore);
    }

    protected IDocumentStore NewDocumentStore() => GetDocumentStore();
}

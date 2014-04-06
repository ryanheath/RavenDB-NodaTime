using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.Linq;
using NodaTime;
using Raven.Abstractions.Indexing;
using Raven.Bundles.NodaTime;
using Raven.Client.Indexes;
using Raven.Database.Config;
using Raven.Tests.Helpers;
using Xunit;

namespace Raven.Client.NodaTime.Tests.TimeZoneConversionTests
{
    public class DateTimeZoneTests_Bcl : RavenTestBase
    {
        protected override void ModifyConfiguration(InMemoryRavenConfiguration configuration)
        {
            configuration.Catalog.Catalogs.Add(new AssemblyCatalog(typeof(NodaTimeCompilationExtension).Assembly));
        }

        [Fact]
        public void Can_Convert_TimeZone_Using_Bcl_DateTimeZone_In_Static_Index()
        {
            using (var documentStore = NewDocumentStore())
            {
                documentStore.ConfigureForNodaTime();
                documentStore.ExecuteIndex(new Foo_ByDate_MultiZone());

                using (var session = documentStore.OpenSession())
                {
                    session.Store(new Foo { Instant = SystemClock.Instance.Now });
                    session.SaveChanges();
                }

                WaitForIndexing(documentStore);

                using (var session = documentStore.OpenSession())
                {
                    var result = session.Query<Foo, Foo_ByDate_MultiZone>()
                                        .ProjectFromIndexFieldsInto<Result>()
                                        .First();

                    Debug.WriteLine("UTC:      {0}", result.Instant);
                    Debug.WriteLine("Eastern:  {0:s}", result.DateTimeEastern);
                    Debug.WriteLine("Central:  {0:s}", result.DateTimeCentral);
                    Debug.WriteLine("Mountain: {0:s}", result.DateTimeMountain);
                    Debug.WriteLine("Pacific:  {0:s}", result.DateTimePacific);
                }
            }
        }

        public class Foo
        {
            public Instant Instant { get; set; }
        }

        public class Result
        {
            public Instant Instant { get; set; }
            public LocalDateTime DateTimeEastern { get; set; }
            public LocalDateTime DateTimeCentral { get; set; }
            public LocalDateTime DateTimeMountain { get; set; }
            public LocalDateTime DateTimePacific { get; set; }
        }

        public class Foo_ByDate_MultiZone : AbstractIndexCreationTask<Foo, Result>
        {
            public Foo_ByDate_MultiZone()
            {
                Map = foos => from foo in foos
                              let zones = DateTimeZoneProviders.Bcl
                              let instant = foo.Instant.AsInstant()
                              select new
                                     {
                                         foo.Instant,
                                         DateTimeEastern = instant.InZone(zones["Eastern Standard Time"]).LocalDateTime.Resolve(),
                                         DateTimeCentral = instant.InZone(zones["Central Standard Time"]).LocalDateTime.Resolve(),
                                         DateTimeMountain = instant.InZone(zones["Mountain Standard Time"]).LocalDateTime.Resolve(),
                                         DateTimePacific = instant.InZone(zones["Pacific Standard Time"]).LocalDateTime.Resolve(),
                                     };
                StoreAllFields(FieldStorage.Yes);
            }
        }
    }
}

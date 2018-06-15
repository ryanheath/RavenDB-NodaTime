using System.Collections.Generic;
using System.Linq;
using NodaTime;
using Raven.Client.Documents;
using Raven.Client.Documents.Indexes;
using Xunit;

namespace Raven.Client.NodaTime.Tests.TimeZoneConversionTests
{
    public class DateTimeZoneTests_Tzdb : MyRavenTestDriver
    {

        [Fact]
        public void Can_Convert_TimeZone_Using_Tzdb_DateTimeZone_In_Static_Index()
        {
            using (var documentStore = NewDocumentStore())
            {
                documentStore.ExecuteIndex(new Foo_ByDate_MultiZone());

                using (var session = documentStore.OpenSession())
                {
                    session.Store(new Foo { Instant = SystemClock.Instance.GetCurrentInstant() });
                    session.SaveChanges();
                }

                WaitForIndexing(documentStore);

                //WaitForUserToContinueTheTest(documentStore);

                using (var session = documentStore.OpenSession())
                {
                    var result = session.Query<Foo, Foo_ByDate_MultiZone>()
                                        .ProjectInto<Result>()
                                        .First();

                    System.Diagnostics.Debug.WriteLine("UTC:      {0}", result.Instant);
                    System.Diagnostics.Debug.WriteLine("Eastern:  {0:s}", result.DateTimeEastern);
                    System.Diagnostics.Debug.WriteLine("Central:  {0:s}", result.DateTimeCentral);
                    System.Diagnostics.Debug.WriteLine("Mountain: {0:s}", result.DateTimeMountain);
                    System.Diagnostics.Debug.WriteLine("Pacific:  {0:s}", result.DateTimePacific);
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
                              let zones = DateTimeZoneProviders.Tzdb
                              let instant = foo.Instant.AsInstant()
                              select new
                                     {
                                         foo.Instant,
                                         DateTimeEastern = instant.InZone(zones["America/New_York"]).LocalDateTime.Resolve(),
                                         DateTimeCentral = instant.InZone(zones["America/Chicago"]).LocalDateTime.Resolve(),
                                         DateTimeMountain = instant.InZone(zones["America/Denver"]).LocalDateTime.Resolve(),
                                         DateTimePacific = instant.InZone(zones["America/Los_Angeles"]).LocalDateTime.Resolve(),
                                     };
                StoreAllFields(FieldStorage.Yes);

                AdditionalSources = new Dictionary<string, string> {
                    { "Raven.Client.NodaTime", NodaTimeCompilationExtension.AdditionalSourcesRavenBundlesNodaTime },
                    { "Raven.Client.NodaTime2", NodaTimeCompilationExtension.AdditionalSourcesNodaTime }
                };
            }
        }
    }
}

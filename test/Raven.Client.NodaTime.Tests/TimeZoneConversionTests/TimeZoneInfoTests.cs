using System;
using System.Linq;
using Raven.Client.Documents;
using Raven.Client.Documents.Indexes;
using Xunit;

namespace Raven.Client.NodaTime.Tests.TimeZoneConversionTests
{
    public class TimeZoneInfoTests : MyRavenTestDriver
    {
        [Fact]
        public void Can_Convert_TimeZone_Using_TimeZoneInfo_In_Static_Index()
        {
            using (var documentStore = NewDocumentStore())
            {
                documentStore.ExecuteIndex(new Foo_ByDate_MultiZone());

                using (var session = documentStore.OpenSession())
                {
                    session.Store(new Foo { DateTimeUtc = DateTime.UtcNow });
                    session.SaveChanges();
                }

                WaitForIndexing(documentStore);

                using (var session = documentStore.OpenSession())
                {
                    var result = session.Query<Foo, Foo_ByDate_MultiZone>()
                                        .ProjectInto<Result>()
                                        .First();

                    System.Diagnostics.Debug.WriteLine("UTC:      {0}", result.DateTimeUtc);
                    System.Diagnostics.Debug.WriteLine("Pacific:  {0}", result.DateTimePacific);
                    System.Diagnostics.Debug.WriteLine("Mountain: {0}", result.DateTimeMountain);
                    System.Diagnostics.Debug.WriteLine("Central:  {0}", result.DateTimeCentral);
                    System.Diagnostics.Debug.WriteLine("Eastern:  {0}", result.DateTimeEastern);
                }
            }
        }

        public class Foo
        {
            public DateTime DateTimeUtc { get; set; }
        }

        public class Result
        {
            public DateTime DateTimeUtc { get; set; }
            public DateTime DateTimePacific { get; set; }
            public DateTime DateTimeMountain { get; set; }
            public DateTime DateTimeCentral { get; set; }
            public DateTime DateTimeEastern { get; set; }
        }

        public class Foo_ByDate_MultiZone : AbstractIndexCreationTask<Foo, Result>
        {
            public Foo_ByDate_MultiZone()
            {
                Map = foos => from foo in foos
                              select new
                                     {
                                         foo.DateTimeUtc,
                                         DateTimePacific = TimeZoneInfo.ConvertTime(foo.DateTimeUtc, TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time")),
                                         DateTimeMountain = TimeZoneInfo.ConvertTime(foo.DateTimeUtc, TimeZoneInfo.FindSystemTimeZoneById("Mountain Standard Time")),
                                         DateTimeCentral = TimeZoneInfo.ConvertTime(foo.DateTimeUtc, TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time")),
                                         DateTimeEastern = TimeZoneInfo.ConvertTime(foo.DateTimeUtc, TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time")),
                                     };
                StoreAllFields(FieldStorage.Yes);
            }
        }
    }
}

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
                                        .ProjectFromIndexFieldsInto<Result>()
                                        .First();

                    Debug.WriteLine("UTC:      {0}", result.DateTimeUtc);
                    Debug.WriteLine("Pacific:  {0}", result.DateTimePacific);
                    Debug.WriteLine("Mountain: {0}", result.DateTimeMountain);
                    Debug.WriteLine("Central:  {0}", result.DateTimeCentral);
                    Debug.WriteLine("Eastern:  {0}", result.DateTimeEastern);
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
                                         DateTimePacific = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(foo.DateTimeUtc, "Pacific Standard Time"),
                                         DateTimeMountain = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(foo.DateTimeUtc, "Mountain Standard Time"),
                                         DateTimeCentral = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(foo.DateTimeUtc, "Central Standard Time"),
                                         DateTimeEastern = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(foo.DateTimeUtc, "Eastern Standard Time"),
                                     };
                StoreAllFields(FieldStorage.Yes);
            }
        }
    }
}

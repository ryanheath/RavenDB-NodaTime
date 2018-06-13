using System.Linq;
using NodaTime;
using Raven.Client.Documents.Commands;
using Raven.Client.Documents.Indexes;
using Sparrow.Json;
using Xunit;

namespace Raven.Client.NodaTime.Tests
{
    public class NodaDateTimeZoneTests : MyRavenTestDriver
    {
        [Fact]
        public void Can_Use_NodaTime_DateTimeZone_In_Document()
        {
            var zone = DateTimeZoneProviders.Tzdb.GetSystemDefault();

            using (var documentStore = NewDocumentStore())
            {
                using (var session = documentStore.OpenSession())
                {
                    session.Store(new Foo { Id = "foos/1", DateTimeZone = zone });
                    session.SaveChanges();
                }

                using (var session = documentStore.OpenSession())
                {
                    var foo = session.Load<Foo>("foos/1");

                    Assert.Equal(zone, foo.DateTimeZone);
                }

                var json = documentStore.DatabaseCommands.Get("foos/1").DataAsJson;
                Debug.WriteLine(json.ToString(Formatting.Indented));
                Assert.Equal(zone.Id, json.Value<string>("DateTimeZone"));
            }
        }

        [Fact]
        public void Can_Use_NodaTime_DateTimeZone_In_Dynamic_Index()
        {
            var zone = DateTimeZoneProviders.Tzdb.GetSystemDefault();

            using (var documentStore = NewDocumentStore())
            {
                using (var session = documentStore.OpenSession())
                {
                    session.Store(new Foo { Id = "foos/1", DateTimeZone = zone });
                    session.SaveChanges();
                }

                using (var session = documentStore.OpenSession())
                {
                    var q1 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults()).Where(x => x.DateTimeZone.Equals(zone));
                    var results1 = q1.ToList();
                    Assert.Single(results1);
                }
            }
        }

        [Fact]
        public void Can_Use_NodaTime_DateTimeZone_In_Static_Index()
        {
            var zone = DateTimeZoneProviders.Tzdb.GetSystemDefault();

            using (var documentStore = NewDocumentStore())
            {
                documentStore.ExecuteIndex(new TestIndex());

                using (var session = documentStore.OpenSession())
                {
                    session.Store(new Foo { Id = "foos/1", DateTimeZone = zone });
                    session.SaveChanges();
                }

                using (var session = documentStore.OpenSession())
                {
                    var q1 = session.Query<Foo, TestIndex>().Customize(x => x.WaitForNonStaleResults()).Where(x => x.DateTimeZone.Equals(zone));
                    var results1 = q1.ToList();
                    Assert.Single(results1);
                }
            }
        }

        public class Foo
        {
            public string Id { get; set; }
            public DateTimeZone DateTimeZone { get; set; }
        }

        public class TestIndex : AbstractIndexCreationTask<Foo>
        {
            public TestIndex()
            {
                Map = foos => from foo in foos
                              select new
                              {
                                  foo.DateTimeZone
                              };
            }
        }
    }
}

using System.Diagnostics;
using System.Linq;
using NodaTime;
using Raven.Client.Indexes;
using Raven.Imports.Newtonsoft.Json;
using Raven.Tests.Helpers;
using Xunit;

namespace Raven.Client.NodaTime.Tests
{
    public class NodaDateTimeZoneTests : RavenTestBase
    {
        [Fact]
        public void Can_Use_NodaTime_DateTimeZone_In_Document()
        {
            var zone = DateTimeZoneProviders.Tzdb.GetSystemDefault();

            using (var documentStore = NewDocumentStore())
            {
                documentStore.ConfigureForNodaTime();

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
                documentStore.ConfigureForNodaTime();

                using (var session = documentStore.OpenSession())
                {
                    session.Store(new Foo { Id = "foos/1", DateTimeZone = zone });
                    session.SaveChanges();
                }

                using (var session = documentStore.OpenSession())
                {
                    var q1 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults()).Where(x => x.DateTimeZone.Equals(zone));
                    var results1 = q1.ToList();
                    Assert.Equal(1, results1.Count);
                }
            }
        }

        [Fact]
        public void Can_Use_NodaTime_DateTimeZone_In_Static_Index()
        {
            var zone = DateTimeZoneProviders.Tzdb.GetSystemDefault();

            using (var documentStore = NewDocumentStore())
            {
                documentStore.ConfigureForNodaTime();
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
                    Assert.Equal(1, results1.Count);
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

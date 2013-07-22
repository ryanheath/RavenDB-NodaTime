using System.Diagnostics;
using System.Linq;
using NodaTime;
using Raven.Client.Indexes;
using Raven.Imports.Newtonsoft.Json;
using Raven.Tests.Helpers;
using Xunit;

namespace Raven.Client.NodaTime.Tests
{
    public class NodaIntervalTests : RavenTestBase
    {
        [Fact]
        public void Can_Use_NodaTime_Interval_In_Document()
        {
            var now = SystemClock.Instance.Now;
            var start = now - Duration.FromMinutes(5);
            var end = now + Duration.FromMinutes(5);
            var interval = new Interval(start, end);

            using (var documentStore = NewDocumentStore())
            {
                documentStore.ConfigureForNodaTime();

                using (var session = documentStore.OpenSession())
                {
                    session.Store(new Foo { Id = "foos/1", Interval = interval });
                    session.SaveChanges();
                }

                using (var session = documentStore.OpenSession())
                {
                    var foo = session.Load<Foo>("foos/1");

                    Assert.Equal(interval, foo.Interval);
                }

                var json = documentStore.DatabaseCommands.Get("foos/1").DataAsJson;
                Debug.WriteLine(json.ToString(Formatting.Indented));
                var expectedStart = interval.Start.ToString(NodaUtil.Instant.FullIsoPattern.PatternText, null);
                var expectedEnd = interval.End.ToString(NodaUtil.Instant.FullIsoPattern.PatternText, null);
                Assert.Equal(expectedStart, json["Interval"].Value<string>("Start"));
                Assert.Equal(expectedEnd, json["Interval"].Value<string>("End"));
            }
        }

        [Fact]
        public void Can_Use_NodaTime_Interval_In_Dynamic_Index()
        {
            var now = SystemClock.Instance.Now;
            var start = now - Duration.FromMinutes(5);
            var end = now + Duration.FromMinutes(5);
            var interval = new Interval(start, end);

            using (var documentStore = NewDocumentStore())
            {
                documentStore.ConfigureForNodaTime();

                using (var session = documentStore.OpenSession())
                {
                    session.Store(new Foo { Id = "foos/1", Interval = interval });
                    session.SaveChanges();
                }

                using (var session = documentStore.OpenSession())
                {
                    var q1 = session.Query<Foo>()
                                    .Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.Interval.Start == interval.Start && x.Interval.End == interval.End);
                    var results1 = q1.ToList();
                    Assert.Equal(1, results1.Count);

                    var q2 = session.Query<Foo>()
                                    .Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.Interval.Start <= now && x.Interval.End > now);
                    var results2 = q2.ToList();
                    Assert.Equal(1, results2.Count);
                }
            }
        }

        [Fact]
        public void Can_Use_NodaTime_Interval_In_Static_Index()
        {
            var now = SystemClock.Instance.Now;
            var start = now - Duration.FromMinutes(5);
            var end = now + Duration.FromMinutes(5);
            var interval = new Interval(start, end);

            using (var documentStore = NewDocumentStore())
            {
                documentStore.ConfigureForNodaTime();
                documentStore.ExecuteIndex(new TestIndex());

                using (var session = documentStore.OpenSession())
                {
                    session.Store(new Foo { Id = "foos/1", Interval = interval });
                    session.SaveChanges();
                }

                using (var session = documentStore.OpenSession())
                {
                    var q1 = session.Query<Foo, TestIndex>()
                                    .Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.Interval.Start == interval.Start && x.Interval.End == interval.End);
                    var results1 = q1.ToList();
                    Assert.Equal(1, results1.Count);

                    var q2 = session.Query<Foo, TestIndex>()
                                    .Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.Interval.Start <= now && x.Interval.End > now);
                    var results2 = q2.ToList();
                    Assert.Equal(1, results2.Count);
                }
            }
        }

        public class Foo
        {
            public string Id { get; set; }
            public Interval Interval { get; set; }
        }

        public class TestIndex : AbstractIndexCreationTask<Foo>
        {
            public TestIndex()
            {
                Map = foos => from foo in foos
                              select new
                              {
                                  Interval_Start = foo.Interval.Start,
                                  Interval_End = foo.Interval.End
                              };
            }
        }
    }
}

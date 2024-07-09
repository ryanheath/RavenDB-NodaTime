using System.Linq;
using NodaTime;
using Raven.Client.Documents.Commands;
using Raven.Client.Documents.Conventions;
using Raven.Client.Documents.Indexes;
using Sparrow.Json;
using Xunit;

namespace Raven.Client.NodaTime.Tests
{
    public class NodaIntervalTests : MyRavenTestDriver
    {
        [Fact]
        public void Can_Use_NodaTime_Interval_In_Document()
        {
            var now = SystemClock.Instance.GetCurrentInstant();
            var start = now - Duration.FromMinutes(5);
            var end = now + Duration.FromMinutes(5);
            var interval = new Interval(start, end);

            using (var documentStore = NewDocumentStore())
            {
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

                using (var session = documentStore.OpenSession())
                {
                    var command = new GetDocumentsCommand(new DocumentConventions(), "foos/1", null, false);
                    session.Advanced.RequestExecutor.Execute(command, session.Advanced.Context);
                    var json = (BlittableJsonReaderObject)command.Result.Results[0];
                    System.Diagnostics.Debug.WriteLine(json.ToString());
                    var expectedStart = interval.Start.ToString(NodaUtil.Instant.FullIsoPattern.PatternText, null);
                    var expectedEnd = interval.End.ToString(NodaUtil.Instant.FullIsoPattern.PatternText, null);
                    json.TryGetMember("Interval", out var obj);
                    var bInterval = obj as BlittableJsonReaderObject;
                    bInterval.TryGet("Start", out string valueStart);
                    bInterval.TryGet("End", out string valueEnd);
                    Assert.Equal(expectedStart, valueStart);
                    Assert.Equal(expectedEnd, valueEnd);
                }
            }
        }

        [Fact]
        public void Can_Use_NodaTime_Interval_In_Dynamic_Index()
        {
            var now = SystemClock.Instance.GetCurrentInstant();
            var start = now - Duration.FromMinutes(5);
            var end = now + Duration.FromMinutes(5);
            var interval1 = new Interval(start, end);
            var interval2 = new Interval(end, end + Duration.FromMinutes(5));

            using (var documentStore = NewDocumentStore())
            {
                using (var session = documentStore.OpenSession())
                {
                    session.Store(new Foo { Id = "foos/1", Interval = interval1 });
                    session.Store(new Foo { Id = "foos/2", Interval = interval2 });
                    session.SaveChanges();
                }

                using (var session = documentStore.OpenSession())
                {
                    var q1 = session.Query<Foo>()
                                    .Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.Interval.Start == interval1.Start && x.Interval.End == interval1.End);
                    var results1 = q1.ToList();
                    Assert.Single(results1);

                    var q2 = session.Query<Foo>()
                                    .Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.Interval.Start <= now && x.Interval.End > now);
                    var results2 = q2.ToList();
                    Assert.Single(results2);

                    var q3 = session.Query<Foo>()
                                    .Customize(x => x.WaitForNonStaleResults())
                                    .OrderByDescending(x => x.Interval.Start);
                    var results3 = q3.ToList();
                    Assert.Equal(2, results3.Count);
                    Assert.True(results3[0].Interval.Start > results3[1].Interval.Start);
                }
            }
        }

        [Fact]
        public void Can_Use_NodaTime_Interval_In_Static_Index()
        {
            var now = SystemClock.Instance.GetCurrentInstant();
            var start = now - Duration.FromMinutes(5);
            var end = now + Duration.FromMinutes(5);
            var interval1 = new Interval(start, end);
            var interval2 = new Interval(end, end + Duration.FromMinutes(5));

            using (var documentStore = NewDocumentStore())
            {
                documentStore.ExecuteIndex(new TestIndex());

                using (var session = documentStore.OpenSession())
                {
                    session.Store(new Foo { Id = "foos/1", Interval = interval1 });
                    session.Store(new Foo { Id = "foos/2", Interval = interval2 });
                    session.SaveChanges();
                }

                using (var session = documentStore.OpenSession())
                {
                    var q1 = session.Query<Foo, TestIndex>()
                                    .Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.Interval.Start == interval1.Start && x.Interval.End == interval1.End);
                    var results1 = q1.ToList();
                    Assert.Single(results1);

                    var q2 = session.Query<Foo, TestIndex>()
                                    .Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.Interval.Start <= now && x.Interval.End > now);
                    var results2 = q2.ToList();
                    Assert.Single(results2);

                    var q3 = session.Query<Foo>()
                                    .Customize(x => x.WaitForNonStaleResults())
                                    .OrderByDescending(x => x.Interval.Start);
                    var results3 = q3.ToList();
                    Assert.Equal(2, results3.Count);
                    Assert.True(results3[0].Interval.Start > results3[1].Interval.Start);
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

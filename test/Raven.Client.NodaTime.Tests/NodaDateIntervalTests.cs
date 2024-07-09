using System.Linq;
using NodaTime;
using NodaTime.Text;
using Raven.Client.Documents.Commands;
using Raven.Client.Documents.Conventions;
using Raven.Client.Documents.Indexes;
using Sparrow.Json;
using Xunit;

namespace Raven.Client.NodaTime.Tests
{
    public class NodaDateIntervalTests : MyRavenTestDriver
    {
        [Fact]
        public void Can_Use_NodaTime_DateInterval_In_Document()
        {
            var today = NodaUtil.LocalDate.Today;
            var start = today.PlusDays(-1);
            var end = today.PlusDays(1);
            var dateInterval = new DateInterval(start, end);

            using (var documentStore = NewDocumentStore())
            {
                using (var session = documentStore.OpenSession())
                {
                    session.Store(new Foo { Id = "foos/1", DateInterval = dateInterval });
                    session.SaveChanges();
                }

                using (var session = documentStore.OpenSession())
                {
                    var foo = session.Load<Foo>("foos/1");

                    Assert.Equal(dateInterval, foo.DateInterval);
                }

                using (var session = documentStore.OpenSession())
                {
                    var command = new GetDocumentsCommand(new DocumentConventions(), "foos/1", null, false);
                    session.Advanced.RequestExecutor.Execute(command, session.Advanced.Context);
                    var json = (BlittableJsonReaderObject)command.Result.Results[0];
                    System.Diagnostics.Debug.WriteLine(json.ToString());
                    var expectedStart = dateInterval.Start.ToString(LocalDatePattern.Iso.PatternText, null);
                    var expectedEnd = dateInterval.End.ToString(LocalDatePattern.Iso.PatternText, null);
                    json.TryGetMember("DateInterval", out var obj);
                    var bInterval = obj as BlittableJsonReaderObject;
                    bInterval.TryGet("Start", out string valueStart);
                    bInterval.TryGet("End", out string valueEnd);
                    Assert.Equal(expectedStart, valueStart);
                    Assert.Equal(expectedEnd, valueEnd);
                }
            }
        }

        [Fact]
        public void Can_Use_NodaTime_DateInterval_In_Dynamic_Index()
        {
            var today = NodaUtil.LocalDate.Today;
            var start = today.PlusDays(-1);
            var end = today.PlusDays(1);
            var dateInterval1 = new DateInterval(start, end);
            var dateInterval2 = new DateInterval(end, end.PlusDays(1));

            using (var documentStore = NewDocumentStore())
            {
                using (var session = documentStore.OpenSession())
                {
                    session.Store(new Foo { Id = "foos/1", DateInterval = dateInterval1 });
                    session.Store(new Foo { Id = "foos/2", DateInterval = dateInterval2 });
                    session.SaveChanges();
                }

                using (var session = documentStore.OpenSession())
                {
                    var q1 = session.Query<Foo>()
                                    .Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.DateInterval.Start == dateInterval1.Start && x.DateInterval.End == dateInterval1.End);
                    var results1 = q1.ToList();
                    Assert.Single(results1);

                    var q2 = session.Query<Foo>()
                                    .Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.DateInterval.Start <= today && x.DateInterval.End > today);
                    var results2 = q2.ToList();
                    Assert.Single(results2);

                    var q3 = session.Query<Foo>()
                                    .Customize(x => x.WaitForNonStaleResults())
                                    .OrderByDescending(x => x.DateInterval.Start);
                    var results3 = q3.ToList();
                    Assert.Equal(2, results3.Count);
                    Assert.True(results3[0].DateInterval.Start > results3[1].DateInterval.Start);
                }
            }
        }

        [Fact]
        public void Can_Use_NodaTime_DateInterval_In_Static_Index()
        {
            var today = NodaUtil.LocalDate.Today;
            var start = today.PlusDays(-1);
            var end = today.PlusDays(1);
            var dateInterval1 = new DateInterval(start, end);
            var dateInterval2 = new DateInterval(end, end.PlusDays(1));

            using (var documentStore = NewDocumentStore())
            {
                documentStore.ExecuteIndex(new TestIndex());

                using (var session = documentStore.OpenSession())
                {
                    session.Store(new Foo { Id = "foos/1", DateInterval = dateInterval1 });
                    session.Store(new Foo { Id = "foos/2", DateInterval = dateInterval2 });
                    session.SaveChanges();
                }

                using (var session = documentStore.OpenSession())
                {
                    var q1 = session.Query<Foo, TestIndex>()
                                    .Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.DateInterval.Start == dateInterval1.Start && x.DateInterval.End == dateInterval1.End);
                    var results1 = q1.ToList();
                    Assert.Single(results1);

                    var q2 = session.Query<Foo, TestIndex>()
                                    .Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.DateInterval.Start <= today && x.DateInterval.End > today);
                    var results2 = q2.ToList();
                    Assert.Single(results2);

                    var q3 = session.Query<Foo, TestIndex>()
                                    .Customize(x => x.WaitForNonStaleResults())
                                    .OrderByDescending(x => x.DateInterval.Start);
                    var results3 = q3.ToList();
                    Assert.Equal(2, results3.Count);
                    Assert.True(results3[0].DateInterval.Start > results3[1].DateInterval.Start);
                }
            }
        }

        public class Foo
        {
            public string Id { get; set; }
            public DateInterval DateInterval { get; set; }
        }

        public class TestIndex : AbstractIndexCreationTask<Foo>
        {
            public TestIndex()
            {
                Map = foos => from foo in foos
                              select new
                              {
                                  DateInterval_Start = foo.DateInterval.Start,
                                  DateInterval_End = foo.DateInterval.End
                              };
            }
        }
    }
}
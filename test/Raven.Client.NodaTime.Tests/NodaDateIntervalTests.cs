using NodaTime;
using NodaTime.Text;
using Raven.Client.Documents.Commands;
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
                    var command = new GetDocumentsCommand("foos/1", null, false);
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

        public class Foo
        {
            public string Id { get; set; }
            public DateInterval DateInterval { get; set; }
        }
    }
}
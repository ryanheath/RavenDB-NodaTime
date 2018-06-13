using System.Linq;
using NodaTime;
using Raven.Client.Documents.Commands;
using Raven.Client.Documents.Indexes;
using Sparrow.Json;
using Xunit;

namespace Raven.Client.NodaTime.Tests
{
    // TODO: Periods are tricky.  We should probably normalize them and allow for equivalency when querying.

    public class NodaPeriodTests : MyRavenTestDriver
    {
        [Fact]
        public void Can_Use_NodaTime_Period_In_Document_Positive()
        {
            Can_Use_NodaTime_Period_In_Document(Period.FromHours(2));
        }

        [Fact]
        public void Can_Use_NodaTime_Period_In_Document_Negative()
        {
            Can_Use_NodaTime_Period_In_Document(Period.FromHours(-5));
        }

        [Fact]
        public void Can_Use_NodaTime_Period_In_Document_Min()
        {
            Can_Use_NodaTime_Period_In_Document(NodaUtil.Period.MinValue);
        }

        [Fact]
        public void Can_Use_NodaTime_Period_In_Document_Max()
        {
            Can_Use_NodaTime_Period_In_Document(NodaUtil.Period.MaxValue);
        }

        private void Can_Use_NodaTime_Period_In_Document(Period period)
        {
            using (var documentStore = NewDocumentStore())
            {
                using (var session = documentStore.OpenSession())
                {
                    session.Store(new Foo { Id = "foos/1", Period = period });
                    session.SaveChanges();
                }

                using (var session = documentStore.OpenSession())
                {
                    var foo = session.Load<Foo>("foos/1");

                    Assert.Equal(period, foo.Period);
                }

                using (var session = documentStore.OpenSession())
                {
                    var command = new GetDocumentsCommand("foos/1", null, false);
                    session.Advanced.RequestExecutor.Execute(command, session.Advanced.Context);
                    var json = (BlittableJsonReaderObject)command.Result.Results[0];
                    System.Diagnostics.Debug.WriteLine(json.ToString());
                    var expected = period.ToString();
                    json.TryGet("Period", out string value);
                    Assert.Equal(expected, value);
                }
            }
        }

        [Fact]
        public void Can_Use_NodaTime_Period_In_Dynamic_Index_Positive()
        {
            Can_Use_NodaTime_Period_In_Dynamic_Index1(Period.FromHours(2));
        }

        [Fact]
        public void Can_Use_NodaTime_Period_In_Dynamic_Index_Negative()
        {
            Can_Use_NodaTime_Period_In_Dynamic_Index2(Period.FromHours(-5));
        }

        [Fact]
        public void Can_Use_NodaTime_Period_In_Dynamic_Index_Min()
        {
            Can_Use_NodaTime_Period_In_Dynamic_Index1(NodaUtil.Period.MinValue);
        }

        [Fact]
        public void Can_Use_NodaTime_Period_In_Dynamic_Index_Max()
        {
            Can_Use_NodaTime_Period_In_Dynamic_Index2(NodaUtil.Period.MaxValue);
        }

        private void Can_Use_NodaTime_Period_In_Dynamic_Index1(Period period)
        {
            using (var documentStore = NewDocumentStore())
            {
                using (var session = documentStore.OpenSession())
                {
                    session.Store(new Foo { Id = "foos/1", Period = period });
                    session.Store(new Foo { Id = "foos/2", Period = period + Period.FromHours(1) });
                    session.Store(new Foo { Id = "foos/3", Period = period + Period.FromHours(2) });
                    session.SaveChanges();
                }

                using (var session = documentStore.OpenSession())
                {
                    var q1 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults()).OrderBy(x => x.Period).Where(x => x.Period == period);
                    var results1 = q1.ToList();
                    Assert.Single(results1);

                    // Period does not implement IComparable, so you can't query with greater then or less than
                }
            }
        }

        private void Can_Use_NodaTime_Period_In_Dynamic_Index2(Period period)
        {
            using (var documentStore = NewDocumentStore())
            {
                using (var session = documentStore.OpenSession())
                {
                    session.Store(new Foo { Id = "foos/1", Period = period });
                    session.Store(new Foo { Id = "foos/2", Period = period - Period.FromHours(1) });
                    session.Store(new Foo { Id = "foos/3", Period = period - Period.FromHours(2) });
                    session.SaveChanges();
                }

                using (var session = documentStore.OpenSession())
                {
                    var q1 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults()).Where(x => x.Period == period);
                    var results1 = q1.ToList();
                    Assert.Single(results1);

                    // Period does not implement IComparable, so you can't query with greater then or less than
                }
            }
        }

        [Fact]
        public void Can_Use_NodaTime_Period_In_Static_Index_Positive()
        {
            Can_Use_NodaTime_Period_In_Static_Index1(Period.FromHours(2));
        }

        [Fact]
        public void Can_Use_NodaTime_Period_In_Static_Index_Negative()
        {
            Can_Use_NodaTime_Period_In_Static_Index2(Period.FromHours(-5));
        }

        [Fact]
        public void Can_Use_NodaTime_Period_In_Static_Index_Min()
        {
            Can_Use_NodaTime_Period_In_Static_Index1(NodaUtil.Period.MinValue);
        }

        [Fact]
        public void Can_Use_NodaTime_Period_In_Static_Index_Max()
        {
            Can_Use_NodaTime_Period_In_Static_Index2(NodaUtil.Period.MaxValue);
        }

        private void Can_Use_NodaTime_Period_In_Static_Index1(Period period)
        {
            using (var documentStore = NewDocumentStore())
            {
                documentStore.ExecuteIndex(new TestIndex());

                using (var session = documentStore.OpenSession())
                {
                    session.Store(new Foo { Id = "foos/1", Period = period });
                    session.Store(new Foo { Id = "foos/2", Period = period + Period.FromHours(1) });
                    session.Store(new Foo { Id = "foos/3", Period = period + Period.FromHours(2) });
                    session.SaveChanges();
                }

                using (var session = documentStore.OpenSession())
                {
                    var q1 = session.Query<Foo, TestIndex>().Customize(x => x.WaitForNonStaleResults()).Where(x => x.Period == period);
                    var results1 = q1.ToList();
                    Assert.Single(results1);

                    // Period does not implement IComparable, so you can't query with greater then or less than
                }
            }
        }

        private void Can_Use_NodaTime_Period_In_Static_Index2(Period period)
        {
            using (var documentStore = NewDocumentStore())
            {
                documentStore.ExecuteIndex(new TestIndex());

                using (var session = documentStore.OpenSession())
                {
                    session.Store(new Foo { Id = "foos/1", Period = period });
                    session.Store(new Foo { Id = "foos/2", Period = period - Period.FromHours(1) });
                    session.Store(new Foo { Id = "foos/3", Period = period - Period.FromHours(2) });
                    session.SaveChanges();
                }

                using (var session = documentStore.OpenSession())
                {
                    var q1 = session.Query<Foo, TestIndex>().Customize(x => x.WaitForNonStaleResults()).Where(x => x.Period == period);
                    var results1 = q1.ToList();
                    Assert.Single(results1);

                    // Period does not implement IComparable, so you can't query with greater then or less than
                }
            }
        }

        public class Foo
        {
            public string Id { get; set; }
            public Period Period { get; set; }
        }

        public class TestIndex : AbstractIndexCreationTask<Foo>
        {
            public TestIndex()
            {
                Map = foos => from foo in foos
                              select new
                                     {
                                         foo.Period
                                     };
            }
        }
    }
}

using System;
using System.Linq;
using NodaTime;
using Raven.Client.Documents.Commands;
using Raven.Client.Documents.Indexes;
using Sparrow.Json;
using Xunit;

namespace Raven.Client.NodaTime.Tests
{
    public static class Ext
    {
        public static LocalDateTime At(this LocalDate localDate, LocalTime localTime)
        {
            return localDate + localTime;
        }
    }

    public class NodaInstantTests : MyRavenTestDriver
    {
        [Fact]
        public void Can_Use_NodaTime_Instant_In_Document_Now()
        {
            Can_Use_NodaTime_Instant_In_Document(SystemClock.Instance.GetCurrentInstant());
        }

        [Fact]
        public void Can_Use_NodaTime_Instant_In_Document_IsoMin()
        {
            Can_Use_NodaTime_Instant_In_Document(NodaUtil.Instant.MinIsoValue);
        }

        [Fact]
        public void Can_Use_NodaTime_Instant_In_Document_IsoMax()
        {
            Can_Use_NodaTime_Instant_In_Document(NodaUtil.Instant.MaxIsoValue);
        }

        [Fact]
        public void Cannot_Use_NodaTime_Instant_In_Document_When_Too_Small()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Can_Use_NodaTime_Instant_In_Document(Instant.MinValue));
        }

        [Fact]
        public void Can_Use_NodaTime_Instant_In_Document_Instant_MaxValue()
        {
            Can_Use_NodaTime_Instant_In_Document(Instant.MaxValue);
        }

        private void Can_Use_NodaTime_Instant_In_Document(Instant instant)
        {
            using (var documentStore = NewDocumentStore())
            {
                using (var session = documentStore.OpenSession())
                {
                    session.Store(new Foo { Id = "foos/1", Instant = instant });
                    session.SaveChanges();
                }

                using (var session = documentStore.OpenSession())
                {
                    var foo = session.Load<Foo>("foos/1");

                    Assert.Equal(instant.ToUnixTimeTicks(), foo.Instant.ToUnixTimeTicks());
                }

                using (var session = documentStore.OpenSession())
                {
                    var command = new GetDocumentsCommand("foos/1", null, false);
                    session.Advanced.RequestExecutor.Execute(command, session.Advanced.Context);
                    var json = (BlittableJsonReaderObject)command.Result.Results[0];
                    System.Diagnostics.Debug.WriteLine(json.ToString());
                    var expected = instant.ToString(NodaUtil.Instant.FullIsoPattern.PatternText, null);
                    json.TryGet("Instant", out string value);
                    Assert.Equal(expected, value);
                }
            }
        }

        [Fact]
        public void Can_Use_NodaTime_Instant_In_Dynamic_Index_Now()
        {
            Can_Use_NodaTime_Instant_In_Dynamic_Index1(SystemClock.Instance.GetCurrentInstant());
        }

        [Fact]
        public void Can_Use_NodaTime_Instant_In_Dynamic_Index_IsoMin()
        {
            Can_Use_NodaTime_Instant_In_Dynamic_Index1(NodaUtil.Instant.MinIsoValue);
        }

        [Fact]
        public void Can_Use_NodaTime_Instant_In_Dynamic_Index_IsoMax()
        {
            Can_Use_NodaTime_Instant_In_Dynamic_Index2(NodaUtil.Instant.MaxIsoValue);
        }

        [Fact]
        public void Cannot_Use_NodaTime_Instant_In_Dynamic_Index_When_Too_Small()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Can_Use_NodaTime_Instant_In_Dynamic_Index1(Instant.MinValue));
        }

        [Fact]
        public void Can_Use_NodaTime_Instant_In_Dynamic_Index_MaxValue()
        {
            Can_Use_NodaTime_Instant_In_Dynamic_Index2(Instant.MaxValue);
        }

        private void Can_Use_NodaTime_Instant_In_Dynamic_Index1(Instant instant)
        {
            using (var documentStore = NewDocumentStore())
            {
                using (var session = documentStore.OpenSession())
                {
                    session.Store(new Foo { Id = "foos/1", Instant = instant });
                    session.Store(new Foo { Id = "foos/2", Instant = instant + Duration.FromMinutes(1) });
                    session.Store(new Foo { Id = "foos/3", Instant = instant + Duration.FromMinutes(2) });
                    session.SaveChanges();
                }

                using (var session = documentStore.OpenSession())
                {
                    var q1 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.Instant == instant);
                    var results1 = q1.ToList();
                    Assert.Single(results1);

                    var q2 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.Instant > instant)
                                    .OrderByDescending(x => x.Instant);
                    var results2 = q2.ToList();
                    Assert.Equal(2, results2.Count);
                    Assert.True(results2[0].Instant > results2[1].Instant);

                    var q3 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.Instant >= instant)
                                    .OrderByDescending(x => x.Instant);
                    var results3 = q3.ToList();
                    Assert.Equal(3, results3.Count);
                    Assert.True(results3[0].Instant > results3[1].Instant);
                    Assert.True(results3[1].Instant > results3[2].Instant);
                }
            }
        }

        private void Can_Use_NodaTime_Instant_In_Dynamic_Index2(Instant instant)
        {
            using (var documentStore = NewDocumentStore())
            {
                using (var session = documentStore.OpenSession())
                {
                    session.Store(new Foo { Id = "foos/1", Instant = instant });
                    session.Store(new Foo { Id = "foos/2", Instant = instant + Duration.FromMinutes(-1) });
                    session.Store(new Foo { Id = "foos/3", Instant = instant + Duration.FromMinutes(-2) });
                    session.SaveChanges();
                }

                using (var session = documentStore.OpenSession())
                {
                    var q1 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.Instant == instant);
                    var results1 = q1.ToList();
                    Assert.Single(results1);

                    var q2 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.Instant < instant)
                                    .OrderBy(x => x.Instant);
                    var results2 = q2.ToList();
                    Assert.Equal(2, results2.Count);
                    Assert.True(results2[0].Instant < results2[1].Instant);

                    var q3 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.Instant <= instant)
                                    .OrderBy(x => x.Instant);
                    var results3 = q3.ToList();
                    Assert.Equal(3, results3.Count);
                    Assert.True(results3[0].Instant < results3[1].Instant);
                    Assert.True(results3[1].Instant < results3[2].Instant);
                }
            }
        }

        [Fact]
        public void Can_Use_NodaTime_Instant_In_Static_Index_Now()
        {
            Can_Use_NodaTime_Instant_In_Static_Index1(SystemClock.Instance.GetCurrentInstant());
        }

        [Fact]
        public void Can_Use_NodaTime_Instant_In_Static_Index_IsoMin()
        {
            Can_Use_NodaTime_Instant_In_Static_Index1(NodaUtil.Instant.MinIsoValue);
        }

        [Fact]
        public void Can_Use_NodaTime_Instant_In_Static_Index_IsoMax()
        {
            Can_Use_NodaTime_Instant_In_Static_Index2(NodaUtil.Instant.MaxIsoValue);
        }

        [Fact]
        public void Cannot_Use_NodaTime_Instant_In_Static_Index_When_Too_Small()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Can_Use_NodaTime_Instant_In_Static_Index1(Instant.MinValue));
        }

        [Fact]
        public void Can_Use_NodaTime_Instant_In_Static_Index_MaxValue()
        {
            Can_Use_NodaTime_Instant_In_Static_Index2(Instant.MaxValue);
        }

        private void Can_Use_NodaTime_Instant_In_Static_Index1(Instant instant)
        {
            using (var documentStore = NewDocumentStore())
            {
                documentStore.ExecuteIndex(new TestIndex());

                using (var session = documentStore.OpenSession())
                {
                    session.Store(new Foo { Id = "foos/1", Instant = instant });
                    session.Store(new Foo { Id = "foos/2", Instant = instant + Duration.FromMinutes(1) });
                    session.Store(new Foo { Id = "foos/3", Instant = instant + Duration.FromMinutes(2) });
                    session.SaveChanges();
                }

                using (var session = documentStore.OpenSession())
                {
                    var q1 = session.Query<Foo, TestIndex>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.Instant == instant);
                    var results1 = q1.ToList();
                    Assert.Single(results1);

                    var q2 = session.Query<Foo, TestIndex>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.Instant > instant)
                                    .OrderByDescending(x => x.Instant);
                    var results2 = q2.ToList();
                    Assert.Equal(2, results2.Count);
                    Assert.True(results2[0].Instant > results2[1].Instant);
                    
                    var q3 = session.Query<Foo, TestIndex>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.Instant >= instant)
                                    .OrderByDescending(x => x.Instant);
                    var results3 = q3.ToList();
                    Assert.Equal(3, results3.Count);
                    Assert.True(results3[0].Instant > results3[1].Instant);
                    Assert.True(results3[1].Instant > results3[2].Instant);
                }
            }
        }

        private void Can_Use_NodaTime_Instant_In_Static_Index2(Instant instant)
        {
            using (var documentStore = NewDocumentStore())
            {
                documentStore.ExecuteIndex(new TestIndex());

                using (var session = documentStore.OpenSession())
                {
                    session.Store(new Foo { Id = "foos/1", Instant = instant });
                    session.Store(new Foo { Id = "foos/2", Instant = instant + Duration.FromMinutes(-1) });
                    session.Store(new Foo { Id = "foos/3", Instant = instant + Duration.FromMinutes(-2) });
                    session.SaveChanges();
                }

                using (var session = documentStore.OpenSession())
                {
                    var q1 = session.Query<Foo, TestIndex>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.Instant == instant);
                    var results1 = q1.ToList();
                    Assert.Single(results1);

                    var q2 = session.Query<Foo, TestIndex>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.Instant < instant)
                                    .OrderBy(x => x.Instant);
                    var results2 = q2.ToList();
                    Assert.Equal(2, results2.Count);
                    Assert.True(results2[0].Instant < results2[1].Instant);
                    
                    var q3 = session.Query<Foo, TestIndex>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.Instant <= instant)
                                    .OrderBy(x => x.Instant);
                    var results3 = q3.ToList();
                    Assert.Equal(3, results3.Count);
                    Assert.True(results3[0].Instant < results3[1].Instant);
                    Assert.True(results3[1].Instant < results3[2].Instant);
                }
            }
        }

        public class Foo
        {
            public string Id { get; set; }
            public Instant Instant { get; set; }
        }

        public class TestIndex : AbstractIndexCreationTask<Foo>
        {
            public TestIndex()
            {
                Map = foos => from foo in foos
                              select new
                              {
                                  foo.Instant
                              };
            }
        }
    }
}

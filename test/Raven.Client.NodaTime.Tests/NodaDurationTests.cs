using System;
using System.Linq;
using NodaTime;
using Raven.Client.Documents.Commands;
using Raven.Client.Documents.Indexes;
using Sparrow.Json;
using Xunit;

namespace Raven.Client.NodaTime.Tests
{
    public class NodaDurationTests : MyRavenTestDriver
    {
        [Fact]
        public void Can_Use_NodaTime_Duration_In_Document_Positive()
        {
            Can_Use_NodaTime_Duration_In_Document(Duration.FromHours(2));
        }

        [Fact]
        public void Can_Use_NodaTime_Duration_In_Document_Negative()
        {
            Can_Use_NodaTime_Duration_In_Document(Duration.FromHours(-5));
        }

        [Fact]
        public void Can_Use_NodaTime_Duration_In_Document_Min()
        {
            Can_Use_NodaTime_Duration_In_Document(NodaUtil.Duration.MinValue);
        }

        [Fact]
        public void Can_Use_NodaTime_Duration_In_Document_Max()
        {
            Can_Use_NodaTime_Duration_In_Document(NodaUtil.Duration.MaxValue);
        }

        private void Can_Use_NodaTime_Duration_In_Document(Duration duration)
        {
            using (var documentStore = NewDocumentStore())
            {
                using (var session = documentStore.OpenSession())
                {
                    session.Store(new Foo { Id = "foos/1", Duration = duration });
                    session.SaveChanges();
                }

                using (var session = documentStore.OpenSession())
                {
                    var foo = session.Load<Foo>("foos/1");

                    Assert.Equal(duration, foo.Duration);
                }

                var json = documentStore.DatabaseCommands.Get("foos/1").DataAsJson;
                Debug.WriteLine(json.ToString(Formatting.Indented));
                var expected = duration.ToTimeSpan().ToString();
                Assert.Equal(expected, json.Value<string>("Duration"));
            }
        }

        [Fact]
        public void Can_Use_NodaTime_Duration_In_Dynamic_Index_Positive()
        {
            Can_Use_NodaTime_Duration_In_Dynamic_Index1(Duration.FromHours(2));
        }

        [Fact]
        public void Can_Use_NodaTime_Duration_In_Dynamic_Index_Negative()
        {
            Can_Use_NodaTime_Duration_In_Dynamic_Index2(Duration.FromHours(-5));
        }

        [Fact]
        public void Can_Use_NodaTime_Duration_In_Dynamic_Index_Min()
        {
            Can_Use_NodaTime_Duration_In_Dynamic_Index1(NodaUtil.Duration.MinValue);
        }

        [Fact]
        public void Can_Use_NodaTime_Duration_In_Dynamic_Index_Max()
        {
            Can_Use_NodaTime_Duration_In_Dynamic_Index2(NodaUtil.Duration.MaxValue);
        }

        private void Can_Use_NodaTime_Duration_In_Dynamic_Index1(Duration duration)
        {
            using (var documentStore = NewDocumentStore())
            {
                using (var session = documentStore.OpenSession())
                {
                    session.Store(new Foo { Id = "foos/1", Duration = duration });
                    session.Store(new Foo { Id = "foos/2", Duration = duration + Duration.FromHours(1) });
                    session.Store(new Foo { Id = "foos/3", Duration = duration + Duration.FromHours(2) });
                    session.SaveChanges();
                }

                using (var session = documentStore.OpenSession())
                {
                    var q1 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.Duration == duration);
                    var results1 = q1.ToList();
                    Assert.Equal(1, results1.Count);

                    var q2 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.Duration > duration)
                                    .OrderByDescending(x => x.Duration);
                    var results2 = q2.ToList();
                    Assert.Equal(2, results2.Count);
                    Assert.True(results2[0].Duration > results2[1].Duration);
                    
                    var q3 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.Duration >= duration)
                                    .OrderByDescending(x => x.Duration);
                    var results3 = q3.ToList();
                    Assert.Equal(3, results3.Count);
                    Assert.True(results3[0].Duration > results3[1].Duration);
                    Assert.True(results3[1].Duration > results3[2].Duration);
                }
            }
        }

        private void Can_Use_NodaTime_Duration_In_Dynamic_Index2(Duration duration)
        {
            using (var documentStore = NewDocumentStore())
            {
                using (var session = documentStore.OpenSession())
                {
                    session.Store(new Foo { Id = "foos/1", Duration = duration });
                    session.Store(new Foo { Id = "foos/2", Duration = duration - Duration.FromHours(1) });
                    session.Store(new Foo { Id = "foos/3", Duration = duration - Duration.FromHours(2) });
                    session.SaveChanges();
                }

                using (var session = documentStore.OpenSession())
                {
                    var q1 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.Duration == duration);
                    var results1 = q1.ToList();
                    Assert.Equal(1, results1.Count);

                    var q2 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.Duration < duration)
                                    .OrderBy(x => x.Duration);
                    var results2 = q2.ToList();
                    Assert.Equal(2, results2.Count);
                    Assert.True(results2[0].Duration < results2[1].Duration);
                    
                    var q3 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.Duration <= duration)
                                    .OrderBy(x => x.Duration);
                    var results3 = q3.ToList();
                    Assert.Equal(3, results3.Count);
                    Assert.True(results3[0].Duration < results3[1].Duration);
                    Assert.True(results3[1].Duration < results3[2].Duration);
                }
            }
        }

        [Fact]
        public void Can_Use_NodaTime_Duration_In_Static_Index_Positive()
        {
            Can_Use_NodaTime_Duration_In_Static_Index1(Duration.FromHours(2));
        }

        [Fact]
        public void Can_Use_NodaTime_Duration_In_Static_Index_Negative()
        {
            Can_Use_NodaTime_Duration_In_Static_Index2(Duration.FromHours(-5));
        }

        [Fact]
        public void Can_Use_NodaTime_Duration_In_Static_Index_Min()
        {
            Can_Use_NodaTime_Duration_In_Static_Index1(NodaUtil.Duration.MinValue);
        }

        [Fact]
        public void Can_Use_NodaTime_Duration_In_Static_Index_Max()
        {
            Can_Use_NodaTime_Duration_In_Static_Index2(NodaUtil.Duration.MaxValue);
        }

        private void Can_Use_NodaTime_Duration_In_Static_Index1(Duration duration)
        {
            using (var documentStore = NewDocumentStore())
            {
                documentStore.ExecuteIndex(new TestIndex());

                using (var session = documentStore.OpenSession())
                {
                    session.Store(new Foo { Id = "foos/1", Duration = duration });
                    session.Store(new Foo { Id = "foos/2", Duration = duration + Duration.FromHours(1) });
                    session.Store(new Foo { Id = "foos/3", Duration = duration + Duration.FromHours(2) });
                    session.SaveChanges();
                }

                using (var session = documentStore.OpenSession())
                {
                    var q1 = session.Query<Foo, TestIndex>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.Duration == duration);
                    var results1 = q1.ToList();
                    Assert.Equal(1, results1.Count);

                    var q2 = session.Query<Foo, TestIndex>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.Duration > duration)
                                    .OrderByDescending(x => x.Duration);
                    var results2 = q2.ToList();
                    Assert.Equal(2, results2.Count);
                    Assert.True(results2[0].Duration > results2[1].Duration);
                    
                    var q3 = session.Query<Foo, TestIndex>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.Duration >= duration)
                                    .OrderByDescending(x => x.Duration);
                    var results3 = q3.ToList();
                    Assert.Equal(3, results3.Count);
                    Assert.True(results3[0].Duration > results3[1].Duration);
                    Assert.True(results3[1].Duration > results3[2].Duration);
                }
            }
        }

        private void Can_Use_NodaTime_Duration_In_Static_Index2(Duration duration)
        {
            using (var documentStore = NewDocumentStore())
            {
                documentStore.ExecuteIndex(new TestIndex());

                using (var session = documentStore.OpenSession())
                {
                    session.Store(new Foo { Id = "foos/1", Duration = duration });
                    session.Store(new Foo { Id = "foos/2", Duration = duration - Duration.FromHours(1) });
                    session.Store(new Foo { Id = "foos/3", Duration = duration - Duration.FromHours(2) });
                    session.SaveChanges();
                }

                using (var session = documentStore.OpenSession())
                {
                    var q1 = session.Query<Foo, TestIndex>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.Duration == duration);
                    var results1 = q1.ToList();
                    Assert.Equal(1, results1.Count);

                    var q2 = session.Query<Foo, TestIndex>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.Duration < duration)
                                    .OrderBy(x => x.Duration);
                    var results2 = q2.ToList();
                    Assert.Equal(2, results2.Count);
                    Assert.True(results2[0].Duration < results2[1].Duration);

                    var q3 = session.Query<Foo, TestIndex>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.Duration <= duration)
                                    .OrderBy(x => x.Duration);
                    var results3 = q3.ToList();
                    Assert.Equal(3, results3.Count);
                    Assert.True(results3[0].Duration < results3[1].Duration);
                    Assert.True(results3[1].Duration < results3[2].Duration);
                }
            }
        }

        public class Foo
        {
            public string Id { get; set; }
            public Duration Duration { get; set; }
        }

        public class TestIndex : AbstractIndexCreationTask<Foo>
        {
            public TestIndex()
            {
                Map = foos => from foo in foos
                              select new
                              {
                                  foo.Duration
                              };

            }
        }
    }
}

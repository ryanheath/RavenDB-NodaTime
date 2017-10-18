using System.Diagnostics;
using System.Linq;
using NodaTime;
using Raven.Client.Indexes;
using Raven.Imports.Newtonsoft.Json;
using Raven.Tests.Helpers;
using Xunit;

namespace Raven.Client.NodaTime.Tests
{
    public class NodaLocalTimeTests : RavenTestBase
    {
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Can_Use_NodaTime_LocalTime_In_Document_Now(bool useRelaxedConverters)
        {
            Can_Use_NodaTime_LocalTime_In_Document(NodaUtil.LocalTime.Now, useRelaxedConverters);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Can_Use_NodaTime_LocalTime_In_Document_IsoMin(bool useRelaxedConverters)
        {
            Can_Use_NodaTime_LocalTime_In_Document(NodaUtil.LocalTime.MinIsoValue, useRelaxedConverters);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Can_Use_NodaTime_LocalTime_In_Document_IsoMax(bool useRelaxedConverters)
        {
            Can_Use_NodaTime_LocalTime_In_Document(NodaUtil.LocalTime.MaxIsoValue, useRelaxedConverters);
        }

        private void Can_Use_NodaTime_LocalTime_In_Document(LocalTime lt, bool useRelaxedConverters)
        {
            using (var documentStore = NewDocumentStore())
            {
                if (useRelaxedConverters)
                {
                    using (var session = documentStore.OpenSession())
                    {
                        session.Store(new Foo {Id = "foos/1", LocalTime = lt});

                        // save localtime as nodatime localdate
                        session.SaveChanges();
                    }
                }

                documentStore.ConfigureForNodaTime(useRelaxedConverters);

                using (var session = documentStore.OpenSession())
                {
                    if (useRelaxedConverters)
                    {
                        var foo = session.Load<Foo>("foos/1");

                        // we can read localtime saved as nodatime localtime
                        Assert.Equal(lt, foo.LocalTime);

                        session.Store(foo);
                    }
                    else
                    {
                        session.Store(new Foo { Id = "foos/1", LocalTime = lt });
                    }

                    // save duration as timespan
                    session.SaveChanges();
                }

                using (var session = documentStore.OpenSession())
                {
                    var foo = session.Load<Foo>("foos/1");

                    // we can read localtime saved as timespan
                    Assert.Equal(lt, foo.LocalTime);
                }

                var json = documentStore.DatabaseCommands.Get("foos/1").DataAsJson;
                Debug.WriteLine(json.ToString(Formatting.Indented));
                var expected = lt.ToTimeSpan().ToString("c");
                Assert.Equal(expected, json.Value<string>("LocalTime"));
            }
        }

        [Fact]
        public void Can_Use_NodaTime_LocalTime_In_Dynamic_Index_Now()
        {
            Can_Use_NodaTime_LocalTime_In_Dynamic_Index1(NodaUtil.LocalTime.Now);
        }

        [Fact]
        public void Can_Use_NodaTime_LocalTime_In_Dynamic_Index_IsoMin()
        {
            Can_Use_NodaTime_LocalTime_In_Dynamic_Index1(NodaUtil.LocalTime.MinIsoValue);
        }

        [Fact]
        public void Can_Use_NodaTime_LocalTime_In_Dynamic_Index_IsoMax()
        {
            Can_Use_NodaTime_LocalTime_In_Dynamic_Index2(NodaUtil.LocalTime.MaxIsoValue);
        }

        private void Can_Use_NodaTime_LocalTime_In_Dynamic_Index1(LocalTime lt)
        {
            using (var documentStore = NewDocumentStore())
            {
                documentStore.ConfigureForNodaTime();

                using (var session = documentStore.OpenSession())
                {
                    session.Store(new Foo { Id = "foos/1", LocalTime = lt });
                    session.Store(new Foo { Id = "foos/2", LocalTime = lt + Period.FromMinutes(1) });
                    session.Store(new Foo { Id = "foos/3", LocalTime = lt + Period.FromMinutes(2) });
                    session.SaveChanges();
                }

                using (var session = documentStore.OpenSession())
                {
                    var q1 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.LocalTime == lt);
                    var results1 = q1.ToList();
                    Assert.Equal(1, results1.Count);

                    var q2 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.LocalTime > lt)
                                    .OrderByDescending(x => x.LocalTime);
                    var results2 = q2.ToList();
                    Assert.Equal(2, results2.Count);
                    Assert.True(results2[0].LocalTime > results2[1].LocalTime);
                    Assert.True(results2[0].LocalTime > results2[1].LocalTime);

                    var q3 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.LocalTime >= lt)
                                    .OrderByDescending(x => x.LocalTime);
                    var results3 = q3.ToList();
                    Assert.Equal(3, results3.Count);
                    Assert.True(results3[0].LocalTime > results3[1].LocalTime);
                    Assert.True(results3[1].LocalTime > results3[2].LocalTime);
                }
            }
        }

        private void Can_Use_NodaTime_LocalTime_In_Dynamic_Index2(LocalTime lt)
        {
            using (var documentStore = NewDocumentStore())
            {
                documentStore.ConfigureForNodaTime();

                using (var session = documentStore.OpenSession())
                {
                    session.Store(new Foo { Id = "foos/1", LocalTime = lt });
                    session.Store(new Foo { Id = "foos/2", LocalTime = lt - Period.FromMinutes(1) });
                    session.Store(new Foo { Id = "foos/3", LocalTime = lt - Period.FromMinutes(2) });
                    session.SaveChanges();
                }

                using (var session = documentStore.OpenSession())
                {
                    var q1 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.LocalTime == lt);
                    var results1 = q1.ToList();
                    Assert.Equal(1, results1.Count);

                    var q2 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.LocalTime < lt)
                                    .OrderBy(x => x.LocalTime);
                    var results2 = q2.ToList();
                    Assert.Equal(2, results2.Count);
                    Assert.True(results2[0].LocalTime < results2[1].LocalTime);

                    var q3 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.LocalTime <= lt)
                                    .OrderBy(x => x.LocalTime);
                    var results3 = q3.ToList();
                    Assert.Equal(3, results3.Count);
                    Assert.True(results3[0].LocalTime < results3[1].LocalTime);
                    Assert.True(results3[1].LocalTime < results3[2].LocalTime);
                }
            }
        }

        [Fact]
        public void Can_Use_NodaTime_LocalTime_In_Static_Index_Now()
        {
            Can_Use_NodaTime_LocalTime_In_Static_Index1(NodaUtil.LocalTime.Now);
        }

        [Fact]
        public void Can_Use_NodaTime_LocalTime_In_Static_Index_IsoMin()
        {
            Can_Use_NodaTime_LocalTime_In_Static_Index1(NodaUtil.LocalTime.MinIsoValue);
        }

        [Fact]
        public void Can_Use_NodaTime_LocalTime_In_Static_Index_IsoMax()
        {
            Can_Use_NodaTime_LocalTime_In_Static_Index2(NodaUtil.LocalTime.MaxIsoValue);
        }

        private void Can_Use_NodaTime_LocalTime_In_Static_Index1(LocalTime lt)
        {
            using (var documentStore = NewDocumentStore())
            {
                documentStore.ConfigureForNodaTime();
                documentStore.ExecuteIndex(new TestIndex());

                using (var session = documentStore.OpenSession())
                {
                    session.Store(new Foo { Id = "foos/1", LocalTime = lt });
                    session.Store(new Foo { Id = "foos/2", LocalTime = lt + Period.FromMinutes(1) });
                    session.Store(new Foo { Id = "foos/3", LocalTime = lt + Period.FromMinutes(2) });
                    session.SaveChanges();
                }

                using (var session = documentStore.OpenSession())
                {
                    var q1 = session.Query<Foo, TestIndex>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.LocalTime == lt);
                    var results1 = q1.ToList();
                    Assert.Equal(1, results1.Count);

                    var q2 = session.Query<Foo, TestIndex>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.LocalTime > lt)
                                    .OrderByDescending(x => x.LocalTime);
                    var results2 = q2.ToList();
                    Assert.Equal(2, results2.Count);
                    Assert.True(results2[0].LocalTime > results2[1].LocalTime);

                    var q3 = session.Query<Foo, TestIndex>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.LocalTime >= lt)
                                    .OrderByDescending(x => x.LocalTime);
                    var results3 = q3.ToList();
                    Assert.Equal(3, results3.Count);
                    Assert.True(results3[0].LocalTime > results3[1].LocalTime);
                    Assert.True(results3[1].LocalTime > results3[2].LocalTime);
                }
            }
        }

        private void Can_Use_NodaTime_LocalTime_In_Static_Index2(LocalTime lt)
        {
            using (var documentStore = NewDocumentStore())
            {
                documentStore.ConfigureForNodaTime();
                documentStore.ExecuteIndex(new TestIndex());

                using (var session = documentStore.OpenSession())
                {
                    session.Store(new Foo { Id = "foos/1", LocalTime = lt });
                    session.Store(new Foo { Id = "foos/2", LocalTime = lt - Period.FromMinutes(1) });
                    session.Store(new Foo { Id = "foos/3", LocalTime = lt - Period.FromMinutes(2) });
                    session.SaveChanges();
                }

                using (var session = documentStore.OpenSession())
                {
                    var q1 = session.Query<Foo, TestIndex>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.LocalTime == lt);
                    var results1 = q1.ToList();
                    Assert.Equal(1, results1.Count);

                    var q2 = session.Query<Foo, TestIndex>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.LocalTime < lt)
                                    .OrderBy(x => x.LocalTime);
                    var results2 = q2.ToList();
                    Assert.Equal(2, results2.Count);
                    Assert.True(results2[0].LocalTime < results2[1].LocalTime);

                    var q3 = session.Query<Foo, TestIndex>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.LocalTime <= lt)
                                    .OrderBy(x => x.LocalTime);
                    var results3 = q3.ToList();
                    Assert.Equal(3, results3.Count);
                    Assert.True(results3[0].LocalTime < results3[1].LocalTime);
                    Assert.True(results3[1].LocalTime < results3[2].LocalTime);
                }
            }
        }

        public class Foo
        {
            public string Id { get; set; }
            public LocalTime LocalTime { get; set; }
        }

        public class TestIndex : AbstractIndexCreationTask<Foo>
        {
            public TestIndex()
            {
                Map = foos => from foo in foos
                              select new
                              {
                                  foo.LocalTime
                              };
            }
        }
    }
}

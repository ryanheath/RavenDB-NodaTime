using System.Diagnostics;
using System.Linq;
using NodaTime;
using Raven.Client.Indexes;
using Raven.Imports.Newtonsoft.Json;
using Raven.Tests.Helpers;
using Xunit;

namespace Raven.Client.NodaTime.Tests
{
    public class NodaLocalDateTimeTests : RavenTestBase
    {
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Can_Use_NodaTime_LocalDateTime_In_Document_Now(bool useRelaxedConverters)
        {
            Can_Use_NodaTime_LocalDateTime_In_Document(NodaUtil.LocalDateTime.Now, useRelaxedConverters);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Can_Use_NodaTime_LocalDateTime_In_Document_IsoMin(bool useRelaxedConverters)
        {
            Can_Use_NodaTime_LocalDateTime_In_Document(NodaUtil.LocalDateTime.MinIsoValue, useRelaxedConverters);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Can_Use_NodaTime_LocalDateTime_In_Document_IsoMax(bool useRelaxedConverters)
        {
            Can_Use_NodaTime_LocalDateTime_In_Document(NodaUtil.LocalDateTime.MaxIsoValue, useRelaxedConverters);
        }

        private void Can_Use_NodaTime_LocalDateTime_In_Document(LocalDateTime ldt, bool useRelaxedConverters)
        {
            using (var documentStore = NewDocumentStore())
            {
                if (useRelaxedConverters)
                {
                    using (var session = documentStore.OpenSession())
                    {
                        session.Store(new Foo {Id = "foos/1", LocalDateTime = ldt});

                        // save localdatetime as nodatime localdatetime
                        session.SaveChanges();
                    }
                }

                documentStore.ConfigureForNodaTime(useRelaxedConverters);

                using (var session = documentStore.OpenSession())
                {
                    if (useRelaxedConverters)
                    {
                        var foo = session.Load<Foo>("foos/1");

                        // we can read localdatetime saved as nodatime localdatetime
                        Assert.Equal(ldt, foo.LocalDateTime);

                        session.Store(foo);
                    }
                    else
                    {
                        session.Store(new Foo { Id = "foos/1", LocalDateTime = ldt });
                    }

                    // save duration as full iso pattern
                    session.SaveChanges();
                }

                using (var session = documentStore.OpenSession())
                {
                    var foo = session.Load<Foo>("foos/1");

                    Assert.Equal(ldt, foo.LocalDateTime);
                }

                var json = documentStore.DatabaseCommands.Get("foos/1").DataAsJson;
                Debug.WriteLine(json.ToString(Formatting.Indented));
                var expected = ldt.ToString(NodaUtil.LocalDateTime.FullIsoPattern.PatternText, null);
                Assert.Equal(expected, json.Value<string>("LocalDateTime"));
            }
        }

        [Fact]
        public void Can_Use_NodaTime_LocalDateTime_In_Dynamic_Index_Now()
        {
            Can_Use_NodaTime_LocalDateTime_In_Dynamic_Index1(NodaUtil.LocalDateTime.Now);
        }

        [Fact]
        public void Can_Use_NodaTime_LocalDateTime_In_Dynamic_Index_IsoMin()
        {
            Can_Use_NodaTime_LocalDateTime_In_Dynamic_Index1(NodaUtil.LocalDateTime.MinIsoValue);
        }

        [Fact]
        public void Can_Use_NodaTime_LocalDateTime_In_Dynamic_Index_IsoMax()
        {
            Can_Use_NodaTime_LocalDateTime_In_Dynamic_Index2(NodaUtil.LocalDateTime.MaxIsoValue);
        }

        private void Can_Use_NodaTime_LocalDateTime_In_Dynamic_Index1(LocalDateTime ldt)
        {
            using (var documentStore = NewDocumentStore())
            {
                documentStore.ConfigureForNodaTime();

                using (var session = documentStore.OpenSession())
                {
                    session.Store(new Foo { Id = "foos/1", LocalDateTime = ldt });
                    session.Store(new Foo { Id = "foos/2", LocalDateTime = ldt + Period.FromMinutes(1) });
                    session.Store(new Foo { Id = "foos/3", LocalDateTime = ldt + Period.FromMinutes(2) });
                    session.SaveChanges();
                }

                using (var session = documentStore.OpenSession())
                {
                    var q1 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.LocalDateTime == ldt);
                    var results1 = q1.ToList();
                    Assert.Equal(1, results1.Count);

                    var q2 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.LocalDateTime > ldt)
                                    .OrderByDescending(x => x.LocalDateTime);
                    var results2 = q2.ToList();
                    Assert.Equal(2, results2.Count);
                    Assert.True(results2[0].LocalDateTime > results2[1].LocalDateTime);
                    
                    var q3 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.LocalDateTime >= ldt)
                                    .OrderByDescending(x => x.LocalDateTime);
                    var results3 = q3.ToList();
                    Assert.Equal(3, results3.Count);
                    Assert.True(results3[0].LocalDateTime > results3[1].LocalDateTime);
                    Assert.True(results3[1].LocalDateTime > results3[2].LocalDateTime);
                }
            }
        }

        private void Can_Use_NodaTime_LocalDateTime_In_Dynamic_Index2(LocalDateTime ldt)
        {
            using (var documentStore = NewDocumentStore())
            {
                documentStore.ConfigureForNodaTime();

                using (var session = documentStore.OpenSession())
                {
                    session.Store(new Foo { Id = "foos/1", LocalDateTime = ldt });
                    session.Store(new Foo { Id = "foos/2", LocalDateTime = ldt - Period.FromMinutes(1) });
                    session.Store(new Foo { Id = "foos/3", LocalDateTime = ldt - Period.FromMinutes(2) });
                    session.SaveChanges();
                }

                using (var session = documentStore.OpenSession())
                {
                    var q1 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.LocalDateTime == ldt);
                    var results1 = q1.ToList();
                    Assert.Equal(1, results1.Count);

                    var q2 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.LocalDateTime < ldt)
                                    .OrderBy(x => x.LocalDateTime);
                    var results2 = q2.ToList();
                    Assert.Equal(2, results2.Count);
                    Assert.True(results2[0].LocalDateTime < results2[1].LocalDateTime);

                    var q3 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.LocalDateTime <= ldt)
                                    .OrderBy(x => x.LocalDateTime);
                    var results3 = q3.ToList();
                    Assert.Equal(3, results3.Count);
                    Assert.True(results3[0].LocalDateTime < results3[1].LocalDateTime);
                    Assert.True(results3[1].LocalDateTime < results3[2].LocalDateTime);
                }
            }
        }

        [Fact]
        public void Can_Use_NodaTime_LocalDateTime_In_Static_Index_Now()
        {
            Can_Use_NodaTime_LocalDateTime_In_Static_Index1(NodaUtil.LocalDateTime.Now);
        }

        [Fact]
        public void Can_Use_NodaTime_LocalDateTime_In_Static_Index_IsoMin()
        {
            Can_Use_NodaTime_LocalDateTime_In_Static_Index1(NodaUtil.LocalDateTime.MinIsoValue);
        }

        [Fact]
        public void Can_Use_NodaTime_LocalDateTime_In_Static_Index_IsoMax()
        {
            Can_Use_NodaTime_LocalDateTime_In_Static_Index2(NodaUtil.LocalDateTime.MaxIsoValue);
        }

        private void Can_Use_NodaTime_LocalDateTime_In_Static_Index1(LocalDateTime ldt)
        {
            using (var documentStore = NewDocumentStore())
            {
                documentStore.ConfigureForNodaTime();
                documentStore.ExecuteIndex(new TestIndex());

                using (var session = documentStore.OpenSession())
                {
                    session.Store(new Foo { Id = "foos/1", LocalDateTime = ldt });
                    session.Store(new Foo { Id = "foos/2", LocalDateTime = ldt + Period.FromMinutes(1) });
                    session.Store(new Foo { Id = "foos/3", LocalDateTime = ldt + Period.FromMinutes(2) });
                    session.SaveChanges();
                }

                using (var session = documentStore.OpenSession())
                {
                    var q1 = session.Query<Foo, TestIndex>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.LocalDateTime == ldt);
                    var results1 = q1.ToList();
                    Assert.Equal(1, results1.Count);

                    var q2 = session.Query<Foo, TestIndex>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.LocalDateTime > ldt)
                                    .OrderByDescending(x => x.LocalDateTime);
                    var results2 = q2.ToList();
                    Assert.Equal(2, results2.Count);
                    Assert.True(results2[0].LocalDateTime > results2[1].LocalDateTime);
                    
                    var q3 = session.Query<Foo, TestIndex>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.LocalDateTime >= ldt)
                                    .OrderByDescending(x => x.LocalDateTime);
                    var results3 = q3.ToList();
                    Assert.Equal(3, results3.Count);
                    Assert.True(results3[0].LocalDateTime > results3[1].LocalDateTime);
                    Assert.True(results3[1].LocalDateTime > results3[2].LocalDateTime);
                }
            }
        }

        private void Can_Use_NodaTime_LocalDateTime_In_Static_Index2(LocalDateTime ldt)
        {
            using (var documentStore = NewDocumentStore())
            {
                documentStore.ConfigureForNodaTime();
                documentStore.ExecuteIndex(new TestIndex());

                using (var session = documentStore.OpenSession())
                {
                    session.Store(new Foo { Id = "foos/1", LocalDateTime = ldt });
                    session.Store(new Foo { Id = "foos/2", LocalDateTime = ldt - Period.FromMinutes(1) });
                    session.Store(new Foo { Id = "foos/3", LocalDateTime = ldt - Period.FromMinutes(2) });
                    session.SaveChanges();
                }

                using (var session = documentStore.OpenSession())
                {
                    var q1 = session.Query<Foo, TestIndex>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.LocalDateTime == ldt);
                    var results1 = q1.ToList();
                    Assert.Equal(1, results1.Count);

                    var q2 = session.Query<Foo, TestIndex>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.LocalDateTime < ldt)
                                    .OrderBy(x => x.LocalDateTime);
                    var results2 = q2.ToList();
                    Assert.Equal(2, results2.Count);
                    Assert.True(results2[0].LocalDateTime < results2[1].LocalDateTime);
                    
                    var q3 = session.Query<Foo, TestIndex>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.LocalDateTime <= ldt)
                                    .OrderBy(x => x.LocalDateTime);
                    var results3 = q3.ToList();
                    Assert.Equal(3, results3.Count);
                    Assert.True(results3[0].LocalDateTime < results3[1].LocalDateTime);
                    Assert.True(results3[1].LocalDateTime < results3[2].LocalDateTime);
                }
            }
        }

        public class Foo
        {
            public string Id { get; set; }
            public LocalDateTime LocalDateTime { get; set; }
        }

        public class TestIndex : AbstractIndexCreationTask<Foo>
        {
            public TestIndex()
            {
                Map = foos => from foo in foos
                              select new
                              {
                                  foo.LocalDateTime
                              };
            }
        }
    }
}

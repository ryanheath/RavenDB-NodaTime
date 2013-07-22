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
        [Fact]
        public void Can_Use_NodaTime_LocalTime_In_Document_Now()
        {
            Can_Use_NodaTime_LocalTime_In_Document(NodaUtil.LocalTime.Now);
        }

        [Fact]
        public void Can_Use_NodaTime_LocalTime_In_Document_IsoMin()
        {
            Can_Use_NodaTime_LocalTime_In_Document(NodaUtil.LocalTime.MinIsoValue);
        }

        [Fact]
        public void Can_Use_NodaTime_LocalTime_In_Document_IsoMax()
        {
            Can_Use_NodaTime_LocalTime_In_Document(NodaUtil.LocalTime.MaxIsoValue);
        }

        private void Can_Use_NodaTime_LocalTime_In_Document(LocalTime lt)
        {
            using (var documentStore = NewDocumentStore())
            {
                documentStore.ConfigureForNodaTime();

                using (var session = documentStore.OpenSession())
                {
                    session.Store(new Foo { Id = "foos/1", LocalTime = lt });
                    session.SaveChanges();
                }

                using (var session = documentStore.OpenSession())
                {
                    var foo = session.Load<Foo>("foos/1");

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
                    var q1 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults()).Where(x => x.LocalTime == lt);
                    var results1 = q1.ToList();
                    Assert.Equal(1, results1.Count);

                    var q2 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults()).Where(x => x.LocalTime > lt);
                    var results2 = q2.ToList();
                    Assert.Equal(2, results2.Count);

                    var q3 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults()).Where(x => x.LocalTime >= lt);
                    var results3 = q3.ToList();
                    Assert.Equal(3, results3.Count);
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
                    var q1 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults()).Where(x => x.LocalTime == lt);
                    var results1 = q1.ToList();
                    Assert.Equal(1, results1.Count);

                    var q2 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults()).Where(x => x.LocalTime < lt);
                    var results2 = q2.ToList();
                    Assert.Equal(2, results2.Count);

                    var q3 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults()).Where(x => x.LocalTime <= lt);
                    var results3 = q3.ToList();
                    Assert.Equal(3, results3.Count);
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
                    var q1 = session.Query<Foo, TestIndex>().Customize(x => x.WaitForNonStaleResults()).Where(x => x.LocalTime == lt);
                    var results1 = q1.ToList();
                    Assert.Equal(1, results1.Count);

                    var q2 = session.Query<Foo, TestIndex>().Customize(x => x.WaitForNonStaleResults()).Where(x => x.LocalTime > lt);
                    var results2 = q2.ToList();
                    Assert.Equal(2, results2.Count);

                    var q3 = session.Query<Foo, TestIndex>().Customize(x => x.WaitForNonStaleResults()).Where(x => x.LocalTime >= lt);
                    var results3 = q3.ToList();
                    Assert.Equal(3, results3.Count);
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
                    var q1 = session.Query<Foo, TestIndex>().Customize(x => x.WaitForNonStaleResults()).Where(x => x.LocalTime == lt);
                    var results1 = q1.ToList();
                    Assert.Equal(1, results1.Count);

                    var q2 = session.Query<Foo, TestIndex>().Customize(x => x.WaitForNonStaleResults()).Where(x => x.LocalTime < lt);
                    var results2 = q2.ToList();
                    Assert.Equal(2, results2.Count);

                    var q3 = session.Query<Foo, TestIndex>().Customize(x => x.WaitForNonStaleResults()).Where(x => x.LocalTime <= lt);
                    var results3 = q3.ToList();
                    Assert.Equal(3, results3.Count);
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

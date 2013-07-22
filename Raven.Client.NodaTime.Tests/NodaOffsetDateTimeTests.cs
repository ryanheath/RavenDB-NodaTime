using System;
using System.Diagnostics;
using System.Linq;
using NodaTime;
using Raven.Client.Indexes;
using Raven.Imports.Newtonsoft.Json;
using Raven.Tests.Helpers;
using Xunit;

namespace Raven.Client.NodaTime.Tests
{
    public class NodaOffsetDateTimeTests : RavenTestBase
    {
        [Fact]
        public void Can_Use_NodaTime_OffsetDateTime_In_Document_Now()
        {
            Can_Use_NodaTime_OffsetDateTime_In_Document(NodaUtil.OffsetDateTime.Now);
        }

        [Fact]
        public void Can_Use_NodaTime_OffsetDateTime_In_Document_IsoMin()
        {
            Can_Use_NodaTime_OffsetDateTime_In_Document(NodaUtil.OffsetDateTime.MinIsoValue);
        }

        [Fact]
        public void Can_Use_NodaTime_OffsetDateTime_In_Document_IsoMax()
        {
            Can_Use_NodaTime_OffsetDateTime_In_Document(NodaUtil.OffsetDateTime.MaxIsoValue);
        }

        private void Can_Use_NodaTime_OffsetDateTime_In_Document(OffsetDateTime odt)
        {
            using (var documentStore = NewDocumentStore())
            {
                documentStore.ConfigureForNodaTime();

                using (var session = documentStore.OpenSession())
                {
                    session.Store(new Foo { Id = "foos/1", OffsetDateTime = odt });
                    session.SaveChanges();
                }

                using (var session = documentStore.OpenSession())
                {
                    var foo = session.Load<Foo>("foos/1");

                    Assert.Equal(odt, foo.OffsetDateTime);
                }

                var json = documentStore.DatabaseCommands.Get("foos/1").DataAsJson;
                Debug.WriteLine(json.ToString(Formatting.Indented));
                var expected = odt.ToDateTimeOffset().ToString("o");
                Assert.Equal(expected, json.Value<string>("OffsetDateTime"));
            }
        }

        [Fact]
        public void Can_Use_NodaTime_OffsetDateTime_In_Dynamic_Index_Now()
        {
            Can_Use_NodaTime_OffsetDateTime_In_Dynamic_Index1(NodaUtil.OffsetDateTime.Now);
        }

        [Fact]
        public void Can_Use_NodaTime_OffsetDateTime_In_Dynamic_Index_IsoMin()
        {
            Can_Use_NodaTime_OffsetDateTime_In_Dynamic_Index1(NodaUtil.OffsetDateTime.MinIsoValue);
        }

        [Fact]
        public void Can_Use_NodaTime_OffsetDateTime_In_Dynamic_Index_IsoMax()
        {
            Can_Use_NodaTime_OffsetDateTime_In_Dynamic_Index2(NodaUtil.OffsetDateTime.MaxIsoValue);
        }

        private void Can_Use_NodaTime_OffsetDateTime_In_Dynamic_Index1(OffsetDateTime odt)
        {
            using (var documentStore = NewDocumentStore())
            {
                documentStore.ConfigureForNodaTime();

                using (var session = documentStore.OpenSession())
                {
                    session.Store(new Foo { Id = "foos/1", OffsetDateTime = odt });
                    session.Store(new Foo { Id = "foos/2", OffsetDateTime = OffsetDateTime.FromDateTimeOffset(odt.ToDateTimeOffset().AddMinutes(1)) });
                    session.Store(new Foo { Id = "foos/3", OffsetDateTime = OffsetDateTime.FromDateTimeOffset(odt.ToDateTimeOffset().AddMinutes(2)) });
                    session.SaveChanges();
                }

                using (var session = documentStore.OpenSession())
                {
                    var q1 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults()).Where(x => x.OffsetDateTime == odt);
                    var results1 = q1.ToList();
                    Assert.Equal(1, results1.Count);

                    // OffsetDateTime is not directly comparable.
                    // Depending on context, one will either convert to an Instant, or just compare the LocalDateTime component.
                    // These are both supported by custom query expression handlers

                    var q2 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults()).Where(x => x.OffsetDateTime.ToInstant() > odt.ToInstant());
                    var results2 = q2.ToList();
                    Assert.Equal(2, results2.Count);

                    var q3 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults()).Where(x => x.OffsetDateTime.ToInstant() >= odt.ToInstant());
                    var results3 = q3.ToList();
                    Assert.Equal(3, results3.Count);

                    var q4 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults()).Where(x => x.OffsetDateTime.LocalDateTime > odt.LocalDateTime);
                    var results4 = q4.ToList();
                    Assert.Equal(2, results4.Count);

                    var q5 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults()).Where(x => x.OffsetDateTime.LocalDateTime >= odt.LocalDateTime);
                    var results5 = q5.ToList();
                    Assert.Equal(3, results5.Count);
                }
            }
        }

        private void Can_Use_NodaTime_OffsetDateTime_In_Dynamic_Index2(OffsetDateTime odt)
        {
            using (var documentStore = NewDocumentStore())
            {
                documentStore.ConfigureForNodaTime();

                using (var session = documentStore.OpenSession())
                {
                    session.Store(new Foo { Id = "foos/1", OffsetDateTime = odt });
                    session.Store(new Foo
                    {
                        Id = "foos/2",
                        OffsetDateTime = OffsetDateTime.FromDateTimeOffset(odt.ToDateTimeOffset().AddMinutes(-1))
                    });
                    session.Store(new Foo
                    {
                        Id = "foos/3",
                        OffsetDateTime = OffsetDateTime.FromDateTimeOffset(odt.ToDateTimeOffset().AddMinutes(-2))
                    });
                    session.SaveChanges();
                }

                using (var session = documentStore.OpenSession())
                {
                    var q1 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults()).Where(x => x.OffsetDateTime == odt);
                    var results1 = q1.ToList();
                    Assert.Equal(1, results1.Count);

                    // OffsetDateTime is not directly comparable.
                    // Depending on context, one will either convert to an Instant, or just compare the LocalDateTime component.
                    // These are both supported by custom query expression handlers

                    var q2 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults()).Where(x => x.OffsetDateTime.ToInstant() < odt.ToInstant());
                    var results2 = q2.ToList();
                    Assert.Equal(2, results2.Count);

                    var q3 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults()).Where(x => x.OffsetDateTime.ToInstant() <= odt.ToInstant());
                    var results3 = q3.ToList();
                    Assert.Equal(3, results3.Count);

                    var q4 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults()).Where(x => x.OffsetDateTime.LocalDateTime < odt.LocalDateTime);
                    var results4 = q4.ToList();
                    Assert.Equal(2, results4.Count);

                    var q5 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults()).Where(x => x.OffsetDateTime.LocalDateTime <= odt.LocalDateTime);
                    var results5 = q5.ToList();
                    Assert.Equal(3, results5.Count);
                }
            }
        }

        [Fact]
        public void Can_Use_NodaTime_OffsetDateTime_In_Static_Index_Now()
        {
            Can_Use_NodaTime_OffsetDateTime_In_Static_Index1(NodaUtil.OffsetDateTime.Now);
        }

        [Fact]
        public void Can_Use_NodaTime_OffsetDateTime_In_Static_Index_IsoMin()
        {
            Can_Use_NodaTime_OffsetDateTime_In_Static_Index1(NodaUtil.OffsetDateTime.MinIsoValue);
        }

        [Fact]
        public void Can_Use_NodaTime_OffsetDateTime_In_Static_Index_IsoMax()
        {
            Can_Use_NodaTime_OffsetDateTime_In_Static_Index2(NodaUtil.OffsetDateTime.MaxIsoValue);
        }

        private void Can_Use_NodaTime_OffsetDateTime_In_Static_Index1(OffsetDateTime odt)
        {
            using (var documentStore = NewDocumentStore())
            {
                documentStore.ConfigureForNodaTime();
                documentStore.ExecuteIndex(new TestIndex());

                using (var session = documentStore.OpenSession())
                {
                    session.Store(new Foo { Id = "foos/1", OffsetDateTime = odt });
                    session.Store(new Foo { Id = "foos/2", OffsetDateTime = OffsetDateTime.FromDateTimeOffset(odt.ToDateTimeOffset().AddMinutes(1)) });
                    session.Store(new Foo { Id = "foos/3", OffsetDateTime = OffsetDateTime.FromDateTimeOffset(odt.ToDateTimeOffset().AddMinutes(2)) });
                    session.SaveChanges();
                }

                using (var session = documentStore.OpenSession())
                {
                    var q1 = session.Query<Foo, TestIndex>().Customize(x => x.WaitForNonStaleResults()).Where(x => x.OffsetDateTime == odt);
                    var results1 = q1.ToList();
                    Assert.Equal(1, results1.Count);

                    // OffsetDateTime is not directly comparable.
                    // Depending on context, one will either convert to an Instant, or just compare the LocalDateTime component.
                    // These are both supported by custom query expression handlers

                    var q2 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults()).Where(x => x.OffsetDateTime.ToInstant() > odt.ToInstant());
                    var results2 = q2.ToList();
                    Assert.Equal(2, results2.Count);

                    var q3 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults()).Where(x => x.OffsetDateTime.ToInstant() >= odt.ToInstant());
                    var results3 = q3.ToList();
                    Assert.Equal(3, results3.Count);

                    var q4 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults()).Where(x => x.OffsetDateTime.LocalDateTime > odt.LocalDateTime);
                    Debug.WriteLine(q4);
                    WaitForUserToContinueTheTest(documentStore);
                    var results4 = q4.ToList();
                    Assert.Equal(2, results4.Count);

                    var q5 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults()).Where(x => x.OffsetDateTime.LocalDateTime >= odt.LocalDateTime);
                    var results5 = q5.ToList();
                    Assert.Equal(3, results5.Count);


                }
            }
        }

        private void Can_Use_NodaTime_OffsetDateTime_In_Static_Index2(OffsetDateTime odt)
        {
            using (var documentStore = NewDocumentStore())
            {
                documentStore.ConfigureForNodaTime();
                documentStore.ExecuteIndex(new TestIndex());

                using (var session = documentStore.OpenSession())
                {
                    session.Store(new Foo { Id = "foos/1", OffsetDateTime = odt });
                    session.Store(new Foo
                    {
                        Id = "foos/2",
                        OffsetDateTime = OffsetDateTime.FromDateTimeOffset(odt.ToDateTimeOffset().AddMinutes(-1))
                    });
                    session.Store(new Foo
                    {
                        Id = "foos/3",
                        OffsetDateTime = OffsetDateTime.FromDateTimeOffset(odt.ToDateTimeOffset().AddMinutes(-2))
                    });
                    session.SaveChanges();
                }

                using (var session = documentStore.OpenSession())
                {
                    var q1 = session.Query<Foo, TestIndex>().Customize(x => x.WaitForNonStaleResults()).Where(x => x.OffsetDateTime == odt);
                    var results1 = q1.ToList();
                    Assert.Equal(1, results1.Count);

                    // OffsetDateTime is not directly comparable.
                    // Depending on context, one will either convert to an Instant, or just compare the LocalDateTime component.
                    // These are both supported by custom query expression handlers

                    var q2 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults()).Where(x => x.OffsetDateTime.ToInstant() < odt.ToInstant());
                    var results2 = q2.ToList();
                    Assert.Equal(2, results2.Count);

                    var q3 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults()).Where(x => x.OffsetDateTime.ToInstant() <= odt.ToInstant());
                    var results3 = q3.ToList();
                    Assert.Equal(3, results3.Count);

                    var q4 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults()).Where(x => x.OffsetDateTime.LocalDateTime < odt.LocalDateTime);
                    var results4 = q4.ToList();
                    Assert.Equal(2, results4.Count);

                    var q5 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults()).Where(x => x.OffsetDateTime.LocalDateTime <= odt.LocalDateTime);
                    var results5 = q5.ToList();
                    Assert.Equal(3, results5.Count);
                }
            }
        }

        public class Foo
        {
            public string Id { get; set; }
            public OffsetDateTime OffsetDateTime { get; set; }
        }

        public class TestIndex : AbstractIndexCreationTask<Foo>
        {
            public TestIndex()
            {
                Map = foos => from foo in foos
                              select new
                              {
                                  foo.OffsetDateTime,

                                  //TODO: This is a hack.  With a bundle, hopefully we can do this server side and keep it a DateTimeOffset
                                  OffsetDateTime_DateTime = ((DateTimeOffset)(object)foo.OffsetDateTime).DateTime
                              };
            }
        }
    }
}

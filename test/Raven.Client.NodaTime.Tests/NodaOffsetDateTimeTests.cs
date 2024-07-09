using System;
using System.Linq;
using NodaTime;
using Raven.Client.Documents.Commands;
using Raven.Client.Documents.Conventions;
using Raven.Client.Documents.Indexes;
using Sparrow.Json;
using Xunit;

namespace Raven.Client.NodaTime.Tests
{
    public class NodaOffsetDateTimeTests : MyRavenTestDriver
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

                using (var session = documentStore.OpenSession())
                {
                    var command = new GetDocumentsCommand(new DocumentConventions(), "foos/1", null, false);
                    session.Advanced.RequestExecutor.Execute(command, session.Advanced.Context);
                    var json = (BlittableJsonReaderObject)command.Result.Results[0];
                    System.Diagnostics.Debug.WriteLine(json.ToString());
                    var expected = odt.ToDateTimeOffset().ToString("o");
                    json.TryGet("OffsetDateTime", out string value);
                    Assert.Equal(expected, value);
                }
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
                using (var session = documentStore.OpenSession())
                {
                    session.Store(new Foo { Id = "foos/1", OffsetDateTime = odt });
                    session.Store(new Foo { Id = "foos/2", OffsetDateTime = OffsetDateTime.FromDateTimeOffset(odt.ToDateTimeOffset().AddMinutes(1)) });
                    session.Store(new Foo { Id = "foos/3", OffsetDateTime = OffsetDateTime.FromDateTimeOffset(odt.ToDateTimeOffset().AddMinutes(2)) });
                    session.SaveChanges();
                }

                using (var session = documentStore.OpenSession())
                {
                    var q1 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.OffsetDateTime == odt);
                    var results1 = q1.ToList();
                    Assert.Single(results1);

                    // OffsetDateTime is not directly comparable.
                    // Depending on context, one will either convert to an Instant, or just compare the LocalDateTime component.
                    // These are both supported by custom query expression handlers

                    var q2 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.OffsetDateTime.ToInstant() > odt.ToInstant())
                                    .OrderByDescending(x => x.OffsetDateTime.ToInstant());
                    var results2 = q2.ToList();
                    Assert.Equal(2, results2.Count);
                    Assert.True(results2[0].OffsetDateTime.ToInstant() > results2[1].OffsetDateTime.ToInstant());

                    var q3 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.OffsetDateTime.ToInstant() >= odt.ToInstant())
                                    .OrderByDescending(x => x.OffsetDateTime.ToInstant());
                    var results3 = q3.ToList();
                    Assert.Equal(3, results3.Count);
                    Assert.True(results3[0].OffsetDateTime.ToInstant() > results3[1].OffsetDateTime.ToInstant());
                    Assert.True(results3[1].OffsetDateTime.ToInstant() > results3[2].OffsetDateTime.ToInstant());

                    var q4 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.OffsetDateTime.LocalDateTime > odt.LocalDateTime)
                                    .OrderByDescending(x => x.OffsetDateTime.LocalDateTime);
                    var results4 = q4.ToList();
                    Assert.Equal(2, results4.Count);
                    Assert.True(results4[0].OffsetDateTime.LocalDateTime > results4[1].OffsetDateTime.LocalDateTime);

                    var q5 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.OffsetDateTime.LocalDateTime >= odt.LocalDateTime)
                                    .OrderByDescending(x => x.OffsetDateTime.LocalDateTime);
                    var results5 = q5.ToList();
                    Assert.Equal(3, results5.Count);
                    Assert.True(results5[0].OffsetDateTime.LocalDateTime > results5[1].OffsetDateTime.LocalDateTime);
                    Assert.True(results5[1].OffsetDateTime.LocalDateTime > results5[2].OffsetDateTime.LocalDateTime);
                }
            }
        }

        private void Can_Use_NodaTime_OffsetDateTime_In_Dynamic_Index2(OffsetDateTime odt)
        {
            using (var documentStore = NewDocumentStore())
            {
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
                    var q1 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.OffsetDateTime == odt);
                    var results1 = q1.ToList();
                    Assert.Single(results1);

                    // OffsetDateTime is not directly comparable.
                    // Depending on context, one will either convert to an Instant, or just compare the LocalDateTime component.
                    // These are both supported by custom query expression handlers

                    var q2 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.OffsetDateTime.ToInstant() < odt.ToInstant())
                                    .OrderBy(x => x.OffsetDateTime.ToInstant());
                    var results2 = q2.ToList();
                    Assert.Equal(2, results2.Count);
                    Assert.True(results2[0].OffsetDateTime.ToInstant() < results2[1].OffsetDateTime.ToInstant());

                    var q3 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.OffsetDateTime.ToInstant() <= odt.ToInstant())
                                    .OrderBy(x => x.OffsetDateTime.ToInstant());
                    var results3 = q3.ToList();
                    Assert.Equal(3, results3.Count);
                    Assert.True(results3[0].OffsetDateTime.ToInstant() < results3[1].OffsetDateTime.ToInstant());
                    Assert.True(results3[1].OffsetDateTime.ToInstant() < results3[2].OffsetDateTime.ToInstant());

                    var q4 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.OffsetDateTime.LocalDateTime < odt.LocalDateTime)
                                    .OrderBy(x => x.OffsetDateTime.LocalDateTime);
                    var results4 = q4.ToList();
                    Assert.Equal(2, results4.Count);
                    Assert.True(results4[0].OffsetDateTime.LocalDateTime < results4[1].OffsetDateTime.LocalDateTime);

                    var q5 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.OffsetDateTime.LocalDateTime <= odt.LocalDateTime)
                                    .OrderBy(x => x.OffsetDateTime.LocalDateTime);
                    var results5 = q5.ToList();
                    Assert.Equal(3, results5.Count);
                    Assert.True(results5[0].OffsetDateTime.LocalDateTime < results5[1].OffsetDateTime.LocalDateTime);
                    Assert.True(results5[1].OffsetDateTime.LocalDateTime < results5[2].OffsetDateTime.LocalDateTime);
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
                    var q1 = session.Query<Foo, TestIndex>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.OffsetDateTime == odt);
                    var results1 = q1.ToList();
                    Assert.Single(results1);

                    // OffsetDateTime is not directly comparable.
                    // Depending on context, one will either convert to an Instant, or just compare the LocalDateTime component.
                    // These are both supported by custom query expression handlers

                    var q2 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.OffsetDateTime.ToInstant() > odt.ToInstant())
                                    .OrderByDescending(x => x.OffsetDateTime.ToInstant());
                    var results2 = q2.ToList();
                    Assert.Equal(2, results2.Count);
                    Assert.True(results2[0].OffsetDateTime.ToInstant() > results2[1].OffsetDateTime.ToInstant());

                    var q3 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.OffsetDateTime.ToInstant() >= odt.ToInstant())
                                    .OrderByDescending(x => x.OffsetDateTime.ToInstant());
                    var results3 = q3.ToList();
                    Assert.Equal(3, results3.Count);
                    Assert.True(results3[0].OffsetDateTime.ToInstant() > results3[1].OffsetDateTime.ToInstant());
                    Assert.True(results3[1].OffsetDateTime.ToInstant() > results3[2].OffsetDateTime.ToInstant());

                    var q4 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.OffsetDateTime.LocalDateTime > odt.LocalDateTime)
                                    .OrderByDescending(x => x.OffsetDateTime.LocalDateTime);
                    System.Diagnostics.Debug.WriteLine(q4);
                    //WaitForUserToContinueTheTest(documentStore);
                    var results4 = q4.ToList();
                    Assert.Equal(2, results4.Count);
                    Assert.True(results4[0].OffsetDateTime.LocalDateTime > results4[1].OffsetDateTime.LocalDateTime);

                    var q5 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.OffsetDateTime.LocalDateTime >= odt.LocalDateTime)
                                    .OrderByDescending(x => x.OffsetDateTime.LocalDateTime);
                    var results5 = q5.ToList();
                    Assert.Equal(3, results5.Count);
                    Assert.True(results5[0].OffsetDateTime.LocalDateTime > results5[1].OffsetDateTime.LocalDateTime);
                    Assert.True(results5[1].OffsetDateTime.LocalDateTime > results5[2].OffsetDateTime.LocalDateTime);


                }
            }
        }

        private void Can_Use_NodaTime_OffsetDateTime_In_Static_Index2(OffsetDateTime odt)
        {
            using (var documentStore = NewDocumentStore())
            {
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
                    var q1 = session.Query<Foo, TestIndex>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.OffsetDateTime == odt);
                    var results1 = q1.ToList();
                    Assert.Single(results1);

                    // OffsetDateTime is not directly comparable.
                    // Depending on context, one will either convert to an Instant, or just compare the LocalDateTime component.
                    // These are both supported by custom query expression handlers

                    var q2 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.OffsetDateTime.ToInstant() < odt.ToInstant())
                                    .OrderBy(x => x.OffsetDateTime.ToInstant());
                    var results2 = q2.ToList();
                    Assert.Equal(2, results2.Count);
                    Assert.True(results2[0].OffsetDateTime.ToInstant() < results2[1].OffsetDateTime.ToInstant());

                    var q3 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.OffsetDateTime.ToInstant() <= odt.ToInstant())
                                    .OrderBy(x => x.OffsetDateTime.ToInstant());
                    var results3 = q3.ToList();
                    Assert.Equal(3, results3.Count);
                    Assert.True(results3[0].OffsetDateTime.ToInstant() < results3[1].OffsetDateTime.ToInstant());
                    Assert.True(results3[1].OffsetDateTime.ToInstant() < results3[2].OffsetDateTime.ToInstant());

                    var q4 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.OffsetDateTime.LocalDateTime < odt.LocalDateTime)
                                    .OrderBy(x => x.OffsetDateTime.LocalDateTime);
                    var results4 = q4.ToList();
                    Assert.Equal(2, results4.Count);
                    Assert.True(results4[0].OffsetDateTime.LocalDateTime < results4[1].OffsetDateTime.LocalDateTime);

                    var q5 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.OffsetDateTime.LocalDateTime <= odt.LocalDateTime)
                                    .OrderBy(x => x.OffsetDateTime.LocalDateTime);
                    var results5 = q5.ToList();
                    Assert.Equal(3, results5.Count);
                    Assert.True(results5[0].OffsetDateTime.LocalDateTime < results5[1].OffsetDateTime.LocalDateTime);
                    Assert.True(results5[1].OffsetDateTime.LocalDateTime < results5[2].OffsetDateTime.LocalDateTime);
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

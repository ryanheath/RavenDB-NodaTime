using System.Linq;
using NodaTime;
using Raven.Client.Documents.Commands;
using Raven.Client.Documents.Conventions;
using Raven.Client.Documents.Indexes;
using Sparrow.Json;
using Xunit;

namespace Raven.Client.NodaTime.Tests
{
    public class NodaOffsetTests : MyRavenTestDriver
    {
        [Fact]
        public void Can_Use_NodaTime_Offset_In_Document_Positive()
        {
            Can_Use_NodaTime_Offset_In_Document(Offset.FromHours(2));
        }

        [Fact]
        public void Can_Use_NodaTime_Offset_In_Document_Negative()
        {
            Can_Use_NodaTime_Offset_In_Document(Offset.FromHoursAndMinutes(-5, 30));
        }

        [Fact]
        public void Can_Use_NodaTime_Offset_In_Document_Min()
        {
            Can_Use_NodaTime_Offset_In_Document(Offset.MinValue);
        }

        [Fact]
        public void Can_Use_NodaTime_Offset_In_Document_Max()
        {
            Can_Use_NodaTime_Offset_In_Document(Offset.MaxValue);
        }

        private void Can_Use_NodaTime_Offset_In_Document(Offset offset)
        {
            using (var documentStore = NewDocumentStore())
            {
                using (var session = documentStore.OpenSession())
                {
                    session.Store(new Foo { Id = "foos/1", Offst = offset });
                    session.SaveChanges();
                }

                using (var session = documentStore.OpenSession())
                {
                    var foo = session.Load<Foo>("foos/1");

                    Assert.Equal(offset, foo.Offst);
                }

                using (var session = documentStore.OpenSession())
                {
                    var command = new GetDocumentsCommand(new DocumentConventions(), "foos/1", null, false);
                    session.Advanced.RequestExecutor.Execute(command, session.Advanced.Context);
                    var json = (BlittableJsonReaderObject)command.Result.Results[0];
                    System.Diagnostics.Debug.WriteLine(json.ToString());
                    var expected = offset.ToTimeSpan().ToString("c");
                    json.TryGet("Offst", out string value);
                    Assert.Equal(expected, value);
                }
            }
        }

        [Fact]
        public void Can_Use_NodaTime_Offset_In_Dynamic_Index_Positive()
        {
            Can_Use_NodaTime_Offset_In_Dynamic_Index1(Offset.FromHours(2));
        }

        [Fact]
        public void Can_Use_NodaTime_Offset_In_Dynamic_Index_Negative()
        {
            Can_Use_NodaTime_Offset_In_Dynamic_Index2(Offset.FromHoursAndMinutes(-5, 30));
        }

        [Fact]
        public void Can_Use_NodaTime_Offset_In_Dynamic_Index_Min()
        {
            Can_Use_NodaTime_Offset_In_Dynamic_Index1(Offset.MinValue);
        }

        [Fact]
        public void Can_Use_NodaTime_Offset_In_Dynamic_Index_Max()
        {
            Can_Use_NodaTime_Offset_In_Dynamic_Index2(Offset.MaxValue);
        }

        private void Can_Use_NodaTime_Offset_In_Dynamic_Index1(Offset offset)
        {
            using (var documentStore = NewDocumentStore())
            {
                using (var session = documentStore.OpenSession())
                {
                    session.Store(new Foo { Id = "foos/1", Offst = offset });
                    session.Store(new Foo { Id = "foos/2", Offst = offset + Offset.FromHours(1) });
                    session.Store(new Foo { Id = "foos/3", Offst = offset + Offset.FromHours(2) });
                    session.SaveChanges();
                }

                using (var session = documentStore.OpenSession())
                {
                    var q1 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.Offst == offset);
                    var results1 = q1.ToList();
                    Assert.Single(results1);

                    var q2 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.Offst > offset)
                                    .OrderByDescending(x => x.Offst);
                    var results2 = q2.ToList();
                    Assert.Equal(2, results2.Count);
                    Assert.True(results2[0].Offst > results2[1].Offst);

                    var q3 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.Offst >= offset)
                                    .OrderByDescending(x => x.Offst);
                    var results3 = q3.ToList();
                    Assert.Equal(3, results3.Count);
                    Assert.True(results3[0].Offst > results3[1].Offst);
                    Assert.True(results3[1].Offst > results3[2].Offst);
                }
            }
        }

        //class Bar
        //{
        //    public string Id { get; set; }
        //    public int Offset { get; set; }
        //}

        //[Fact]
        //public void Can_Use_Offset_As_PropertyName()
        //{
        //    var offset = 50;

        //    using (var documentStore = NewDocumentStore())
        //    {
        //        using (var session = documentStore.OpenSession())
        //        {
        //            session.Store(new Bar { Id = "foos/1", Offset = offset });
        //            session.Store(new Bar { Id = "foos/2", Offset = offset + 1 });
        //            session.Store(new Bar { Id = "foos/3", Offset = offset + 2 });
        //            session.SaveChanges();
        //        }

        //        using (var session = documentStore.OpenSession())
        //        {
        //            var q1 = session.Query<Bar>().Customize(x => x.WaitForNonStaleResults())
        //                            .Where(x => x.Offset == offset);
        //            var results1 = q1.ToList();
        //            Assert.Single(results1);

        //            var q2 = session.Query<Bar>().Customize(x => x.WaitForNonStaleResults())
        //                            .Where(x => x.Offset > offset)
        //                            .OrderByDescending(x => x.Offset);
        //            var results2 = q2.ToList();
        //            Assert.Equal(2, results2.Count);
        //            Assert.True(results2[0].Offset > results2[1].Offset);

        //            var q3 = session.Query<Bar>().Customize(x => x.WaitForNonStaleResults())
        //                            .Where(x => x.Offset >= offset)
        //                            .OrderByDescending(x => x.Offset);
        //            var results3 = q3.ToList();
        //            Assert.Equal(3, results3.Count);
        //            Assert.True(results3[0].Offset > results3[1].Offset);
        //            Assert.True(results3[1].Offset > results3[2].Offset);
        //        }
        //    }
        //}

        private void Can_Use_NodaTime_Offset_In_Dynamic_Index2(Offset offset)
        {
            using (var documentStore = NewDocumentStore())
            {
                using (var session = documentStore.OpenSession())
                {
                    session.Store(new Foo { Id = "foos/1", Offst = offset });
                    session.Store(new Foo { Id = "foos/2", Offst = offset - Offset.FromHours(1) });
                    session.Store(new Foo { Id = "foos/3", Offst = offset - Offset.FromHours(2) });
                    session.SaveChanges();
                }

                using (var session = documentStore.OpenSession())
                {
                    var q1 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.Offst == offset);
                    var results1 = q1.ToList();
                    Assert.Single(results1);

                    var q2 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.Offst < offset)
                                    .OrderBy(x => x.Offst);
                    var results2 = q2.ToList();
                    Assert.Equal(2, results2.Count);
                    Assert.True(results2[0].Offst < results2[1].Offst);
                    
                    var q3 = session.Query<Foo>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.Offst <= offset)
                                    .OrderBy(x => x.Offst);
                    var results3 = q3.ToList();
                    Assert.Equal(3, results3.Count);
                    Assert.True(results3[0].Offst < results3[1].Offst);
                    Assert.True(results3[1].Offst < results3[2].Offst);
                }
            }
        }

        [Fact]
        public void Can_Use_NodaTime_Offset_In_Static_Index_Positive()
        {
            Can_Use_NodaTime_Offset_In_Static_Index1(Offset.FromHours(2));
        }

        [Fact]
        public void Can_Use_NodaTime_Offset_In_Static_Index_Negative()
        {
            Can_Use_NodaTime_Offset_In_Static_Index2(Offset.FromHoursAndMinutes(-5, 30));
        }

        [Fact]
        public void Can_Use_NodaTime_Offset_In_Static_Index_Min()
        {
            Can_Use_NodaTime_Offset_In_Static_Index1(Offset.MinValue);
        }

        [Fact]
        public void Can_Use_NodaTime_Offset_In_Static_Index_Max()
        {
            Can_Use_NodaTime_Offset_In_Static_Index2(Offset.MaxValue);
        }

        private void Can_Use_NodaTime_Offset_In_Static_Index1(Offset offset)
        {
            using (var documentStore = NewDocumentStore())
            {
                documentStore.ExecuteIndex(new TestIndex());

                using (var session = documentStore.OpenSession())
                {
                    session.Store(new Foo { Id = "foos/1", Offst = offset });
                    session.Store(new Foo { Id = "foos/2", Offst = offset + Offset.FromHours(1) });
                    session.Store(new Foo { Id = "foos/3", Offst = offset + Offset.FromHours(2) });
                    session.SaveChanges();
                }

                using (var session = documentStore.OpenSession())
                {
                    var q1 = session.Query<Foo, TestIndex>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.Offst == offset);
                    var results1 = q1.ToList();
                    Assert.Single(results1);

                    var q2 = session.Query<Foo, TestIndex>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.Offst > offset)
                                    .OrderByDescending(x => x.Offst);
                    var results2 = q2.ToList();
                    Assert.Equal(2, results2.Count);
                    Assert.True(results2[0].Offst > results2[1].Offst);
                    
                    var q3 = session.Query<Foo, TestIndex>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.Offst >= offset)
                                    .OrderByDescending(x => x.Offst);
                    var results3 = q3.ToList();
                    Assert.Equal(3, results3.Count);
                    Assert.True(results3[0].Offst > results3[1].Offst);
                    Assert.True(results3[1].Offst > results3[2].Offst);
                }
            }
        }

        private void Can_Use_NodaTime_Offset_In_Static_Index2(Offset offset)
        {
            using (var documentStore = NewDocumentStore())
            {
                documentStore.ExecuteIndex(new TestIndex());

                using (var session = documentStore.OpenSession())
                {
                    session.Store(new Foo { Id = "foos/1", Offst = offset });
                    session.Store(new Foo { Id = "foos/2", Offst = offset - Offset.FromHours(1) });
                    session.Store(new Foo { Id = "foos/3", Offst = offset - Offset.FromHours(2) });
                    session.SaveChanges();
                }

                using (var session = documentStore.OpenSession())
                {
                    var q1 = session.Query<Foo, TestIndex>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.Offst == offset);
                    var results1 = q1.ToList();
                    Assert.Single(results1);

                    var q2 = session.Query<Foo, TestIndex>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.Offst < offset)
                                    .OrderBy(x => x.Offst);
                    var results2 = q2.ToList();
                    Assert.Equal(2, results2.Count);
                    Assert.True(results2[0].Offst < results2[1].Offst);
                    
                    var q3 = session.Query<Foo, TestIndex>().Customize(x => x.WaitForNonStaleResults())
                                    .Where(x => x.Offst <= offset)
                                    .OrderBy(x => x.Offst);
                    var results3 = q3.ToList();
                    Assert.Equal(3, results3.Count);
                    Assert.True(results3[0].Offst < results3[1].Offst);
                    Assert.True(results3[1].Offst < results3[2].Offst);
                }
            }
        }

        public class Foo
        {
            public string Id { get; set; }
            public Offset Offst { get; set; }
        }

        public class TestIndex : AbstractIndexCreationTask<Foo>
        {
            public TestIndex()
            {
                Map = foos => from foo in foos
                              select new
                              {
                                  foo.Offst
                              };

            }
        }
    }
}

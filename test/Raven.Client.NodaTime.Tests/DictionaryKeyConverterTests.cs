using System.Collections.Generic;
using NodaTime;
using Xunit;

namespace Raven.Client.NodaTime.Tests
{
    public class DictionaryKeyConverterTests : MyRavenTestDriver
    {
        [Fact]
        public void Can_Use_NodaTime_Instant_In_Dictionary_Key()
        {
            using (var documentStore = NewDocumentStore())
            {
                Instant i1 = SystemClock.Instance.GetCurrentInstant();
                Instant i2 = i1 + Duration.FromHours(1);

                using (var session = documentStore.OpenSession())
                {
                    var testData = new Dictionary<Instant, string> {{i1, "i1"}, {i2, "i2"}};
                    session.Store(new Foo { Id = "foos/1", TestData = testData});
                    session.SaveChanges();
                }

                using (var session = documentStore.OpenSession())
                {
                    var foo = session.Load<Foo>("foos/1");

                    Assert.Equal("i1", foo.TestData[i1]);
                    Assert.Equal("i2", foo.TestData[i2]);
                }
            }
        }

        [Fact]
        public void Can_Use_NodaTime_Instant_In_IDictionary_Key()
        {
            using (var documentStore = NewDocumentStore())
            {
                Instant i1 = SystemClock.Instance.GetCurrentInstant();
                Instant i2 = i1 + Duration.FromHours(1);

                using (var session = documentStore.OpenSession())
                {
                    var testData = new Dictionary<Instant, string> { { i1, "i1" }, { i2, "i2" } };
                    session.Store(new Bar { Id = "bars/1", TestData = testData });
                    session.SaveChanges();
                }

                using (var session = documentStore.OpenSession())
                {
                    var bar = session.Load<Bar>("bars/1");

                    Assert.Equal("i1", bar.TestData[i1]);
                    Assert.Equal("i2", bar.TestData[i2]);
                }
            }
        }

        public class Foo
        {
            public string Id { get; set; }
            public Dictionary<Instant,string> TestData { get; set; }
        }

        public class Bar
        {
            public string Id { get; set; }
            public IDictionary<Instant, string> TestData { get; set; }
        }
    }
}

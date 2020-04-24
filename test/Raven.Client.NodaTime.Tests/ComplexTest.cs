using System;
using System.Collections.Generic;
using System.Linq;
using NodaTime;
using Raven.Client.Documents;
using Raven.Client.Documents.Indexes;
using Xunit;

namespace Raven.Client.NodaTime.Tests
{
    public class ComplexTest : MyRavenTestDriver
    {
        public class Business
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string TimeZone { get; set; }
        }

        public class Schedule
        {
            public string BusinessId { get; set; }
            public LocalDate FromDate { get; set; }
            public LocalDate ToDate { get; set; }
            public IList<HoursOfOperation> BusinessHours { get; set; }

            public Schedule()
            {
                BusinessHours = new List<HoursOfOperation>();
            }
        }

        public class HoursOfOperation
        {
            public IsoDayOfWeek DayOfWeek { get; set; }
            public LocalTime Open { get; set; }
            public LocalTime Close { get; set; }
        }


        [Fact(Skip = "Raven can't compile, TODO later")]
        public void Get_All_Scheduled_Dates()
        {
            using (var documentStore = NewDocumentStore())
            {
                documentStore.ExecuteIndex(new SchedulesIndex());

                PopulateSchedules(documentStore);

                WaitForIndexing(documentStore);

                //WaitForUserToContinueTheTest(documentStore);

                using (var session = documentStore.OpenSession())
                {
                    var q = session.Query<HoursForDate, SchedulesIndex>()
                        .ProjectInto<HoursForDate>()
                        .Take(1024);

                    System.Diagnostics.Debug.WriteLine(q);

                    var results = q.ToList();

                    documentStore.DebugWriteJson(results);

                    Assert.Equal(2 * 364, results.Count);
                }
            }
        }

        [Fact(Skip = "Raven can't compile, TODO later")]
        public void Get_Schedule_For_Instant()
        {
            using (var documentStore = NewDocumentStore())
            {
                documentStore.ExecuteIndex(new SchedulesIndex());

                PopulateSchedules(documentStore);

                WaitForIndexing(documentStore);

                //WaitForUserToContinueTheTest(documentStore);

                using (var session = documentStore.OpenSession())
                {
                    //var now = SystemClock.Instance.Now;
                    var now = Instant.FromUtc(2013, 4, 6, 18, 0);

                    var q = session.Query<HoursForDate, SchedulesIndex>()
                        .Where(x => x.Open <= now && x.Close > now)
                        .ProjectInto<HoursForDate>();

                    System.Diagnostics.Debug.WriteLine(q);

                    var results = q.ToList();

                    documentStore.DebugWriteJson(results);

                    Assert.Single(results);
                }
            }
        }

        private static void PopulateSchedules(IDocumentStore documentStore)
        {
            using (var session = documentStore.OpenSession())
            {
                session.Store(new Business
                {
                    Id = "businesses/1",
                    Name = "Joe's Bar and Grill",
                    TimeZone = "America/Chicago",
                });

                #region 2013 Schedule

                session.Store(new Schedule
                {
                    BusinessId = "businesses/1",
                    FromDate = new LocalDate(2013, 1, 1),
                    ToDate = new LocalDate(2013, 12, 31),
                    BusinessHours =
                    {
                        new HoursOfOperation
                        {
                            DayOfWeek = IsoDayOfWeek.Sunday,
                            Open = new LocalTime(11, 0),
                            Close = new LocalTime(22, 0)
                        },
                        new HoursOfOperation
                        {
                            DayOfWeek = IsoDayOfWeek.Monday,
                            Open = new LocalTime(11, 0),
                            Close = new LocalTime(22, 0)
                        },
                        new HoursOfOperation
                        {
                            DayOfWeek = IsoDayOfWeek.Tuesday,
                            Open = new LocalTime(11, 0),
                            Close = new LocalTime(22, 0)
                        },
                        new HoursOfOperation
                        {
                            DayOfWeek = IsoDayOfWeek.Wednesday,
                            Open = new LocalTime(11, 0),
                            Close = new LocalTime(22, 0)
                        },
                        new HoursOfOperation
                        {
                            DayOfWeek = IsoDayOfWeek.Thursday,
                            Open = new LocalTime(11, 0),
                            Close = new LocalTime(22, 0)
                        },
                        new HoursOfOperation
                        {
                            DayOfWeek = IsoDayOfWeek.Friday,
                            Open = new LocalTime(11, 0),
                            Close = new LocalTime(2, 0)
                        },
                        new HoursOfOperation
                        {
                            DayOfWeek = IsoDayOfWeek.Saturday,
                            Open = new LocalTime(11, 0),
                            Close = new LocalTime(2, 0)
                        }
                    }
                });

                #endregion

                #region 2014 Schedule

                session.Store(new Schedule
                {
                    BusinessId = "businesses/1",
                    FromDate = new LocalDate(2014, 1, 1),
                    ToDate = new LocalDate(2014, 12, 31),
                    BusinessHours =
                    {
                        new HoursOfOperation
                        {
                            DayOfWeek = IsoDayOfWeek.Sunday,
                            Open = new LocalTime(6, 0),
                            Close = new LocalTime(22, 0)
                        },
                        new HoursOfOperation
                        {
                            DayOfWeek = IsoDayOfWeek.Monday,
                            Open = new LocalTime(11, 0),
                            Close = new LocalTime(22, 0)
                        },
                        new HoursOfOperation
                        {
                            DayOfWeek = IsoDayOfWeek.Tuesday,
                            Open = new LocalTime(11, 0),
                            Close = new LocalTime(22, 0)
                        },
                        new HoursOfOperation
                        {
                            DayOfWeek = IsoDayOfWeek.Wednesday,
                            Open = new LocalTime(11, 0),
                            Close = new LocalTime(22, 0)
                        },
                        new HoursOfOperation
                        {
                            DayOfWeek = IsoDayOfWeek.Thursday,
                            Open = new LocalTime(11, 0),
                            Close = new LocalTime(22, 0)
                        },
                        new HoursOfOperation
                        {
                            DayOfWeek = IsoDayOfWeek.Friday,
                            Open = new LocalTime(11, 0),
                            Close = new LocalTime(2, 0)
                        },
                        new HoursOfOperation
                        {
                            DayOfWeek = IsoDayOfWeek.Saturday,
                            Open = new LocalTime(6, 0),
                            Close = new LocalTime(2, 0)
                        }
                    }
                });

                #endregion

                session.SaveChanges();
            }
        }

        public class HoursForDate
        {
            public string BusinessId { get; set; }
            public string BusinessName { get; set; }
            public LocalDateTime LocalOpen { get; set; }
            public LocalDateTime LocalClose { get; set; }
            public Instant Open { get; set; }
            public Instant Close { get; set; }
        }

        public class SchedulesIndex : AbstractIndexCreationTask<Schedule, HoursForDate>
        {
            public SchedulesIndex()
            {
                Map = schedules =>
                    from schedule in schedules

                    let business = LoadDocument<Business>(schedule.BusinessId)
                    let tz = global::NodaTime.DateTimeZoneProviders.Tzdb[business.TimeZone]

                    let fromDate = schedule.FromDate.AsLocalDate()
                    let toDate = schedule.ToDate.AsLocalDate()
                    let daysInPeriod = fromDate.DaysBetween(toDate)
                    from day in Enumerable.Range(0, daysInPeriod).Cast<int>()
                    let date = fromDate.PlusDays(day)
                    let hours = schedule.BusinessHours.FirstOrDefault(x => x.DayOfWeek.ToString() == date.DayOfWeek.ToString())
                    let localOpen = date + hours.Open.AsLocalTime()
                    let localClose = date.PlusDays(hours.Close > hours.Open ? 0 : 1) + hours.Close.AsLocalTime()
                    select new
                    {
                        schedule.BusinessId,
                        BusinessName = business.Name,
                        LocalOpen = localOpen.Resolve(),
                        LocalClose = localClose.Resolve(),
                        Open = localOpen.InZoneLeniently(tz).ToInstant().Resolve(),
                        Close = localClose.InZoneLeniently(tz).ToInstant().Resolve()
                    };

                StoreAllFields(FieldStorage.Yes);

                AdditionalSources = new Dictionary<string, string> {
                    { "Raven.Client.NodaTime", NodaTimeCompilationExtension.AdditionalSourcesRavenBundlesNodaTime },
                    { "Raven.Client.NodaTime2", NodaTimeCompilationExtension.AdditionalSourcesNodaTime }
                };
            }
        }

    }
}

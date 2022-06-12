using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Diagnostics;

namespace Website
{
    public class PageViewsMultiTimeline
    {
        public DateTime StartTime { get; private set; }
        private PageViewsTimeline secondsTimeline { get; set; }
        private PageViewsTimeline minutesTimeline { get; set; }
        private PageViewsTimeline hoursTimeline { get; set; }
        private PageViewsTimeline daysTimeline { get; set; }

        public PageViewsMultiTimeline()
        {
            StartTime = DateTime.Now;

            secondsTimeline = new PageViewsTimeline(60, TimeSpan.FromSeconds(1), allowMedian:true);
            minutesTimeline = new PageViewsTimeline(60, TimeSpan.FromMinutes(1), allowMedian: true);
            hoursTimeline = new PageViewsTimeline(60, TimeSpan.FromHours(1));
            daysTimeline = new PageViewsTimeline(30, TimeSpan.FromDays(1));
        }

        public List<PageViewStats> SecondsTimeline
        {
            get { lock (this) return secondsTimeline.Series; }
        }

        public List<PageViewStats> MinutesTimeline
        {
            get { lock (this) return minutesTimeline.Series; }
        }

        public List<PageViewStats> HoursTimeline
        {
            get { lock (this) return hoursTimeline.Series; }
        }

        public List<PageViewStats> DaysTimeline
        {
            get { lock (this) return daysTimeline.Series; }
        }


        public PageViewStats MostRecentCompletedSecond
        {
            get { lock (this) return secondsTimeline.MostRecentCompletedPeriod; }
        }

        public PageViewStats MostRecentCompletedMinute
        {
            get { lock (this) return minutesTimeline.MostRecentCompletedPeriod; }
        }

        public PageViewStats MostRecentCompletedHour
        {
            get { lock (this) return hoursTimeline.MostRecentCompletedPeriod; }
        }

        public PageViewStats MostRecentCompletedDay
        {
            get { lock (this) return daysTimeline.MostRecentCompletedPeriod; }
        }


        public void Bump(PageViewStats stats)
        {
            lock (this)
            {
                secondsTimeline.Bump(stats);
                minutesTimeline.Bump(stats);
                hoursTimeline.Bump(stats);
                daysTimeline.Bump(stats);
            }
        }

    }
}
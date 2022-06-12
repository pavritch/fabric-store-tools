using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Diagnostics;

namespace Website
{
    public class MultiTimeline
    {
        public DateTime StartTime { get; private set; }
        private Timeline secondsTimeline { get; set; }
        private Timeline minutesTimeline { get; set; }
        private Timeline hoursTimeline { get; set; }
        private Timeline daysTimeline { get; set; }

        public MultiTimeline()
        {
            StartTime = DateTime.Now;

            secondsTimeline = new Timeline(60, TimeSpan.FromSeconds(1));
            minutesTimeline = new Timeline(60, TimeSpan.FromMinutes(1));
            hoursTimeline = new Timeline(60, TimeSpan.FromHours(1));
            daysTimeline = new Timeline(30, TimeSpan.FromDays(1));
        }

        public List<int> SecondsTimeline
        {
            get { lock (this) return secondsTimeline.Series; }
        }

        public List<int> MinutesTimeline
        {
            get { lock (this) return minutesTimeline.Series; }
        }

        public List<int> HoursTimeline
        {
            get { lock (this) return hoursTimeline.Series; }
        }

        public List<int> DaysTimeline
        {
            get { lock (this) return daysTimeline.Series; }
        }

        public void Bump()
        {
            lock (this)
            {
                secondsTimeline.Bump();
                minutesTimeline.Bump();
                hoursTimeline.Bump();
                daysTimeline.Bump();
            }
        }


    }
}
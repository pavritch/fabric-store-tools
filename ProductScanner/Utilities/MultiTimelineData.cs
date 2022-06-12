using System.Collections.Generic;

namespace Utilities
{
    public class MultiTimelineData
    {
        public List<int> SecondsTimeline { get; set; }
        public List<int> MinutesTimeline { get; set; }
        public List<int> HoursTimeline { get; set; }
        public List<int> DaysTimeline { get; set; }

        public MultiTimelineData(List<int> secondsTimeline, List<int> minutesTimeline, List<int> hoursTimeline, List<int> daysTimeline)
        {
            SecondsTimeline = secondsTimeline;
            MinutesTimeline = minutesTimeline;
            DaysTimeline = daysTimeline;
            HoursTimeline = hoursTimeline;
        }
    }
}
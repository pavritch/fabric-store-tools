using Utilities.Extensions;

namespace Utilities
{
    public static class MultiTimelineExtensions
    {
        public static MultiTimelineData ToDto(this MultiTimeline timeline)
        {
            return new MultiTimelineData(timeline.SecondsTimeline, timeline.MinutesTimeline, timeline.HoursTimeline, timeline.DaysTimeline);
        }

        public static MultiTimelineData Combine(MultiTimelineData a, MultiTimelineData b)
        {
            return new MultiTimelineData(a.SecondsTimeline.Combine(b.SecondsTimeline),
                a.MinutesTimeline.Combine(b.MinutesTimeline),
                a.HoursTimeline.Combine(b.HoursTimeline),
                a.DaysTimeline.Combine(b.DaysTimeline));
        }
    }
}
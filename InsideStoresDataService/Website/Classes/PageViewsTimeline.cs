using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Diagnostics;

namespace Website
{
    /// <summary>
    /// Represents a timeline of page view statistics.
    /// </summary>
    public class PageViewsTimeline
    {
        private class CircularQueue<T> : Queue<T> where T : new()
        {
            public T LastElement {get; private set;}

            public CircularQueue(int capacity)
            {
                for (int i = 0; i < capacity; i++)
                {
                    LastElement = new T();
                    this.Enqueue(LastElement);
                }
            }

            public void Put(T data)
            {
                // discard frontmost item
                Dequeue();
                Enqueue(data);
                LastElement = data;
            }

        }

        private readonly DateTime timelineStartTime;
        private DateTime intervalStart;
        private readonly TimeSpan intervalTime;
        private readonly CircularQueue<PageViewStats> ringBuffer;
        private PageViewStats currentIntervalSlice;
        private readonly bool allowMedian;

        public PageViewsTimeline(int IntervalCount, TimeSpan Interval,bool allowMedian=false)
        {
            var now = DateTime.Now;
            this.allowMedian = allowMedian;
            currentIntervalSlice = new PageViewStats(allowMedian);

            // even out the seconds
            timelineStartTime = now.Subtract(TimeSpan.FromMilliseconds(now.Millisecond));
            intervalStart = timelineStartTime;
            intervalTime = Interval;
            ringBuffer = new CircularQueue<PageViewStats>(IntervalCount);
        }

        private void SpinToNow()
        {
            var endInterval = intervalStart + intervalTime;
            while (DateTime.Now >= endInterval)
            {
                currentIntervalSlice.PrepareForArchiving();
                ringBuffer.Put(currentIntervalSlice);
                currentIntervalSlice = new PageViewStats(allowMedian); 
                intervalStart = endInterval;
                endInterval += intervalTime;
            }
        }

        public void Bump(PageViewStats stats)
        {
            lock (this)
            {
                SpinToNow();
                currentIntervalSlice.Bump(stats);
            }
        }

        public PageViewStats MostRecentCompletedPeriod
        {
            get
            {
                lock (this)
                {
                    SpinToNow();
                    return ringBuffer.LastElement;
                }
            }
        }


        public List<PageViewStats> Series
        {
            get
            {
                var items = new List<PageViewStats>();
                lock (this)
                {
                    // all completed intervals, oldest first, followed by current in-progress bucket

                    SpinToNow();
                    items.AddRange(ringBuffer);
                    items.Add(currentIntervalSlice);
                }

                return items;
            }
        }

    }
}
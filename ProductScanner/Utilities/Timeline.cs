using System;
using System.Collections.Generic;

namespace Utilities
{
    public class Timeline
    {
        private class CircularQueue<T> : Queue<T>
        {
            public CircularQueue(int capacity)
            {
                for (int i = 0; i < capacity; i++)
                    this.Enqueue(default(T));
            }

            public void Put(T data)
            {
                // discard frontmost item
                Dequeue();
                Enqueue(data);
            }
        }

        private DateTime timelineStartTime;
        private DateTime intervalStart;
        private TimeSpan intervalTime;
        private CircularQueue<int> ringBuffer;
        private int currentIntervalCount = 0;

        public Timeline(int IntervalCount, TimeSpan Interval)
        {
            var now = DateTime.Now;

            // even out the seconds
            timelineStartTime = now.Subtract(TimeSpan.FromMilliseconds(now.Millisecond));
            intervalStart = timelineStartTime;
            intervalTime = Interval;
            ringBuffer = new CircularQueue<int>(IntervalCount);
        }

        private void SpinToNow()
        {
            var endInterval = intervalStart + intervalTime;
            while (DateTime.Now >= endInterval)
            {
                ringBuffer.Put(currentIntervalCount);
                currentIntervalCount = 0;
                intervalStart = endInterval;
                endInterval += intervalTime;
            }
        }

        public void Bump()
        {
            lock (this)
            {
                SpinToNow();
                currentIntervalCount++;
            }
        }

        public List<int> Series
        {
            get
            {
                List<int> items = new List<int>();
                lock (this)
                {
                    // all completed intervals, oldest first, followed by current in-progress bucket

                    SpinToNow();
                    items.AddRange(ringBuffer);
                    items.Add(currentIntervalCount);
                }
                return items;
            }
        }
    }
}
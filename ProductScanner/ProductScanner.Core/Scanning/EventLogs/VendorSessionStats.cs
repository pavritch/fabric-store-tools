using System;

namespace ProductScanner.Core.Scanning.EventLogs
{
    public class VendorSessionStats
    {
        public event EventHandler StatsChanged;

        private void FireStatusChangedEvent()
        {
            if (StatsChanged != null) StatsChanged(this, null);
        }

        public void Clear()
        {
            TotalItems = 0;
            CompletedItems = 0;
            ErrorCount = 0;
            CurrentTask = string.Empty;
        }

        private int _totalItems;
        public int TotalItems
        {
            get { return _totalItems; }
            set
            {
                _totalItems = value;

                FireStatusChangedEvent();
            }
        }

        private int _completedItems;
        public int CompletedItems
        {
            get { return _completedItems; }
            set
            {
                _completedItems = value;

                FireStatusChangedEvent();
            }
        }

        private int _errorCount;

        public int ErrorCount
        {
            get { return _errorCount; }
            set
            {
                _errorCount = value;
                
                FireStatusChangedEvent();
            }
        }

        public double PercentComplete
        {
            get
            {
                if (CurrentTask == string.Empty) return 0;
                if (TotalItems == 0) return -1;
                return 100*(CompletedItems/(double)TotalItems);
            }
        }

        private string _currentTask;

        public string CurrentTask
        {
            get { return _currentTask; }
            set
            {
                _currentTask = value;
                if (value != string.Empty)
                    _currentTask = value + "...";
                _completedItems = 0;

                FireStatusChangedEvent();
            }
        }

        public int Progress
        {
            get
            {
                if (TotalItems == 0) return 0;
                return Convert.ToInt32(100*(RemainingItems/(float)TotalItems));
            }
        }

        public int RemainingItems
        {
            get
            {
                if (TotalItems == 0) return 0;
                return TotalItems - CompletedItems;
            }
        }

        public string SecondaryMessage
        {
            get
            {
                if (RemainingItems != 0) return string.Format("Remaining: {0}", RemainingItems);
                return string.Empty;
            }
        }
    }
}
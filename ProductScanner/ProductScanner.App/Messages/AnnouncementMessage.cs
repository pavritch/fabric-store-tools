using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ProductScanner.App
{

    /// <summary>
    /// Kinds of announcements supported by the AnnouncementMessage type notification.
    /// </summary>
    public enum Announcement
    {

        // controls and models initialized, ready to show home page to user.
        // sent by Main.xaml.cs on view loaded event. Nav svc sinks this to kick off 
        // initial navigation to the home dashboard.
        ApplicationReady, 

        AppModelRefreshCompleted,
        EnableBackNavigation, // sent by nav svc to announce that back nav is presently allowed
        DisableBackNavigation, // sent by nav svc to announce that back nav is presently prevented (stack empty)
        RequestBackNavigation, // sent by back nav control when clicked
        RequestFlushBackNavigation, // ask to clear out the back nav stack
        ApplicationCloseBlocked, // the counter just went above 0
        ApplicationCloseUnBlocked, // the counter just went to 0
        RequestIncrementApplicationCloseBlocked, // bump the counter, if goes above 0, then close gets blocked
        RequestDecrementApplicationCloseBlocked, // down the counter, if goes to 0, then close allowed
        RequestFreezeGridColumns,
        RequestUnFreezeGridColumns,

        OneSecondIntervalTimer, // sent by App every second
    }

    class AnnouncementMessage : IMessage
    {

        public Announcement Kind { get; private set; }

        public AnnouncementMessage(Announcement kind)
        {
            this.Kind = kind;
        }
    }
}

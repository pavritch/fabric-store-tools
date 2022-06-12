using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace ControlPanel
{
    public static class MediatorMessages
    {
        public const string BeginFullScreenMode = "BeginFullScreenMode";
        public const string EndFullScreenMode = "EndFullScreenMode";

        public const string BeginNavigateToScreen = "BeginNavigateToScreen";
        public const string EndNavigateToScreen = "EndNavigateToScreen";
        public const string ApplicationLogin = "ApplicationLogin";
        public const string ApplicationLogout = "ApplicationLogout";

        public const string DisplayHomeScreen = "DisplayHomeScreen";
        public const string DisplayLoginScreen = "DisplayLoginScreen";

        // parameter is string message
        public const string SetErrorMessage = "SetErrorMessage";

        public const string ClearErrorMessage = "ClearErrorMessage";

        // parameter is string page title
        public const string SetPageTitle = "SetPageTitle";
        public const string SetPageSubTitle = "SetPageSubTitle";
    
    }
}

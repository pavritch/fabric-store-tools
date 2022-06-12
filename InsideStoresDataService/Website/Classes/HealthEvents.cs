using System;
using System.Data;
using System.Configuration;
using System.Diagnostics;
using System.Web;
using System.Web.Management;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

namespace Website
{

    /// <summary>
    /// Set of error codes logged by the health monitoring event system.
    /// </summary>
    public enum WebsiteEventCode
    {
        /// <summary>
        /// Application started up.
        /// </summary>
        ApplicationStart = WebEventCodes.WebExtendedBase + 1,
        /// <summary>
        /// Application stopping.
        /// </summary>
        ApplicationStop,
        /// <summary>
        /// Unhandled exception detected.
        /// </summary>
        UnhandledException,
        /// <summary>
        /// Invalid password or user credentials.
        /// </summary>
        AuthenticationFailure,
        /// <summary>
        /// Access to the requested page has been denied.
        /// </summary>
        PageAccessDenied,
        /// <summary>
        /// Configuration error detected.
        /// </summary>
        ConfigurationError,
        
        Notification,
    }
    
    public static class HealthEvents
    {

        /// <summary>
        /// Record a detected website configuration error in the event log. Shows using warning icon.
        /// </summary>
        /// <param name="msg">String to associate with the error.</param>
        /// <param name="sender">Object raising the event.</param>
        public static void RaiseConfigurationError(object sender, string msg)
        {
            Debug.WriteLine("Website Configuration Error: " + msg);
            new WebsiteConfigurationErrorEvent(msg, sender).Raise();
        }
        
    }
    
    public class WebsiteConfigurationErrorEvent : WebErrorEvent
    {
        public WebsiteConfigurationErrorEvent(string message, object source)
            : base(message, source, (int)WebsiteEventCode.ConfigurationError, new Exception("Configuration Error"))
        {
        }

        public override void Raise()
        {
            base.Raise();
        }

        public override void FormatCustomEventDetails(
            WebEventFormatter formatter)
        {
            //formatter.AppendLine("Button clicked at " + _time.ToString());
        }
    }
    public class WebsiteRequestErrorEvent : WebRequestErrorEvent
    {
        public WebsiteRequestErrorEvent(string message, object source, WebsiteEventCode eventCode, Exception ex)
            : base(message, source, (int)eventCode, ex)
        {
        }

        // never called, just to keep resharper happy
        public WebsiteRequestErrorEvent()
            : base(string.Empty, null, default(int), null)
        {
            throw new NotImplementedException();
        }

        public override void Raise()
        {
            base.Raise();
        }

        public override void FormatCustomEventDetails(
            WebEventFormatter formatter)
        {
            //formatter.AppendLine("Button clicked at " + _time.ToString());
        }
    }

    public class WebsiteApplicationLifetimeEvent : WebApplicationLifetimeEvent
    {

        // never called, just to keep resharper happy
        public WebsiteApplicationLifetimeEvent()
            : base(string.Empty, null, default(int))
        {
            throw new NotImplementedException();
        }
        
        public WebsiteApplicationLifetimeEvent(string message, object source, WebsiteEventCode eventCode)
            : base(message, source, (int)eventCode)
        {
        }

        public override void Raise()
        {
            base.Raise();
        }
    }

    public class WebsiteAuthenticationFailureAuditEvent : WebAuthenticationFailureAuditEvent
    {
        public WebsiteAuthenticationFailureAuditEvent(string message, object source, WebsiteEventCode eventCode, string userName)
            : base(message, source, (int)eventCode, userName)
        {
        }

        // never called, just to keep resharper happy
        public WebsiteAuthenticationFailureAuditEvent()
            : base(string.Empty, null, default(int), null)
        {
            throw new NotImplementedException();
        }
        
        public override void Raise()
        {
            base.Raise();
        }
    }

    public class WebsiteAuthorizationFailureAuditEvent : WebAuthenticationFailureAuditEvent
    {
        public WebsiteAuthorizationFailureAuditEvent(string message, object source, WebsiteEventCode eventCode, string userName)
            : base(message, source, (int)eventCode, userName)
        {
        }

        // never called, just to keep resharper happy
        public WebsiteAuthorizationFailureAuditEvent()
            : base(string.Empty, null, default(int), null)
        {
            throw new NotImplementedException();
        }
        public override void Raise()
        {
            base.Raise();
        }
    }


}

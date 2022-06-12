using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Postal;

namespace Website.Emails
{
    // instructions for using Postal templates
    // http://aboutcode.net/postal/

    public class EmailTemplate : Email
    {
        public string To { get; set; }
        public string From { get; set; }
        public string Subject { get; set; }

        public bool UsePremailer { get; set; }

        /// <summary>
        /// Set true to bypass open tracking.
        /// </summary>
        /// <remarks>
        /// Ignored when text-only email.
        /// </remarks>
        public bool SkipOpenTracking { get; set; }

        /// <summary>
        /// Set true to bypass click tracking.
        /// </summary>
        public bool SkipClickTracking { get; set; }

        /// <summary>
        /// Add a substitution coded link for unsusbscribing.
        /// </summary>
        /// <remarks>
        /// Assumes -unsubscribe- appears in the body or sendgrid template.
        /// Will craft the link as needed to hit the insidefabric unsubscribe page for the master list.
        /// </remarks>
        public bool IncludeUnsubscribeSubstitution { get; set; }

        /// <summary>
        /// The keyword to find in the text to replace with the coded unsubscribe line when feature is enabled.
        /// </summary>
        public string UnsubscribePlaceholder { get; set; }

        /// <summary>
        /// Optional category for reporting.
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Optional collection of substitutions.
        /// </summary>
        /// <remarks>
        /// Text in body or sendgrid template matching a key will have the corresponding substitution.
        /// </remarks>
        public Dictionary<string, string> Substitutions { get; set; }

        /// <summary>
        /// Optional sendgrid template identifier when using their template system.
        /// </summary>
        public string SendGridTemplateID { get; set; }

        public void PerformUnsubscribeLinkSubstitution(string placeholder)
        {
            UnsubscribePlaceholder = placeholder;
            IncludeUnsubscribeSubstitution = true;
        }

        public EmailTemplate()
        {
            UnsubscribePlaceholder = "-unsubscribe-";
        }
    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using Newtonsoft.Json;
using Website.Entities;
using Gen4;
using System.ComponentModel;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization.Formatters;
using Website.Emails;
using System.Text;

namespace Website
{

    /// <summary>
    /// For when we need access to some of the protected methods.
    /// </summary>
    public class GenericTicklerCampaignHandler : TicklerCampaignHandler
    {
        public override TicklerCampaignKind Kind
        {
            get
            {
                return TicklerCampaignKind.Generic;
            }
        }


        public GenericTicklerCampaignHandler(IWebStore store, ITicklerCampaignsManager manager) :
            base(store, manager)
        {

        }

    }
}
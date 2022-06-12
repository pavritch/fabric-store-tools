//------------------------------------------------------------------------------
// 
// Class: ControlPanelDomainContext 
//
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.ServiceModel.DomainServices.Client;
using System.Windows;

namespace Website.Services
{
    public partial class ControlPanelDomainContext
    {
        /// <summary>
        /// Opportunity to adjust the default timeout to something greater than 60 seconds.
        /// </summary>
        partial void OnCreated()
        {
            // set default timeout to 5 hrs
            ((WebDomainClient<IControlPanelDomainServiceContract>)this.DomainClient).ChannelFactory.Endpoint.Binding.SendTimeout = new TimeSpan(5, 0, 0);
        }
    }
}


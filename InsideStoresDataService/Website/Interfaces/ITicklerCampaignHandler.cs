//------------------------------------------------------------------------------
// 
// Interface: ITicklerCampaignHandler 
//
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Website.Entities;

namespace Website
{
    public interface ITicklerCampaignHandler
    {
        void WakeUp(TicklerCampaign record);
    }
}
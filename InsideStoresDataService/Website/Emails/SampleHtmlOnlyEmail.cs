using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Postal;

namespace Website.Emails
{
  public class SampleHtmlOnlyEmail : EmailTemplate
  {

    public SampleHtmlOnlyEmail()
    {
      Subject = "My SampleHtmlOnlyEmail is here. <hi>";
      From = "Inside Stores <customersupport@example.com>";
    }

  }
}
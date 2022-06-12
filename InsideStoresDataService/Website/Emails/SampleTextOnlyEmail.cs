using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Postal;

namespace Website.Emails
{
  public class SampleTextOnlyEmail : EmailTemplate
  {
    public SampleTextOnlyEmail()
    {
      Subject = "My SampleTextOnlyEmail is here.";
      From = "Inside Stores <customersupport@example.com>";
    }
  }
}
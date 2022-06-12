using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.ServiceModel.DomainServices.Hosting;
using System.ServiceModel.DomainServices.Server;
using System.Diagnostics;


namespace Website.Services
{
    public class DomainServiceBase : DomainService
    {

        #region Locals
        protected const string ErrorProcessingRequest = "Error processing request.";
        protected static object staticLockObj = new object();

        #endregion

        #region Properties
        /// <summary>
        /// The current host name in xxx.domain.com format, or whatever makes sense for the given runtime mode.
        /// </summary>
        protected string ServiceHostName
        {
            get { return MvcApplication.Current.ServiceHostName; }
        }

        #endregion


        protected bool ProcessException(Exception Ex)
        {
            if (Ex is ValidationException)
                return true;
#if DEBUG
            Debug.WriteLine("Exception: " + Ex.Message);
            return true;
#else
            throw new Exception(ErrorProcessingRequest);
#endif
        }

    }
}
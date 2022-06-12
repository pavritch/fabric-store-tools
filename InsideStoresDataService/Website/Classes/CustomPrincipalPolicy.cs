using System;
using System.Collections.Generic;
using System.Text;
using System.IdentityModel.Policy;
using System.IdentityModel.Claims;
using System.Security.Principal;
using System.IdentityModel.Tokens;
using System.Web;

namespace Website
{
    // Custom Principals and WCF
    // http://www.leastprivilege.com/CustomPrincipalsAndWCF.aspx
    // C:\Dev\Samples\WcfCustomPrincipals\CustomPrincipalService for sample project
    // also http://www.leastprivilege.com/HTTPBasicAuthenticationAgainstNonWindowsAccountsInIISASPNETPart3AddingWCFSupport.aspx
    
    /// <summary>
    /// Authorization policy for WCF.
    /// </summary>
    /// <remarks>
    /// This class is required for the principal permissions plumbing to work
    /// with WCF. Otherwise, thread current principal will not be set right
    /// and things like [PrincipalPermission] attribute will not work.
    /// Also requires that the WFC behavior be updated with to refer to this class.
    /// </remarks>
    class CustomPrincipalPolicy : IAuthorizationPolicy
    {
        // Add below snip to WFC behavior:
        //          
        //<serviceAuthorization principalPermissionMode="Custom">
        //  <authorizationPolicies>
        //    <add policyType="Gen4.DomainService.Web.CustomPrincipalPolicy, Gen4.DomainService.Web"/>
        //  </authorizationPolicies>
        //</serviceAuthorization>   
          
        Guid id = Guid.NewGuid();

        public bool Evaluate(EvaluationContext evaluationContext, ref object state)
        {
            // The way to get to the authenticated identity and the principal in the Evaluate 
            // method is a little "hidden". The evaluation context that gets passed into Evaluate
            // contains a collection named Properties. This collection has two well known keys 
            // called Identities and Principal    

            // Based on that identity you typically create your custom principal and set it back
            // to the properties collection. The WCF plumbing takes the principal from there
            // and sets it on T.CP. 

            evaluationContext.Properties["Principal"] = HttpContext.Current.User;

            return true;
        }

        /// <summary>
        /// Find identity passed in over the wire.
        /// </summary>
        /// <remarks>
        /// In our case, we're not taking in windows identities over the wire, so 
        /// will be empty list. Leaving this code in just to show how it works.
        /// </remarks>
        /// <param name="evaluationContext"></param>
        /// <returns></returns>
        private IIdentity GetClientIdentity(EvaluationContext evaluationContext)
        {
            object obj;
            if (!evaluationContext.Properties.TryGetValue("Identities", out obj))
                throw new Exception("No Identity found");

            IList<IIdentity> identities = obj as IList<IIdentity>;
            if (identities == null || identities.Count <= 0)
                throw new Exception("No Identity found");

            return identities[0];
        }

        public System.IdentityModel.Claims.ClaimSet Issuer
        {
            get { return ClaimSet.System; }
        }

        public string Id
        {
            get { return id.ToString(); }
        }
    }
}

//------------------------------------------------------------------------------
// 
// Class: WcfFaultModifier 
//
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;

namespace Website
{


    /// <summary>
    /// WCF Behavior to deal with the fact that SL3 needs to gets faults/exceptions with HTTP 200 rather
    /// than HTTP 500 per SOAP. This class just detects when faults are being passed back and modifies
    /// the HTTP response code. We're only talking to ourselves - so this is fine, even thought it
    /// technically violates the SOAP protocol of sending back 500.
    /// </summary>
    
    public class WcfFaultModifier : BehaviorExtensionElement, IEndpointBehavior
    {
        // change HTTP 500 response to HTTP 200 - so silverlight can get the details passed through.
        // Also requires behavior changes in web.config
                
        // http://msdn.microsoft.com/en-us/library/dd470096(VS.96).aspx
        // http://developers.de/blogs/damir_dobric/archive/2009/08/22/soap-faults-and-new-network-stack-in-silverlight-3.aspx    
    
        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
           WcfFaultMessageInspector inspector = new WcfFaultMessageInspector();
            endpointDispatcher.DispatchRuntime.MessageInspectors.Add(inspector);
        }


        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
        }


        public void Validate(ServiceEndpoint endpoint)
        {
        }

        public override System.Type BehaviorType
        {
            get { return typeof(WcfFaultModifier); }
        }

        protected override object CreateBehavior()
        {
            return new WcfFaultModifier();
        }
    }    
    

    public class WcfFaultMessageInspector : IDispatchMessageInspector
    {

        public void BeforeSendReply(ref Message reply, object correlationState)
        {
            if (reply.IsFault)
            {
                HttpResponseMessageProperty property = new HttpResponseMessageProperty();

                // Here the response code is changed to 200.
                property.StatusCode = System.Net.HttpStatusCode.OK;


                reply.Properties[HttpResponseMessageProperty.Name] = property;
            }
        }

        public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
        {
            // Do nothing to the incoming message.
            return null;
        }
    }
        
}
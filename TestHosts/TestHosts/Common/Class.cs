using System;
using System.Collections.ObjectModel;
using CoreWCF;
using CoreWCF.Channels;
using CoreWCF.Description;
using CoreWCF.Dispatcher;
using System.Linq;

namespace TestHosts.Common
{
    public class CorrelationIdMessageInspector : IDispatchMessageInspector
    {
        public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
        {
            if (request.Properties.TryGetValue(HttpRequestMessageProperty.Name, out Object httpRequestMessageObject)
                && httpRequestMessageObject is HttpRequestMessageProperty httpRequest)
            {
                String correlationId = httpRequest.Headers["correlationId"];

                if (!string.IsNullOrWhiteSpace(correlationId))
                {
                    using (NLog.ScopeContext.PushProperty("CorrelationId", correlationId)) {
                        // Store it globally per operation (e.g., ThreadStatic, AsyncLocal, or Logging Context)
                        NLog.ScopeContext.PushProperty("CorrelationId", correlationId);
                    }
                }
            }

            return null;
        }

        public void BeforeSendReply(ref Message reply, object correlationState)
        {
            
        }
    }

    public class CorrelationIdBehavior : IServiceBehavior
    {
        public void Validate(ServiceDescription serviceDescription,
                             ServiceHostBase serviceHostBase) {
            
        }

        public void AddBindingParameters(ServiceDescription serviceDescription,
                                         ServiceHostBase serviceHostBase,
                                         Collection<ServiceEndpoint> endpoints,
                                         BindingParameterCollection bindingParameters) {
            
        }

        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            foreach (ChannelDispatcher dispatcher in serviceHostBase.ChannelDispatchers.OfType<ChannelDispatcher>())
            {
                foreach (EndpointDispatcher endpoint in dispatcher.Endpoints)
                {
                    endpoint.DispatchRuntime.MessageInspectors.Add(new CorrelationIdMessageInspector());
                }
            }
        }
    }
}

using System.Collections.ObjectModel;
using System.Linq;
using CoreWCF;
using CoreWCF.Channels;
using CoreWCF.Description;
using CoreWCF.Dispatcher;

namespace TestHosts.Common;

public class CorrelationIdBehavior : IServiceBehavior
{
    public void Validate(ServiceDescription serviceDescription,
                         ServiceHostBase serviceHostBase) {
        // No validation needed for correlation ID behavior
    }

    public void AddBindingParameters(ServiceDescription serviceDescription,
                                     ServiceHostBase serviceHostBase,
                                     Collection<ServiceEndpoint> endpoints,
                                     BindingParameterCollection bindingParameters) {
        // No binding parameters needed for correlation ID behavior            
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
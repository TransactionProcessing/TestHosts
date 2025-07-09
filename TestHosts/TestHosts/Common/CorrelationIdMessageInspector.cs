using System;
using CoreWCF;
using CoreWCF.Channels;
using CoreWCF.Dispatcher;

namespace TestHosts.Common;

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
        // Optionally, you can add the correlation ID to the reply message headers            
    }
}
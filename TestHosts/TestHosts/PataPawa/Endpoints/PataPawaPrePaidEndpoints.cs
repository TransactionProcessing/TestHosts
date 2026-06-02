using System.Threading;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Shared.EntityFramework;
using TestHosts.PataPawa.Database;
using TestHosts.PataPawa.Handlers;

namespace TestHosts.PataPawa.Endpoints;

public static class PataPawaPrePaidEndpoints {
    public static WebApplication MapPataPawaPrepayEndpoints(this WebApplication app) {
        // Keep the endpoint mapping minimal: read the form and delegate to handler functions.
        app.MapPost("/api/patapawaprepay", async (HttpRequest req,
                                                  IDbContextResolver<PataPawaContext> contextResolver,
                                                  CancellationToken cancellationToken) => {
            IFormCollection form = await req.ReadFormAsync(cancellationToken);
            return await PrePayHandlers.SingleFunction(form, contextResolver, cancellationToken);
        });

        return app;
    }
}
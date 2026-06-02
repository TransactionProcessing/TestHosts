using System.Threading;
using Microsoft.AspNetCore.Builder;
using Shared.EntityFramework;
using TestHosts.PataPawa.Database;
using TestHosts.PataPawa.DataTransferObjects;
using TestHosts.PataPawa.DataTransferObjects.PrePay;
using TestHosts.PataPawa.Handlers;

namespace TestHosts.PataPawa.Endpoints
{
    public static class PataPawaDeveloperEndpoints {
        public static WebApplication MapPataPawaDeveloperEndpoints(this WebApplication app) {
            app.MapPost("/api/patapawa/developer/patapawaprepay/createuser", async (CreatePatapawaPrePayUser request,
                                                                                    IDbContextResolver<PataPawaContext> contextResolver,
                                                                                    CancellationToken cancellationToken) => await DeveloperEndpointHandlers.CreatePrepayUser(request, contextResolver, cancellationToken));

            app.MapPut("/api/patapawa/developer/patapawaprepay/adduserdebt", async (AddPatapawaPrePayUserDebt request,
                                                                                    IDbContextResolver<PataPawaContext> contextResolver,
                                                                                    CancellationToken cancellationToken) => await DeveloperEndpointHandlers.AddUserDebt(request, contextResolver, cancellationToken));

            app.MapPost("/api/patapawa/developer/patapawaprepay/createmeter", async (CreatePatapawaPrePayMeter request,
                                                                                     IDbContextResolver<PataPawaContext> contextResolver,
                                                                                     CancellationToken cancellationToken) => await DeveloperEndpointHandlers.CreatePrepayMeter(request, contextResolver, cancellationToken));

            app.MapPost("/api/patapawa/developer/patapawapostpay/createbill", async (CreatePataPawaPostPayBill request,
                                                                                     IDbContextResolver<PataPawaContext> contextResolver,
                                                                                     CancellationToken cancellationToken) => await DeveloperEndpointHandlers.CreateHostConfiguration(request, contextResolver, cancellationToken));

            return app;
        }
    }
}

using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TestHosts.AgencyBanking.Database;
using TestHosts.DataTransferObjects.AgencyBanking;

namespace TestHosts.AgencyBanking.Endpoints;

public static class AuthenticationEndpoints
{
    public static WebApplication MapAgencyBankingAuthenticationEndpoints(this WebApplication app) {
        app.MapPost("/api/agencybanking/auth/login",
            async (
                LoginRequest request,
                AgencyBankingDbContext db) =>
            {
                var agent = await db.Agents
                    .FirstOrDefaultAsync(x =>
                        x.AgentId == request.AgentId);

                if (agent == null)
                {
                    return Results.BadRequest(new ApiResponse
                    {
                        ResponseCode = "14",
                        ResponseMessage = "Invalid Agent"
                    });
                }

                if (agent.Pin != request.Pin)
                {
                    return Results.BadRequest(new ApiResponse
                    {
                        ResponseCode = "55",
                        ResponseMessage = "Invalid PIN"
                    });
                }

                return Results.Ok(new
                {
                    responseCode = "00",
                    token = Guid.NewGuid()
                });
            });
        return app;
    }
        
}
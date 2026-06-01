using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Shared.EntityFramework;
using TestHosts.PataPawa.Database;
using TestHosts.PataPawa.DataTransferObjects;
using TestHosts.PataPawa.DataTransferObjects.PrePay;
using TestHosts.PataPawa.Endpoints;

namespace TestHosts.PataPawa.Handlers;

public static class DeveloperEndpointHandlers
{
    public static async Task<IResult> CreatePrepayUser(CreatePatapawaPrePayUser request, IDbContextResolver<PataPawaContext> contextResolver, CancellationToken cancellationToken)
    {
        using ResolvedDbContext<PataPawaContext>? resolvedContext = contextResolver.Resolve(Constants.PataPawaReadModelConfig);

        Guid userId = Guid.NewGuid();

        PrePayUser user = await resolvedContext.Context.PrePayUsers.SingleOrDefaultAsync(u => u.UserName == request.UserName, cancellationToken);

        if (user == null)
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(userId.ToString());
            string base64String = Convert.ToBase64String(bytes);

            await resolvedContext.Context.PrePayUsers.AddAsync(new PrePayUser
            {
                Balance = 0,
                Key = base64String,
                Password = request.Password,
                UserId = userId,
                UserName = request.UserName,
            }, cancellationToken);

            await resolvedContext.Context.SaveChangesAsync(cancellationToken);
        }

        return Results.Ok();
    }

    public static async Task<IResult> AddUserDebt(AddPatapawaPrePayUserDebt request, IDbContextResolver<PataPawaContext> contextResolver, CancellationToken cancellationToken)
    {
        using ResolvedDbContext<PataPawaContext>? resolvedContext = contextResolver.Resolve(Constants.PataPawaReadModelConfig);

        PrePayUser user = await resolvedContext.Context.PrePayUsers.SingleOrDefaultAsync(u => u.UserName == request.UserName, cancellationToken);

        if (user == null)
        {
            return Results.NotFound();
        }

        user.Balance += request.DebtAmount;

        await resolvedContext.Context.SaveChangesAsync(cancellationToken);

        return Results.Ok();
    }

    public static async Task<IResult> CreatePrepayMeter(CreatePatapawaPrePayMeter request, IDbContextResolver<PataPawaContext> contextResolver, CancellationToken cancellationToken)
    {
        using ResolvedDbContext<PataPawaContext>? resolvedContext = contextResolver.Resolve(Constants.PataPawaReadModelConfig);

        Guid meterId = Guid.NewGuid();

        PrePayMeter meter = await resolvedContext.Context.PrePayMeters.SingleOrDefaultAsync(m => m.MeterNumber == request.MeterNumber, cancellationToken);

        if (meter == null)
        {
            await resolvedContext.Context.PrePayMeters.AddAsync(new PrePayMeter
            {
                MeterNumber = request.MeterNumber,
                CustomerName = request.CustomerName,
                MeterId = meterId
            }, cancellationToken);

            await resolvedContext.Context.SaveChangesAsync(cancellationToken);
        }

        return Results.Ok();
    }

    public static async Task<IResult> CreateHostConfiguration(CreatePataPawaPostPayBill request, IDbContextResolver<PataPawaContext> contextResolver, CancellationToken cancellationToken)
    {
        using ResolvedDbContext<PataPawaContext>? resolvedContext = contextResolver.Resolve(Constants.PataPawaReadModelConfig);

        Guid billIdentifier = Guid.NewGuid();

        await resolvedContext.Context.PostPaidBills.AddAsync(new PostPaidBill
        {
            Amount = request.Amount,
            AccountNumber = request.AccountNumber,
            DueDate = request.DueDate,
            AccountName = request.AccountName,
            IsFullyPaid = false,
            PostPaidBillId = billIdentifier
        }, cancellationToken);

        await resolvedContext.Context.SaveChangesAsync(cancellationToken);

        return Results.Ok(new { billIdentifier });
    }
}
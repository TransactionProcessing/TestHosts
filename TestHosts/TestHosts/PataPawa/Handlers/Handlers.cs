using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Shared.EntityFramework;
using TestHosts.PataPawa.Common;
using TestHosts.PataPawa.Database;
using TestHosts.PataPawa.DataTransferObjects;
using TestHosts.PataPawa.DataTransferObjects.PrePay;
using TestHosts.PataPawa.Factories;

namespace TestHosts.PataPawa.Handlers;

public static class PrePayHandlers {
    public static async Task<IResult> SingleFunction(IFormCollection form,
                                                     IDbContextResolver<PataPawaContext> contextResolver,
                                                     CancellationToken cancellationToken) {
        RequestType xlatedRequestType = TranslateRequestType(form["request"].ToString());

        return xlatedRequestType switch {
            RequestType.login => await HandleLoginRequest(form, contextResolver, cancellationToken),
            RequestType.meter => await HandleMeterRequest(form, contextResolver, cancellationToken),
            RequestType.vend => await HandleVendRequest(form, contextResolver, cancellationToken),
            RequestType.balance => await HandleBalanceRequest(form, contextResolver, cancellationToken),
            RequestType.lastvendfull => await HandleLastVendRequest(xlatedRequestType, form, contextResolver, cancellationToken),
            RequestType.lastvendfullfail => await HandleLastVendRequest(xlatedRequestType, form, contextResolver, cancellationToken),
            _ => Results.BadRequest($"Request type {form["request"]} not supported.")
        };
    }

    private static RequestType TranslateRequestType(string formRequest)
    {
        return formRequest switch
        {
            "login" => RequestType.login,
            "meter" => RequestType.meter,
            "vend" => RequestType.vend,
            "balance" => RequestType.balance,
            "last-vend-full" => RequestType.lastvendfull,
            "last-vend-full-fail" => RequestType.lastvendfullfail,
            _ => RequestType.unknownrequestype
        };
    }

    private static async Task<(PrePayMeter meterDetails, IResult result)> ValidateMeterDetails(string meterNumber,
                                                                                               IDbContextResolver<PataPawaContext> contextResolver,
                                                                                               CancellationToken cancellationToken)
    {
        if (meterNumber == "01234567890")
        {
            return (null, Results.Ok(new MeterResponse { status = 1, msg = "Request timed out. please fetch the response again", code = "elec100" }));
        }

        if (meterNumber == "01234567891")
        {
            return (null, Results.Ok(new MeterResponse { status = 1, msg = "Kenya Power link down, repeat the transaction after sometime", code = "elec100" }));
        }

        using ResolvedDbContext<PataPawaContext>? resolvedContext = contextResolver.Resolve(Constants.PataPawaReadModelConfig);

        PrePayMeter meterDetails = await resolvedContext.Context.PrePayMeters.SingleOrDefaultAsync(m => m.MeterNumber == meterNumber, cancellationToken);

        if (meterDetails == default)
        {
            MeterResponse errorResponse = new() { status = 1, msg = "Meter number not found" };
            return (null, Results.Ok(errorResponse));
        }

        return (meterDetails, null);
    }

    private static async Task<IResult> HandleBalanceRequest(IFormCollection requestForm,
                                                            IDbContextResolver<PataPawaContext> contextResolver,
                                                            CancellationToken cancellationToken) {
        string username = requestForm["username"].ToString();
        string key = requestForm["key"].ToString();

        using ResolvedDbContext<PataPawaContext>? resolvedContext = contextResolver.Resolve(Constants.PataPawaReadModelConfig);

        PrePayUser user = await resolvedContext.Context.PrePayUsers.SingleOrDefaultAsync(u => u.UserName == username && u.Key == key, cancellationToken);

        BalanceResponse response = new() { status = 0, msg = Responses.Success, balance = user.Balance.ToString(), };
        return Results.Ok(response);
    }

    private static async Task<IResult> HandleLastVendRequest(RequestType xlatedRequestType,
                                                             IFormCollection requestForm,
                                                             IDbContextResolver<PataPawaContext> contextResolver,
                                                             CancellationToken cancellationToken) {
        string meter = requestForm["meter"].ToString();

        (PrePayMeter meterDetails, IResult result) meterValidation = await ValidateMeterDetails(meter, contextResolver, cancellationToken);
        if (meterValidation.result != null)
            return meterValidation.result;

        using ResolvedDbContext<PataPawaContext>? resolvedContext = contextResolver.Resolve(Constants.PataPawaReadModelConfig);

        IQueryable<Database.Transaction> transactions = resolvedContext.Context.Transactions.Where(t => t.MeterNumber == meter).AsQueryable();

        transactions = xlatedRequestType switch {
            RequestType.lastvendfull => transactions.Where(t => t.Status == 0),
            _ => transactions.Where(t => t.Status == 1)
        };
        Database.Transaction transaction = await transactions.OrderByDescending(t => t.Date).SingleOrDefaultAsync(cancellationToken);

        if (transaction == null) {
            VendResponse response = new() { status = 0, msg = "Record not found" };
            return Results.Ok(response);
        }
        else {
            VendResponse response = ResponseFactory.CreateVendResponse(transaction);
            return Results.Ok(response);
        }
    }

    private static async Task<IResult> HandleLoginRequest(IFormCollection requestForm,
                                                          IDbContextResolver<PataPawaContext> contextResolver,
                                                          CancellationToken cancellationToken) {
        string username = requestForm["username"].ToString();
        string password = requestForm["password"].ToString();

        using ResolvedDbContext<PataPawaContext>? resolvedContext = contextResolver.Resolve(Constants.PataPawaReadModelConfig);

        PrePayUser user = await resolvedContext.Context.PrePayUsers.SingleOrDefaultAsync(u => u.UserName == username && u.Password == password, cancellationToken);

        if (user == default) {
            LoginResponse errorResponse = new() { status = 1, msg = "Wrong Username or Password" };
            return Results.Ok(errorResponse);
        }

        LoginResponse response = new LoginResponse {
            status = 0, msg = Responses.Success, balance = user.Balance.ToString(), key = user.Key,
        };
        return Results.Ok(response);
    }

    private static async Task<IResult> HandleMeterRequest(IFormCollection requestForm,
                                                          IDbContextResolver<PataPawaContext> contextResolver,
                                                          CancellationToken cancellationToken) {
        string meter = requestForm["meter"].ToString();

        (PrePayMeter meterDetails, IResult result) meterValidation = await ValidateMeterDetails(meter, contextResolver, cancellationToken);
        if (meterValidation.result != null)
            return meterValidation.result;

        MeterResponse response = new() { status = 0, msg = Responses.Success, customerName = meterValidation.meterDetails.CustomerName };
        return Results.Ok(response);
    }

    private static async Task<IResult> HandleVendRequest(IFormCollection requestForm,
                                                         IDbContextResolver<PataPawaContext> contextResolver,
                                                         CancellationToken cancellationToken) {
        string meter = requestForm["meter"].ToString();
        string amount = requestForm["amount"].ToString();

        (PrePayMeter meterDetails, IResult result) meterValidation = await ValidateMeterDetails(meter, contextResolver, cancellationToken);
        if (meterValidation.result != null)
            return meterValidation.result;

        using ResolvedDbContext<PataPawaContext>? resolvedContext = contextResolver.Resolve(Constants.PataPawaReadModelConfig);

        PataPawa.Database.Transaction transaction = DatabaseHelpers.CreateTransactionRecord(amount, meterValidation.meterDetails);

        await resolvedContext.Context.Transactions.AddAsync(transaction, cancellationToken);
        if (transaction.Charges != null) {
            await resolvedContext.Context.TransactionCharges.AddRangeAsync(transaction.Charges, cancellationToken);
        }

        await resolvedContext.Context.SaveChangesAsync(cancellationToken);

        VendResponse response = ResponseFactory.CreateVendResponse(transaction);

        return Results.Ok(response);
    }
}
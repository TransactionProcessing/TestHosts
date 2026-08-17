using System;
using Shared.Logger;

namespace TestHosts.AgencyBanking.Services;

public static class AgencyBankingServiceLogging
{
    public static void Started(string operation, string context = "")
    {
        Logger.LogInformation(Format(operation, "started", context));
    }

    public static void Completed(string operation, string context = "")
    {
        Logger.LogInformation(Format(operation, "completed", context));
    }

    public static void Warn(string operation, string message, string context = "")
    {
        Logger.LogWarning(Format(operation, message, context));
    }

    public static void Failed(string operation, Exception exception, string context = "")
    {
        Logger.LogError(Format(operation, "failed", context), exception);
    }

    private static string Format(string operation, string state, string context)
    {
        if (string.IsNullOrWhiteSpace(context))
        {
            return $"Agency Banking {operation} {state}.";
        }

        return $"Agency Banking {operation} {state}. {context}";
    }
}

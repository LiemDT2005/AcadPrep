using Domain.Constants;

namespace AcadPrep.WebUI.Billing;

internal static class PaywallError
{
    public static (bool RequiresPro, string Message, string? Code) Parse(string? error)
    {
        if (string.IsNullOrWhiteSpace(error))
        {
            return (false, "An error occurred.", null);
        }

        if (!BillingCodes.IsProRequired(error) && !error.Contains('|'))
        {
            return (false, error, null);
        }

        var parts = error.Split('|', 2);
        var code = parts[0];
        var message = parts.Length > 1 ? parts[1] : error;

        if (BillingCodes.IsProRequired(code) || BillingCodes.IsProRequired(error))
        {
            return (true, message, code.StartsWith(BillingCodes.ProRequiredPrefix) ? code : null);
        }

        return (false, error, null);
    }
}

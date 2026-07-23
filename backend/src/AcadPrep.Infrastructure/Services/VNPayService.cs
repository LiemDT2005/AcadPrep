using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Application.Common.Interfaces;
using Application.Common.Options;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services;

/// <summary>
/// VNPay v2.1.0 — HMAC-SHA512, amount = VND * 100.
/// Spec: https://sandbox.vnpayment.vn/apis/docs/huong-dan-tich-hop/
/// </summary>
public sealed class VNPayService : IVNPayService
{
    private readonly VNPaySettings _settings;
    private readonly TimeProvider _timeProvider;

    public VNPayService(IOptions<VNPaySettings> settings, TimeProvider timeProvider)
    {
        _settings = settings.Value;
        _timeProvider = timeProvider;
    }

    public string CreatePaymentUrl(VNPayPaymentRequest request)
    {
        if (!_settings.IsConfigured)
        {
                throw new InvalidOperationException(
                "VNPay is not configured. Set VNPay:TmnCode and VNPay:HashSecret (User Secrets / env).");
        }

        if (request.AmountVnd <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(request.AmountVnd), "Amount must be greater than 0.");
        }

        var createdAt = request.CreatedAtUtc == default
            ? _timeProvider.GetUtcNow().UtcDateTime
            : request.CreatedAtUtc;

        // VNPay yêu cầu giờ Việt Nam (UTC+7)
        var createDateVn = TimeZoneInfo.ConvertTimeFromUtc(
            DateTime.SpecifyKind(createdAt, DateTimeKind.Utc),
            GetVietnamTimeZone());

        var expireDateVn = createDateVn.AddMinutes(_settings.OrderExpiryMinutes);

        var returnUrl = ResolveAbsoluteUrl(request.ReturnUrlOverride ?? _settings.ReturnUrl);

        var parameters = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["vnp_Version"] = _settings.Version,
            ["vnp_Command"] = _settings.Command,
            ["vnp_TmnCode"] = _settings.TmnCode,
            ["vnp_Amount"] = (request.AmountVnd * 100).ToString(CultureInfo.InvariantCulture),
            ["vnp_CurrCode"] = _settings.CurrCode,
            ["vnp_TxnRef"] = request.OrderCode,
            ["vnp_OrderInfo"] = request.OrderInfo,
            ["vnp_OrderType"] = "other",
            ["vnp_Locale"] = _settings.Locale,
            ["vnp_ReturnUrl"] = returnUrl,
            ["vnp_IpAddr"] = string.IsNullOrWhiteSpace(request.ClientIp) ? "127.0.0.1" : request.ClientIp,
            ["vnp_CreateDate"] = createDateVn.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture),
            ["vnp_ExpireDate"] = expireDateVn.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture)
        };

        var signData = BuildSignData(parameters);
        var secureHash = HmacSha512(_settings.HashSecret, signData);

        var query = string.Join("&", parameters.Select(kv =>
            $"{WebUtility.UrlEncode(kv.Key)}={WebUtility.UrlEncode(kv.Value)}"));

        return $"{_settings.PaymentUrl}?{query}&vnp_SecureHash={secureHash}";
    }

    public VNPayCallbackResult ParseAndValidate(IEnumerable<KeyValuePair<string, string>> query)
    {
        var raw = query
            .Where(kv => !string.IsNullOrEmpty(kv.Key))
            .GroupBy(kv => kv.Key, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.Last().Value, StringComparer.OrdinalIgnoreCase);

        if (!raw.TryGetValue("vnp_SecureHash", out var secureHash) || string.IsNullOrWhiteSpace(secureHash))
        {
            return new VNPayCallbackResult
            {
                IsSignatureValid = false,
                IsSuccess = false,
                Message = "Missing vnp_SecureHash.",
                Raw = raw
            };
        }

        var signParams = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var (key, value) in raw)
        {
            if (key.Equals("vnp_SecureHash", StringComparison.OrdinalIgnoreCase) ||
                key.Equals("vnp_SecureHashType", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (key.StartsWith("vnp_", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(value))
            {
                signParams[key] = value;
            }
        }

        var signData = BuildSignData(signParams);
        var computed = HmacSha512(_settings.HashSecret, signData);
        var isValid = SecureEqualsHex(computed, secureHash);

        raw.TryGetValue("vnp_ResponseCode", out var responseCode);
        raw.TryGetValue("vnp_TxnRef", out var orderCode);
        raw.TryGetValue("vnp_TransactionNo", out var txnNo);
        raw.TryGetValue("vnp_BankCode", out var bankCode);
        raw.TryGetValue("vnp_Amount", out var amountRaw);

        long? amountVnd = null;
        if (long.TryParse(amountRaw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var amountScaled))
        {
            amountVnd = amountScaled / 100;
        }

        var isSuccess = isValid &&
                        string.Equals(responseCode, "00", StringComparison.Ordinal);

        return new VNPayCallbackResult
        {
            IsSignatureValid = isValid,
            IsSuccess = isSuccess,
            OrderCode = orderCode,
            ResponseCode = responseCode,
            TransactionNo = txnNo,
            BankCode = bankCode,
            AmountVnd = amountVnd,
            Message = DescribeResponseCode(responseCode),
            Raw = raw
        };
    }

    private static string BuildSignData(SortedDictionary<string, string> parameters) =>
        string.Join("&", parameters.Select(kv =>
            $"{WebUtility.UrlEncode(kv.Key)}={WebUtility.UrlEncode(kv.Value)}"));

    private static string HmacSha512(string key, string data)
    {
        var keyBytes = Encoding.UTF8.GetBytes(key);
        var dataBytes = Encoding.UTF8.GetBytes(data);
        var hash = HMACSHA512.HashData(keyBytes, dataBytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static bool SecureEqualsHex(string a, string b)
    {
        if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b))
        {
            return false;
        }

        var left = Encoding.UTF8.GetBytes(a.Trim().ToUpperInvariant());
        var right = Encoding.UTF8.GetBytes(b.Trim().ToUpperInvariant());
        if (left.Length != right.Length)
        {
            return false;
        }

        return CryptographicOperations.FixedTimeEquals(left, right);
    }

    private string ResolveAbsoluteUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return string.Empty;
        }

        if (Uri.TryCreate(url, UriKind.Absolute, out _))
        {
            return url;
        }

        // Relative path — caller (Checkout handler) should pass absolute override.
        // Keep as-is; CreatePaymentOrder will resolve via HttpContext.
        return url;
    }

    private static TimeZoneInfo GetVietnamTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");
        }
    }

    private static string DescribeResponseCode(string? code) => code switch
    {
        "00" => "Transaction successful.",
        "07" => "Debited successfully. Transaction is flagged as suspicious.",
        "09" => "Card/Account is not registered for Internet Banking.",
        "10" => "Card/Account authentication failed more than 3 times.",
        "11" => "Payment session expired.",
        "12" => "Card/Account is locked.",
        "13" => "Incorrect OTP.",
        "24" => "Customer cancelled the transaction.",
        "51" => "Insufficient account balance.",
        "65" => "Daily transaction limit exceeded.",
        "75" => "Payment bank is under maintenance.",
        "79" => "Incorrect payment password too many times.",
        "97" => "Invalid signature.",
        null or "" => "Missing response code.",
        _ => $"Transaction failed (code {code})."
    };
}

namespace Application.Common.Interfaces;

public sealed class VNPayPaymentRequest
{
    public required string OrderCode { get; init; }
    public required long AmountVnd { get; init; }
    public required string OrderInfo { get; init; }
    public required string ClientIp { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public string? ReturnUrlOverride { get; init; }
    public string? IpnUrlOverride { get; init; }
}

public sealed class VNPayCallbackResult
{
    public bool IsSignatureValid { get; init; }
    public bool IsSuccess { get; init; }
    public string? OrderCode { get; init; }
    public string? ResponseCode { get; init; }
    public string? TransactionNo { get; init; }
    public string? BankCode { get; init; }
    public long? AmountVnd { get; init; }
    public string? Message { get; init; }
    public IReadOnlyDictionary<string, string> Raw { get; init; } =
        new Dictionary<string, string>();
}

/// <summary>Tạo URL thanh toán và xác thực callback/IPN VNPay.</summary>
public interface IVNPayService
{
    string CreatePaymentUrl(VNPayPaymentRequest request);
    VNPayCallbackResult ParseAndValidate(IEnumerable<KeyValuePair<string, string>> query);
}

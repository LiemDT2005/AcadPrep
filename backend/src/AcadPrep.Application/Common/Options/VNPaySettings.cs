namespace Application.Common.Options;

/// <summary>
/// Cấu hình VNPay — bind từ section "VNPay".
/// Production: dùng https://vnpayment.vn/paymentv2/vpcpay.html
/// Sandbox: https://sandbox.vnpayment.vn/paymentv2/vpcpay.html
/// </summary>
public sealed class VNPaySettings
{
    public const string SectionName = "VNPay";

    /// <summary>Mã website tại VNPay (vnp_TmnCode).</summary>
    public string TmnCode { get; set; } = string.Empty;

    /// <summary>Chuỗi bí mật ký HMAC SHA512 (vnp_HashSecret).</summary>
    public string HashSecret { get; set; } = string.Empty;

    /// <summary>URL cổng thanh toán (sandbox hoặc production).</summary>
    public string PaymentUrl { get; set; } = "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";

    /// <summary>
    /// URL trình duyệt quay về sau thanh toán (vnp_ReturnUrl).
    /// Có thể absolute hoặc relative (sẽ resolve theo request host).
    /// </summary>
    public string ReturnUrl { get; set; } = "/Payment/Return";

    /// <summary>
    /// URL IPN server-to-server (vnp_IpnUrl). Phải public HTTPS trên production.
    /// </summary>
    public string IpnUrl { get; set; } = "/Payment/Ipn";

    public string Version { get; set; } = "2.1.0";
    public string Command { get; set; } = "pay";
    public string CurrCode { get; set; } = "VND";
    public string Locale { get; set; } = "en";

    /// <summary>Phút hết hạn đơn Pending trước khi coi là Expired.</summary>
    public int OrderExpiryMinutes { get; set; } = 15;

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(TmnCode) &&
        !string.IsNullOrWhiteSpace(HashSecret) &&
        !TmnCode.Contains("${", StringComparison.Ordinal) &&
        !HashSecret.Contains("${", StringComparison.Ordinal);
}

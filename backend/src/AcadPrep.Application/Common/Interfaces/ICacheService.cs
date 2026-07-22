namespace Application.Common.Interfaces;

/// <summary>
/// Abstraction cho Redis distributed cache.
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Lấy giá trị từ cache theo key.
    /// Trả về default(T) nếu key không tồn tại.
    /// </summary>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lưu giá trị vào cache với thời gian hết hạn tùy chọn.
    /// Mặc định cache 1 giờ nếu không truyền slidingExpiration.
    /// </summary>
    Task SetAsync<T>(string key, T value, TimeSpan? slidingExpiration = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Xóa một entry khỏi cache.
    /// </summary>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy giá trị "version" hiện tại của một nhóm cache (trả về 0 nếu chưa có).
    /// Dùng để nhúng vào cache key nhằm vô hiệu hóa hàng loạt entry mà không cần quét prefix.
    /// </summary>
    Task<long> GetVersionAsync(string versionKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Tăng "version" của một nhóm cache, khiến toàn bộ key cũ (chứa version trước đó) bị bỏ qua.
    /// </summary>
    Task BumpVersionAsync(string versionKey, CancellationToken cancellationToken = default);
}

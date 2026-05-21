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
}

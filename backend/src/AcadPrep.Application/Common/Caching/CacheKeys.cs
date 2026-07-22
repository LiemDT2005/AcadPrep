namespace AcadPrep.Application.Common.Caching;

/// <summary>
/// Tập trung các key/version dùng cho cache để tránh gõ chuỗi rời rạc.
/// </summary>
public static class CacheKeys
{
    /// <summary>
    /// Version key cho toàn bộ danh sách đề thi công khai (GetExamList).
    /// Bump key này khi có thay đổi ảnh hưởng tới danh sách (tạo, sửa, ẩn, khôi phục, đổi trạng thái).
    /// </summary>
    public const string ExamListVersion = "ExamList:version";

    /// <summary>
    /// Version key cho chi tiết một đề thi (GetExamDetail), phân theo từng đề.
    /// </summary>
    public static string ExamDetailVersion(int examId) => $"ExamDetail:version:{examId}";
}

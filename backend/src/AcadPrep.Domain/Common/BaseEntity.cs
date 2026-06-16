namespace Domain.Common;

/// <summary>
/// Base entity cho tất cả entity có PK đơn (single-column primary key).
/// Không áp dụng cho bảng junction có composite PK (AttemptAnswer, SavedVocabulary).
/// </summary>
public abstract class BaseEntity<TKey> where TKey : notnull
{
    public TKey Id { get; set; } = default!;
}

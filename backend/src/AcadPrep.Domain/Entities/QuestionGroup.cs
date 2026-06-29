using Domain.Common;
using System.Collections.Generic;

namespace Domain.Entities;

public class QuestionGroup : BaseEntity<int>
{
    public string Name { get; set; } = null!;

    // Navigation properties
    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
}

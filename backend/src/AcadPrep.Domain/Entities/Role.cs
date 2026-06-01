using System.Collections.Generic;

namespace Domain.Entities;

public class Role
{
    public int RoleId { get; set; }
    public string RoleName { get; set; } = null!;

    // Navigation properties
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}

using System;
using System.Collections.Generic;

namespace UnAd.Data.Users.Models;

public partial class User
{
    public string Id { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string RoleId { get; set; } = null!;

    public virtual Role Role { get; set; } = null!;
}







using System;
using System.Collections.Generic;

namespace CommerceBack.Entities;

public partial class User
{
    public int Id { get; set; }

    public string Email { get; set; } = null!;

    public bool IsEmailConfirmed { get; set; }

    public bool IsDisabled { get; set; }

    public bool IsLocked { get; set; }

    public int AccessAttempts { get; set; }

    public DateTime CreationDate { get; set; }

    public string Username { get; set; } = null!;

    public string Salt { get; set; } = null!;

    public string Password { get; set; } = null!;

    public int Role { get; set; }

    public string? RefreshToken { get; set; }

    public DateTime LastAccessDate { get; set; }

    public string? Firstname { get; set; }

    public string? Lastname { get; set; }

    public int Cart { get; set; }

    public virtual Cart? CartNavigation { get; set; }

    public virtual Role RoleNavigation { get; set; } = null!;
}

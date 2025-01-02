using System;
using System.Collections.Generic;

namespace CommerceBack.Entities;

public partial class Token
{
    public int Id { get; set; }

    public string Value { get; set; } = null!;

    public int TokenType { get; set; }

    public int UserId { get; set; }

    public int Status { get; set; }

    public DateTime Expiration { get; set; }

    public virtual TokenStatus StatusNavigation { get; set; } = null!;

    public virtual TokenType TokenTypeNavigation { get; set; } = null!;
}

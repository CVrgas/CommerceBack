using System;
using System.Collections.Generic;

namespace CommerceBack.Entities;

public partial class TokenStatus
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Token> Tokens { get; set; } = new List<Token>();
}

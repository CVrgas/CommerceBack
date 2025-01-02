using System;
using System.Collections.Generic;

namespace CommerceBack.Entities;

public partial class TokenType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int StatusDefault { get; set; }

    public decimal TimeSpanDefault { get; set; }

    public virtual ICollection<Token> Tokens { get; set; } = new List<Token>();
}

using System;
using System.Collections.Generic;

namespace CommerceBack.Entities;

public partial class Cart
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int Products { get; set; }

    public virtual ICollection<CartProduct> CartProducts { get; set; } = new List<CartProduct>();

    public virtual User User { get; set; } = null!;
}

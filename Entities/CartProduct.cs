using System;
using System.Collections.Generic;

namespace CommerceBack.Entities;

public partial class CartProduct
{
    public int Id { get; set; }

    public int ProductsId { get; set; }

    public int CartId { get; set; }

    public int Quantity { get; set; }

    public virtual Cart Cart { get; set; } = null!;

    public virtual Product Products { get; set; } = null!;
}

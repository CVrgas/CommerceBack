using System;
using System.Collections.Generic;

namespace CommerceBack.Entities;

public partial class Product
{
    public int Id { get; set; }

    public DateTime CreateAt { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public decimal Price { get; set; }

    public string? ImageUrl { get; set; }

    public bool IsActive { get; set; }

    public bool Disabled { get; set; }

    public decimal RatingSum { get; set; }

    public int RatingCount { get; set; }

    public decimal Rating { get; set; }

    public int Category { get; set; }

    public virtual ICollection<CartProduct> CartProducts { get; set; } = new List<CartProduct>();

    public virtual ProductCategory CategoryNavigation { get; set; } = null!;
}

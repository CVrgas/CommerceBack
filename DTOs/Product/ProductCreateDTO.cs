namespace CommerceBack.DTOs.Product;

public class ProductCreateDto
{
    public decimal Price { get; set; }
    public string Name { get; set; } = null!;
    public string? ImageUrl { get; set; }
    public string Description { get; set; } = null!;
    
    public int Category { get; set; }
}
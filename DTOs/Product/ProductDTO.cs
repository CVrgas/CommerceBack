namespace CommerceBack.DTOs.Product;

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    public decimal Rating { get; set; }
    public int RatingCount { get; set; }
}
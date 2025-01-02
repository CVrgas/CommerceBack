namespace CommerceBack.DTOs.Product;

public class ProductDto
{
    public ProductDto(){}
    public ProductDto(Entities.Product product)
    {
        this.Id = product.Id;
        this.Name = product.Name;
        this.Description = product.Description;
        this.Price = product.Price;
        this.ImageUrl = product.ImageUrl;
        this.Rating = product.Rating;
        this.RatingSum = product.RatingSum;
        this.RatingCount = product.RatingCount;
    }
    
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    public decimal Rating { get; set; }
    public decimal RatingSum { get; set; }
    public int RatingCount { get; set; }
}
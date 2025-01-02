namespace CommerceBack.DTOs.Product;

public class ProductUpdateDto
{
    public int Id { get; set; }
    public decimal? Price { get; set; }
    public string? Name { get; set; }
    public string? ImageUrl { get; set; }
    public string? Description { get; set; }
}
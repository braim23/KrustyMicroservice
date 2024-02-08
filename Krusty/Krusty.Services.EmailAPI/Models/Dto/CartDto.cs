namespace Krusty.Services.EmailAPI.Models.Dto;

public class CartDto
{
    public CartHeaderDto CartHeader { get; set; }
    public IEnumerable<CartDetailsDto>? CartDetailsDto { get; set; }
}

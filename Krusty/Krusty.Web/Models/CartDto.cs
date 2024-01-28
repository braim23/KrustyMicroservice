namespace Krusty.Web.Models;

public class CartDto
{
    public CartHeaderDto CartHeader { get; set; }
    public IEnumerable<CartDetailsDto>? CartDetailsDto { get; set; }
}

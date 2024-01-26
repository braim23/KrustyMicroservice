using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Krusty.Services.ShoppingCartAPI.Models.Dto;

namespace Krusty.Services.ShoppingCartAPI.Models;

public class CartDetails
{
    [Key]
    public int CartDetailsId { get; set; }
    public int CartHeaderId { get; set; }
    [ForeignKey("CartHeaderId")]
    public CartHeader CartHeader { get; set; }
    public int ProductId { get; set; }
    [NotMapped]
    public ProductDto ProductDto { get; set; }
    public int Count { get; set; }

}

namespace GeneX_Backend.Modules.Cart.DTOs
{
    public class AddToCartDTO
    {
        public required Guid ProductId { get; set; }
        public required int Quantity{ get; set; }
    }
}
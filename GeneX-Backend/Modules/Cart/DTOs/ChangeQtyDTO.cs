namespace GeneX_Backend.Modules.Cart.DTOs
{
    public class ChangeQtyDTO
    {
        public required Guid CartItemId { get; set; }
        public int? Quantity{ get; set; }
    }
}
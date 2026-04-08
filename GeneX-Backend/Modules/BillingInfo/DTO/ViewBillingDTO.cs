using GeneX_Backend.Shared.Enums;

namespace GeneX_Backend.Modules.BillingInfo.DTO
{
    public class ViewBillingDTO
    {
        public required Guid BillingInfoId { get; set; }
        public required string FullName { get; set; }
        public required string PhoneNUmber { get; set; }
        public required string Province { get; set; }
        public required string City { get; set; }
        public required string Address { get; set; }
        public string? LandMark { get; set; }
        public required BillingLabel Label { get; set; }
    }
}
using System.ComponentModel.DataAnnotations;
using GeneX_Backend.Modules.Users.Entities;
using GeneX_Backend.Shared.Enums;

namespace GeneX_Backend.Modules.BillingInfo.Entity
{
    public class BillingInfoEntity
    {
        [Key]
        public Guid BillingInfoId { get; set; }
        public required Guid UserId { get; set; }
        public required string FullName { get; set; }
        public required string PhoneNumber { get; set; }
        public required string Province { get; set; }
        public required string City { get; set; }
        public required string Address { get; set; }
        public string? LandMark { get; set; }
        public required BillingLabel Label { get; set; }
        public required bool IsDeleted{ get; set; }
        
        public UserEntity User { get; set; }
        
    }
}
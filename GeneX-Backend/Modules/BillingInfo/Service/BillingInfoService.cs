

using GeneX_Backend.Infrastructure.Data;
using GeneX_Backend.Modules.BillingInfo.DTO;
using GeneX_Backend.Modules.BillingInfo.Entity;
using GeneX_Backend.Modules.BillingInfo.Interface;
using GeneX_Backend.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace GeneX_Backend.Modules.BillingInfo.Service
{
    public class BillingInfoService : IBillingInfoService
    {

        private readonly AppDbContext _appDbContext;

        public BillingInfoService(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task AddBillingInfo(AddBillingDTO addBillingDTO, Guid UserId)
        {

            _appDbContext.BillingInfos.Add(
                new BillingInfoEntity
                {
                    UserId = UserId,
                    FullName = addBillingDTO.FullName,
                    PhoneNumber = addBillingDTO.PhoneNumber,
                    Province = addBillingDTO.Province,
                    City = addBillingDTO.City,
                    Address = addBillingDTO.Address,
                    LandMark = addBillingDTO.LandMark,
                    Label = addBillingDTO.Label,
                    IsDeleted = false
                }
            );

            await _appDbContext.SaveChangesAsync();
        }

        public async Task DeleteBillingInfo(Guid BillingInfoId)
        {
            BillingInfoEntity? billingInfo = await _appDbContext.BillingInfos.FirstOrDefaultAsync(bi => bi.BillingInfoId == BillingInfoId);
            if (billingInfo == null) throw new NotFoundException("Billing info not found");

            billingInfo.IsDeleted = true;
            _appDbContext.BillingInfos.Update(billingInfo);
            await _appDbContext.SaveChangesAsync();
        }

        public async Task<ViewBillingDTO> ViewBillingInfo(Guid BillingInfoId)
        {
            ViewBillingDTO? billingInfo = await _appDbContext.BillingInfos
                .Where(bi => bi.BillingInfoId == BillingInfoId && !bi.IsDeleted)
                .Select(b => new ViewBillingDTO
                {
                    BillingInfoId = b.BillingInfoId,
                    FullName = b.FullName,
                    PhoneNUmber = b.PhoneNumber,
                    Province = b.Province,
                    City = b.City,
                    Address = b.Address,
                    LandMark = b.LandMark,
                    Label = b.Label
                })
                .FirstOrDefaultAsync();

            if (billingInfo == null) throw new NotFoundException("Billing info not found");
            
            return billingInfo;
        }

        public async Task UpdateBillingInfo(AddBillingDTO addBillingDTO, Guid BillingInfoId)
        {
            BillingInfoEntity? billingInfo = await _appDbContext.BillingInfos.FirstOrDefaultAsync(bi => bi.BillingInfoId == BillingInfoId && !bi.IsDeleted);
            if (billingInfo == null) throw new NotFoundException("Billing info not found");

            billingInfo.FullName = addBillingDTO.FullName;
            billingInfo.PhoneNumber = addBillingDTO.PhoneNumber;
            billingInfo.Province = addBillingDTO.Province;
            billingInfo.City = addBillingDTO.City;
            billingInfo.Address = addBillingDTO.Address;
            billingInfo.LandMark = addBillingDTO.LandMark;
            billingInfo.Label = addBillingDTO.Label;

            _appDbContext.BillingInfos.Update(billingInfo);
            await _appDbContext.SaveChangesAsync();
        }

        public async Task<List<ViewBillingDTO>> ViewAllBillingInfo(Guid UserId)
        {
            var billingInfos = await _appDbContext.BillingInfos
                .Where(bi => bi.UserId == UserId && !bi.IsDeleted)
                .Select(b => new ViewBillingDTO
                {
                    BillingInfoId = b.BillingInfoId,
                    FullName = b.FullName,
                    PhoneNUmber = b.PhoneNumber,
                    Province = b.Province,
                    City = b.City,
                    Address = b.Address,
                    LandMark = b.LandMark,
                    Label = b.Label
                })
                .ToListAsync();

            return billingInfos;
        }

    }
}
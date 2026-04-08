using GeneX_Backend.Modules.SMS.DTO;

namespace GeneX_Backend.Modules.SMS.Interface
{
    public interface ISparrowSmsService
    {
        string SendSmsAsync(SendSMSDTO request);
    }
}

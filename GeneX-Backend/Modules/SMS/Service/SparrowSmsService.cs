using GeneX_Backend.Modules.SMS.DTO;
using GeneX_Backend.Modules.SMS.
    Interface;
using System.Collections.Specialized;
using System.Net;
using System.Text;


namespace GeneX_Backend.Modules.SMS.Services
{
    public class SparrowSmsService : ISparrowSmsService
    {
        private readonly string _token;
        private readonly string _from;

        public SparrowSmsService(IConfiguration configuration)
        {
            // Fetch token from appsettings.json or secrets
            _token = configuration["SparrowSMS:Token"]
                ?? throw new InvalidOperationException("Sparrow SMS token is not configured.");

            _from = configuration["SparrowSMS:From"]
                ?? throw new InvalidOperationException("Sparrow SMS 'from' number is not configured.");
        }

        public string SendSmsAsync(SendSMSDTO request)
        {
            using var client = new WebClient();
            var values = new NameValueCollection
            {
                ["from"] = _from,
                ["token"] = _token,
                ["to"] = request.To,
                ["text"] = request.Text
            };

            try
            {
                var response = client.UploadValues("http://api.sparrowsms.com/v2/sms/", "Post", values);
                var responseString = Encoding.Default.GetString(response);

                return responseString;
            }
            catch (Exception ex)
            {
                // Optionally log ex here or rethrow
                throw new InvalidOperationException($"Failed to send SMS: {ex.Message}", ex);
            }
        }
    }
}
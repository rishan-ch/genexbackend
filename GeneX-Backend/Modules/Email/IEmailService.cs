namespace GeneX_Backend.Modules.Email
{
    public interface IEmailService
    {
        Task SendConfirmationEmail(string Firstname, string callbackUrl, string toEmail);
        Task SendEmailAsync(string toEmail, string subject, string body);
        Task SendPassowordResetLink(string Firstname, string callbackUrl, string toEmail);
        Task SendInvoiceEmailAsync(string customerName, string toEmail, byte[] invoicePdf, string invoiceNumber, string orderNumber);
    }
}
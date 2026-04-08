using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;

namespace GeneX_Backend.Modules.Email
{
    public class EmailService : IEmailService
    {
        private readonly string _fromEmail;
        private readonly string _password;
        private readonly string _fromName = "GeneX International"; 

        public EmailService(IConfiguration configuration)
        {
            _fromEmail = configuration["EmailCredentials:Sender"];
            _password = configuration["EmailCredentials:Password"];
        }

        // user email confirmation email
        public async Task SendConfirmationEmail(string Firstname, string callbackUrl, string toEmail)
        {
            var subject = "Email Confirmation";

            var body = $@"
                <html>
                    <body>
                        <p>Hello {Firstname},</p>
                        <p>Please confirm your email address by clicking the button below:</p>
                        <a href=""{callbackUrl}"" style=""display: inline-block; padding: 10px 20px; font-size: 16px; 
                            color: #fff; background-color: #007bff; text-decoration: none; border-radius: 5px;"">
                            Confirm Email
                        </a>
                        <p>If you did not request this, please ignore this email.</p>
                        {callbackUrl}
                    </body>
                </html>";

            await SendEmailAsync(toEmail, subject, body);
        }

        // sends the email
        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(_fromName, _fromEmail)); // ✅ Added company name
            email.To.Add(MailboxAddress.Parse(toEmail));

            email.Subject = subject;
            email.Body = new TextPart(TextFormat.Html)
            {
                Text = body
            };

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_fromEmail, _password);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }

        public async Task SendPassowordResetLink(string Firstname, string callbackUrl, string toEmail)
        {
            var subject = "Reset your Genex password";
            var body = $@"
                <html>
                    <body>
                        <p>Hello {Firstname},</p>
                        <p>You recently requested to reset your password. Click the button below to proceed:</p>
                        <a href=""{callbackUrl}"" style=""display: inline-block; padding: 10px 20px; font-size: 16px; 
                            color: #fff; background-color: #dc3545; text-decoration: none; border-radius: 5px;"">
                            Reset Password
                        </a>
                        <p>If you did not request a password reset, please ignore this email or contact support.</p>
                        for testing ={callbackUrl}
                    </body>
                </html>";

            await SendEmailAsync(toEmail, subject, body);
        }

        // this email is sent after the order is placed by the user
        public async Task SendInvoiceEmailAsync(string customerName, string toEmail, byte[] invoicePdf, string invoiceNumber, string orderNumber)
        {
            var subject = $"Invoice #{invoiceNumber} - GeneX Store";

            var body = $@"
                <html>
                    <body style=""font-family: Arial, sans-serif; line-height: 1.6; color: #333;"">
                        <div style=""max-width: 600px; margin: 0 auto; padding: 20px;"">
                            <div style=""text-align: center; margin-bottom: 30px;"">
                                <h1 style=""color: #007bff; margin: 0;"">GeneX Store</h1>
                                <p style=""color: #666; margin: 5px 0;"">Premium E-commerce Experience</p>
                            </div>
                            
                            <h2 style=""color: #333; border-bottom: 2px solid #007bff; padding-bottom: 10px;"">
                                Thank you for your order!
                            </h2>
                            
                            <p>Dear {customerName},</p>
                            
                            <p>Thank you for your recent purchase with GeneX Store. Please find your invoice attached to this email.</p>
                            
                            <div style=""background-color: #f8f9fa; padding: 20px; border-radius: 8px; margin: 20px 0;"">
                                <h3 style=""margin-top: 0; color: #007bff;"">Order Details:</h3>
                                <p><strong>Invoice Number:</strong> {invoiceNumber}</p>
                                <p><strong>Order Number:</strong> {orderNumber}</p>
                                <p><strong>Invoice Date:</strong> {DateTime.UtcNow:MMM dd, yyyy}</p>
                            </div>
                            
                            <p>If you have any questions about your order or need assistance, please don't hesitate to contact our support team.</p>
                            
                            <div style=""margin-top: 40px; padding-top: 20px; border-top: 1px solid #eee; text-align: center; color: #666;"">
                                <p><strong>GeneX Store</strong></p>
                                <p>Kathmandu, Nepal</p>
                                <p>Email: support@genexstore.com | Phone: +977-01-1234567</p>
                                <p style=""font-size: 12px; margin-top: 20px;"">
                                    This is an automated email. Please do not reply to this message.
                                </p>
                            </div>
                        </div>
                    </body>
                </html>";

            await SendEmailWithAttachmentAsync(toEmail, subject, body, invoicePdf, $"Invoice-{invoiceNumber}.pdf");
        }

        private async Task SendEmailWithAttachmentAsync(string toEmail, string subject, string body, byte[] attachment, string attachmentName)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(_fromName, _fromEmail)); // ✅ Added company name
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = subject;

            // Create the multipart message
            var multipart = new Multipart("mixed");

            // Add the HTML body
            var htmlPart = new TextPart(TextFormat.Html)
            {
                Text = body
            };
            multipart.Add(htmlPart);

            // Add the PDF attachment
            var attachmentPart = new MimePart("application", "pdf")
            {
                Content = new MimeContent(new MemoryStream(attachment)),
                ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                ContentTransferEncoding = ContentEncoding.Base64,
                FileName = attachmentName
            };
            multipart.Add(attachmentPart);

            email.Body = multipart;

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_fromEmail, _password);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
    }
}

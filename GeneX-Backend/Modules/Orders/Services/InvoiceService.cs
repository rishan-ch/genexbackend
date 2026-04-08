using GeneX_Backend.Infrastructure.Data;
using GeneX_Backend.Modules.Orders.Entities;
using GeneX_Backend.Modules.Orders.Interfaces;
using GeneX_Backend.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using PuppeteerSharp;
using PuppeteerSharp.Media;

namespace GeneX_Backend.Modules.Orders.Services
{

    public class InvoiceService : IInvoiceService
    {
        private readonly AppDbContext _context;

        public InvoiceService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<string> GenerateInvoiceAsync(Guid orderId)
        {
            OrderEntity? order = await GetOrderWithDetailsAsync(orderId);
            if (order == null)
                throw new ArgumentException("Order not found", nameof(orderId));

            return GenerateInvoiceHtml(order);
        }

        public async Task<byte[]> GenerateInvoicePdfAsync(Guid orderId)
        {
            var invoiceHtml = await GenerateInvoiceAsync(orderId);
            return await GeneratePdfFromHtmlQuest(invoiceHtml);
        }

        private async Task<OrderEntity?> GetOrderWithDetailsAsync(Guid orderId)
        {
            return await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);
        }

        private async Task<byte[]> GeneratePdfFromHtmlQuest(string html)
        {
            await new BrowserFetcher().DownloadAsync();
            using var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });
            using var page = await browser.NewPageAsync();

            await page.SetContentAsync(html);

            var pdfOptions = new PdfOptions
            {
                Format = PaperFormat.A4,
                PrintBackground = true,
                MarginOptions = new MarginOptions
                {
                    Top = "20px",
                    Right = "20px",
                    Bottom = "20px",
                    Left = "20px"
                }
            };

            return await page.PdfDataAsync(pdfOptions);
        }

        private string GenerateInvoiceHtml(OrderEntity order)
        {
            var invoiceNumber = GenerateInvoiceNumber(order.OrderId);
            var discountAmount = order.AmountBeforeDiscount - order.AmountAfterDiscount;

            var html = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Invoice #{invoiceNumber}</title>
    <style>
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            margin: 0;
            padding: 20px;
            color: #333;
            background-color: #f5f5f5;
        }}
        .invoice-container {{
            max-width: 800px;
            margin: 0 auto;
            background: white;
            padding: 40px;
            border-radius: 10px;
            box-shadow: 0 0 20px rgba(0,0,0,0.1);
        }}
        .header {{
            text-align: center;
            margin-bottom: 40px;
            border-bottom: 3px solid #007bff;
            padding-bottom: 20px;
        }}
        .company-name {{
            font-size: 32px;
            font-weight: bold;
            color: #007bff;
            margin-bottom: 5px;
        }}
        .company-tagline {{
            font-size: 14px;
            color: #666;
            margin-bottom: 15px;
        }}
        .invoice-title {{
            font-size: 28px;
            color: #333;
            margin: 15px 0 10px 0;
            font-weight: 300;
        }}
        .invoice-number {{
            font-size: 16px;
            color: #007bff;
            font-weight: 600;
        }}
        .invoice-details {{
            display: flex;
            justify-content: space-between;
            margin-bottom: 40px;
        }}
        .invoice-info, .customer-info {{
            width: 48%;
            background-color: #f8f9fa;
            padding: 20px;
            border-radius: 8px;
        }}
        .info-title {{
            font-weight: bold;
            font-size: 18px;
            margin-bottom: 15px;
            color: #007bff;
            border-bottom: 2px solid #007bff;
            padding-bottom: 5px;
        }}
        .info-content {{
            line-height: 1.8;
            font-size: 14px;
        }}
        .items-table {{
            width: 100%;
            border-collapse: collapse;
            margin-bottom: 30px;
            border-radius: 8px;
            overflow: hidden;
            box-shadow: 0 2px 8px rgba(0,0,0,0.1);
        }}
        .items-table th {{
            background: linear-gradient(135deg, #007bff, #0056b3);
            color: white;
            padding: 15px 12px;
            text-align: left;
            font-weight: 600;
            font-size: 14px;
        }}
        .items-table td {{
            padding: 12px;
            border-bottom: 1px solid #eee;
            font-size: 14px;
        }}
        .items-table tbody tr:hover {{
            background-color: #f8f9fa;
        }}
        .items-table .quantity, .items-table .unit-price, .items-table .line-total {{
            text-align: right;
            font-weight: 500;
        }}
        .product-name {{
            font-weight: 600;
            color: #333;
        }}
        .totals {{
            float: right;
            width: 350px;
            background-color: #f8f9fa;
            padding: 20px;
            border-radius: 8px;
            margin-top: 20px;
        }}
        .total-row {{
            display: flex;
            justify-content: space-between;
            padding: 10px 0;
            border-bottom: 1px solid #ddd;
            font-size: 14px;
        }}
        .total-row.subtotal {{
            font-weight: 500;
        }}
        .total-row.discount {{
            color: #dc3545;
            font-weight: 500;
        }}
        .total-row.final {{
            font-weight: bold;
            font-size: 18px;
            border-bottom: 3px solid #007bff;
            color: #007bff;
            border-top: 2px solid #007bff;
            padding-top: 15px;
            margin-top: 10px;
        }}
        .footer {{
            margin-top: 50px;
            text-align: center;
            font-size: 12px;
            color: #666;
            border-top: 1px solid #eee;
            padding-top: 20px;
        }}
        .payment-info {{
            background: linear-gradient(135deg, #f8f9fa, #e9ecef);
            padding: 20px;
            border-radius: 8px;
            margin-top: 30px;
            border-left: 4px solid #007bff;
        }}
        .status {{
            display: inline-block;
            padding: 8px 16px;
            border-radius: 20px;
            font-size: 12px;
            font-weight: bold;
            text-transform: uppercase;
        }}
        .status.placed {{ background-color: #fff3cd; color: #856404; }}
        .status.dispatched {{ background-color: #d1ecf1; color: #0c5460; }}
        .status.cancelled {{ background-color: #f8d7da; color: #721c24; }}
        .company-info {{
            text-align: center;
            font-size: 12px;
            color: #666;
            margin-top: 10px;
        }}
        .clearfix {{ clear: both; }}
    </style>
</head>
<body>
    <div class='invoice-container'>
        <div class='header'>
            <div class='company-name'>GeneX Store</div>
            <div class='company-tagline'>Premium E-commerce Experience</div>
            <div class='invoice-title'>INVOICE</div>
            <div class='invoice-number'>#{invoiceNumber}</div>
            <div class='company-info'>
                Kathmandu, Nepal | support@genexstore.com | +977-01-1234567
            </div>
        </div>

        <div class='invoice-details'>
            <div class='invoice-info'>
                <div class='info-title'>Invoice Details</div>
                <div class='info-content'>
                    <strong>Invoice Date:</strong> {order.OrderDateTime.ToLocalTime():MMM dd, yyyy}<br>
                    <strong>Order ID:</strong> {order.OrderId.ToString().Substring(0, 8)}<br>
                    <strong>Payment Method:</strong> {order.PaymentMethod}<br>
                    <strong>Status:</strong> <span class='status {order.Status.ToString().ToLower()}'>{order.Status}</span>
                </div>
            </div>
            <div class='customer-info'>
                <div class='info-title'>Bill To</div>
                <div class='info-content'>
                    <strong>{order.BillingInfo.FullName}</strong><br>
                    {order.User.Email}<br>
                    {(string.IsNullOrEmpty(order.BillingInfo.PhoneNumber) ? "" : order.BillingInfo.PhoneNumber + "<br>")}{order.BillingInfo.Address}
                </div>
            </div>
        </div>

        <table class='items-table'>
            <thead>
                <tr>
                    <th>Item Description</th>
                    <th class='quantity'>Qty</th>
                    <th class='unit-price'>Unit Price</th>
                    <th class='line-total'>Total</th>
                </tr>
            </thead>
            <tbody>";

            foreach (var item in order.OrderItems)
            {
                html += $@"
                <tr>
                    <td>
                        <div class='product-name'>{item.Product.ProductName}</div>
                    </td>
                    <td class='quantity'>{item.Quantity}</td>
                    <td class='unit-price'>Rs. {item.UnitPrice:F2}</td>
                    <td class='line-total'>Rs. {item.LineTotal:F2}</td>
                </tr>";
            }

            html += $@"
            </tbody>
        </table>

        <div class='totals'>
            <div class='total-row subtotal'>
                <span>Subtotal:</span>
                <span>Rs. {order.AmountBeforeDiscount:F2}</span>
            </div>";

            if (discountAmount > 0)
            {
                html += $@"
            <div class='total-row discount'>
                <span>Discount:</span>
                <span>-Rs. {discountAmount:F2}</span>
            </div>";
            }

            html += $@"
            <div class='total-row final'>
                <span>Total Amount:</span>
                <span>Rs. {order.AmountAfterDiscount:F2}</span>
            </div>
        </div>

        <div class='clearfix'></div>

        <div class='payment-info'>
            <div class='info-title'>Payment Information</div>
            <strong>Payment Method:</strong> {order.PaymentMethod}<br>
            <strong>Payment Status:</strong> {order.Status}<br>
            <strong>Transaction Date:</strong> {order.OrderDateTime.ToLocalTime():MMM dd, yyyy HH:mm}
        </div>

        <div class='footer'>
            <p><strong>Thank you for your business!</strong></p>
            <p>For any questions regarding this invoice, please contact us at support@genexstore.com</p>
            <p>Generated on {DateTime.UtcNow:MMM dd, yyyy} at {DateTime.UtcNow:HH:mm} UTC</p>
        </div>
    </div>
</body>
</html>";

            return html;
        }

        private string GenerateInvoiceNumber(Guid orderId)
        {
            var orderIdString = orderId.ToString("N");
            var invoiceNumber = "INV-" + orderIdString.Substring(0, 8).ToUpper();
            return invoiceNumber;
        }
    }

}
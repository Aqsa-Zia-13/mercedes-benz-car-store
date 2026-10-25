using System.IO;
using iText.Kernel.Pdf;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.IO.Font.Constants;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Layout.Borders;
using Car.Models;

namespace Car.Database
{
    public static class ReceiptGenerator
    {
        // Folder beside the .exe
        private static string ReceiptsFolder
        {
            get
            {
                string folder = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory, "Receipts");
                Directory.CreateDirectory(folder);
                return folder;
            }
        }

        /// <summary>
        /// Builds PDF, saves to disk, updates DB, returns saved path.
        /// </summary>
        public static string GenerateAndSave(Order order,
            string customerEmail, string customerPhone)
        {
            // Sanitise receipt number so it is safe as a filename
            string safeName = order.ReceiptNumber
                .Replace(":", "-").Replace("/", "-").Replace("\\", "-");
            string filePath = Path.Combine(ReceiptsFolder,
                $"Receipt_{safeName}.pdf");

            try
            {
                BuildPdf(order, customerEmail, customerPhone, filePath);
            }
            catch (Exception ex)
            {
                throw new Exception("PDF generation failed: " + ex.Message, ex);
            }

            // Persist path in Orders table
            OrderRepository.SaveReceiptPath(order.OrderId, filePath);

            return filePath;
        }

        /// <summary>
        /// Opens an existing receipt PDF with the system default viewer.
        /// </summary>
        public static void OpenReceipt(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                System.Windows.MessageBox.Show(
                    "Receipt file not found.\nExpected path:\n" + filePath,
                    "File Not Found",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Warning);
                return;
            }

            System.Diagnostics.Process.Start(
                new System.Diagnostics.ProcessStartInfo(filePath)
                { UseShellExecute = true });
        }

        // ────────────────────────────────────────────────────────
        private static void BuildPdf(Order order,
            string email, string phone, string savePath)
        {
            // Delete old file if it exists to avoid write lock
            if (File.Exists(savePath))
                File.Delete(savePath);

            var writer = new PdfWriter(savePath);
            var pdfDoc = new PdfDocument(writer);
            var document = new Document(pdfDoc);

            try
            {
                var bold = PdfFontFactory.CreateFont(
                    StandardFonts.HELVETICA_BOLD);
                var regular = PdfFontFactory.CreateFont(
                    StandardFonts.HELVETICA);

                var navy = new DeviceRgb(28, 43, 74);
                var blue = new DeviceRgb(46, 109, 180);
                var gray = new DeviceRgb(120, 140, 160);
                var light = new DeviceRgb(240, 246, 255);
                var white = ColorConstants.WHITE;

                // ── TOP HEADER BAR ──────────────────────────────
                document.Add(new Paragraph("MERCEDES-BENZ STORE")
                    .SetFont(bold)
                    .SetFontSize(20)
                    .SetFontColor(white)
                    .SetBackgroundColor(navy)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetPadding(14)
                    .SetMarginBottom(4));

                document.Add(new Paragraph("OFFICIAL PAYMENT RECEIPT")
                    .SetFont(bold)
                    .SetFontSize(12)
                    .SetFontColor(blue)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetMarginBottom(16));

                // ── RECEIPT META ────────────────────────────────
                document.Add(Row(bold, regular, gray,
                    "Receipt No", order.ReceiptNumber));
                document.Add(Row(bold, regular, gray,
                    "Date", order.OrderDate
                        .ToString("dd MMMM yyyy   HH:mm")));
                document.Add(Row(bold, regular, gray,
                    "Status", order.Status));

                document.Add(new Paragraph(" ").SetMarginBottom(6));

                // ── CUSTOMER SECTION ────────────────────────────
                document.Add(SectionHead("CUSTOMER DETAILS",
                    bold, light, navy));
                document.Add(Row(bold, regular, gray,
                    "Name", order.CustomerName));
                document.Add(Row(bold, regular, gray,
                    "Email", string.IsNullOrEmpty(email)
                             ? "—" : email));
                document.Add(Row(bold, regular, gray,
                    "Phone", string.IsNullOrEmpty(phone)
                             ? "—" : phone));

                document.Add(new Paragraph(" ").SetMarginBottom(6));

                // ── VEHICLE SECTION ─────────────────────────────
                document.Add(SectionHead("VEHICLE DETAILS",
                    bold, light, navy));
                document.Add(Row(bold, regular, gray,
                    "Vehicle",
                    $"Mercedes-Benz {order.CarName}"));
                document.Add(Row(bold, regular, gray,
                    "Model", order.CarModel));
                document.Add(Row(bold, regular, gray,
                    "Year", order.CarYear.ToString()));
                document.Add(Row(bold, regular, gray,
                    "Payment", order.PaymentMethod));

                document.Add(new Paragraph(" ").SetMarginBottom(10));

                // ── TOTAL AMOUNT BOX ────────────────────────────
                document.Add(new Paragraph(
                        $"Total Paid:   PKR {order.TotalAmount:N0}")
                    .SetFont(bold)
                    .SetFontSize(15)
                    .SetFontColor(white)
                    .SetBackgroundColor(blue)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetPadding(12)
                    .SetMarginBottom(20));

                // ── FOOTER ──────────────────────────────────────
                document.Add(new Paragraph(
                    "Thank you for choosing Mercedes-Benz Store. " +
                    "This is your official payment receipt.")
                    .SetFont(regular)
                    .SetFontSize(10)
                    .SetFontColor(gray)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetMarginBottom(4));

                document.Add(new Paragraph(
                    "© 2025 Mercedes-Benz Store — Lahore, Pakistan")
                    .SetFont(regular)
                    .SetFontSize(9)
                    .SetFontColor(gray)
                    .SetTextAlignment(TextAlignment.CENTER));
            }
            finally
            {
                // Always close — even if something throws above
                document.Close();
                pdfDoc.Close();
                writer.Close();
            }
        }

        // ── HELPERS ─────────────────────────────────────────────
        private static Paragraph SectionHead(string text,
            PdfFont bold, DeviceRgb bg, DeviceRgb fg) =>
            new Paragraph(text)
                .SetFont(bold)
                .SetFontSize(10)
                .SetFontColor(fg)
                .SetBackgroundColor(bg)
                .SetPadding(5)
                .SetMarginBottom(5);

        private static Paragraph Row(PdfFont bold, PdfFont regular,
            DeviceRgb gray, string label, string value) =>
            new Paragraph()
                .Add(new Text($"{label,-12}: ")
                    .SetFont(bold).SetFontSize(11))
                .Add(new Text(value)
                    .SetFont(regular).SetFontSize(11)
                    .SetFontColor(gray))
                .SetMarginBottom(3);
    }
}
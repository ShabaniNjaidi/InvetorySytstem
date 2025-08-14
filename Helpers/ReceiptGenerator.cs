using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.draw;
using System;
using System.Collections.Generic;
using System.IO;

namespace InventorySystem.Helpers
{
    public class ReceiptItem
    {
        public string Name { get; set; }
        public int Qty { get; set; }
        public decimal Price { get; set; }
        public decimal Total => Qty * Price;
    }

    internal static class ReceiptGenerator
    {
        public static void GenerateReceipt(
      List<ReceiptItem> items,
      decimal vatRate,
      string filePath,
      string shopName,
      string tagline,
      string address,
      string contactInfo,
      string customerName = "",
      decimal discount = 0,
      decimal paid = 0,
      decimal change = 0,
      string paymentMethod = "Cash",
      string paymentRef = "",
      string watermarkText = "ISMS")
        {
            // Custom fonts and colors
            BaseColor primaryColor = new BaseColor(0, 100, 0); // Dark green
            BaseColor secondaryColor = new BaseColor(70, 130, 180); // Steel blue
            BaseColor accentColor = new BaseColor(220, 20, 60); // Crimson red

            Document doc = new Document(PageSize.A4, 25, 25, 15, 15);
            PdfWriter writer = PdfWriter.GetInstance(doc, new FileStream(filePath, FileMode.Create));
            writer.PageEvent = new WatermarkPageEvent(watermarkText);

            doc.Open();

            // Header
            var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 24, primaryColor);
            var title = new Paragraph(shopName.ToUpper(), titleFont) { Alignment = Element.ALIGN_CENTER, SpacingAfter = 5f };
            doc.Add(title);

            var taglineFont = FontFactory.GetFont(FontFactory.HELVETICA_OBLIQUE, 10, secondaryColor);
            var taglinePara = new Paragraph(tagline, taglineFont) { Alignment = Element.ALIGN_CENTER, SpacingAfter = 10f };
            doc.Add(taglinePara);

            var infoFont = FontFactory.GetFont(FontFactory.HELVETICA, 9, BaseColor.DARK_GRAY);
            doc.Add(new Paragraph(address, infoFont) { Alignment = Element.ALIGN_CENTER });
            doc.Add(new Paragraph(contactInfo, infoFont) { Alignment = Element.ALIGN_CENTER });

            doc.Add(new Chunk(new LineSeparator(1.5f, 100f, primaryColor, Element.ALIGN_CENTER, 8)));

            // Metadata
            var metaFont = FontFactory.GetFont(FontFactory.HELVETICA, 8, BaseColor.GRAY);
            doc.Add(new Paragraph($"Date: {DateTime.Now:f}", metaFont));
            doc.Add(new Paragraph($"Receipt #: {DateTime.Now.Ticks.ToString().Substring(10)}", metaFont));
            doc.Add(new Paragraph(" "));

            if (!string.IsNullOrWhiteSpace(customerName))
            {
                var customerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, BaseColor.BLACK);
                var customerPara = new Paragraph($"Customer: {customerName}", customerFont)
                {
                    Alignment = Element.ALIGN_LEFT,
                    SpacingAfter = 10f
                };
                doc.Add(customerPara);
            }

            // Items table
            PdfPTable table = new PdfPTable(4) { WidthPercentage = 100 };
            table.SetWidths(new float[] { 50, 15, 20, 15 });

            var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, BaseColor.WHITE);

            PdfPCell cell = new PdfPCell(new Phrase("ITEM DESCRIPTION", headerFont)) { BackgroundColor = primaryColor, HorizontalAlignment = Element.ALIGN_LEFT, Padding = 5, BorderWidth = 0.5f };
            table.AddCell(cell);
            cell = new PdfPCell(new Phrase("QTY", headerFont)) { BackgroundColor = primaryColor, HorizontalAlignment = Element.ALIGN_CENTER, Padding = 5, BorderWidth = 0.5f };
            table.AddCell(cell);
            cell = new PdfPCell(new Phrase("UNIT PRICE", headerFont)) { BackgroundColor = primaryColor, HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 5, BorderWidth = 0.5f };
            table.AddCell(cell);
            cell = new PdfPCell(new Phrase("AMOUNT", headerFont)) { BackgroundColor = primaryColor, HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 5, BorderWidth = 0.5f };
            table.AddCell(cell);

            var cellFont = FontFactory.GetFont(FontFactory.HELVETICA, 9);
            var altCellFont = FontFactory.GetFont(FontFactory.HELVETICA, 9, BaseColor.DARK_GRAY);

            decimal subtotal = 0;
            bool alternateRow = false;

            foreach (var item in items)
            {
                var rowColor = alternateRow ? new BaseColor(245, 245, 245) : BaseColor.WHITE;
                alternateRow = !alternateRow;

                cell = new PdfPCell(new Phrase(item.Name, cellFont)) { BackgroundColor = rowColor, Padding = 5, BorderWidth = 0.25f };
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(item.Qty.ToString(), altCellFont)) { BackgroundColor = rowColor, HorizontalAlignment = Element.ALIGN_CENTER, Padding = 5, BorderWidth = 0.25f };
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(item.Price.ToString("N0") + " TZS", altCellFont)) { BackgroundColor = rowColor, HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 5, BorderWidth = 0.25f };
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(item.Total.ToString("N0") + " TZS", cellFont)) { BackgroundColor = rowColor, HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 5, BorderWidth = 0.25f };
                table.AddCell(cell);

                subtotal += item.Total;
            }

            doc.Add(table);
            doc.Add(new Paragraph(" "));

            // Summary
            decimal vat = subtotal * vatRate;
            decimal totalBeforeDiscount = subtotal + vat;
            decimal totalAfterDiscount = totalBeforeDiscount - discount;

            PdfPTable summaryTable = new PdfPTable(2) { WidthPercentage = 50, HorizontalAlignment = Element.ALIGN_RIGHT };
            summaryTable.SetWidths(new float[] { 60, 40 });

            var summaryFont = FontFactory.GetFont(FontFactory.HELVETICA, 9);
            var summaryLabelFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9);

            // Subtotal
            cell = new PdfPCell(new Phrase("Subtotal:", summaryLabelFont)) { Border = PdfPCell.NO_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 3 };
            summaryTable.AddCell(cell);
            cell = new PdfPCell(new Phrase(subtotal.ToString("N0") + " TZS", summaryFont)) { Border = PdfPCell.NO_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 3 };
            summaryTable.AddCell(cell);

            // VAT
            cell = new PdfPCell(new Phrase($"VAT ({vatRate * 100}%):", summaryLabelFont)) { Border = PdfPCell.NO_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 3 };
            summaryTable.AddCell(cell);
            cell = new PdfPCell(new Phrase(vat.ToString("N0") + " TZS", summaryFont)) { Border = PdfPCell.NO_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 3 };
            summaryTable.AddCell(cell);

            // Discount
            if (discount > 0)
            {
                cell = new PdfPCell(new Phrase("Discount:", summaryLabelFont)) { Border = PdfPCell.NO_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 3 };
                summaryTable.AddCell(cell);
                cell = new PdfPCell(new Phrase(discount.ToString("N0") + " TZS", summaryFont)) { Border = PdfPCell.NO_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 3 };
                summaryTable.AddCell(cell);
            }

            // Total
            cell = new PdfPCell(new Phrase("TOTAL:", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, accentColor))) { Border = PdfPCell.TOP_BORDER | PdfPCell.BOTTOM_BORDER, BorderColor = BaseColor.LIGHT_GRAY, HorizontalAlignment = Element.ALIGN_RIGHT, PaddingTop = 5, PaddingBottom = 5 };
            summaryTable.AddCell(cell);
            cell = new PdfPCell(new Phrase(totalAfterDiscount.ToString("N0") + " TZS", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, accentColor))) { Border = PdfPCell.TOP_BORDER | PdfPCell.BOTTOM_BORDER, BorderColor = BaseColor.LIGHT_GRAY, HorizontalAlignment = Element.ALIGN_RIGHT, PaddingTop = 5, PaddingBottom = 5 };
            summaryTable.AddCell(cell);

            doc.Add(summaryTable);
            doc.Add(new Paragraph(" "));

            // Payment info
            string paymentText = "Payment Method: " + paymentMethod.ToUpper();
            if (!string.Equals(paymentMethod, "Cash", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(paymentRef))
            {
                paymentText += $" (Ref: {paymentRef})";
            }
            doc.Add(new Paragraph(paymentText, metaFont));
            doc.Add(new Paragraph($"Amount Paid: {paid:N0} TZS", metaFont));
            //doc.Add(new Paragraph($"Change: {change:N0} TZS", metaFont));
            doc.Add(new Paragraph(" "));

            // Footer
            doc.Add(new Chunk(new LineSeparator(0.5f, 100f, BaseColor.GRAY, Element.ALIGN_CENTER, -1)));
            var footerFont = FontFactory.GetFont(FontFactory.HELVETICA_OBLIQUE, 8, BaseColor.GRAY);
            Paragraph footer = new Paragraph();
            footer.Add(new Phrase("Thank you for trust Us \n", footerFont));
            footer.Add(new Phrase("Returns accepted within 14 days with receipt\n", footerFont));
            footer.Add(new Phrase(shopName + " - " + contactInfo, footerFont));
            footer.Alignment = Element.ALIGN_CENTER;
            doc.Add(footer);

            doc.Close();
        }

        // Watermark class
        private class WatermarkPageEvent : PdfPageEventHelper
        {
            private readonly string _watermarkText;
            public WatermarkPageEvent(string watermarkText)
            {
                _watermarkText = watermarkText;
            }

            public override void OnEndPage(PdfWriter writer, Document document)
            {
                PdfContentByte canvas = writer.DirectContentUnder;
                BaseFont font = BaseFont.CreateFont(BaseFont.HELVETICA_BOLD, BaseFont.WINANSI, BaseFont.EMBEDDED);

                canvas.SaveState();
                PdfGState graphicsState = new PdfGState
                {
                    FillOpacity = 0.1f,
                    StrokeOpacity = 0.1f
                };
                canvas.SetGState(graphicsState);
                canvas.BeginText();
                canvas.SetColorFill(BaseColor.GRAY);
                canvas.SetFontAndSize(font, 72);

                // Center the watermark both horizontally and vertically
                float x = document.PageSize.Width / 2;
                float y = document.PageSize.Height / 2;

                canvas.ShowTextAligned(Element.ALIGN_CENTER, _watermarkText, x, y, 30);
                canvas.EndText();
                canvas.RestoreState();
            }
        }
    }
}
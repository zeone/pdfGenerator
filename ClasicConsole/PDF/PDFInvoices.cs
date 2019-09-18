using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClasicConsole.DTO;
using ClasicConsole.Utils;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Shapes;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;
using PdfSharp.Pdf;

namespace ClasicConsole.PDF
{
    public static class PDFInvoices
    {
        // static readonly List<PdfContactService.ContactColumnsPositions> ContColPos = new List<PdfContactService.ContactColumnsPositions>();
        static readonly Color TableBorder = new Color(210, 210, 210);
        static readonly Color FontColor = new Color(102, 102, 102);
        /// <summary>
        /// The MigraDoc document that represents the invoice.
        /// </summary>
        //   static Document _document;
        private static PdfDocument _document = new PdfDocument();
        /// <summary>
        /// The table of the MigraDoc document that contains the invoice items.
        /// </summary>
        static Table _table;



        internal static PdfDocument CreateDocument(BillInvoiceDto bill)
        {
            //  _document = new Document { Info = { Title = "Some Name" } };


            //  DefineStyles();


            //LABELS
            CreatePage(bill);
            //  FillRows(bill);

            return _document;
        }

        private static void CreatePage(BillInvoiceDto bill)
        {
            // You always need a MigraDoc document for rendering.
            Document doc = new Document();
            Section sec = doc.AddSection();
            PdfPage page = _document.AddPage();
            //setup size to letter type
            //612 pixels 
            XUnit pdfWidth = new XUnit(216, XGraphicsUnit.Millimeter);
            //790 pixels
            XUnit pdfHeight = new XUnit(279, XGraphicsUnit.Millimeter);
            page.Height = pdfHeight;
            page.Width = pdfWidth;
            page.Orientation = PageOrientation.Portrait;
            XGraphics gfx = XGraphics.FromPdfPage(page);

            // HACK²
            gfx.MUH = PdfFontEncoding.Unicode;

            gfx.MFEH = PdfFontEmbedding.Default;
            XFont regulerFontBold = new XFont("Verdana", 9, XFontStyle.Bold);
            XFont CompanyFont = new XFont("Verdana", 20, XFontStyle.Bold);
            XFont regularFont = new XFont("Verdana", 9, XFontStyle.Regular);
            XFont smallFont = new XFont("Verdana", 7, XFontStyle.Regular);
            //Header
            //Company name
            gfx.DrawString("CompanyName", CompanyFont, XBrushes.Black,
                new XRect(30, 30, 50, 300), XStringFormats.TopLeft);
            //address
            gfx.DrawString("some address", regulerFontBold, XBrushes.Black,
                new XRect(30, 55, 30, 300), XStringFormats.TopLeft);
            //contact
            gfx.DrawString("some contacts", regulerFontBold, XBrushes.Black,
                new XRect(30, 65, 30, 300), XStringFormats.TopLeft);

            //Line
            //thin line
            DrawLine(gfx, 30, 77, 480, 77, 1);
            DrawLine(gfx, 547, 77, 582, 77, 1);
            //fat line
            DrawLine(gfx, 30, 81, 480, 81, 3);
            DrawLine(gfx, 547, 81, 582, 81, 3);
            //thin line
            DrawLine(gfx, 30, 85, 480, 85, 1);
            DrawLine(gfx, 547, 85, 582, 85, 1);
            gfx.DrawString("INVOICE", new XFont("Verdana", 13, XFontStyle.BoldItalic), XBrushes.Black,
                new XRect(481, 73, 481, 70), XStringFormats.TopLeft);


            //invoice number and dates
            //invoice number
            gfx.DrawString("Invoice No:", regularFont, XBrushes.Black,
                new XRect(30, 120, 100, 120), XStringFormats.TopLeft);
            gfx.DrawString(bill.InvoiceNo?.ToString() ?? "", regularFont, XBrushes.Black,
                new XRect(90, 120, 170, 120), XStringFormats.TopLeft);
            // invoice dates
            gfx.DrawString("Invoice Date:", regularFont, XBrushes.Black,
                new XRect(400, 120, 440, 120), XStringFormats.TopLeft);
            gfx.DrawString(bill.Date.ToShortDateString(), regularFont, XBrushes.Black,
                new XRect(470, 120, 530, 120), XStringFormats.TopLeft);
            gfx.DrawString("Hebrew:", regularFont, XBrushes.Black,
                new XRect(400, 135, 440, 135), XStringFormats.TopLeft);
            gfx.DrawString("13 Tishrei 5778", regularFont, XBrushes.Black,
                new XRect(470, 135, 530, 135), XStringFormats.TopLeft);

            Styles(ref doc);
            //bill to
            DocumentRenderer docRenderer = new DocumentRenderer(doc);

            docRenderer.PrepareDocument();
            var billToPar = sec.AddParagraph();
            billToPar.AddText("Bill To:");
            billToPar.Format.Shading.Color = Colors.Black;
            billToPar.Format.Font.Color = Colors.White;
            billToPar.Format.Font.Size = 9;
            docRenderer.RenderObject(gfx, 40, 180, 30, billToPar);
            DrawLine(gfx, 70, 180, 350, 180, 1);

            // bill address
            var addresPar = sec.AddParagraph();
            addresPar.AddText($"{bill.Family} \n" +
                              $"{bill.CompanyName}\n" +
                              $"{bill.Address} \n" +
                              $"{bill.City}, {bill.State} {bill.Zip}\n" +
                              $"{bill.Country}");
            addresPar.Format.Font.Color = Colors.Black;
            addresPar.Format.Font.Size = 9;
            docRenderer.RenderObject(gfx, 70, 190, 240, addresPar);

            _table = sec.AddTable();
            _table.Style = "Table";

            _table.Borders.Color = TableBorder;
            _table.Borders.Width = 0.25;
            _table.Borders.Left.Width = 0.5;
            _table.Borders.Right.Width = 0.5;
            _table.Borders.Top.Width = 0;
            _table.Rows.LeftIndent = 0;
            _table.Rows.HeightRule = RowHeightRule.AtLeast;
            _table.Rows.Height = 12;

            CreateColumns();
            FillRows(bill);
            docRenderer.PrepareDocument();
            docRenderer.RenderObject(gfx, 30, 282, 582, _table);

            //footer
            //text and line
            XTextFormatter tf = new XTextFormatter(gfx);
            tf.Alignment = XParagraphAlignment.Right;
            gfx.DrawString("Please include bottom portion of invoice with your payment", regularFont, XBrushes.Black,
                new XRect(30, 600, 200, 20), XStringFormats.TopLeft);
            DrawLine(gfx, 30, 615, 582, 615, 1, true);

            gfx.DrawString("Total Debits:", regularFont, XBrushes.Black,
                new XRect(400, 630, 50, 50), XStringFormats.TopLeft);
            tf.DrawString(bill.Amount.ToMoneyString(), regularFont, XBrushes.Black,
                new XRect(460, 630, 50, 50), XStringFormats.TopLeft);

            gfx.DrawString("Total Paid:", regularFont, XBrushes.Black,
                new XRect(410, 645, 50, 50), XStringFormats.TopLeft);
            tf.DrawString(GetPayd(bill).ToMoneyString(), regularFont, XBrushes.Black,
                new XRect(460, 645, 50, 50), XStringFormats.TopLeft);

            gfx.DrawString("Amount Due:", regulerFontBold, XBrushes.Black,
                new XRect(393, 665, 50, 50), XStringFormats.TopLeft);

            tf.Alignment = XParagraphAlignment.Right;
            tf.DrawString(bill.AmountDue != null ? ((decimal)bill.AmountDue).ToMoneyString() : 0M.ToMoneyString(), regulerFontBold, XBrushes.Black,
                new XRect(460, 665, 50, 50), XStringFormats.TopLeft);
            //additional invoice number
            gfx.DrawString("Invoice No:", smallFont, XBrushes.Black,
                new XRect(350, 710, 50, 50), XStringFormats.TopLeft);

            tf.Alignment = XParagraphAlignment.Left;
            tf.DrawString(bill.InvoiceNo?.ToString() ?? "", smallFont, XBrushes.Black,
                new XRect(400, 710, 50, 50), XStringFormats.TopLeft);
            //additional recipient name
            gfx.DrawString(bill.Family, smallFont, XBrushes.Black,
                new XRect(350, 720, 100, 50), XStringFormats.TopLeft);
        }

        private static void FillRows(BillInvoiceDto bill)
        {
            foreach (BillDetailsDto detail in bill.BillDetails.Where(r => r.TransType == 2))
            {
                //fill bill detail
                Row row = _table.AddRow();
                //date
                FillRow(row, ParagraphAlignment.Left, false, Colors.White, 0, detail.DateDue.ToShortDateString(), false, ParagraphAlignment.Left, VerticalAlignment.Center, fontColor: Colors.Black);
                //qty
                FillRow(row, ParagraphAlignment.Left, false, Colors.White, 1, detail.Quantity.ToString(), false, ParagraphAlignment.Left, VerticalAlignment.Center, fontColor: Colors.Black);
                //description
                FillRow(row, ParagraphAlignment.Left, false, Colors.White, 2, string.IsNullOrEmpty(detail.Note) ? "" : detail.Note, false, ParagraphAlignment.Left, VerticalAlignment.Center, fontColor: Colors.Black);
                //unitPrice
                FillRow(row, ParagraphAlignment.Left, false, Colors.White, 3, detail.UnitPrice.ToMoneyString(), false, ParagraphAlignment.Left, VerticalAlignment.Center, fontColor: Colors.Black);
                //Disc
                FillRow(row, ParagraphAlignment.Left, false, Colors.White, 4, detail.Discount.ToMoneyString(), false, ParagraphAlignment.Left, VerticalAlignment.Center, fontColor: Colors.Black);
                //Payment
                FillRow(row, ParagraphAlignment.Left, false, Colors.White, 5, "", false, ParagraphAlignment.Left, VerticalAlignment.Center, fontColor: Colors.Black);
                //Total
                FillRow(row, ParagraphAlignment.Left, false, Colors.White, 6, detail.Amount.ToMoneyString(), false, ParagraphAlignment.Left, VerticalAlignment.Center, fontColor: Colors.Black);
                AddPaymentRow(bill, detail.SubcategoryId);
            }
        }

        private static void AddPaymentRow(BillInvoiceDto bill, int detailSubcategoryId)
        {
            var payments = bill.BillDetails.Where(r => r.TransType == 1 && r.SubcategoryId == detailSubcategoryId).ToList();

            if (!payments.Any()) return;
            foreach (BillDetailsDto payment in payments)
            {
                Row row = _table.AddRow();
                //date
                FillRow(row, ParagraphAlignment.Left, false, Colors.White, 0, payment.DateDue.ToShortDateString(), false, ParagraphAlignment.Left, VerticalAlignment.Center, fontColor: Colors.Black);
                //qty
                FillRow(row, ParagraphAlignment.Left, false, Colors.White, 1,"1", false, ParagraphAlignment.Left, VerticalAlignment.Center, fontColor: Colors.Black);
                //description
                FillRow(row, ParagraphAlignment.Left, false, Colors.White, 2, string.IsNullOrEmpty(payment.Note) ? "" : payment.Note, false, ParagraphAlignment.Left, VerticalAlignment.Center, fontColor: Colors.Black);
                //unitPrice
                FillRow(row, ParagraphAlignment.Left, false, Colors.White, 3, "", false, ParagraphAlignment.Left, VerticalAlignment.Center, fontColor: Colors.Black);
                //Disc
                FillRow(row, ParagraphAlignment.Left, false, Colors.White, 4, "", false, ParagraphAlignment.Left, VerticalAlignment.Center, fontColor: Colors.Black);
                //Payment
                FillRow(row, ParagraphAlignment.Left, false, Colors.White, 5, payment.Amount.ToMoneyString(), false, ParagraphAlignment.Left, VerticalAlignment.Center, fontColor: Colors.Black);
                //Total
                FillRow(row, ParagraphAlignment.Left, false, Colors.White, 6, "", false, ParagraphAlignment.Left, VerticalAlignment.Center, fontColor: Colors.Black);
            }
           
        }

        private static void CreateColumns()
        {
            //max width 552
            //date
            AddCol(69);
            //qty
            AddCol(27.6f);
            //desc
            AddCol(193);
            //unitprice
            AddCol(69);
            //disc
            AddCol(55);
            //pay
            AddCol(69);
            //total
            AddCol(69);
            Row row = _table.AddRow();
            FillRow(row, ParagraphAlignment.Center, true, Colors.Black, 0, "Date (Due)", true, ParagraphAlignment.Center, VerticalAlignment.Center, true, Colors.White, fontSize: 9);
            FillRow(row, ParagraphAlignment.Center, true, Colors.Black, 1, "Qty", true, ParagraphAlignment.Center, VerticalAlignment.Center, true, Colors.White, fontSize: 9);
            FillRow(row, ParagraphAlignment.Center, true, Colors.Black, 2, "Description", true, ParagraphAlignment.Center, VerticalAlignment.Center, true, Colors.White, fontSize: 9);
            FillRow(row, ParagraphAlignment.Center, true, Colors.Black, 3, "Unit Price", true, ParagraphAlignment.Center, VerticalAlignment.Center, true, Colors.White, fontSize: 9);
            FillRow(row, ParagraphAlignment.Center, true, Colors.Black, 4, "Disc.", true, ParagraphAlignment.Center, VerticalAlignment.Center, true, Colors.White, fontSize: 9);
            FillRow(row, ParagraphAlignment.Center, true, Colors.Black, 5, "Payment (Cr.)", true, ParagraphAlignment.Center, VerticalAlignment.Center, true, Colors.White, fontSize: 9);
            FillRow(row, ParagraphAlignment.Center, true, Colors.Black, 6, "Total", true, ParagraphAlignment.Center, VerticalAlignment.Center, true, Colors.White, fontSize: 9);
        }

        static void FillRow(Row row, ParagraphAlignment rowParAligment, bool rowBold, Color rowBackgroundColor, int cellIndex, string colText,
            bool colBold, ParagraphAlignment colParAligment, VerticalAlignment colVerAlignment, bool rowHeadingFormat = false, Color? fontColor = null, Unit? rowHeight = null, int? fontSize = null)
        {
            row.HeadingFormat = rowHeadingFormat;
            row.Format.Alignment = rowParAligment;
            row.Format.Font.Bold = rowBold;
            if (rowHeight != null) row.Height = (float)rowHeight;
            row.Shading.Color = rowBackgroundColor;
            row.Cells[cellIndex].AddParagraph(colText);
            row.Cells[cellIndex].Format.Font.Bold = colBold;
            if (fontSize != null) row.Cells[cellIndex].Format.Font.Size = (int)fontSize;
            row.Cells[cellIndex].Format.Alignment = colParAligment;
            row.Cells[cellIndex].VerticalAlignment = colVerAlignment;
            if (fontColor != null) row.Cells[cellIndex].Format.Font.Color = (Color)fontColor;
        }

        static void AddCol(float size)
        {
            Column column = _table.AddColumn();
            column.Format.Alignment = ParagraphAlignment.Center;
            column.Width = size;
        }


        static void DrawLine(XGraphics gfx, int xStartPos, int yStartPos, int xEndPos, int yEndtPos, int lineSize, bool isDash = false)
        {
            XPen pen = new XPen(XColors.Black, lineSize);
            if (isDash) pen.DashStyle = XDashStyle.Dash;
            gfx.DrawLine(pen, xStartPos, yStartPos, xEndPos, yEndtPos);
        }

        static decimal GetPayd(BillInvoiceDto bill)
        {
            if (bill.AmountDue == null) return 0;
            return bill.Amount - (decimal)bill.AmountDue;
        }

        //private static void CreatePageOLD()
        //{

        //    _section = _document.AddSection();
        //    var pageSize = _document.DefaultPageSetup.Clone();
        //    pageSize.Orientation = Orientation.Portrait;
        //    pageSize.PageFormat = PageFormat.Letter;
        //    pageSize.PageHeight = new Unit { Millimeter = 279 };
        //    pageSize.PageWidth = new Unit { Millimeter = 216 };
        //    pageSize.LeftMargin = new Unit { Centimeter = 1.5 };
        //    pageSize.RightMargin = new Unit { Centimeter = 1.5 };
        //    pageSize.OddAndEvenPagesHeaderFooter = true;
        //    pageSize.StartingNumber = 1;
        //    _section.PageSetup = pageSize;
        //    //company name

        //    var addressFrame = _section.AddTextFrame();
        //    addressFrame.Height = "3.0cm";
        //    addressFrame.Width = "7.0cm";
        //    addressFrame.Left = ShapePosition.Left;
        //    addressFrame.RelativeHorizontal = RelativeHorizontal.Margin;
        //    addressFrame.Top = "1.0cm";
        //    addressFrame.RelativeVertical = RelativeVertical.Page;

        //    Paragraph compName = addressFrame.AddParagraph();
        //    compName.AddText("CompanyName");
        //    compName.Format.Font.Size = 22;
        //    compName.Format.Font.Bold = true;
        //    compName.Format.Font.Italic = true;
        //    compName.Format.Alignment = ParagraphAlignment.Left;
        //    //cmpany address
        //    Paragraph compAddress = addressFrame.AddParagraph();
        //    compAddress.Format.SpaceBefore = "0.3cm";
        //    compAddress.AddText("Company address \n company contacts");
        //    compAddress.Format.Font.Size = 9;
        //    compAddress.Format.Font.Bold = false;
        //    compAddress.Format.Alignment = ParagraphAlignment.Left;




        //    XGraphics dfx = _section.AddImage();
        //    DrawLine(gfx);
        //}




        static void Styles(ref Document _document)
        {
            Style style = _document.Styles["Normal"];
            // Because all styles are derived from Normal, the next line changes the 
            // font of the whole document. Or, more exactly, it changes the font of
            // all styles and paragraphs that do not redefine the font.
            style.Font.Size = 9;
            style.Font.Name = "Verdana";


            style = _document.Styles.AddStyle("Table", "Normal");
            style.Font.Name = "Verdana";
            style.Font.Name = "Times New Roman";
            style.Font.Size = 9;
            style.Font.Color = FontColor;
        }

        //static void DefineStyles()
        //{
        //    // Get the predefined style Normal.
        //    Style style = _document.Styles["Normal"];
        //    // Because all styles are derived from Normal, the next line changes the 
        //    // font of the whole document. Or, more exactly, it changes the font of
        //    // all styles and paragraphs that do not redefine the font.
        //    style.Font.Name = "Verdana";





        //    // Create a new style called Table based on style Normal
        //    style = _document.Styles.AddStyle("Table", "Normal");
        //    style.Font.Name = "Verdana";
        //    style.Font.Name = "Times New Roman";
        //    style.Font.Size = 9;
        //    style.Font.Color = FontColor;

        //    // Create a new style called Table based on style Normal
        //    style = _document.Styles.AddStyle("TableLabel", "Normal");
        //    style.Font.Name = "Verdana";
        //    style.Font.Name = "Times New Roman";
        //    style.Font.Size = 11;
        //    style.Font.Color = Color.Empty;

        //    // Create a new style called Reference based on style Normal
        //    style = _document.Styles.AddStyle("Reference", "Normal");
        //    style.ParagraphFormat.SpaceBefore = "5mm";
        //    style.ParagraphFormat.SpaceAfter = "5mm";
        //    style.ParagraphFormat.TabStops.AddTabStop("16cm", TabAlignment.Right);

        //}
    }
}

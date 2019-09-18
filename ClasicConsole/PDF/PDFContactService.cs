using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClasicConsole.DTO;
using ClasicConsole.Reports.DTO;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;
using PdfSharp.Pdf;

namespace ClasicConsole.PDF
{
    public static class PdfContactService
    {
        static readonly List<ContactColumnsPositions> ContColPos = new List<ContactColumnsPositions>();
        static readonly Color TableBorder = new Color(210, 210, 210);
        static readonly Color TableGray = new Color(242, 242, 242);
        static readonly Color TableHeaderBackground = new Color(26, 189, 156);
        static readonly Color TableHeaderFont = new Color(255, 255, 255);
        static readonly Color FontColor = new Color(102, 102, 102);
        /// <summary>
        /// The MigraDoc document that represents the invoice.
        /// </summary>
        static Document _document;

        /// <summary>
        /// The table of the MigraDoc document that contains the invoice items.
        /// </summary>
        static Table _table;

        private static int colsRows = 0;
        private static Section _section;
        static PdfDocument outputDocument = new PdfDocument();
        public static Document CreateContectReportDocument(FilterContactReport filter, List<ContactReportResultDto> contacts)
        {
            _document = new Document { Info = { Title = filter.Name } };
            var cosCount = GetColCount(filter);

            DefineStyles();
            // CreateEnvelopPage(contacts, sender);
            //Envelopes

            //LABELS
            CreateLabelPage();
            FillLabelRows(filter, contacts);
            //TABLE
            //CreateConcatcPage(filter, cosCount);
            //if (filter.Columns.Any(r => (int)r.Column >= 8 && (int)r.Column <= 12) && !string.Equals(filter.GroupBy, "None", StringComparison.InvariantCultureIgnoreCase))
            //    FillGrouped(cosCount, GroupBy(contacts, filter), filter);
            //else
            //    FillContent(contacts, filter);
            //AddBottom("Contact Report Summary", "Number of Addresses", colsRows);
            return _document;
        }

        public static PdfDocument CreateContectReportDocument(FilterContactReport filter,
            List<ContactReportResultDto> contacts, string sender)
        {
            _document = new Document();
            // DefineStyles();
            FillEnvelops(contacts, sender, filter);
            //Test();
            return outputDocument;
        }

        #region test

        static void Test()
        {
            const string text =
                "Facin exeraessisit la consenim iureet dignibh eu facilluptat vercil dunt autpat. " +
                "Ecte magna faccum dolor sequisc iliquat, quat, quipiss equipit accummy niate magna " +
                "facil iure eraesequis am velit, quat atis dolore dolent luptat nulla adio odipissectet " +
                "lan venis do essequatio conulla facillandrem zzriusci bla ad minim inis nim velit eugait " +
                "aut aut lor at ilit ut nulla ate te eugait alit augiamet ad magnim iurem il eu feuissi.\n" +
                "Guer sequis duis eu feugait luptat lum adiamet, si tate dolore mod eu facidunt adignisl in " +
                "henim dolorem nulla faccum vel inis dolutpatum iusto od min ex euis adio exer sed del " +
                "dolor ing enit veniamcon vullutat praestrud molenis ciduisim doloborem ipit nulla consequisi.\n" +
                "Nos adit pratetu eriurem delestie del ut lumsandreet nis exerilisit wis nos alit venit praestrud " +
                "dolor sum volore facidui blaor erillaortis ad ea augue corem dunt nis  iustinciduis euisi.\n" +
                "Ut ulputate volore min ut nulpute dolobor sequism olorperilit autatie modit wisl illuptat dolore " +
                "min ut in ute doloboreet ip ex et am dunt at.";



            PdfPage page = outputDocument.AddPage();
            XGraphics gfx = XGraphics.FromPdfPage(page);
            XFont font = new XFont("Times New Roman", 10, XFontStyle.Bold);
            XTextFormatter tf = new XTextFormatter(gfx);

            XRect rect = new XRect(40, 100, 250, 220);
            gfx.DrawRectangle(XBrushes.SeaShell, rect);
            //tf.Alignment = ParagraphAlignment.Left;
            tf.DrawString(text, font, XBrushes.Black, rect, XStringFormats.TopLeft);

            rect = new XRect(310, 100, 250, 220);
            gfx.DrawRectangle(XBrushes.SeaShell, rect);
            tf.Alignment = XParagraphAlignment.Right;
            tf.DrawString(text, font, XBrushes.Black, rect, XStringFormats.TopLeft);

            rect = new XRect(40, 400, 250, 220);
            gfx.DrawRectangle(XBrushes.SeaShell, rect);
            tf.Alignment = XParagraphAlignment.Center;
            tf.DrawString(text, font, XBrushes.Black, rect, XStringFormats.TopLeft);

            rect = new XRect(310, 400, 250, 220);
            gfx.DrawRectangle(XBrushes.SeaShell, rect);
            tf.Alignment = XParagraphAlignment.Justify;
            tf.DrawString(text, font, XBrushes.Black, rect, XStringFormats.TopLeft);
        }

        #endregion

        #region Envelopes

        static void FillEnvelops(List<ContactReportResultDto> contacts, string senderAddr, FilterContactReport filter)
        {


            foreach (ContactReportResultDto contact in contacts.Where(e => e.Addresses != null && e.Addresses.Any()))
            {
                foreach (ContactReportAddress addr in contact.Addresses)
                {
                    if (filter.ReportType == TransFilterType.Family)
                        AddEnvelopPage(senderAddr, contact.FamilyName, addr, filter);
                    if (filter.ReportType == TransFilterType.Member)
                        CreateEnvelopsForMembers(senderAddr, contact, addr, filter);
                }
            }
        }

        private static void CreateEnvelopsForMembers(string senderAddr, ContactReportResultDto contact, ContactReportAddress addr, FilterContactReport filter)
        {
            foreach (ContactReportMambers member in contact.Members)
            {
                AddEnvelopPage(senderAddr, (filter.SkipTitles ? "" : member.Title) + member.FirstName + " " + member.LastName, addr, filter);
            }
        }

        static void AddEnvelopPage(string senderAddr, string receipt, ContactReportAddress addr, FilterContactReport filter)
        {
            XUnit pdfWidth = new XUnit(4.125, XGraphicsUnit.Inch);
            XUnit pdfHeight = new XUnit(9.5, XGraphicsUnit.Inch);
            XFont font = new XFont("Times New Roman", 10, XFontStyle.Bold);
            outputDocument.PageLayout = PdfPageLayout.SinglePage;

            PdfPage page = outputDocument.AddPage();

            page.Height = pdfHeight;
            page.Width = pdfWidth;
            page.Orientation = PageOrientation.Landscape;
            // Get an XGraphics object for drawing

            XGraphics gfx = XGraphics.FromPdfPage(page);
            //sender

            XTextFormatter tf = new XTextFormatter(gfx);
            XRect rect = new XRect(30, 30, (pdfHeight / 2) - 30, (pdfWidth / 2) - 30);
            // gfx.DrawRectangle(XBrushes.SeaShell, rect);
            tf.Alignment = XParagraphAlignment.Left;
            tf.DrawString(senderAddr, font, XBrushes.Black, rect, XStringFormats.TopLeft);
            // recipient
            rect = new XRect((pdfHeight / 2) - 30, (pdfWidth / 2) - 30, pdfHeight, pdfWidth);
            tf.Alignment = XParagraphAlignment.Left;
            tf.DrawString($"{receipt}\n" + GetFullAddressLine(addr, filter), font, XBrushes.Black, rect, XStringFormats.TopLeft);
        }
        #endregion

        #region Labels

        static void CreateLabelPage()
        {
            // Each MigraDoc document needs at least one section.
            _section = _document.AddSection();
            //_section.PageSetup = _document.DefaultPageSetup.Clone();
            var pageSize = _document.DefaultPageSetup.Clone();

            pageSize.PageFormat = PageFormat.Letter;
            pageSize.Orientation = Orientation.Portrait;
            pageSize.LeftMargin = new Unit { Inch = 0.19 };
            pageSize.RightMargin = new Unit { Inch = 0.19 };
            pageSize.TopMargin = new Unit { Inch = 0.5 };
            pageSize.BottomMargin = new Unit { Inch = 0.5 };

            _section.PageSetup = pageSize;
            _section.PageSetup.PageWidth = new Unit { Inch = 8.5 };
            _section.PageSetup.PageHeight = new Unit { Inch = 11 };

            // Create the item table
            _table = _section.AddTable();
            _table.Style = "TableLabel";
            _table.Borders.Color = TableBorder;

            // _table.Borders.Color = Color.Empty;
            //   _table.Borders.Width = 0.25;
            //_table.Borders.Left.Width = 0.5;
            //_table.Borders.Right.Width = 0.5;
            //_table.Borders.Top.Width = new Unit { Millimeter = 3 };
            _table.Rows.LeftIndent = 0;
            _table.Rows.HeightRule = RowHeightRule.Exactly;
            _table.Rows.Height = new Unit { Inch = 1 };

            CreateLabelColumns();
            // _table.SetEdge(0, 0, 3, 1, Edge.Box, BorderStyle.Single, 0.75, Color.Empty);

        }

        private static void CreateLabelColumns()
        {
            Unit mainSize = new Unit { Inch = 2.63 };
            Unit innSize = new Unit { Inch = 0.12 };
            for (int i = 0; i < 5; i++)
            {
                Column column = _table.AddColumn();

                column.Format.Alignment = ParagraphAlignment.Center;
                column.Width = (i % 2 != 0) ? innSize : mainSize;
            }


        }

        static void FillLabelRows(FilterContactReport filter, List<ContactReportResultDto> contacts)
        {
            int countCols = 0;
            Row row = _table.AddRow();
            foreach (ContactReportResultDto contact in contacts.Where(a => a.Addresses != null && a.Addresses.Any()))
            {
                if (filter.ReportType == TransFilterType.Family)
                {
                    foreach (ContactReportAddress addr in contact.Addresses)
                    {
                        countCols++;
                        if ((countCols % 2 == 0) && countCols != 0) continue;
                        if (countCols > 5)
                        {
                            countCols = 1;
                            row = _table.AddRow();
                        }
                        var rowStr = $"{contact.FamilyName}\n{GetFullAddressLine(addr, filter)}";
                        FillRow(row, countCols - 1, rowStr,
                            false, ParagraphAlignment.Left,
                            VerticalAlignment.Top, marginLeft: new Unit { Millimeter = 3 });
                    }
                }
                if (filter.ReportType == TransFilterType.Member)
                {
                    foreach (ContactReportAddress addr in contact.Addresses)
                    {
                        foreach (ContactReportMambers member in contact.Members)
                        {
                            countCols++;
                            if ((countCols % 2 == 0) && countCols != 0) continue;
                            if (countCols > 5)
                            {
                                countCols = 1;
                                row = _table.AddRow();
                            }
                            var memberName = (filter.SkipTitles ? "" : member.Title) + member.FirstName + " " + member.LastName;
                            var rowStr = $"{memberName}\n{GetFullAddressLine(addr, filter)}";
                            FillRow(row, countCols - 1, rowStr,
                                false, ParagraphAlignment.Left,
                                VerticalAlignment.Top, marginLeft: new Unit { Millimeter = 3 });
                        }
                    }
                }
            }
        }



        #endregion



        static void DefineStyles()
        {
            // Get the predefined style Normal.
            Style style = _document.Styles["Normal"];
            // Because all styles are derived from Normal, the next line changes the 
            // font of the whole document. Or, more exactly, it changes the font of
            // all styles and paragraphs that do not redefine the font.
            style.Font.Name = "Verdana";





            // Create a new style called Table based on style Normal
            style = _document.Styles.AddStyle("Table", "Normal");
            style.Font.Name = "Verdana";
            style.Font.Name = "Times New Roman";
            style.Font.Size = 9;
            style.Font.Color = FontColor;

            // Create a new style called Table based on style Normal
            style = _document.Styles.AddStyle("TableLabel", "Normal");
            style.Font.Name = "Verdana";
            style.Font.Name = "Times New Roman";
            style.Font.Size = 11;
            style.Font.Color = Color.Empty;

            // Create a new style called Reference based on style Normal
            style = _document.Styles.AddStyle("Reference", "Normal");
            style.ParagraphFormat.SpaceBefore = "5mm";
            style.ParagraphFormat.SpaceAfter = "5mm";
            style.ParagraphFormat.TabStops.AddTabStop("16cm", TabAlignment.Right);

        }
        static int CreateColumns(FilterContactReport filter, int colsCount, Unit? sectionWidth = null)
        {
            Unit? colWidth = null;
            if (sectionWidth != null)
            {
                colWidth = sectionWidth / colsCount;
            }
            List<ReportColumn> checkedCols = new List<ReportColumn>();
            Column column = _table.AddColumn();
            //first name col
            column.Format.Alignment = ParagraphAlignment.Center;
            if (colWidth != null) column.Width = (Unit)colWidth;
            //create cols obj
            checkedCols = filter.Columns.Where(r => r.IsChecked).ToList();
            //company
            if (checkedCols.Any(e => e.Column == ReportColumns.Company))
            {
                column = _table.AddColumn();
                if (colWidth != null) column.Width = (Unit)colWidth;
                column.Format.Alignment = ParagraphAlignment.Right;
            }
            //Address
            if (checkedCols.Any(e => (int)e.Column >= 8 && (int)e.Column <= 12))
            {
                //col for address
                if (checkedCols.Any(r => r.Column == ReportColumns.Address && r.IsChecked))
                {
                    column = _table.AddColumn();
                    if (colWidth != null) column.Width = (Unit)colWidth;
                    column.Format.Alignment = ParagraphAlignment.Right;
                }
                //col for city,state,zip, country
                if (checkedCols.Any(e => (int)e.Column >= 8 && (int)e.Column <= 12 && e.IsChecked))
                {
                    column = _table.AddColumn();
                    if (colWidth != null) column.Width = (Unit)colWidth;
                    column.Format.Alignment = ParagraphAlignment.Right;
                }
            }
            //Contact data
            foreach (ReportColumn col in checkedCols.Where(e => (int)e.Column <= 7).OrderBy(e => e.Column))
            {
                column = _table.AddColumn();
                if (colWidth != null) column.Width = (Unit)colWidth;
                column.Format.Alignment = ParagraphAlignment.Right;
            }



            //fill col obj
            var startedIsnex = 1;
            Row row = _table.AddRow();
            //Name
            FillRow(row, ParagraphAlignment.Center, true, TableHeaderBackground, 0, Grouping.GetTranslation("new_report_Name"), true, ParagraphAlignment.Left, VerticalAlignment.Bottom, true, TableHeaderFont, fontSize: 11);
            //Company
            if (checkedCols.Any(e => e.Column == ReportColumns.Company))
            {
                startedIsnex++;
                var col = checkedCols.First(e => e.Column == ReportColumns.Company);
                FillRow(row, ParagraphAlignment.Center, true, TableHeaderBackground, 1,
                    Grouping.GetTranslation(Grouping.GetTranslation(col.Name)), true, ParagraphAlignment.Center, VerticalAlignment.Bottom, true, TableHeaderFont, fontSize: 11);

            }
            //Address
            if (checkedCols.Any(e => (int)e.Column >= 8 && (int)e.Column <= 12))
            {

                var colName = "";
                var colPosition = checkedCols.Any(r => r.Column == ReportColumns.Company) ? 2 : 1;
                if (checkedCols.Any(e => e.Column == ReportColumns.Address && e.IsChecked))
                {
                    colName += Grouping.GetTranslation(checkedCols.First(e => e.Column == ReportColumns.Address).Name);
                    startedIsnex++;
                    FillRow(row, ParagraphAlignment.Center, true, TableHeaderBackground, colPosition,
                          colName, true, ParagraphAlignment.Center, VerticalAlignment.Bottom, true, TableHeaderFont, fontSize: 11);
                    colPosition++;
                }
                colName = "";
                if (checkedCols.Any(e => (int)e.Column >= 8 && (int)e.Column <= 12 && e.IsChecked))
                {
                    if (checkedCols.Any(e => e.Column == ReportColumns.City))
                        colName +=
                            $"{Grouping.GetTranslation(checkedCols.First(e => e.Column == ReportColumns.City).Name)}";
                    if (checkedCols.Any(e => e.Column == ReportColumns.State))
                        colName +=
                            $"/{Grouping.GetTranslation(checkedCols.First(e => e.Column == ReportColumns.State).Name)}";
                    if (checkedCols.Any(e => e.Column == ReportColumns.Zip))
                        colName +=
                            $"/{Grouping.GetTranslation(checkedCols.First(e => e.Column == ReportColumns.Zip).Name)}";
                    if (checkedCols.Any(e => e.Column == ReportColumns.Country))
                        colName +=
                            $"/{Grouping.GetTranslation(checkedCols.First(e => e.Column == ReportColumns.Country).Name)}";
                    startedIsnex++;
                    FillRow(row, ParagraphAlignment.Center, true, TableHeaderBackground, colPosition,
                        colName, true, ParagraphAlignment.Center, VerticalAlignment.Bottom, true, TableHeaderFont,
                        fontSize: 11);
                }
            }
            //Contact data
            checkedCols = checkedCols.Where(e => (int)e.Column <= 7).OrderBy(e => e.Column).ToList();

            for (var i = 0; i < checkedCols.Count; i++)
            {
                var index = startedIsnex + i;
                ReportColumn col = checkedCols[i];
                FillRow(row, ParagraphAlignment.Center, true, TableHeaderBackground, index,
                    Grouping.GetTranslation(Grouping.GetTranslation(col.Name)), true, ParagraphAlignment.Center, VerticalAlignment.Bottom, true, TableHeaderFont, fontSize: 11);

            }

            var colCount = row.Cells.Count;


            return colCount;
        }

        static int CreateConcatcPage(FilterContactReport filter, int colCount)
        {
            // Each MigraDoc document needs at least one section.
            _section = _document.AddSection();
            //_section.PageSetup = _document.DefaultPageSetup.Clone();
            var pageSize = _document.DefaultPageSetup.Clone();
            //  pageSize.Orientation = colCount < 5 ? Orientation.Portrait : Orientation.Landscape;
            pageSize.Orientation = Orientation.Portrait;
            pageSize.PageFormat = PageFormat.A4;
            pageSize.LeftMargin = new Unit { Centimeter = 1.5 };
            pageSize.RightMargin = new Unit { Centimeter = 1.5 };
            pageSize.OddAndEvenPagesHeaderFooter = true;
            pageSize.StartingNumber = 1;
            _section.PageSetup = pageSize;
            Paragraph paragraph = _section.Headers.Primary.AddParagraph();
            paragraph.AddText(filter.Name);
            paragraph.Format.Font.Size = 9;
            paragraph.Format.Alignment = ParagraphAlignment.Center;


            // Create the item table
            _table = _section.AddTable();
            _table.Style = "Table";

            _table.Borders.Color = TableBorder;
            _table.Borders.Width = 0.25;
            _table.Borders.Left.Width = 0.5;
            _table.Borders.Right.Width = 0.5;
            _table.Rows.LeftIndent = 0;
            _table.Rows.HeightRule = RowHeightRule.AtLeast;
            _table.Rows.Height = 12;
            Unit curPageWidth = new Unit { Centimeter = pageSize.Orientation == Orientation.Landscape ? 29.7 : 21 };
            Unit? pageWidth = curPageWidth - _section.PageSetup.LeftMargin - _section.PageSetup.RightMargin;



            colCount = CreateColumns(filter, colCount, pageWidth);
            _table.SetEdge(0, 0, colCount, 1, Edge.Box, BorderStyle.Single, 0.75, Color.Empty);

            //page num
            var style = _document.Styles[StyleNames.Footer];
            style.ParagraphFormat.AddTabStop("9cm", TabAlignment.Center);
            style.Font.Name = "Verdana";
            style.Font.Name = "Times New Roman";
            style.Font.Size = 9;
            style.Font.Color = FontColor;
            // Create a paragraph with centered page number. See definition of style "Footer".
            Paragraph paragraph2 = new Paragraph();
            paragraph2.Style = StyleNames.Footer;
            paragraph2.AddTab();
            paragraph2.AddPageField();

            // Add paragraph to footer for odd pages.
            _section.Footers.Primary.Add(paragraph2);
            // Add clone of paragraph to footer for odd pages. Cloning is necessary because an object must
            // not belong to more than one other object. If you forget cloning an exception is thrown.
            _section.Footers.EvenPage.Add(paragraph2.Clone());


            return colCount;
        }

        static void AddBottom(string name, string descriptionName, int count)
        {
            Paragraph totalText = _section.AddParagraph();
            totalText.AddText($"{name} \n " +
                              $"{descriptionName} {count}");
            totalText.Format.Font.Size = 9;
            totalText.Format.Alignment = ParagraphAlignment.Left;
            totalText.Format.LeftIndent = 10;
        }
        static void FillGrouped(int colsCount, List<GroupedContactReport> grp, FilterContactReport filter)
        {
            foreach (GroupedContactReport grouped in grp)
            {
                Row rowHead = _table.AddRow();
                FillRow(rowHead, 0, grouped.Name, true, ParagraphAlignment.Left, VerticalAlignment.Center);
                rowHead.Cells[0].MergeRight = colsCount - 1;
                FillContent(grouped.Contacts, filter);
            }
        }
        static void FillContent(List<ContactReportResultDto> contacts, FilterContactReport filter)
        {
            foreach (ContactReportResultDto contact in contacts)
            {
                if (filter.ReportType == TransFilterType.Family)
                {
                    if (filter.Columns.Any(r => (int)r.Column >= 8 && (int)r.Column <= 12))
                        FillWithAddresses(filter, contact);
                    else
                    {
                        FillContRow(contact, filter, "", null);
                    }
                }
                if (filter.ReportType == TransFilterType.Member)
                {
                    if (filter.Columns.Any(r => (int)r.Column >= 8 && (int)r.Column <= 12))
                        FillWithAddresses(filter, contact);
                    else
                    {
                        foreach (ContactReportMambers member in contact.Members)
                        {
                            FillContRow(member, filter, "", null);
                        }

                    }
                }
            }
        }

        private static void FillWithAddresses(FilterContactReport filter, ContactReportResultDto contact)
        {

            if (filter.ReportType == TransFilterType.Family)
            {
                if (contact.Addresses.Any())
                {
                    foreach (ContactReportAddress address in contact.Addresses)
                    {
                        FillContRow(contact, filter, address.CompanyName, address);
                    }
                }
                else
                {
                    FillContRow(contact, filter, "", null);
                }
            }
            if (filter.ReportType == TransFilterType.Member)
            {

                foreach (ContactReportMambers member in contact.Members)
                {
                    if (contact.Addresses.Any())
                    {
                        foreach (ContactReportAddress address in contact.Addresses)
                        {
                            FillContRow(member, filter, address.CompanyName, address);
                        }
                    }
                    else
                    {
                        FillContRow(member, filter, "", null);
                    }
                }

            }


        }

        static void FillContRow(ContactReportMambers member, FilterContactReport filter, string companyName, ContactReportAddress address)
        {
            colsRows++;
            Row row = _table.AddRow();
            bool addressChecked = filter.Columns.Any(r => (int)r.Column >= 8 && (int)r.Column <= 12);
            // var startIndex = addressChecked ? 2 : 1;
            var startIndex = 1;
            //fam name
            FillRow(row, 0,
               (filter.SkipTitles ? "" : member.Title) + member.FirstName + " " + member.LastName,
               false, ParagraphAlignment.Left,
               VerticalAlignment.Center);
            //company
            if (filter.Columns.Any(r => r.Column == ReportColumns.Company))
            {
                startIndex++;
                FillRow(row, 1,
                  string.IsNullOrEmpty(companyName) ? "" : companyName,
                    false, ParagraphAlignment.Left,
                    VerticalAlignment.Center);
            }
            // addresses
            int position = filter.Columns.Any(r => r.Column == ReportColumns.Company) ? 2 : 1;
            if (filter.Columns.Any(e => e.Column == ReportColumns.Address && e.IsChecked))
            {
                FillRow(row, position, address != null ? (string.IsNullOrEmpty(address.Address) ? "" : address.Address) : "",
                    false, ParagraphAlignment.Left,
                    VerticalAlignment.Center);
                startIndex++;
                position++;
            }
            if (filter.Columns.Any(e => (int)e.Column >= 8 && (int)e.Column <= 12 && e.IsChecked))
            {
                startIndex++;
                FillRow(row, position, address != null ? GetShortAddressLine(address, filter) : "",
                    false, ParagraphAlignment.Left,
                    VerticalAlignment.Center);
            }
            FillContactsForMember(row, startIndex, member, filter);
        }

        static void FillContRow(ContactReportResultDto contact, FilterContactReport filter, string companyName, ContactReportAddress address)
        {
            colsRows++;
            Row row = _table.AddRow();

            var startIndex = 1;
            //fam name
            FillRow(row, 0, contact.FamilyName,
                false, ParagraphAlignment.Left,
                VerticalAlignment.Center);
            //company
            if (filter.Columns.Any(r => r.Column == ReportColumns.Company))
            {
                startIndex++;
                FillRow(row, 1,
                    string.IsNullOrEmpty(companyName) ? "" : companyName,
                    false, ParagraphAlignment.Left,
                    VerticalAlignment.Center);
            }
            // addresses
            int position = filter.Columns.Any(r => r.Column == ReportColumns.Company) ? 2 : 1;
            if (filter.Columns.Any(e => e.Column == ReportColumns.Address && e.IsChecked))
            {
                FillRow(row, position, address != null ? (string.IsNullOrEmpty(address.Address) ? "" : address.Address) : "",
                    false, ParagraphAlignment.Left,
                    VerticalAlignment.Center);
                startIndex++;
                position++;
            }
            if (filter.Columns.Any(e => (int)e.Column >= 8 && (int)e.Column <= 12 && e.IsChecked))
            {
                startIndex++;
                FillRow(row, position, address != null ? GetShortAddressLine(address, filter) : "",
                    false, ParagraphAlignment.Left,
                    VerticalAlignment.Center);
            }
            //contacts
            var cols = filter.Columns.Where(e => (int)e.Column >= 0 && (int)e.Column <= 7)
                .OrderBy(q => q.Column)
                .ToList();
            for (int i = 0; i < cols.Count; i++)
            {
                var index = startIndex + i;
                var col = cols[i];
                FillRow(row, index,
                    GetContactsForFamily(contact.Members.ToList(), filter, (int)col.Column), false,
                    ParagraphAlignment.Left,
                    VerticalAlignment.Center);
            }
        }

        /// <summary>
        /// return string with full address
        /// </summary>
        /// <param name="address"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        static string GetFullAddressLine(ContactReportAddress address, FilterContactReport filter)
        {
            var response = "";
            if (filter.Columns.Any(e => e.Column == ReportColumns.Company))
                response += string.IsNullOrEmpty(address.CompanyName) ? "" : address.CompanyName + "\n";
            if (filter.Columns.Any(e => e.Column == ReportColumns.Address))
                response += string.IsNullOrEmpty(address.Address) && string.IsNullOrEmpty(address.Address2) ? "" :
                    address.Address + (!string.IsNullOrEmpty(address.Address2) ? $", {address.Address2}" : "") + "\n";
            if (filter.Columns.Any(e => e.Column == ReportColumns.City))
                response += $" {address.City}";
            if (filter.Columns.Any(e => e.Column == ReportColumns.State))
                response += $" {address.State}";
            if (filter.Columns.Any(e => e.Column == ReportColumns.Zip))
                response += $" {address.Zip}";
            if (filter.Columns.Any(e => e.Column == ReportColumns.Country))
                response += $" {address.Country}";
            return response;
        }
        /// <summary>
        /// Return addres line without address
        /// </summary>
        /// <param name="address"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        static string GetShortAddressLine(ContactReportAddress address, FilterContactReport filter)
        {
            var response = "";

            if (filter.Columns.Any(e => e.Column == ReportColumns.City))
                response += $" {address.City}";
            if (filter.Columns.Any(e => e.Column == ReportColumns.State))
                response += $" {address.State}";
            if (filter.Columns.Any(e => e.Column == ReportColumns.Zip))
                response += $" {address.Zip}";
            if (filter.Columns.Any(e => e.Column == ReportColumns.Country))
                response += $" {address.Country}";
            return response;
        }

        static string GetContactsForFamily(List<ContactReportMambers> members,
            FilterContactReport filter, int colId)
        {
            return members.Aggregate("", (current, member) => current + (GetContactStringFromList(member.Contacts.Where(r => r.PhoneTypeID == colId).ToList(), filter, member.TypeID) + "\n"));
        }

        static void FillContactsForMember(Row row, int startIndex, ContactReportMambers member, FilterContactReport filter)
        {
            var cols = filter.Columns.Where(e => (int)e.Column >= 0 && (int)e.Column <= 7).OrderBy(w => w.Column).ToList();

            for (int i = 0; i < cols.Count(); i++)
            {
                var index = startIndex + i;
                var col = cols[i];
                FillRow(row, index, GetContactStringFromList(member.Contacts.Where(r => r.PhoneTypeID == (int)col.Column).ToList(), filter, member.TypeID)
                    , false, ParagraphAlignment.Left,
                    VerticalAlignment.Center);

            }

        }

        static string GetContactStringFromList(List<ContactReportContactInfo> contacts, FilterContactReport filter, int memberType)
        {
            return contacts.Aggregate("", (current, cont) => current + (GetContactLine(cont, filter, memberType) + "\n"));
        }
        static string GetContactLine(ContactReportContactInfo contact, FilterContactReport filter, int memberType)
        {
            var response = "";
            var prefix = "";
            if (memberType == 1) prefix = "His: ";
            if (memberType == 2) prefix = "Her: ";
            string isNc = contact.NoCall != null && (bool)contact.NoCall && filter.ShowNC ? "(NC)" : "";
            return $"{prefix}{isNc} {contact.PhoneNumber}";


        }
        #region primate methods
        static void FillRow(Row row, int cellIndex, string colText, bool colBold, ParagraphAlignment colParAligment, VerticalAlignment colVerAlignment,
            Color? fontColor = null, float? marginLeft = null, int? fontSize = null)
        {

            if (fontSize != null) row.Cells[cellIndex].Format.Font.Size = (int)fontSize;
            if (fontColor != null) row.Cells[cellIndex].Format.Font.Color = (Color)fontColor;
            if (marginLeft != null) row.Cells[cellIndex].Format.LeftIndent = (float)marginLeft;
            row.Cells[cellIndex].AddParagraph(colText);
            Paragraph p = new Paragraph();
            row.Cells[cellIndex].Format.Font.Bold = colBold;
            row.Cells[cellIndex].Format.Alignment = colParAligment;
            row.Cells[cellIndex].VerticalAlignment = colVerAlignment;
        }
        static void FillRow(Row row, ParagraphAlignment rowParAligment, bool rowBold, Color rowBackgroundColor, int cellIndex, string colText,
            bool colBold, ParagraphAlignment colParAligment, VerticalAlignment colVerAlignment, bool rowHeadingFormat = false, Color? fontColor = null, Unit? rowHeight = null, int? fontSize = null)
        {
            row.HeadingFormat = rowHeadingFormat;
            row.Format.Alignment = rowParAligment;
            row.Format.Font.Bold = rowBold;
            if (rowHeight != null) row.Height = (float)rowHeight;
            // if (fontColor != null) row.Format.Font.Color = (Color)fontColor;
            row.Shading.Color = rowBackgroundColor;
            row.Cells[cellIndex].AddParagraph(colText);
            row.Cells[cellIndex].Format.Font.Bold = colBold;
            if (fontSize != null) row.Cells[cellIndex].Format.Font.Size = (int)fontSize;
            row.Cells[cellIndex].Format.Alignment = colParAligment;
            row.Cells[cellIndex].VerticalAlignment = colVerAlignment;
            if (fontColor != null) row.Cells[cellIndex].Format.Font.Color = (Color)fontColor;
        }


        static int GetColCount(FilterContactReport filter)
        {
            int resp = 1;
            //contact info cols
            resp += filter.Columns.Count(r => (int)r.Column >= 0 && (int)r.Column <= 7);
            //address cols
            if (filter.Columns.Any(r => (int)r.Column >= 8 && (int)r.Column <= 12))
                resp += 2;
            if (filter.Columns.Any(r => r.Column == ReportColumns.Company))
                resp++;
            return resp;
        }
        //used for mapping col position in table
        class ContactColumnsPositions
        {
            public ReportColumns Column { get; set; }
            public int ColPosition { get; set; }
        }

        static List<GroupedContactReport> GroupBy(List<ContactReportResultDto> contacts, FilterContactReport filter)
        {
            if (filter.GroupBy == "City")
            {
                return GroupByCity(contacts);
            }

            if (filter.GroupBy == "State")
            {
                return GroupByState(contacts);
            }
            if (filter.GroupBy == "Zip")
            {
                return GroupByZip(contacts);
            }
            if (filter.GroupBy == "Country")
            {
                return GroupByCountry(contacts);
            }
            return null;
        }

        private static List<GroupedContactReport> GroupByCountry(List<ContactReportResultDto> contacts)
        {
            var countries = contacts.SelectMany(e => e.Addresses.Select(r => r.Country)).Distinct().ToList();
            var resp = new List<GroupedContactReport>();
            foreach (string cont in countries)
            {
                var grp = new GroupedContactReport();
                grp.Name = string.IsNullOrEmpty(cont) ? "No country" : cont;
                grp.Contacts = contacts
                    .Where(e => e.Addresses.Any(
                        w => string.Equals(w.Country, cont, StringComparison.InvariantCultureIgnoreCase))).Select(t =>
                    {
                        return new ContactReportResultDto()
                        {
                            Addresses = null,
                            Members = t.Members.Select(r => (ContactReportMambers)r.Clone()).ToList(),
                            FamilyID = t.FamilyID,
                            FamilyName = t.FamilyName,
                            FamilySalutation = t.FamilySalutation,
                            HisSalutation = t.HisSalutation,
                            HerSalutation = t.HerSalutation,
                            FamilyLabel = t.FamilyLabel,
                            HisLabel = t.HisLabel,
                            HerLabel = t.HerLabel
                        };
                    })
                    .ToList();

                // choose only addreses with current city
                foreach (ContactReportResultDto contact in grp.Contacts)
                {
                    contact.Addresses = contacts.Where(t => t.FamilyID == contact.FamilyID).SelectMany(r => r.Addresses
                            .Where(e => string.Equals(e.Country, cont, StringComparison.InvariantCultureIgnoreCase)))
                        .ToList();
                }
                resp.Add(grp);
            }
            return resp;
        }

        private static List<GroupedContactReport> GroupByZip(List<ContactReportResultDto> contacts)
        {
            var zips = contacts.SelectMany(e => e.Addresses.Select(r => r.Zip)).Distinct().ToList();
            var resp = new List<GroupedContactReport>();
            foreach (string zip in zips)
            {
                var grp = new GroupedContactReport();
                grp.Name = string.IsNullOrEmpty(zip) ? "No zip" : zip;
                grp.Contacts = contacts
                    .Where(e => e.Addresses.Any(
                        w => string.Equals(w.Zip, zip, StringComparison.InvariantCultureIgnoreCase))).Select(t =>
                    {
                        return new ContactReportResultDto()
                        {
                            Addresses = null,
                            Members = t.Members.Select(r => (ContactReportMambers)r.Clone()).ToList(),
                            FamilyID = t.FamilyID,
                            FamilyName = t.FamilyName,
                            FamilySalutation = t.FamilySalutation,
                            HisSalutation = t.HisSalutation,
                            HerSalutation = t.HerSalutation,
                            FamilyLabel = t.FamilyLabel,
                            HisLabel = t.HisLabel,
                            HerLabel = t.HerLabel
                        };
                    })
                    .ToList();

                // choose only addreses with current city
                foreach (ContactReportResultDto contact in grp.Contacts)
                {
                    contact.Addresses = contacts.Where(t => t.FamilyID == contact.FamilyID).SelectMany(r => r.Addresses
                               .Where(e => string.Equals(e.Zip, zip, StringComparison.InvariantCultureIgnoreCase)))
                        .ToList();
                }
                resp.Add(grp);
            }
            return resp;
        }

        private static List<GroupedContactReport> GroupByState(List<ContactReportResultDto> contacts)
        {
            var states = contacts.SelectMany(e => e.Addresses.Select(r => r.State)).Distinct().ToList();
            var resp = new List<GroupedContactReport>();
            foreach (string state in states)
            {
                var grp = new GroupedContactReport();
                grp.Name = string.IsNullOrEmpty(state) ? "No state" : state;
                grp.Contacts = contacts
                    .Where(e => e.Addresses.Any(
                        w => string.Equals(w.State, state, StringComparison.InvariantCultureIgnoreCase))).Select(t =>
                    {
                        return new ContactReportResultDto()
                        {
                            Addresses = null,
                            Members = t.Members.Select(r => (ContactReportMambers)r.Clone()).ToList(),
                            FamilyID = t.FamilyID,
                            FamilyName = t.FamilyName,
                            FamilySalutation = t.FamilySalutation,
                            HisSalutation = t.HisSalutation,
                            HerSalutation = t.HerSalutation,
                            FamilyLabel = t.FamilyLabel,
                            HisLabel = t.HisLabel,
                            HerLabel = t.HerLabel
                        };
                    })
                    .ToList();

                // choose only addreses with current city
                foreach (ContactReportResultDto contact in grp.Contacts)
                {
                    contact.Addresses = contacts.Where(t => t.FamilyID == contact.FamilyID).SelectMany(r => r.Addresses
                            .Where(e => string.Equals(e.State, state, StringComparison.InvariantCultureIgnoreCase)))
                        .ToList();
                }
                resp.Add(grp);
            }
            return resp;
        }

        static List<GroupedContactReport> GroupByCity(List<ContactReportResultDto> contacts)
        {
            var cities = contacts.SelectMany(e => e.Addresses.Select(r => r.City)).Distinct().ToList();
            var resp = new List<GroupedContactReport>();
            foreach (string city in cities)
            {
                var grp = new GroupedContactReport();
                grp.Name = string.IsNullOrEmpty(city) ? "No city" : city; ;
                grp.Contacts = contacts
                    .Where(e => e.Addresses.Any(
                        w => string.Equals(w.City, city, StringComparison.InvariantCultureIgnoreCase))).Select(t =>
                    {
                        return new ContactReportResultDto()
                        {
                            Addresses = null,
                            Members = t.Members.Select(r => (ContactReportMambers)r.Clone()).ToList(),
                            FamilyID = t.FamilyID,
                            FamilyName = t.FamilyName,
                            FamilySalutation = t.FamilySalutation,
                            HisSalutation = t.HisSalutation,
                            HerSalutation = t.HerSalutation,
                            FamilyLabel = t.FamilyLabel,
                            HisLabel = t.HisLabel,
                            HerLabel = t.HerLabel
                        };
                    })
                    .ToList();

                // choose only addreses with current city
                foreach (ContactReportResultDto contact in grp.Contacts)
                {
                    contact.Addresses = contacts.Where(t => t.FamilyID == contact.FamilyID).SelectMany(r => r.Addresses
                            .Where(e => string.Equals(e.City, city, StringComparison.InvariantCultureIgnoreCase)))
                        .ToList();
                }
                resp.Add(grp);
            }
            return resp;
        }
        #endregion
    }
}

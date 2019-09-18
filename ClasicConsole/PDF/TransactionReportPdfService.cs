using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClasicConsole.DTO;
using ClasicConsole.Reports.DTO;
using ClasicConsole.Utils;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;

namespace ClasicConsole.PDF
{
    public class TransactionReportPdfService : ReportPdfService
    {
        Unit? colWidth = null;
        /// <summary>
        /// Initializate collections in case if we need name of one of them
        /// </summary>
        /// <param name="transFunc">Use delegate for translation method wich work with localResource file</param>
        /// <param name="paymentMethods"></param>
        /// <param name="solicitors"></param>
        /// <param name="mailings"></param>
        /// <param name="departments"></param>
        /// <param name="categoryTree"></param>
        public byte[] CreateDocument(FilterTransactionReport filter, TransactionGrouped grouped, int countTrans)
        {
            // Create a new MigraDoc document
            _document = new Document { Info = { Title = filter.Name } };


            DefineStyles();
            if (filter.view == TransFilterView.Details)
            {
                var colsCount = CreatePage(filter, countTrans);
                if (string.Equals(filter.subtotalBy, "None", StringComparison.InvariantCultureIgnoreCase)) FillContent(colsCount, grouped.GroupedObj, filter);
                else FillGroupedContent(colsCount, grouped.GroupedObj, filter);
            }
            if (filter.view == TransFilterView.Total)
            {
                if (string.Equals(filter.totalOnlyBy, "totalOnly", StringComparison.InvariantCultureIgnoreCase))
                {
                    CreatePage(filter, countTrans);
                    if (string.Equals(filter.subtotalBy, "None", StringComparison.InvariantCultureIgnoreCase)) FillTotalContent(grouped, filter);
                    if (!string.Equals(filter.subtotalBy, "None", StringComparison.InvariantCultureIgnoreCase)) FillSubGroupedTotalContent(grouped, filter);
                }
                else
                {
                    if (filter.ReportType == TransFilterType.Payment) FillMatrixRows((MatrixDTO)grouped, filter, countTrans);
                }
            }
            //  FillContent(colsCount);
            PdfDocumentRenderer pdfRenderer = new PdfDocumentRenderer(true);
            pdfRenderer.Document = _document;
            pdfRenderer.RenderDocument();
            using (MemoryStream ms = new MemoryStream())
            {
                pdfRenderer.Save(ms, false);
                byte[] buffer = new byte[ms.Length];
                ms.Seek(0, SeekOrigin.Begin);
                ms.Flush();
                ms.Read(buffer, 0, (int)ms.Length);
                ms.Close();
                return buffer;
            }
        }




        int CreateColumns(FilterTransactionReport filter, int colCount, Unit? sectionWidth = null)
        {
            //for total 
            var borders = new Borders
            {
                Bottom = new Border
                {
                    Visible = true,
                    Color = _blackColor
                },
                Visible = false
            };
            if (sectionWidth != null)
            {
                colWidth = sectionWidth / colCount;
            }
            List<ReportColumn> checkedCols = new List<ReportColumn>();
            Column column = _table.AddColumn();
            //first empty col
            column.Format.Alignment = ParagraphAlignment.Center;
            column.Width = (Unit)colWidth;
            //create cols obj
            if (filter.view == TransFilterView.Details)
            {
                checkedCols = filter.Columns.Where(r => r.IsChecked && r.ColumnOnly && (r.TransType == null || r.TransType == filter.ReportType)).ToList();
                for (var index = 0; index < checkedCols.Count; index++)
                {
                    column = _table.AddColumn();
                    column.Width = (Unit)colWidth;
                    column.Format.Alignment = ParagraphAlignment.Right;
                }
            }
            if (filter.ReportType == TransFilterType.Payment)
            {
                //amount
                column = _table.AddColumn();
                column.Width = (Unit)colWidth;
                column.Format.Alignment = ParagraphAlignment.Right;
            }
            if (filter.ReportType == TransFilterType.Bill)
            {
                //bill amount
                column = _table.AddColumn();
                column.Width = (Unit)colWidth;
                column.Format.Alignment = ParagraphAlignment.Right;
                //paid amount
                column = _table.AddColumn();
                column.Width = (Unit)colWidth;
                column.Format.Alignment = ParagraphAlignment.Right;
                //due amount
                column = _table.AddColumn();
                column.Width = (Unit)colWidth;
                column.Format.Alignment = ParagraphAlignment.Right;
                //unasignet amount
                if (filter.ShowUnasinged)
                {
                    column = _table.AddColumn();
                    column.Width = (Unit)colWidth;
                    column.Format.Alignment = ParagraphAlignment.Right;
                }

            }

            //fill col obj
            Row row = _table.AddRow();
            FillRow(row, ParagraphAlignment.Center, true, _tableHeaderBackground, 0, "", true, ParagraphAlignment.Left,
                VerticalAlignment.Center, true, _tableHeaderFont, fontSize: 8, borders: borders);

            if (filter.view == TransFilterView.Details)
            {
                for (var index = 0; index < checkedCols.Count; index++)
                {
                    ReportColumn col = checkedCols[index];
                    FillRow(row, ParagraphAlignment.Center, true, _tableHeaderBackground, index + 1,
                        GetTranslation(col.Name), true, ParagraphAlignment.Center, VerticalAlignment.Center, true, _tableHeaderFont, fontSize: 8, borders: borders);
                    AddColToPositionList((TransactioReportColumns)col.TransactionColumn, index + 1);
                }
            }
            if (filter.ReportType == TransFilterType.Payment)
            {
                var index = checkedCols.Count + 1;
                FillRow(row, ParagraphAlignment.Center, true, _tableHeaderBackground, index, GetTranslation("report_trans_amount"), true, ParagraphAlignment.Center, VerticalAlignment.Center, true, _tableHeaderFont, fontSize: 8, borders: borders);
            }
            if (filter.ReportType == TransFilterType.Bill)
            {
                var index = checkedCols.Count + 1;
                FillRow(row, ParagraphAlignment.Center, true, _tableHeaderBackground, index, GetTranslation("report_trans_billAmount"), true, ParagraphAlignment.Center, VerticalAlignment.Center, true, _tableHeaderFont, fontSize: 8, borders: borders);
                FillRow(row, ParagraphAlignment.Center, true, _tableHeaderBackground, index + 1, GetTranslation("report_trans_paidAmount"), true, ParagraphAlignment.Center, VerticalAlignment.Center, true, _tableHeaderFont, fontSize: 8, borders: borders);
                FillRow(row, ParagraphAlignment.Center, true, _tableHeaderBackground, index + 2, GetTranslation("report_trans_dueAmount"), true, ParagraphAlignment.Center, VerticalAlignment.Center, true, _tableHeaderFont, fontSize: 8, borders: borders);
                if (filter.ShowUnasinged)
                    FillRow(row, ParagraphAlignment.Center, true, _tableHeaderBackground, index + 3, GetTranslation("report_trans_unasignedAmount"), true, ParagraphAlignment.Center, VerticalAlignment.Center, true, _tableHeaderFont, fontSize: 8, borders: borders);
            }


            colCount = row.Cells.Count;


            return colCount;
        }

        void CreateMatrixColumns(List<TransactionMatrixColumn> cols, Unit? sectionWidth = null)
        {
            Unit? colWidth = null;
            if (sectionWidth != null)
            {
                colWidth = sectionWidth / (cols.Count + 1);
            }
            Column column = _table.AddColumn();
            //first empty col
            column.Format.Alignment = ParagraphAlignment.Center;
            if (colWidth != null) column.Width = (Unit)colWidth;
            for (var index = 0; index < cols.Count; index++)
            {
                column = _table.AddColumn();
                if (colWidth != null) column.Width = (Unit)colWidth;
                column.Format.Alignment = ParagraphAlignment.Right;
            }
            Row row = _table.AddRow();
            FillRow(row, ParagraphAlignment.Center, true, _tableHeaderBackground, 0, "", true, ParagraphAlignment.Center, VerticalAlignment.Center, true, _tableHeaderFont, fontSize: 10);


            for (var index = 0; index < cols.Count; index++)
            {
                var col = cols[index];
                FillRow(row, ParagraphAlignment.Center, true, _tableHeaderBackground, index + 1,
                    col.Name, true, ParagraphAlignment.Center, VerticalAlignment.Center, true, _tableHeaderFont, fontSize: 10);
            }
        }



        void DefineStyles()
        {
            // Get the predefined style Normal.
            Style style = _document.Styles["Normal"];
            // Because all styles are derived from Normal, the next line changes the 
            // font of the whole document. Or, more exactly, it changes the font of
            // all styles and paragraphs that do not redefine the font.
            style.Font.Name = "Arial";

            style = _document.Styles[StyleNames.Header];
            style.ParagraphFormat.AddTabStop("16cm", TabAlignment.Right);

            //style = _document.Styles[StyleNames.Footer];
            //style.ParagraphFormat.AddTabStop("1cm", TabAlignment.Center);
            //style.Font.Name = "Arial";
            //style.Font.Name = "Arial";
            //style.Font.Size = 9;
            //style.Font.Color = _fontColor;

            // Create a new style called Table based on style Normal
            style = _document.Styles.AddStyle("Table", "Normal");
            style.Font.Name = "Arial";
            style.Font.Name = "Arial";
            style.Font.Size = 9;
            style.Font.Color = _fontColor;

            // Create a new style called Reference based on style Normal
            style = _document.Styles.AddStyle("Reference", "Normal");
            style.ParagraphFormat.SpaceBefore = "5mm";
            style.ParagraphFormat.SpaceAfter = "5mm";
            style.ParagraphFormat.TabStops.AddTabStop("16cm", TabAlignment.Right);
        }

        int CreatePage(FilterTransactionReport filter, int countTransactions, List<TransactionMatrixColumn> matrixCols = null)
        {
            int colCount = 0;
            if (filter.view == TransFilterView.Details)
            {
                colCount = filter.Columns.Count(r => r.IsChecked && r.ColumnOnly);
                if (filter.ReportType == TransFilterType.Bill) colCount += filter.ShowUnasinged ? 4 : 3;
                //if (filter.ReportType == TransFilterType.Payment) colCount += 2;
                if (filter.ReportType == TransFilterType.Payment) colCount++;
            }
            if (filter.view == TransFilterView.Total)
            {
                if (filter.ReportType == TransFilterType.Bill) colCount = filter.ShowUnasinged ? 5 : 4;
                if (filter.ReportType == TransFilterType.Payment) colCount = 2;
            }
            // Each MigraDoc document needs at least one section.
            _section = _document.AddSection();
            var pageSize = _document.DefaultPageSetup.Clone();
            pageSize.Orientation = colCount < 5 ? Orientation.Portrait : Orientation.Landscape;
            pageSize.PageFormat = PageFormat.A4;
            pageSize.LeftMargin = new Unit { Centimeter = 1.5 };
            pageSize.RightMargin = new Unit { Centimeter = 0.5 };
            //for page numbers
            pageSize.OddAndEvenPagesHeaderFooter = true;
            pageSize.StartingNumber = 1;

            _section.PageSetup = pageSize;

            //Header
            Paragraph paragrapgEven = _section.Headers.EvenPage.AddParagraph();
            paragrapgEven.AddText(GetShortName(filter.Name, pageSize.Orientation == Orientation.Portrait ? 310 : 470));
            paragrapgEven.Format.Font.Size = 9;
            paragrapgEven.Format.LeftIndent = new Unit { Millimeter = 10 };
            paragrapgEven.Format.RightIndent = new Unit { Millimeter = 10 };
            paragrapgEven.Format.Alignment = ParagraphAlignment.Center;

            Paragraph paragraph = _section.Headers.Primary.AddParagraph();
            paragraph.AddText(GetShortName(filter.Name, pageSize.Orientation == Orientation.Portrait ? 310 : 470));
            paragraph.Format.Font.Size = 9;
            paragraph.Format.LeftIndent = new Unit { Millimeter = 10 };
            paragraph.Format.RightIndent = new Unit { Millimeter = 10 };
            paragraph.Format.Alignment = ParagraphAlignment.Center;

            // Create the item table
            _table = _section.AddTable();
            _table.Style = "Table";

            _table.Borders.Color = _tableBorder;
            _table.Borders.Width = 0.25;
            _table.Borders.Left.Width = 0.5;
            _table.Borders.Right.Width = 0.5;
            _table.Rows.LeftIndent = -20;
            // table.Rows.HeightRule = RowHeightRule.Auto;
            // table.Rows.Height = _minRowHeight;
            int colsCount = 0;
            Unit curPageWidth = new Unit { Centimeter = pageSize.Orientation == Orientation.Landscape ? 29.7 : 21 };
            Unit? pageWidth = curPageWidth - _section.PageSetup.LeftMargin - _section.PageSetup.RightMargin;
            //if (filter.view == TransFilterView.Total)
            //    pageWidth = _section.PageSetup.PageWidth - _section.PageSetup.LeftMargin - _section.PageSetup.RightMargin;

            if (!string.Equals(filter.totalOnlyBy, "totalOnly", StringComparison.InvariantCultureIgnoreCase))
                CreateMatrixColumns(matrixCols, pageWidth);
            else
                colsCount = CreateColumns(filter, colCount, pageWidth);
            _table.SetEdge(0, 0, colsCount, 1, Edge.Box, BorderStyle.Single, 0.75, Color.Empty);
            Paragraph totalText = _section.AddParagraph();
            totalText.AddText(
                string.Format("{0} \n {1} {2}", GetTranslation("transaction_report_name"),
                    GetTranslation("transaction_report_numberOfTransactions"), countTransactions));
            totalText.Format.Font.Size = 9;
            totalText.Format.Alignment = ParagraphAlignment.Left;
            totalText.Format.LeftIndent = -20;

            //page num
            var style = _document.Styles[StyleNames.Footer];
            style.ParagraphFormat.AddTabStop(colCount < 5 ? "9cm" : "13.3cm", TabAlignment.Center);
            style.Font.Name = "Arial";
            style.Font.Name = "Arial";
            style.Font.Size = 9;
            style.Font.Color = _fontColor;
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

            return colsCount;
        }
        void FillContent(int colscCount, List<TransactionReportGroupedDTO> trans, FilterTransactionReport filter)
        {
            foreach (TransactionReportGroupedDTO tr in trans)
            {
                Row rowHead = _table.AddRow();
                FillRow(rowHead, 0, "test", true, ParagraphAlignment.Left, VerticalAlignment.Center);
                FillRow(rowHead, 0, GetMainInfo(tr, filter), false, ParagraphAlignment.Left, VerticalAlignment.Center);
                rowHead.Cells[0].MergeRight = colscCount - 1;

                //fill main transactions
                foreach (var innTr in tr.Transactions)
                {
                    Row row1 = _table.AddRow();
                    //empty cell
                    FillRow(row1, 0, "", false, ParagraphAlignment.Center, VerticalAlignment.Center);
                    FillTransaction(innTr, filter, row1, colscCount);


                    if (filter.ShowDetails && filter.ReportType == TransFilterType.Bill && innTr.Details.Any())
                    {
                        Row detailRow = _table.AddRow();
                        FillDetails(detailRow, colscCount, innTr.Details, filter);
                    }

                    _table.SetEdge(0, _table.Rows.Count - 1, colscCount, 1, Edge.Box, BorderStyle.Single, 0.75);
                }
                if (filter.ShowUnasinged && filter.ReportType == TransFilterType.Bill && tr.UnassignedPayments.Any())
                {
                    Row unasignRow = _table.AddRow();
                    FillRow(unasignRow, 0, "Unassigned Payments", true, ParagraphAlignment.Center, VerticalAlignment.Center);
                    unasignRow.Cells[0].MergeRight = colscCount - 1;
                    foreach (TransactionsReportList unTr in tr.UnassignedPayments)
                    {
                        Row unRow = _table.AddRow();
                        FillTransaction(unTr, filter, unRow, colscCount, true);
                        //set unassignet to 0
                        unRow.Cells[colscCount - 1].AddParagraph(unTr.Amount.ToMoneyString());
                    }

                }
                Row rowBot = _table.AddRow();
                FillTotal(rowBot, tr, filter, colscCount);

                AddEmptyRow();
            }

        }
        private void FillGroupedContent(int colsCount, List<TransactionReportGroupedDTO> groupedGroupedObj, FilterTransactionReport filter)
        {
            foreach (TransactionReportGroupedDTO tr in groupedGroupedObj)
            {
                Row rowHead = _table.AddRow();
                FillRow(rowHead, 0, GetMainInfo(tr, filter), true, ParagraphAlignment.Left, VerticalAlignment.Center);
                rowHead.Cells[0].MergeRight = colsCount - 1;

                foreach (TransactionReportGroupedDTO sg in tr.SubGrouped)
                {
                    Row rowSg = _table.AddRow();
                    FillRow(rowSg, 0, sg.Name, true, ParagraphAlignment.Left, VerticalAlignment.Center, marginLeft: 10);
                    rowSg.Cells[0].MergeRight = colsCount - 1;

                    foreach (TransactionsReportList trans in sg.Transactions)
                    {
                        Row row1 = _table.AddRow();
                        //empty cell
                        FillRow(row1, 0, "", false, ParagraphAlignment.Center, VerticalAlignment.Center);
                        FillTransaction(trans, filter, row1, colsCount);
                        if (filter.ShowDetails && filter.ReportType == TransFilterType.Bill && trans.Details.Any())
                        {
                            Row detailRow = _table.AddRow();
                            FillDetails(detailRow, colsCount, trans.Details, filter);
                        }

                        _table.SetEdge(0, _table.Rows.Count - 1, colsCount, 1, Edge.Box, BorderStyle.Single, 0.75);
                    }
                    if (filter.ShowUnasinged && filter.ReportType == TransFilterType.Bill && sg.UnassignedPayments.Any())
                    {
                        Row unasignRow = _table.AddRow();
                        FillRow(unasignRow, 0, "Unnasigment payments", false, ParagraphAlignment.Center, VerticalAlignment.Center);
                        unasignRow.Cells[0].MergeRight = colsCount - 1;

                        foreach (TransactionsReportList unTr in sg.UnassignedPayments)
                        {
                            Row unRow = _table.AddRow();
                            FillTransaction(unTr, filter, unRow, colsCount, true);
                            //set unassignet to 0
                            unRow.Cells[colsCount - 1].AddParagraph(unTr.Amount.ToMoneyString());
                        }
                    }
                    Row subTotal = _table.AddRow();
                    FillTotal(subTotal, sg, filter, colsCount, marginLeft: 10);
                }
                Row rowBot = _table.AddRow();
                FillTotal(rowBot, tr, filter, colsCount);
                AddEmptyRow();
            }
        }

        private void FillTotalContent(TransactionGrouped groupedGroupedObj,
            FilterTransactionReport filter)
        {
            foreach (TransactionReportGroupedDTO trans in groupedGroupedObj.GroupedObj)
            {
                Row rowHead = _table.AddRow();
                FillRow(rowHead, 0, GetMainInfo(trans, filter), false, ParagraphAlignment.Left, VerticalAlignment.Center);
                //Total amount
                FillRow(rowHead, 1, trans.TotalAmount.ToMoneyString(), false, ParagraphAlignment.Right, VerticalAlignment.Center);
                if (filter.ReportType == TransFilterType.Bill)
                {
                    //Paid amount
                    FillRow(rowHead, 2, trans.PaidTotalAmount.ToMoneyString(), false, ParagraphAlignment.Right, VerticalAlignment.Center);
                    //due amount 
                    FillRow(rowHead, 3, trans.DueTotalAmount.ToMoneyString(), false, ParagraphAlignment.Right, VerticalAlignment.Center);
                    // unasignedAmount
                    if (filter.ShowUnasinged) FillRow(rowHead, 4, trans.UnasignedAmount.ToMoneyString(), false, ParagraphAlignment.Right, VerticalAlignment.Center);
                }
            }
            Row gransRow = _table.AddRow();
            FillGrandTotal(gransRow, groupedGroupedObj, filter);
        }

        private void FillSubGroupedTotalContent(TransactionGrouped groupedGroupedObj,
            FilterTransactionReport filter)
        {
            var colSpanCount = filter.ReportType == TransFilterType.Bill ? (filter.ShowUnasinged ? 5 : 4) : 2;
            foreach (TransactionReportGroupedDTO sg in groupedGroupedObj.GroupedObj)
            {
                Row rowHead = _table.AddRow();
                FillRow(rowHead, 0, GetMainInfo(sg, filter), true, ParagraphAlignment.Left, VerticalAlignment.Center);
                rowHead.Cells[0].MergeRight = colSpanCount - 1;
                foreach (TransactionReportGroupedDTO grouped in sg.SubGrouped)
                {
                    Row rowGp = _table.AddRow();
                    FillRow(rowGp, 0, GetMainInfo(grouped, filter), false, ParagraphAlignment.Left, VerticalAlignment.Center, marginLeft: 10);
                    //Total amount
                    FillRow(rowGp, 1, grouped.TotalAmount.ToMoneyString(), false, ParagraphAlignment.Right,
                        VerticalAlignment.Center);
                    if (filter.ReportType == TransFilterType.Bill)
                    {
                        //Paid amount
                        FillRow(rowGp, 2, grouped.PaidTotalAmount.ToMoneyString(), false, ParagraphAlignment.Right, VerticalAlignment.Center);
                        //due amount 
                        FillRow(rowGp, 3, grouped.DueTotalAmount.ToMoneyString(), false, ParagraphAlignment.Right, VerticalAlignment.Center);
                        // unasignedAmount
                        if (filter.ShowUnasinged) FillRow(rowGp, 4, string.IsNullOrEmpty(grouped.UnasignedAmount.ToMoneyString()) ? 0M.ToMoneyString() : grouped.UnasignedAmount.ToMoneyString(), false, ParagraphAlignment.Right, VerticalAlignment.Center);
                    }
                }
                Row subTotal = _table.AddRow();
                FillRow(subTotal, 0, sg.TotalName, true, ParagraphAlignment.Left, VerticalAlignment.Center);
                FillRow(subTotal, 1, sg.TotalAmount.ToMoneyString(), true, ParagraphAlignment.Right, VerticalAlignment.Center);
                if (filter.ReportType == TransFilterType.Bill)
                {
                    FillRow(subTotal, 2, sg.PaidTotalAmount.ToMoneyString(), true, ParagraphAlignment.Right, VerticalAlignment.Center);
                    FillRow(subTotal, 3, sg.DueTotalAmount.ToMoneyString(), true, ParagraphAlignment.Right, VerticalAlignment.Center);
                    if (filter.ShowUnasinged) FillRow(subTotal, 4, sg.UnasignedAmount.ToMoneyString(), true, ParagraphAlignment.Right, VerticalAlignment.Center);
                }
                AddEmptyRow();
            }
            Row grand = _table.AddRow();
            FillGrandTotal(grand, groupedGroupedObj, filter);
        }

        private void FillMatrixRows(MatrixDTO grouped, FilterTransactionReport filter, int countTrans)
        {
            //firs create cols
            // CreateMatrixColumns(filter, grouped.Columns);
            CreatePage(filter, countTrans, grouped.Columns);
            bool lastElement = false;
            for (int i = 0; i < grouped.Rows.Count; i++)
            {
                if ((grouped.Rows.Count - 1) == i) lastElement = true;
                var row = grouped.Rows[i];
                //the las row used only for Total, so we no need to use the first line in it
                if ((grouped.Rows.Count - 1) > i)
                {
                    Row rowGp = _table.AddRow();
                    FillRow(rowGp, 0, row.Name, true, ParagraphAlignment.Left, VerticalAlignment.Center);
                    //if we have some subgroup criterias< the forst line should contendted only name for subgroup
                    if (string.Equals(filter.subtotalBy, "None", StringComparison.InvariantCultureIgnoreCase))
                    {
                        for (int j = 0; j < row.Amounts.Count; j++)
                        {
                            var am = row.Amounts[j];
                            FillRow(rowGp, j + 1, am.ToMoneyString(), false, ParagraphAlignment.Left,
                                VerticalAlignment.Center);
                        }
                    }
                    else
                    {
                        for (int j = 0; j < row.Amounts.Count; j++)
                        {
                            FillRow(rowGp, j + 1, "", false, ParagraphAlignment.Left, VerticalAlignment.Center);
                        }
                    }
                }
                //subgrouping
                if (!string.Equals(filter.subtotalBy, "None", StringComparison.InvariantCultureIgnoreCase) &&
                    row.SubDetails != null)
                {

                    foreach (TransactioMatrixSubDetails rw in row.SubDetails)
                    {
                        Row rowGp2 = _table.AddRow();
                        FillRow(rowGp2, 0, rw.Name, false, ParagraphAlignment.Left, VerticalAlignment.Center, marginLeft: 10);

                        for (int y = 0; y < rw.Amounts.Count; y++)
                        {
                            var am = rw.Amounts[y];
                            FillRow(rowGp2, y + 1, am.ToMoneyString(), false, ParagraphAlignment.Left,
                                VerticalAlignment.Center);
                        }
                    }
                    //fill total

                    Row totalRow = _table.AddRow();
                    FillRow(totalRow, 0, row.TotalName, true, ParagraphAlignment.Left, VerticalAlignment.Center);
                    for (int j = 0; j < row.Amounts.Count; j++)
                    {
                        var am = row.Amounts[j];
                        FillRow(totalRow, j + 1, am.ToMoneyString(), false, ParagraphAlignment.Left,
                            VerticalAlignment.Center);
                    }
                    AddEmptyRow();
                }
                if (lastElement)
                {
                    Row totalRow = _table.AddRow();
                    FillRow(totalRow, 0, row.TotalName, true, ParagraphAlignment.Left, VerticalAlignment.Center);
                    for (int j = 0; j < row.Amounts.Count; j++)
                    {
                        var am = row.Amounts[j];
                        FillRow(totalRow, j + 1, am.ToMoneyString(), false, ParagraphAlignment.Left,
                            VerticalAlignment.Center);
                    }
                }


            }
        }


        string GetMainInfo(TransactionReportGroupedDTO trans, FilterTransactionReport filter)
        {
            StringBuilder strBuilder = new StringBuilder();
            strBuilder.Append(trans.Name);
            //fill company
            if (!string.IsNullOrEmpty(trans.Company) &&
                filter.Columns.Any(r => r.Column == ReportColumns.Company && r.IsChecked))
                strBuilder.AppendLine(trans.Company);

            //fill contacts
            if (trans.FamilyDetails != null)
            {
                if (trans.FamilyDetails.Contacts.Any() && filter.Columns.Any(r => r.IsChecked && r.IsContact))
                {
                    strBuilder.AppendLine();
                    foreach (ContactReportContactInfo cont in trans.FamilyDetails.Contacts)
                    {
                        if (filter.Columns.Where(s => s.Column != null)
                                .Any(r => (int)r.Column == cont.PhoneTypeID && r.IsChecked) && !string.IsNullOrEmpty(cont.PhoneNumber))
                        {
                            strBuilder.AppendFormat(" {0}: {1}; ",
                                GetTranslation(GetContactName(cont.PhoneTypeID, cont.MemberType)),
                                cont.PhoneNumber);
                        }
                    }
                }
                if (trans.FamilyDetails.Addresses.Any() &&
                    filter.Columns.Any(e => e.Column == ReportColumns.Address && e.IsChecked))
                {
                    //home address
                    if (trans.FamilyDetails.Addresses.Any(e => e.AddressTypeID == 1))
                    {
                        strBuilder.AppendLine();
                        strBuilder.Append(GetTranslation("trans_info_homeAddress"));
                        foreach (ContactReportAddress addr in trans.FamilyDetails.Addresses.Where(r => r.AddressTypeID == 1))
                        {
                            // strBuilder.AppendFormat(" {0} {1},{2} {3} {4}; ", addr.Address, addr.City, addr.State, addr.Zip, addr.Country);
                            strBuilder.Append($" {addr.Address} {addr.Address2} {addr.City}, {addr.State} {addr.Zip} {addr.Country}; ");
                        }

                    }
                    //work address
                    if (trans.FamilyDetails.Addresses.Any(e => e.AddressTypeID == 2))
                    {
                        strBuilder.AppendLine();
                        strBuilder.Append(GetTranslation("trans_info_workAddress"));
                        foreach (ContactReportAddress addr in trans.FamilyDetails.Addresses.Where(r => r.AddressTypeID == 2))
                        {
                            //  strBuilder.AppendFormat(" {0} {1},{2} {3} {4}; ", addr.Address, addr.City, addr.State, addr.Zip, addr.Country);
                            strBuilder.Append($" {addr.Address} {addr.Address2} {addr.City}, {addr.State} {addr.Zip} {addr.Country}; ");
                        }

                    }
                }
            }
            return strBuilder.ToString();
        }

        string GetContactName(int phoneTypeId, int contactTypeId)
        {
            if (phoneTypeId == 1 && contactTypeId == 1) return "new_report_HisHomePhone";
            if (phoneTypeId == 1 && contactTypeId == 2) return "new_report_HerHomePhone";
            if (phoneTypeId == 2 && contactTypeId == 1) return "new_report_HisWorkPhone";
            if (phoneTypeId == 2 && contactTypeId == 2) return "new_report_HerWorkPhone";
            if (phoneTypeId == 3 && contactTypeId == 1) return "new_report_HisMobilePhone";
            if (phoneTypeId == 3 && contactTypeId == 2) return "new_report_HerMobilePhone";
            if (phoneTypeId == 4 && contactTypeId == 1) return "new_report_HisOtherPhone";
            if (phoneTypeId == 4 && contactTypeId == 2) return "new_report_HerOtherPhone";
            if (phoneTypeId == 5 && contactTypeId == 1) return "new_report_HisPager";
            if (phoneTypeId == 5 && contactTypeId == 2) return "new_report_HerPager";
            if (phoneTypeId == 6 && contactTypeId == 1) return "new_report_HisFax";
            if (phoneTypeId == 6 && contactTypeId == 2) return "new_report_HerFax";
            if (phoneTypeId == 7 && contactTypeId == 1) return "new_report_HisEmail";
            if (phoneTypeId == 7 && contactTypeId == 2) return "new_report_HerEmail";
            if (phoneTypeId == 8 && contactTypeId == 1) return "new_report_HisEmergencyPhone";
            if (phoneTypeId == 8 && contactTypeId == 2) return "new_report_HerEmergencyPhone";
            return String.Empty;
        }






        void FillDetails(Row row, int colCount, List<TransactionDetailReportList> details, FilterTransactionReport filter)
        {
            FillRow(row, 0, "Details", false, ParagraphAlignment.Left, VerticalAlignment.Center);
            Table detTable = new Table
            {
                Style = "Table",
                Borders =
                {
                    Color = _tableBorder,
                    Width = 0.25,
                    Left = {Width = 0.5},
                    Right = {Width = 0.5}
                },
                Rows = { LeftIndent = 0 }
            };
            Column column;
            //  int nestedColCount = 3;
            bool dateDueIsChecked = filter.Columns.Any(e => e.TransactionColumn == TransactioReportColumns.DateDue && e.IsChecked);
            bool quantityIsChecked = filter.Columns.Any(e => e.TransactionColumn == TransactioReportColumns.Quantity && e.IsChecked);
            // if (dateDueIsChecked) nestedColCount = 4;
            var _colSize = colWidth * (colCount - 1);
            _colSize = (dateDueIsChecked ? quantityIsChecked ? _colSize / 5 : _colSize / 4
                           : quantityIsChecked ? _colSize / 4 : _colSize / 3) - 2;
            if (dateDueIsChecked)
            {
                column = detTable.AddColumn();
                column.Format.Alignment = ParagraphAlignment.Center;
                column.Width = (Unit)_colSize;
            }
            if (quantityIsChecked)
            {
                column = detTable.AddColumn();
                column.Format.Alignment = ParagraphAlignment.Center;
                column.Width = (Unit)_colSize;
            }
            column = detTable.AddColumn();
            column.Width = (Unit)_colSize;
            column.Format.Alignment = ParagraphAlignment.Center;

            column = detTable.AddColumn();
            column.Width = (Unit)_colSize;
            column.Format.Alignment = ParagraphAlignment.Center;

            column = detTable.AddColumn();
            column.Width = (Unit)_colSize;
            column.Format.Alignment = ParagraphAlignment.Center;

            Row nestedRow = detTable.AddRow();
            if (dateDueIsChecked) FillRow(nestedRow, ParagraphAlignment.Center, true, _tableGray, 0, "Date Due", false, ParagraphAlignment.Left, VerticalAlignment.Bottom, true);
            FillRow(nestedRow, ParagraphAlignment.Center, true, _tableGray, dateDueIsChecked ? 1 : 0, "Category/ Subcategory", false, ParagraphAlignment.Left, VerticalAlignment.Bottom, true);
            if (quantityIsChecked) FillRow(nestedRow, ParagraphAlignment.Center, true, _tableGray, dateDueIsChecked ? 2 : 1, "Quantity", false, ParagraphAlignment.Left, VerticalAlignment.Bottom, true);
            FillRow(nestedRow, ParagraphAlignment.Center, true, _tableGray, dateDueIsChecked ? quantityIsChecked ? 3 : 2 : quantityIsChecked ? 2 : 1, "Bill", false, ParagraphAlignment.Left, VerticalAlignment.Bottom, true);
            FillRow(nestedRow, ParagraphAlignment.Center, true, _tableGray, dateDueIsChecked ? quantityIsChecked ? 4 : 3 : quantityIsChecked ? 3 : 2, "Payment", false, ParagraphAlignment.Left, VerticalAlignment.Bottom, true);
            foreach (var det in details)
            {
                if (det.TransType == 1) continue;
                Row rw = detTable.AddRow();
                FillRow(rw, 0, "", false, ParagraphAlignment.Left, VerticalAlignment.Center);
                if (dateDueIsChecked) FillRow(rw, 0, det.DateDue.GetValueOrDefault().ToShortDateString(), false, ParagraphAlignment.Left, VerticalAlignment.Center);
                FillRow(rw, dateDueIsChecked ? 1 : 0,
                    $"{GetCategoryName(det.CategoryID)}/{GetSubCategoryName(det.SubcategoryID)}", false, ParagraphAlignment.Left, VerticalAlignment.Center);
                if (quantityIsChecked) FillRow(rw, dateDueIsChecked ? 2 : 1, det.Quantity == null ? "" : ((int)det.Quantity).ToString(), false, ParagraphAlignment.Left, VerticalAlignment.Center);
                FillRow(rw, dateDueIsChecked ? quantityIsChecked ? 3 : 2 : quantityIsChecked ? 2 : 1, det.Amount.ToMoneyString(), false, ParagraphAlignment.Left, VerticalAlignment.Center);
                FillRow(rw, dateDueIsChecked ? quantityIsChecked ? 4 : 3 : quantityIsChecked ? 3 : 2, det.PaidAmount == null ? "0" : ((decimal)det.PaidAmount).ToMoneyString(), false, ParagraphAlignment.Left, VerticalAlignment.Center);

            }
            row.Cells[1].Elements.Add(detTable);
            //row.Cells[1].Format.Font.Bold = false;
            //row.Cells[1].Format.Alignment = colParAligment;
            //row.Cells[1].VerticalAlignment = colVerAlignment;
            row.Cells[1].MergeRight = colCount - 2;
        }

        void FillTransaction(TransactionsReportList tr, FilterTransactionReport filter, Row row, int colscCount, bool isUnassignet = false)
        {
            int index;
            if (filter.Columns.Any(r => r.TransactionColumn == TransactioReportColumns.CheckNum && r.IsChecked) && TryGetColIndexFromList(TransactioReportColumns.CheckNum, out index)) FillRow(row, index, string.IsNullOrEmpty(tr.CheckNo) ? string.Empty : tr.CheckNo, false, ParagraphAlignment.Center, VerticalAlignment.Center);
            if (filter.Columns.Any(r => r.TransactionColumn == TransactioReportColumns.Date && r.IsChecked) && TryGetColIndexFromList(TransactioReportColumns.Date, out index)) FillRow(row, index, tr.Date.ToShortDateString(), false, ParagraphAlignment.Center, VerticalAlignment.Center);
            if (filter.Columns.Any(r => r.TransactionColumn == TransactioReportColumns.Method && r.IsChecked) && TryGetColIndexFromList(TransactioReportColumns.Method, out index)) FillRow(row, index, GetMethodName(tr.PaymentMethodID), false, ParagraphAlignment.Center, VerticalAlignment.Center);
            if (filter.Columns.Any(r => r.TransactionColumn == TransactioReportColumns.Solicitor && r.IsChecked) && TryGetColIndexFromList(TransactioReportColumns.Solicitor, out index)) FillRow(row, index, GetSolicitorName(tr.SolicitorID), false, ParagraphAlignment.Center, VerticalAlignment.Center);
            if (filter.Columns.Any(r => r.TransactionColumn == TransactioReportColumns.Mailing && r.IsChecked) && TryGetColIndexFromList(TransactioReportColumns.Mailing, out index)) FillRow(row, index, GetMailingName(tr.MailingID), false, ParagraphAlignment.Center, VerticalAlignment.Center);
            if (filter.Columns.Any(r => r.TransactionColumn == TransactioReportColumns.CatSubcat && r.IsChecked) && TryGetColIndexFromList(TransactioReportColumns.CatSubcat, out index)) FillRow(row, index, GetCatSubcatName(tr.Details), false, ParagraphAlignment.Center, VerticalAlignment.Center);
            if (filter.Columns.Any(r => r.TransactionColumn == TransactioReportColumns.Department && r.IsChecked) && TryGetColIndexFromList(TransactioReportColumns.Department, out index)) FillRow(row, index, GetDepartmentName(tr.DepartmentID), false, ParagraphAlignment.Center, VerticalAlignment.Center);
            if (filter.ReportType == TransFilterType.Payment && filter.Columns.Any(r => r.TransactionColumn == TransactioReportColumns.ReceiptNum && r.IsChecked) && TryGetColIndexFromList(TransactioReportColumns.ReceiptNum, out index)) FillRow(row, index, tr.ReceiptNo.ToString(), false, ParagraphAlignment.Center, VerticalAlignment.Center);
            if (filter.ReportType == TransFilterType.Bill && filter.Columns.Any(r => r.TransactionColumn == TransactioReportColumns.InvoiceNum && r.IsChecked) && TryGetColIndexFromList(TransactioReportColumns.InvoiceNum, out index)) FillRow(row, index, tr.InvoiceNo.ToString(), false, ParagraphAlignment.Center, VerticalAlignment.Center);
            if (filter.Columns.Any(r => r.TransactionColumn == TransactioReportColumns.Note && r.IsChecked) && TryGetColIndexFromList(TransactioReportColumns.Note, out index)) FillRow(row, index, string.IsNullOrEmpty(tr.Note) ? String.Empty : tr.Note, false, ParagraphAlignment.Center, VerticalAlignment.Center);
            //amount
            FillRow(row, filter.ReportType == TransFilterType.Payment ? colscCount - 1 : colscCount - (filter.ShowUnasinged ? 4 : 3), tr.Amount.ToMoneyString(), false, ParagraphAlignment.Right, VerticalAlignment.Center);
            if (filter.ReportType == TransFilterType.Bill) FillRow(row, colscCount - (filter.ShowUnasinged ? 3 : 2), isUnassignet ? 0M.ToMoneyString() : CalcPaydedAmount(tr.Amount, tr.AmountDue).ToMoneyString(), false, ParagraphAlignment.Right, VerticalAlignment.Center);
            if (filter.ReportType == TransFilterType.Bill) FillRow(row, colscCount - (filter.ShowUnasinged ? 2 : 1), tr.AmountDue == null ? 0M.ToMoneyString() : ((decimal)tr.AmountDue).ToMoneyString(), false, ParagraphAlignment.Right, VerticalAlignment.Center);
            if (filter.ReportType == TransFilterType.Bill && filter.ShowUnasinged) FillRow(row, colscCount - 1, "", false, ParagraphAlignment.Right, VerticalAlignment.Center);
        }


        void FillTotal(Row row, TransactionReportGroupedDTO tr, FilterTransactionReport filter, int colCount, float? marginLeft = null)
        {
            var borders = new Borders
            {
                Bottom = new Border
                {
                    Visible = true,
                    Color = _blackColor
                },
                Visible = false
            };

            FillRow(row, 0, tr.TotalName, true, ParagraphAlignment.Left, VerticalAlignment.Center, marginLeft: marginLeft, borders: borders);
            if (filter.ReportType == TransFilterType.Payment)
            {
                row.Cells[0].MergeRight = colCount - 2;
                FillRow(row, colCount - 1, tr.TotalAmount.ToMoneyString(), true, ParagraphAlignment.Right, VerticalAlignment.Center, borders: borders);
            }
            if (filter.ReportType == TransFilterType.Bill)
            {
                row.Cells[0].MergeRight = colCount - (filter.ShowUnasinged ? 5 : 6);
                FillRow(row, colCount - (filter.ShowUnasinged ? 4 : 3), tr.TotalAmount.ToMoneyString(), true, ParagraphAlignment.Right, VerticalAlignment.Center, borders: borders);
                FillRow(row, colCount - (filter.ShowUnasinged ? 3 : 2), tr.PaidTotalAmount.ToMoneyString(), true, ParagraphAlignment.Right, VerticalAlignment.Center, borders: borders);
                FillRow(row, colCount - (filter.ShowUnasinged ? 2 : 1), tr.DueTotalAmount.ToMoneyString(), true, ParagraphAlignment.Right, VerticalAlignment.Center, borders: borders);
                if (filter.ShowUnasinged) FillRow(row, colCount - 1, tr.UnasignedAmount.ToMoneyString(), true, ParagraphAlignment.Right, VerticalAlignment.Center, borders: borders);
            }
        }

        void FillGrandTotal(Row row, TransactionGrouped grouped, FilterTransactionReport filter)
        {
            FillRow(row, 0, "Grand Total", true, ParagraphAlignment.Left, VerticalAlignment.Center);
            FillRow(row, 1, grouped.GrandAmount == null ? 0M.ToMoneyString() : ((decimal)grouped.GrandAmount).ToMoneyString(), true, ParagraphAlignment.Right, VerticalAlignment.Center);
            if (filter.ReportType == TransFilterType.Bill)
            {
                FillRow(row, 2, grouped.GrandPaid == null ? 0M.ToMoneyString() : ((decimal)grouped.GrandPaid).ToMoneyString(), true, ParagraphAlignment.Right,
                    VerticalAlignment.Center);
                FillRow(row, 3, grouped.GrandDue == null ? 0M.ToMoneyString() : ((decimal)grouped.GrandDue).ToMoneyString(), true, ParagraphAlignment.Right,
                    VerticalAlignment.Center);
                if (filter.ShowUnasinged) FillRow(row, 4, grouped.GrandUnasignedAmount == null ? 0M.ToMoneyString() : ((decimal)grouped.GrandUnasignedAmount).ToMoneyString(), true, ParagraphAlignment.Right,
                      VerticalAlignment.Center);
            }
        }


        decimal CalcPaydedAmount(decimal amount, decimal? due)
        {
            if (due == null)
                return 0;
            return Math.Abs(amount) - Math.Abs((int)due);
        }
    }

}

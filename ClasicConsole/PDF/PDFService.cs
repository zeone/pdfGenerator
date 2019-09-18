using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ClasicConsole.DTO;
using ClasicConsole.Reports.DTO;
using ClasicConsole.Utils;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;

namespace ClasicConsole.PDF
{
    public static partial class PdfService
    {
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

        private static Section _section;

        static readonly List<ColumnsPositions> ColPos = new List<ColumnsPositions>();
        public static Document CreateDocument(FilterTransactionReport filter, TransactionGrouped grouped, int countTrans)
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
                    if (string.Equals(filter.subtotalBy, "None", StringComparison.InvariantCultureIgnoreCase)) FillTotalContent(grouped, filter);
                    if (!string.Equals(filter.subtotalBy, "None", StringComparison.InvariantCultureIgnoreCase)) FillSubGroupedTotalContent(grouped, filter);
                }
                else
                {
                    if (filter.ReportType == TransFilterType.Payment) FillMatrixRows((MatrixDTO)grouped, filter, countTrans);
                }
            }
            //  FillContent(colsCount);

            return _document;
        }




        static int CreateColumns(FilterTransactionReport filter, Unit? sectionWidth = null)
        {
            //for total 
            Unit? colWidth = null;
            if (sectionWidth != null)
            {
                colWidth = sectionWidth / (filter.ReportType == TransFilterType.Payment ? 2 : 5);
            }
            List<ReportColumn> checkedCols = new List<ReportColumn>();
            Column column = _table.AddColumn();
            //first empty col
            column.Format.Alignment = ParagraphAlignment.Center;
            if (colWidth != null) column.Width = (Unit)colWidth;
            //create cols obj
            if (filter.view == TransFilterView.Details)
            {
                checkedCols = filter.Columns.Where(r => r.IsChecked && r.ColumnOnly).ToList();
                for (var index = 0; index < checkedCols.Count; index++)
                {
                    column = _table.AddColumn();
                    column.Format.Alignment = ParagraphAlignment.Right;
                }
            }
            if (filter.ReportType == TransFilterType.Payment)
            {
                //amount
                column = _table.AddColumn();
                if (colWidth != null) column.Width = (Unit)colWidth;
                column.Format.Alignment = ParagraphAlignment.Right;
            }
            if (filter.ReportType == TransFilterType.Bill)
            {
                //bill amount
                column = _table.AddColumn();
                if (colWidth != null) column.Width = (Unit)colWidth;
                column.Format.Alignment = ParagraphAlignment.Right;
                //paid amount
                column = _table.AddColumn();
                if (colWidth != null) column.Width = (Unit)colWidth;
                column.Format.Alignment = ParagraphAlignment.Right;
                //due amount
                column = _table.AddColumn();
                if (colWidth != null) column.Width = (Unit)colWidth;
                column.Format.Alignment = ParagraphAlignment.Right;
                //unasignet amount
                column = _table.AddColumn();
                if (colWidth != null) column.Width = (Unit)colWidth;
                column.Format.Alignment = ParagraphAlignment.Right;

            }

            //fill col obj
            Row row = _table.AddRow();

            FillRow(row, ParagraphAlignment.Center, true, TableHeaderBackground, 0, "", true, ParagraphAlignment.Left, VerticalAlignment.Bottom, true, TableHeaderFont, fontSize: 11);

            if (filter.view == TransFilterView.Details)
            {
                for (var index = 0; index < checkedCols.Count; index++)
                {
                    ReportColumn col = checkedCols[index];
                    FillRow(row, ParagraphAlignment.Center, true, TableHeaderBackground, index + 1,
                       Grouping.GetTranslation(Grouping.GetTranslation(col.Column.ToString())), true, ParagraphAlignment.Center, VerticalAlignment.Bottom, true, TableHeaderFont, fontSize: 11);
                    AddColToPositionList((TransactioReportColumns) col.TransactionColumn, index + 1);
                }
            }
            if (filter.ReportType == TransFilterType.Payment)
            {
                var index = checkedCols.Count + 1;
                FillRow(row, ParagraphAlignment.Center, true, TableHeaderBackground, index, Grouping.GetTranslation("report_trans_amount"), true, ParagraphAlignment.Center, VerticalAlignment.Bottom, true, TableHeaderFont, fontSize: 11);
            }
            if (filter.ReportType == TransFilterType.Bill)
            {
                var index = checkedCols.Count + 1;
                FillRow(row, ParagraphAlignment.Center, true, TableHeaderBackground, index, Grouping.GetTranslation("report_trans_billAmount"), true, ParagraphAlignment.Center, VerticalAlignment.Center, true, TableHeaderFont, fontSize: 11);
                FillRow(row, ParagraphAlignment.Center, true, TableHeaderBackground, index + 1, Grouping.GetTranslation("report_trans_paidAmount"), true, ParagraphAlignment.Center, VerticalAlignment.Center, true, TableHeaderFont, fontSize: 11);
                FillRow(row, ParagraphAlignment.Center, true, TableHeaderBackground, index + 2, Grouping.GetTranslation("report_trans_dueAmount"), true, ParagraphAlignment.Center, VerticalAlignment.Center, true, TableHeaderFont, fontSize: 11);
                FillRow(row, ParagraphAlignment.Center, true, TableHeaderBackground, index + 3, Grouping.GetTranslation("report_trans_unasignedAmount"), true, ParagraphAlignment.Center, VerticalAlignment.Center, true, TableHeaderFont, fontSize: 11);
            }


            var colCount = row.Cells.Count;


            return colCount;
        }

        static void CreateMatrixColumns(List<TransactionMatrixColumn> cols, Unit? sectionWidth = null)
        {
            Unit? colWidth = null;
            if (sectionWidth != null)
            {
                Unit correction = new Unit { Millimeter = 10 };
                colWidth = sectionWidth / cols.Count - correction;
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
            FillRow(row, ParagraphAlignment.Center, true, TableHeaderBackground, 0, "", true, ParagraphAlignment.Center, VerticalAlignment.Center, true, TableHeaderFont, fontSize: 11);


            for (var index = 0; index < cols.Count; index++)
            {
                var col = cols[index];
                FillRow(row, ParagraphAlignment.Center, true, TableHeaderBackground, index + 1,
                    col.Name, true, ParagraphAlignment.Center, VerticalAlignment.Center, true, TableHeaderFont, fontSize: 11);
            }
        }



        static void DefineStyles()
        {
            // Get the predefined style Normal.
            Style style = _document.Styles["Normal"];
            // Because all styles are derived from Normal, the next line changes the 
            // font of the whole document. Or, more exactly, it changes the font of
            // all styles and paragraphs that do not redefine the font.
            style.Font.Name = "Verdana";

            style = _document.Styles[StyleNames.Header];
            style.ParagraphFormat.AddTabStop("16cm", TabAlignment.Right);

            style = _document.Styles[StyleNames.Footer];
            style.ParagraphFormat.AddTabStop("8cm", TabAlignment.Center);

            // Create a new style called Table based on style Normal
            style = _document.Styles.AddStyle("Table", "Normal");
            style.Font.Name = "Verdana";
            style.Font.Name = "Times New Roman";
            style.Font.Size = 9;
            style.Font.Color = FontColor;

            // Create a new style called Reference based on style Normal
            style = _document.Styles.AddStyle("Reference", "Normal");
            style.ParagraphFormat.SpaceBefore = "5mm";
            style.ParagraphFormat.SpaceAfter = "5mm";
            style.ParagraphFormat.TabStops.AddTabStop("16cm", TabAlignment.Right);
        }

        static int CreatePage(FilterTransactionReport filter, int countTransactions, List<TransactionMatrixColumn> matrixCols = null)
        {
            var colCount = filter.Columns.Count(r => r.IsChecked && r.ColumnOnly);
            if (filter.view == TransFilterView.Details)
            {
                if (filter.ReportType == TransFilterType.Bill) colCount += 5;
                if (filter.ReportType == TransFilterType.Payment) colCount += 2;
            }
            // Each MigraDoc document needs at least one section.
            _section = _document.AddSection();
            //_section.PageSetup = _document.DefaultPageSetup.Clone();
            var pageSize = _document.DefaultPageSetup.Clone();
            pageSize.Orientation = colCount < 5 ? Orientation.Portrait : Orientation.Landscape;
            pageSize.PageFormat = PageFormat.A4;
            pageSize.LeftMargin = new Unit { Centimeter = 1.5 };
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
            _table.Rows.LeftIndent = -20;
             _table.Rows.HeightRule = RowHeightRule.AtLeast;
           _table.Rows.Height = 12;
            int colsCount = 0;
            Unit? pageWidth = null;
            if (filter.view == TransFilterView.Total)
                pageWidth = _section.PageSetup.PageWidth - _section.PageSetup.LeftMargin - _section.PageSetup.RightMargin;

            if (!string.Equals(filter.totalOnlyBy, "totalOnly", StringComparison.InvariantCultureIgnoreCase))
                CreateMatrixColumns(matrixCols, pageWidth);
            else
                colsCount = CreateColumns(filter, pageWidth);
            _table.SetEdge(0, 0, colsCount, 1, Edge.Box, BorderStyle.Single, 0.75, Color.Empty);
            Paragraph totalText = _section.AddParagraph();
            totalText.AddText($"{Grouping.GetTranslation("transaction_report_name")} \n " +
                              $"{Grouping.GetTranslation("transaction_report_numberOfTransactions")} {countTransactions}");
            totalText.Format.Font.Size = 9;
            totalText.Format.Alignment = ParagraphAlignment.Left;
            totalText.Format.LeftIndent = -20;
            return colsCount;
        }
        static void FillContent(int colscCount, List<TransactionReportGroupedDTO> trans, FilterTransactionReport filter)
        {
            foreach (TransactionReportGroupedDTO tr in trans)
            {
                Row rowHead = _table.AddRow();
                FillRow(rowHead, 0, GetMainInfo(tr, filter), true, ParagraphAlignment.Left, VerticalAlignment.Center);
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
                    FillRow(unasignRow, 0, "Unnasigment payments", true, ParagraphAlignment.Center, VerticalAlignment.Center);
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
        private static void FillGroupedContent(int colsCount, List<TransactionReportGroupedDTO> groupedGroupedObj, FilterTransactionReport filter)
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

        private static void FillTotalContent(TransactionGrouped groupedGroupedObj,
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
                    FillRow(rowHead, 3, trans.UnasignedAmount.ToMoneyString(), false, ParagraphAlignment.Right, VerticalAlignment.Center);
                }
            }
            Row gransRow = _table.AddRow();
            FillGrandTotal(gransRow, groupedGroupedObj, filter);
        }

        private static void FillSubGroupedTotalContent(TransactionGrouped groupedGroupedObj,
            FilterTransactionReport filter)
        {
            var colSpanCount = filter.ReportType == TransFilterType.Bill ? 5 : 2;
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
                        FillRow(rowGp, 4, grouped.UnasignedAmount.ToMoneyString(), false, ParagraphAlignment.Right, VerticalAlignment.Center);
                    }
                }
                Row subTotal = _table.AddRow();
                FillRow(subTotal, 0, sg.TotalName, true, ParagraphAlignment.Left, VerticalAlignment.Center);
                FillRow(subTotal, 1, sg.TotalAmount.ToMoneyString(), true, ParagraphAlignment.Right, VerticalAlignment.Center);
                if (filter.ReportType == TransFilterType.Bill)
                {
                    FillRow(subTotal, 2, sg.PaidTotalAmount.ToMoneyString(), true, ParagraphAlignment.Right, VerticalAlignment.Center);
                    FillRow(subTotal, 3, sg.DueTotalAmount.ToMoneyString(), true, ParagraphAlignment.Right, VerticalAlignment.Center);
                    FillRow(subTotal, 4, sg.UnasignedAmount.ToMoneyString(), true, ParagraphAlignment.Right, VerticalAlignment.Center);
                }
                AddEmptyRow();
            }
            Row grand = _table.AddRow();
            FillGrandTotal(grand, groupedGroupedObj, filter);
        }

        private static void FillMatrixRows(MatrixDTO grouped, FilterTransactionReport filter, int countTrans)
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

        /// <summary>
        /// Mostly use for main rows
        /// </summary>
        /// <param name="row"></param>
        /// <param name="rowParAligment"></param>
        /// <param name="rowBold"></param>
        /// <param name="rowBackgroundColor"></param>
        /// <param name="cellIndex"></param>
        /// <param name="colText"></param>
        /// <param name="colBold"></param>
        /// <param name="colParAligment"></param>
        /// <param name="colVerAlignment"></param>
        /// <param name="rowHeadingFormat"></param>
        /// <param name="fontColor"></param>
        /// <param name="rowHeight"></param>
        /// <param name="fontSize"></param>
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
        /// <summary>
        /// Used for generals rows
        /// </summary>
        /// <param name="row"></param>
        /// <param name="cellIndex"></param>
        /// <param name="colText"></param>
        /// <param name="colBold"></param>
        /// <param name="colParAligment"></param>
        /// <param name="colVerAlignment"></param>
        /// <param name="fontColor"></param>
        /// <param name="marginLeft"></param>
        /// <param name="fontSize"></param>
        static void FillRow(Row row, int cellIndex, string colText, bool colBold, ParagraphAlignment colParAligment, VerticalAlignment colVerAlignment,
            Color? fontColor = null, float? marginLeft = null, int? fontSize = null)
        {

            if (fontSize != null) row.Cells[cellIndex].Format.Font.Size = (int)fontSize;
            if (fontColor != null) row.Cells[cellIndex].Format.Font.Color = (Color)fontColor;
            if (marginLeft != null) row.Cells[cellIndex].Format.LeftIndent = (float)marginLeft;
            row.Cells[cellIndex].AddParagraph(colText);
            row.Cells[cellIndex].Format.Font.Bold = colBold;
            row.Cells[cellIndex].Format.Alignment = colParAligment;
            row.Cells[cellIndex].VerticalAlignment = colVerAlignment;
        }

        static string GetMainInfo(TransactionReportGroupedDTO trans, FilterTransactionReport filter)
        {
            StringBuilder strBuilder = new StringBuilder();
            strBuilder.Append(trans.Name);
            //fill company
            if (!string.IsNullOrEmpty(trans.Company) &&
                filter.Columns.Any(r => r.TransactionColumn == TransactioReportColumns.Company && r.IsChecked))
                strBuilder.AppendLine(trans.Company);

            //fill contacts
            if (trans.FamilyDetails != null)
            {
                if (trans.FamilyDetails.Contacts.Any() && filter.Columns.Any(r => r.IsChecked && r.IsContact))
                {
                    strBuilder.AppendLine();
                    foreach (ContactReportContactInfo cont in trans.FamilyDetails.Contacts)
                    {
                        if (filter.Columns.Any(r => (int)r.TransactionColumn == cont.PhoneTypeID && r.IsChecked))
                        {
                            strBuilder.AppendFormat(" {0}: {1}; ",
                                Grouping.GetTranslation(GetContactName(cont.PhoneTypeID, cont.MemberType)),
                                cont.PhoneNumber);
                        }
                    }
                }
                if (trans.FamilyDetails.Addresses.Any() &&
                    filter.Columns.Any(e => e.TransactionColumn == TransactioReportColumns.Address && e.IsChecked))
                {
                    //home address
                    if (trans.FamilyDetails.Addresses.Any(e => e.AddressTypeID == 1))
                    {
                        strBuilder.AppendLine();
                        strBuilder.Append(Grouping.GetTranslation("trans_info_homeAddress"));
                        foreach (ContactReportAddress addr in trans.FamilyDetails.Addresses.Where(r => r.AddressTypeID == 1))
                        {
                            strBuilder.AppendFormat(" {0} {1},{2} {3} {4}; ", addr.Address, addr.City, addr.State, addr.Zip, addr.Country);
                        }

                    }
                    //work address
                    if (trans.FamilyDetails.Addresses.Any(e => e.AddressTypeID == 2))
                    {
                        strBuilder.AppendLine();
                        strBuilder.Append(Grouping.GetTranslation("trans_info_workAddress"));
                        foreach (ContactReportAddress addr in trans.FamilyDetails.Addresses.Where(r => r.AddressTypeID == 2))
                        {
                            strBuilder.AppendFormat(" {0} {1},{2} {3} {4}; ", addr.Address, addr.City, addr.State, addr.Zip, addr.Country);
                        }

                    }
                }
            }
            return strBuilder.ToString();
        }

        static string GetContactName(int phoneTypeId, int contactTypeId)
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
        static void AddEmptyRow()
        {
            Row row = _table.AddRow();
            row.HeightRule = RowHeightRule.Exactly;
            row.Height = 5;
            row.Borders.Visible = false;
        }

        static void AddColToPositionList(TransactioReportColumns col, int index)
        {
            ColPos.Add(new ColumnsPositions
            {
                Column = col,
                ColPosition = index
            });
        }

        static bool TryGetColIndexFromList(TransactioReportColumns col, out int index)
        {
            index = -1;
            var result = ColPos.FirstOrDefault(r => r.Column == col);
            if (result == null)
                return false;
            index = result.ColPosition;
            return true;
        }

        static decimal CalcPaydedAmount(decimal amount, decimal? due)
        {
            if (due == null)
                return 0;
            return Math.Abs(amount) - Math.Abs((int)due);
        }

        static void FillDetails(Row row, int colCount, List<TransactionDetailReportList> details, FilterTransactionReport filter)
        {
            FillRow(row, 0, "Details", false, ParagraphAlignment.Left, VerticalAlignment.Center);
            Table detTable = new Table
            {
                Style = "Table",
                Borders =
                {
                    Color = TableBorder,
                    Width = 0.25,
                    Left = {Width = 0.5},
                    Right = {Width = 0.5}
                },
                Rows = { LeftIndent = 0 }
            };
            Column column = detTable.AddColumn();
            //  int nestedColCount = 3;
            bool dateDueIsChecked = filter.Columns.Any(e => e.TransactionColumn == TransactioReportColumns.DateDue && e.IsChecked);
            // if (dateDueIsChecked) nestedColCount = 4;
            column.Format.Alignment = ParagraphAlignment.Center;
            column = detTable.AddColumn();
            column.Format.Alignment = ParagraphAlignment.Center;
            column = detTable.AddColumn();
            column.Format.Alignment = ParagraphAlignment.Center;
            Row nestedRow = detTable.AddRow();
            if (dateDueIsChecked) FillRow(nestedRow, ParagraphAlignment.Center, true, TableGray, 0, "Date Due", false, ParagraphAlignment.Left, VerticalAlignment.Bottom, true);
            FillRow(nestedRow, ParagraphAlignment.Center, true, TableGray, dateDueIsChecked ? 1 : 0, "Category/ Subcategory", false, ParagraphAlignment.Left, VerticalAlignment.Bottom, true);
            FillRow(nestedRow, ParagraphAlignment.Center, true, TableGray, dateDueIsChecked ? 2 : 1, "Bill", false, ParagraphAlignment.Left, VerticalAlignment.Bottom, true);
            FillRow(nestedRow, ParagraphAlignment.Center, true, TableGray, dateDueIsChecked ? 3 : 2, "Payment", false, ParagraphAlignment.Left, VerticalAlignment.Bottom, true);
            foreach (var det in details)
            {
                Row rw = detTable.AddRow();
                FillRow(rw, 0, "", false, ParagraphAlignment.Left, VerticalAlignment.Center);
                if (dateDueIsChecked) FillRow(rw, 0, det.DateDue.GetValueOrDefault().ToShortDateString(), false, ParagraphAlignment.Left, VerticalAlignment.Center);

                FillRow(rw, dateDueIsChecked ? 1 : 0, $"!! {det.CategoryID}/{det.SubcategoryID}", false, ParagraphAlignment.Left, VerticalAlignment.Center);
                FillRow(rw, dateDueIsChecked ? 2 : 1, det.Amount.ToMoneyString(), false, ParagraphAlignment.Left, VerticalAlignment.Center);
                FillRow(rw, dateDueIsChecked ? 3 : 2, det.PaidAmount == null ? "0" : ((decimal)det.PaidAmount).ToMoneyString(), false, ParagraphAlignment.Left, VerticalAlignment.Center);

            }
            row.Cells[1].Elements.Add(detTable);
            //row.Cells[1].Format.Font.Bold = false;
            //row.Cells[1].Format.Alignment = colParAligment;
            //row.Cells[1].VerticalAlignment = colVerAlignment;
            row.Cells[1].MergeRight = colCount - 2;
        }

        static void FillTransaction(TransactionsReportList tr, FilterTransactionReport filter, Row row, int colscCount, bool isUnassignet = false)
        {
            int index;
            if (filter.Columns.Any(r => r.TransactionColumn == TransactioReportColumns.CheckNum && r.IsChecked) && TryGetColIndexFromList(TransactioReportColumns.CheckNum, out index)) FillRow(row, index, string.IsNullOrEmpty(tr.CheckNo) ? string.Empty : tr.CheckNo, false, ParagraphAlignment.Center, VerticalAlignment.Center);
            if (filter.Columns.Any(r => r.TransactionColumn == TransactioReportColumns.Date && r.IsChecked) && TryGetColIndexFromList(TransactioReportColumns.Date, out index)) FillRow(row, index, tr.Date.ToShortDateString(), false, ParagraphAlignment.Center, VerticalAlignment.Center);
            if (filter.Columns.Any(r => r.TransactionColumn == TransactioReportColumns.Method && r.IsChecked) && TryGetColIndexFromList(TransactioReportColumns.Method, out index)) FillRow(row, index, Grouping.GetMethodName(tr.PaymentMethodID), false, ParagraphAlignment.Center, VerticalAlignment.Center);
            if (filter.Columns.Any(r => r.TransactionColumn == TransactioReportColumns.Solicitor && r.IsChecked) && TryGetColIndexFromList(TransactioReportColumns.Solicitor, out index)) FillRow(row, index, Grouping.GetSolicitorName(tr.SolicitorID), false, ParagraphAlignment.Center, VerticalAlignment.Center);
            if (filter.Columns.Any(r => r.TransactionColumn == TransactioReportColumns.Mailing && r.IsChecked) && TryGetColIndexFromList(TransactioReportColumns.Mailing, out index)) FillRow(row, index, Grouping.GetMailingName(tr.MailingID), false, ParagraphAlignment.Center, VerticalAlignment.Center);
            if (filter.Columns.Any(r => r.TransactionColumn == TransactioReportColumns.CatSubcat && r.IsChecked) && TryGetColIndexFromList(TransactioReportColumns.CatSubcat, out index)) FillRow(row, index, "Category/Subcategory not done!!!", false, ParagraphAlignment.Center, VerticalAlignment.Center);
            if (filter.Columns.Any(r => r.TransactionColumn == TransactioReportColumns.Department && r.IsChecked) && TryGetColIndexFromList(TransactioReportColumns.Department, out index)) FillRow(row, index, Grouping.GetDepartmentName(tr.DepartmentID), false, ParagraphAlignment.Center, VerticalAlignment.Center);
            if (filter.ReportType == TransFilterType.Payment && filter.Columns.Any(r => r.TransactionColumn == TransactioReportColumns.ReceiptNum && r.IsChecked) && TryGetColIndexFromList(TransactioReportColumns.ReceiptNum, out index)) FillRow(row, index, tr.ReceiptNo.ToString(), false, ParagraphAlignment.Center, VerticalAlignment.Center);
            if (filter.ReportType == TransFilterType.Payment && filter.Columns.Any(r => r.TransactionColumn == TransactioReportColumns.InvoiceNum && r.IsChecked) && TryGetColIndexFromList(TransactioReportColumns.InvoiceNum, out index)) FillRow(row, index, tr.InvoiceNo.ToString(), false, ParagraphAlignment.Center, VerticalAlignment.Center);
            if (filter.Columns.Any(r => r.TransactionColumn == TransactioReportColumns.Note && r.IsChecked) && TryGetColIndexFromList(TransactioReportColumns.Note, out index)) FillRow(row, index, tr.Note, false, ParagraphAlignment.Center, VerticalAlignment.Center);
            //amount
            FillRow(row, filter.ReportType == TransFilterType.Payment ? colscCount - 1 : colscCount - 4, tr.Amount.ToMoneyString(), false, ParagraphAlignment.Right, VerticalAlignment.Center);
            if (filter.ReportType == TransFilterType.Bill) FillRow(row, colscCount - 3, isUnassignet ? "0" : CalcPaydedAmount(tr.Amount, tr.AmountDue).ToMoneyString(), false, ParagraphAlignment.Right, VerticalAlignment.Center);
            if (filter.ReportType == TransFilterType.Bill) FillRow(row, colscCount - 2, tr.AmountDue == null ? "0" : ((decimal)tr.AmountDue).ToMoneyString(), false, ParagraphAlignment.Right, VerticalAlignment.Center);
            if (filter.ReportType == TransFilterType.Bill) FillRow(row, colscCount - 1, "", false, ParagraphAlignment.Right, VerticalAlignment.Center);
        }

        static void FillTotal(Row row, TransactionReportGroupedDTO tr, FilterTransactionReport filter, int colCount, float? marginLeft = null)
        {

            FillRow(row, 0, tr.TotalName, true, ParagraphAlignment.Left, VerticalAlignment.Center, marginLeft: marginLeft);
            if (filter.ReportType == TransFilterType.Payment)
            {
                row.Cells[0].MergeRight = colCount - 2;
                FillRow(row, colCount - 1, tr.TotalAmount.ToMoneyString(), true, ParagraphAlignment.Right, VerticalAlignment.Center);
            }
            if (filter.ReportType == TransFilterType.Bill)
            {
                row.Cells[0].MergeRight = colCount - 5;
                FillRow(row, colCount - 4, tr.TotalAmount.ToMoneyString(), true, ParagraphAlignment.Right, VerticalAlignment.Center);
                FillRow(row, colCount - 3, tr.PaidTotalAmount.ToMoneyString(), true, ParagraphAlignment.Right, VerticalAlignment.Center);
                FillRow(row, colCount - 2, tr.DueTotalAmount.ToMoneyString(), true, ParagraphAlignment.Right, VerticalAlignment.Center);
                FillRow(row, colCount - 1, tr.UnasignedAmount.ToMoneyString(), true, ParagraphAlignment.Right, VerticalAlignment.Center);
            }
        }

        static void FillGrandTotal(Row row, TransactionGrouped grouped, FilterTransactionReport filter)
        {
            FillRow(row, 0, "Grand Total", true, ParagraphAlignment.Left, VerticalAlignment.Center);
            FillRow(row, 1, grouped.GrandAmount == null ? 0M.ToMoneyString() : ((decimal)grouped.GrandAmount).ToMoneyString(), true, ParagraphAlignment.Right, VerticalAlignment.Center);
            if (filter.ReportType == TransFilterType.Bill)
            {
                FillRow(row, 2, grouped.GrandPaid == null ? 0M.ToMoneyString() : ((decimal)grouped.GrandPaid).ToMoneyString(), true, ParagraphAlignment.Right,
                    VerticalAlignment.Center);
                FillRow(row, 3, grouped.GrandDue == null ? 0M.ToMoneyString() : ((decimal)grouped.GrandDue).ToMoneyString(), true, ParagraphAlignment.Right,
                    VerticalAlignment.Center);
                FillRow(row, 4, grouped.GrandUnasignedAmount == null ? 0M.ToMoneyString() : ((decimal)grouped.GrandUnasignedAmount).ToMoneyString(), true, ParagraphAlignment.Right,
                    VerticalAlignment.Center);
            }
        }
    }


    //used for mapping col position in table
    class ColumnsPositions
    {
        public TransactioReportColumns Column { get; set; }
        public int ColPosition { get; set; }
    }

}


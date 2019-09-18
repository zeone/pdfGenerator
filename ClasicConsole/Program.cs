using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.UI;
using System.Xml.XPath;
using ClasicConsole.CSV;
using ClasicConsole.DTO;
using ClasicConsole.Injection;
using ClasicConsole.PDF;
using ClasicConsole.Reports;
using ClasicConsole.Reports.DTO;
using Microsoft.Owin.Hosting;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Shapes;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using Ninject;
using PdfSharp.Pdf;
using IQueryProvider = ClasicConsole.Reports.IQueryProvider;


//using Edge = EdgeJs.Edge;

namespace ClasicConsole
{
    class Program
    {
        static readonly IReportService service = NinjectBulder.Container.Get<IReportService>();
        static void Main(string[] args)
        {
            /// CreateInvoice();
            ////  WebApiCtrl();
            //Stopwatch swMain = Stopwatch.StartNew();
            Stopwatch swMain = Stopwatch.StartNew();
            Stopwatch sw = Stopwatch.StartNew();
            int countTrans = 0;
            TimeSpan ts;


            #region transaction report criterias
            ///temp data for transaction reports
            List<ReportColumn> cols = new List<ReportColumn>
            {
                new ReportColumn()
                {
                    TransactionColumn = TransactioReportColumns.Solicitor,
                    ColumnOnly = true,
                    IsChecked = true,
                    Filter = ReportColumnsFilter.All,
                    ReportType = ReportTypes.TransactionReport,
                    Name = "solicitor",
                    Sort = "Solicitor"


                },
                new ReportColumn()
                {
                    TransactionColumn = TransactioReportColumns.Address,
                    ColumnOnly = false,
                    IsChecked = true,
                    Filter = ReportColumnsFilter.All,
                    ReportType = ReportTypes.TransactionReport,
                    Name = "Address",
                    Sort = "Address",
                    Column = ReportColumns.Address

                },
                new ReportColumn()
                {
                    TransactionColumn = TransactioReportColumns.Company,
                    ColumnOnly = false,
                    IsChecked = true,
                    Filter = ReportColumnsFilter.All,
                    ReportType = ReportTypes.TransactionReport,
                    Name = "CompanyName",
                    Sort = "CompanyName",
                    Column = ReportColumns.Company

                },
                new ReportColumn()
                {
                    TransactionColumn = TransactioReportColumns.MobilePhone,
                    ColumnOnly = false,
                    IsChecked = true,
                    Filter = ReportColumnsFilter.All,
                    ReportType = ReportTypes.TransactionReport,
                    IsContact = true,
                    Name = "MobilePhone",
                    Sort = "MobilePhone",
                    Column = ReportColumns.MobilePhone

                },
                new ReportColumn()
                {
                    TransactionColumn = TransactioReportColumns.Email,
                    ColumnOnly = false,
                    IsChecked = true,
                    Filter = ReportColumnsFilter.All,
                    ReportType = ReportTypes.TransactionReport,
                    IsContact = true,
                    Name="MobilePhone",
                    Sort = "Email",
                    Column = ReportColumns.Email

                },
                new ReportColumn()
                {
                    TransactionColumn = TransactioReportColumns.Date,
                    ColumnOnly = true,
                    IsChecked = true,
                    Filter = ReportColumnsFilter.All,
                    ReportType = ReportTypes.TransactionReport,
                    IsContact = false,
                    Name="Date",
                    Sort = "Date"

                },
                new ReportColumn()
                {
                    TransactionColumn = TransactioReportColumns.CatSubcat,
                    ColumnOnly = true,
                    IsChecked = true,
                    Filter = ReportColumnsFilter.All,
                    ReportType = ReportTypes.TransactionReport,
                    IsContact = false,
                    Name="Categoryes",
                    Sort = "Categoryes"

                },
                new ReportColumn()
                {
                    TransactionColumn = TransactioReportColumns.CheckNum,
                    ColumnOnly = true,
                    IsChecked = true,
                    Filter = ReportColumnsFilter.All,
                    ReportType = ReportTypes.TransactionReport,
                    IsContact = false,
                    Name="CheckNum",
                    Sort = "CheckNum"

                }
            };
            var filter = new FilterTransactionReport
            {
                Name = "ReportName",
                ReportType = TransFilterType.Bill,
                view = TransFilterView.Details,
                DueBilled = 0,
                CalcTotalSum = (bool)true,
                Columns = cols,
                // subtotalBy = "NameCompany",
                subtotalBy = "none",
                ShowUnasinged = true,
                ShowDetails = false,
                totalOnlyBy = "totalOnly",
                //totalOnlyBy = "Category",
                GroupBy = "NameCompany"
                // GroupBy = "Solicitor"
                //  GroupBy = "Weeks"

            };
            // transaction report
            TransactionReportPdfService transPdf = new TransactionReportPdfService();
            var pdf = transPdf.CreateDocument(filter, GetGroupedItems(filter, out countTrans), countTrans);
            File.WriteAllBytes("tst.pdf", pdf);
            // Document document = PdfService.CreateDocument(filter, GetGroupedItems(filter, out countTrans), countTrans);
            sw.Stop();
            ts = sw.Elapsed;

            Console.WriteLine($"Prepare collection for PDF: {ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds / 10:00}");

            #endregion

            //#region Contact report
            ///// Contact reports filter
            ////List<ReportColumn> cols = new List<ReportColumn>
            ////{

            ////    new ReportColumn()
            ////    {
            ////        Column = ReportColumns.Address,
            ////        ColumnOnly = false,
            ////        IsChecked = true,
            ////        Filter = ReportColumnsFilter.All,
            ////        ReportType = ReportTypes.ContactReport,
            ////        Name = "Address",
            ////        Sort = "Address"

            ////    },
            ////    new ReportColumn()
            ////    {
            ////        Column = ReportColumns.City,
            ////        ColumnOnly = false,
            ////        IsChecked = true,
            ////        Filter = ReportColumnsFilter.All,
            ////        ReportType = ReportTypes.ContactReport,
            ////        Name = "City",
            ////        Sort = "City"

            ////    },
            ////    //new ReportColumn()
            ////    //{
            ////    //    Column = ReportColumns.Company,
            ////    //    ColumnOnly = false,
            ////    //    IsChecked = true,
            ////    //    Filter = ReportColumnsFilter.All,
            ////    //    ReportType = ReportTypes.ContactReport,
            ////    //    Name = "CompanyName",
            ////    //    Sort = "CompanyName"

            ////    //},
            ////    new ReportColumn()
            ////    {
            ////        Column = ReportColumns.MobilePhone,
            ////        ColumnOnly = false,
            ////        IsChecked = true,
            ////        Filter = ReportColumnsFilter.All,
            ////        ReportType = ReportTypes.ContactReport,
            ////        //IsContact = true,
            ////        Name = "MobilePhone",
            ////        Sort = "MobilePhone"

            ////    },
            ////    new ReportColumn()
            ////    {
            ////        Column = ReportColumns.WorkPhone,
            ////        ColumnOnly = false,
            ////        IsChecked = true,
            ////        Filter = ReportColumnsFilter.All,
            ////        ReportType = ReportTypes.ContactReport,
            ////        //IsContact = true,
            ////        Name = "WorkPhone",
            ////        Sort = "WorkPhone"

            ////    },
            ////    new ReportColumn()
            ////    {
            ////        Column = ReportColumns.Email,
            ////        ColumnOnly = false,
            ////        IsChecked = true,
            ////        Filter = ReportColumnsFilter.All,
            ////        ReportType = ReportTypes.ContactReport,
            ////        //IsContact = true,
            ////        Name="Email",
            ////        Sort = "Email"

            ////    },
            ////    new ReportColumn()
            ////    {
            ////    Column = ReportColumns.Company,
            ////    ColumnOnly = false,
            ////    IsChecked = true,
            ////    Filter = ReportColumnsFilter.All,
            ////    ReportType = ReportTypes.ContactReport,
            ////    //IsContact = true,
            ////    Name="CompanyName",
            ////    Sort = "CompanyName"

            ////}

            ////};
            ////var filter = new FilterContactReport
            ////{
            ////    Name = "ReportName",
            ////    ReportType = TransFilterType.Family,
            ////    Columns = cols,
            ////    TransactionSort = 0,
            ////    SkipTitles = false,
            ////    GroupBy = "Zip"
            ////    //GroupBy = "None"

            ////};
            //////main pdf
            ////Document document = PdfContactService.CreateContectReportDocument(filter, GetContactReportResult(filter));

            //////envelopes
            //////var document = PdfContactService.CreateContectReportDocument(filter, GetContactReportResult(filter),
            //////    " Main Department Some address \nLA, CA, 2512451, US");
            ////sw.Stop();
            ////ts = sw.Elapsed;
            ////Console.WriteLine($"Prepare collection for PDF: {ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds / 10:00}");

            //#endregion





            //    document.UseCmykColor = false;
            //  PdfDocumentRenderer pdfRenderer = new PdfDocumentRenderer(true);

            // Set the MigraDoc document
            // pdfRenderer.Document = document;
            // Create the PDF document
            sw.Restart();
            // pdfRenderer.RenderDocument();
            //using (MemoryStream ms = new MemoryStream())
            //{
            //    pdfRenderer.Save(ms, false);
            //    byte[] buffer = new byte[ms.Length];
            //    ms.Seek(0, SeekOrigin.Begin);
            //    ms.Flush();
            //    ms.Read(buffer, 0, (int)ms.Length);
            //}
            sw.Stop();
            ts = sw.Elapsed;
            Console.WriteLine($"Render pdf document: {ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds / 10:00}");
            // Save the PDF document...
            sw.Restart();
            string filename = "Invoice.pdf";
            //  pdfRenderer.Save(filename);
            // document.Save(filename);
            sw.Stop();
            ts = sw.Elapsed;
            Console.WriteLine($"Save pdf document: {ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds / 10:00}");

            // ...and start a viewer.
            // Process.Start(filename);
            //string t = GetCSVString(filter);
            //var path = @"c:\1.csv";
            //FileStream fcreate = File.Open(path, FileMode.Create);

            //// Create a file to write to.
            //using (StreamWriter sWriter = new StreamWriter(fcreate))
            //{
            //    sWriter.WriteLine(t);
            //}

            //fcreate.Close();

            swMain.Stop();
            ts = swMain.Elapsed;
            Console.WriteLine($"Total Time: {ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds / 10:00}");

            Console.WriteLine("Press Enter to quit.");
            Console.WriteLine("done");
            Console.ReadKey();
        }

        private static void CreateInvoice()
        {
            var result = service.GetAllBills(256).FirstOrDefault();
            //Document document = PDFInvoices.CreateDocument(result);
            PdfDocument document = PDFInvoices.CreateDocument(result);
            // document.UseCmykColor = false;
            //PdfDocumentRenderer pdfRenderer = new PdfDocumentRenderer(true);
            //pdfRenderer.Document = document;
            //pdfRenderer.RenderDocument();
            //string filename = "Invoice.pdf";
            //pdfRenderer.Save(filename);
            string filename = "Invoice.pdf";
            document.Save(filename);
            // ...and start a viewer
            Process.Start(filename);
        }

        public static void WebApiCtrl()
        {

            using (WebApp.Start<Startup>("http://localhost:8080"))
            {
                Console.WriteLine("Web Server is running.");
                Console.WriteLine("Press any key to quit.");
                Console.ReadLine();
            }
        }


        static TransactionGrouped GetGroupedItems(FilterTransactionReport filter, out int countTransactions)
        {
            countTransactions = 0;
            var result = service.FilterTransactionReport(filter);

            Stopwatch sw = Stopwatch.StartNew();
            TransactionGrouped sorted;
            sorted = !string.Equals(filter.totalOnlyBy, "totalOnly", StringComparison.InvariantCultureIgnoreCase)
                ? Grouping.TransactionTotalOnlyBy(result, filter) : Grouping.TotalBy(result, filter);
            sw.Stop();
            TimeSpan ts = sw.Elapsed;
            string elapsedTime = $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds / 10:00}";
            Console.WriteLine($"Grouping by name:{elapsedTime}");
            Console.WriteLine($"total transactions count:{result.Transactions.Count}");
            Console.WriteLine($"total families count: {result.Families.Count}");
            Console.WriteLine($"total details in transactions:{result.Transactions.Sum(r => r.Details.Count)}");
            countTransactions = result.Transactions.Count;
            return sorted;
        }


        static string GetCSVString(FilterTransactionReport filter)
        {
            if (!string.Equals(filter.subtotalBy, "None", StringComparison.InvariantCultureIgnoreCase))
                filter.subtotalBy = "None";
            var result = service.FilterTransactionReport(filter);
            return CSVService.GetCsvString(filter, result);

        }

        static string GetCSVString(FilterContactReport filter)
        {

            return CSVContactService.GetCsvString(filter, GetContactReportResult(filter));

        }

        static List<ContactReportResultDto> GetContactReportResult(FilterContactReport filter)
        {
            return service.FilterContactReport(filter).ToList();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;
using ClasicConsole.DTO;
using ClasicConsole.Reports;
using ClasicConsole.Reports.DTO;
using Ninject;

namespace ClasicConsole.WEBService
{
    [EnableCors(origins: "http://big.cms.local", headers: "*", methods: "*")]
    public class TransactionController : ApiController
    {
        private readonly IReportService service;

       
        public TransactionController()
        {
            service = NinjectBulder.Container.Get<IReportService>();
        }
        public HttpResponseMessage GetGroupedByNameTransaction()
        {
            Stopwatch swMain = Stopwatch.StartNew();

            var filter = new FilterTransactionReport
            {
                ReportType = TransFilterType.Payment,
                DueBilled = 0,
                CalcTotalSum = (bool)true
            };
            var result = service.FilterTransactionReport(filter);
            Stopwatch sw = Stopwatch.StartNew();

            var sorted = Grouping.TotalByAllFamilies(result, filter);
            sw.Stop();
            TimeSpan ts = sw.Elapsed;
            Console.WriteLine($"Grouped by name: {ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds / 10:00}");


            swMain.Stop();
            // ValueType detCount = result.Transactions.Sum(r=>r.Details.Count);
            ts = swMain.Elapsed;
            Console.WriteLine($"Complete time for all operations: {ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds / 10:00}");
            return Request.CreateResponse(HttpStatusCode.OK, sorted);
        }

    }

}

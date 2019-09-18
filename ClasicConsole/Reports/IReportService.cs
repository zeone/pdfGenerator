using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClasicConsole.DTO;
using ClasicConsole.Reports.DTO;

namespace ClasicConsole.Reports
{
    public interface IReportService
    {
        IList<ContactReportResultDto> FilterContactReport(FilterContactReport filter);
        TransactionReportResultDto FilterTransactionReport(FilterTransactionReport filter);

        IEnumerable<BillInvoiceDto> GetAllBills(int? transactionId = null);
        //bool SaveReport(ReportDto report);
        //ReportDto SelectReport(int reportId);
        //IList<ReportDto> SelectReports(ReportTypes? type = null);
        //bool UpdateReport(ReportDto report);
        //bool DeleteReport(int reportId);
    }
}

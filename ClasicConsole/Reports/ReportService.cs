using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using AutoMapper;
using ClasicConsole.DTO;
using ClasicConsole.Reports.DTO;
using ClasicConsole.Reports.SP;
using ClasicConsole.Utils;

namespace ClasicConsole.Reports
{
    public class ReportService : BaseService, IReportService
    {
        private readonly IXMLService _xmlService;
        public ReportService(IQueryProvider queryProvider) : base(queryProvider)
        {
            _xmlService = new XMLService();
        }
        /// <summary>
        /// Use for contact report
        /// </summary>
        /// <param name="filter">object with params for filtering</param>
        /// <returns>List of filtered contacts</returns>
        public IList<ContactReportResultDto> FilterContactReport(FilterContactReport filter)
        {
            Mapper.Initialize(r => r.CreateMap<FilterContactReport, SelectContactReportStoredProcedure>()
                .ForMember(q => q.TransactionSort, e => e.MapFrom(t => (int)t.TransactionSort))
                .ConstructUsing(
                    x => QueryProvider.CreateQuery<SelectContactReportStoredProcedure>(ConfigurationManager.AppSettings["schema"]))
            );

            var queryMember = Mapper.Map<FilterContactReport, SelectContactReportStoredProcedure>(filter);
            var resultq = queryMember.Execute();
            return !resultq.HasNoDataRows ? ConvertResultToContactReport(resultq.ResultToArray<ContactReportDto>().ToList())
                : new List<ContactReportResultDto>();
        }

        /// <summary>
        /// Use for transaction report
        /// </summary>
        /// <param name="filter">object with params for filtering</param>
        /// <returns>Return object with filtered transactions and families</returns>
        public TransactionReportResultDto FilterTransactionReport(FilterTransactionReport filter)
        {
            Mapper.Initialize(e => e.CreateMap<FilterTransactionReport, SelectTransactionsReportStoredProcedure>()
                .ConstructUsing(r => QueryProvider.CreateQuery<SelectTransactionsReportStoredProcedure>(ConfigurationManager.AppSettings["schema"]))
                .ForMember(t => t.MinAmount, r => r.MapFrom(w => w.MinSum)));
            var queryMember = Mapper.Map<FilterTransactionReport, SelectTransactionsReportStoredProcedure>(filter);
            Stopwatch sw = new Stopwatch();
            sw.Start();
            var resultqr = queryMember.Execute();
            sw.Stop();
            TimeSpan ts = sw.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
            Console.WriteLine($"Build response from database to DTO: {elapsedTime}");
            sw.Reset();
            var result = new TransactionReportResultDto();
            sw.Start();
            if (!resultqr.HasNoDataRows) ConvertResultToTransactionReport(result, resultqr.ResultToArray<TransactionsReportDto>().ToList(), filter.ReportType, filter);
            sw.Stop();
            ts = sw.Elapsed;
            elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
               ts.Hours, ts.Minutes, ts.Seconds,
               ts.Milliseconds / 10);
            Console.WriteLine($"Preparing response from database to dto:{elapsedTime}");
            return result;
        }

        #region Report CRUD
        /// <summary>
        /// Seve report criterias in ds
        /// </summary>
        /// <param name="report"></param>
        /// <returns></returns>
        //public bool SaveReport(ReportDto report)
        //{
        //    var request = QueryProvider.CreateQuery<InsertReportStoredProcedure>("big");
        //    request.Name = report.Name;
        //    request.ReportType = (int)report.ReportType;
        //    request.Author = string.Format("{0} {1}", ExecutingUser.LastName, ExecutingUser.FirstName);
        //    request.Criterias = report.Filter;
        //    var result = request.Execute();
        //    if (!result.NonZeroReturnCode)
        //    {
        //        report.Author = request.Author;
        //        report.Created = DateTime.Now;
        //        report.ReportId = request.ReportId;
        //    }
        //    return !result.NonZeroReturnCode;
        //}
        /// <summary>
        /// Get report by id
        /// </summary>
        /// <param name="reportId"></param>
        /// <returns></returns>
        //public ReportDto SelectReport(int reportId)
        //{
        //    return GetReports(reportId).FirstOrDefault();
        //}

        /// <summary>
        /// Get all reports
        /// </summary>
        /// <param name="type">Optional value, allow choose type of report</param>
        /// <returns></returns>
        //public IList<ReportDto> SelectReports(ReportTypes? type = null)
        //{
        //    return GetReports(type: type);
        //}

        //IList<ReportDto> GetReports(int? reportId = null, ReportTypes? type = null)
        //{
        //    int? repType = null;
        //    if (type != null) repType = (int)type;
        //    var query = QueryProvider.CreateQuery<SelectReportStoredProcedure>("big");
        //    query.ReportId = reportId;
        //    query.ReportType = repType;
        //    var result = query.Execute();
        //    return result.HasNoDataRows || result.NonZeroReturnCode ? new List<ReportDto>() : result.ResultToArray<ReportDto>().ToList();
        //}

        /// <summary>
        /// Change report criterias
        /// </summary>
        /// <param name="report"></param>
        /// <returns></returns>
        //public bool UpdateReport(ReportDto report)
        //{
        //    var query = QueryProvider.CreateQuery<UpdateReportStoredProcedure>("big");
        //    query.ReportId = report.ReportId;
        //    query.Name = report.Name;
        //    query.ReportType = (int)report.ReportType;
        //    query.Criterias = report.Filter;
        //    var result = query.Execute();
        //    return !result.NonZeroReturnCode;
        //}

        /// <summary>
        /// Delete report by ID
        /// </summary>
        /// <param name="reportId"></param>
        /// <returns></returns>
        //public bool DeleteReport(int reportId)
        //{
        //    var query = QueryProvider.CreateQuery<DeleteReportStoredProcedure>("big");
        //    query.ReportId = reportId;
        //    var result = query.Execute();
        //    return !result.NonZeroReturnCode;
        //}
        #endregion


        #region billDet
        public IEnumerable<BillInvoiceDto> GetAllBills(int? transactionId = null)
        {
            var selectBillsSp = QueryProvider.CreateQuery<SelectBillDetailsStoredProcedure>(ConfigurationManager.AppSettings["schema"]);
           // selectBillsSp.FamilyId = familyId;
            selectBillsSp.TransactionId = transactionId;
            var spResult = selectBillsSp.Execute();
            var res = spResult.DataRows != null ? spResult.ResultToArray<BillInvoiceDto>() : StringUtils.ArrayUtils.Empty<BillInvoiceDto>();
            return RecalculateAmount(res);
        }
        public static IEnumerable<BillInvoiceDto> RecalculateAmount(IEnumerable<BillInvoiceDto> bills)
        {
            return bills.Select((b) => {
                b.Amount = -b.Amount;
                if (b.BillDetails != null)
                    b.BillDetails = b.BillDetails.Select((bt) =>
                    {
                        bt.Amount = -bt.Amount;
                        return bt;
                    }).ToArray();
                return b;
            });
        }
        #endregion
        #region private methods

        /// <summary>
        /// Convert response from database and returl list of ContactReportResultDto
        /// </summary>
        /// <param name="rep">List objects from DB</param>
        /// <returns></returns>
        IList<ContactReportResultDto> ConvertResultToContactReport(IList<ContactReportDto> rep)
        {
            var fIds = rep.Select(e => e.FamilyID).Distinct();
            var result = new List<ContactReportResultDto>();
            foreach (int fId in fIds)
            {
                var famRows = rep.Where(r => r.FamilyID == fId);
                var famiyInfo = famRows.FirstOrDefault();
                result.Add(new ContactReportResultDto()
                {
                    FamilyID = famiyInfo.FamilyID,
                    FamilyName = famiyInfo.FamilyName,
                    FamilySalutation = famiyInfo.FamilySalutation,
                    HerSalutation = famiyInfo.HerSalutation,
                    HisSalutation = famiyInfo.HisSalutation,
                    FamilyLabel = famiyInfo.FamilyLabel,
                    HisLabel = famiyInfo.HisLabel,
                    HerLabel = famiyInfo.HerLabel,
                    Addresses = famRows.GroupBy(e => e.AddressID).Select(r => r.FirstOrDefault()).Select(s => new ContactReportAddress()
                    {
                        City = s.City,
                        Address = s.Address,
                        State = s.State,
                        Zip = s.Zip,
                        Country = s.Country,
                        CompanyName = s.CompanyName,
                        AddressID = s.AddressID,
                        AddrCurrent = s.AddrCurrent,
                        AddrNoMail = s.AddrNoMail,
                        AddrPrimary = s.AddrPrimary,
                        Address2 = s.Address2,
                        AddressTypeID = s.AddressTypeID
                    }).ToList(),
                    Members = famRows.GroupBy(m => m.MemberID).Select(e => e.First()).Select(e => new ContactReportMambers()
                    {
                        FirstName = e.FirstName,
                        LastName = e.LastName,
                        MemberID = e.MemberID,
                        Title = e.Title,
                        TypeID = e.TypeID,
                        Contacts = famRows.Where(t => t.MemberID == e.MemberID).Select(w => new ContactReportContactInfo()
                        {
                            IsPrimary = w.IsPrimary,
                            NoCall = w.NoCall,
                            PhoneNumber = w.PhoneNumber,
                            PhoneTypeID = w.PhoneTypeID
                        }).GroupBy(r => r.PhoneNumber).Select(c => c.First()).ToList()

                    }).GroupBy(r => r.MemberID).Select(c => c.First()).ToList()
                });
            }
            return result;
        }

        /// <summary>
        /// Convert respose from db and convert to transaction report
        /// </summary>
        /// <param name="result">ref obj result</param>
        /// <param name="response">response from db</param>
        /// <param name="reportType">Report type: 1 - payment, 2 - bill</param>
        /// <param name="filter"></param>
        void ConvertResultToTransactionReport(TransactionReportResultDto result, IList<TransactionsReportDto> response, TransFilterType reportType, FilterTransactionReport filter)
        {
            List<TransactionsReportList> unassignedpayments;
            List<int> categoriesIds;
            var familiesIds = response.AsParallel().Select(r => r.FamilyID).Distinct().ToList();
            result.Families = FillFamilies(familiesIds, response);
            result.Transactions = FillTransactions(response, reportType, out unassignedpayments, out categoriesIds).ToList();
            result.UnassignedPayments = unassignedpayments;
            result.CategoriesIDs = categoriesIds;
            if (filter.ExcludeBillsWithCards != null && (bool)filter.ExcludeBillsWithCards)
                result.Transactions = ExcludeAutoBills(result.Transactions);
        }

        /// <summary>
        /// if bill has a card and date(date due) are larger than current date, this transaction will be skip
        /// </summary>
        /// <param name="resultTransactions"></param>
        /// <returns></returns>
        private List<TransactionsReportList> ExcludeAutoBills(List<TransactionsReportList> resultTransactions)
        {
            var res = new List<TransactionsReportList>();
            //Parallel.ForEach(resultTransactions, trans =>
            //{
            //    lock (res)
            //    {
            //        if (trans.TransType == 2 && trans.PayID != null && trans.Date > DateTime.Now &&
            //            trans.Details.Any(e => e.TransType == 2 && e.DateDue > DateTime.Now)) continue;
            //        res.Add(trans);
            //    }
            //});
            res.AddRange(resultTransactions.AsParallel().Where(trans => trans.TransType != 2 || trans.PayID == null || trans.Date <= DateTime.Now || !trans.Details.Any(e => e.TransType == 2 && e.DateDue > DateTime.Now)));
            return res;
        }


        /// <summary>
        /// Cuild list for families
        /// </summary>
        /// <param name="familiesIds">list of families ids</param>
        /// <param name="response">Response from database</param>
        /// <returns>List of families</returns>
        List<TransactionReportFamily> FillFamilies(List<int> familiesIds, IList<TransactionsReportDto> response)
        {
            var result = new List<TransactionReportFamily>();
            Parallel.ForEach(familiesIds, familyId =>
            {
                var famRows = response.Where(r => r.FamilyID == familyId).ToList();
                var firstFamRow = famRows.First();
                var family = new TransactionReportFamily
                {
                    FamilyID = firstFamRow.FamilyID,
                    Family = firstFamRow.Family,
                    AddressID = firstFamRow.AddressID,
                    FamilySalutation = firstFamRow.FamilySalutation,
                    HisSalutation = firstFamRow.HisSalutation,
                    HerSalutation = firstFamRow.HerSalutation,
                    FamilyLabel = firstFamRow.FamilyLabel,
                    HisLabel = firstFamRow.HisLabel,
                    HerLabel = firstFamRow.HerLabel,
                    Contacts = FillContacts(famRows),
                    Addresses = FillAddress(famRows)
                };
                family.Company = family.Addresses.Any(e => string.IsNullOrEmpty(e.CompanyName))
                    ? family.Addresses.FirstOrDefault(e => string.IsNullOrEmpty(e.CompanyName)).CompanyName
                    : string.Empty;
                lock (result)
                {

                    result.Add(family);
                }
            });
            //foreach (int familyId in familiesIds)
            //{
            //    var famRows = response.Where(r => r.FamilyID == familyId).ToList();
            //    var firstFamRow = famRows.First();
            //    var family = new TransactionReportFamily
            //    {
            //        FamilyID = firstFamRow.FamilyID,
            //        Family = firstFamRow.Family,
            //        AddressID = firstFamRow.AddressID,
            //        FamilySalutation = firstFamRow.FamilySalutation,
            //        HisSalutation = firstFamRow.HisSalutation,
            //        HerSalutation = firstFamRow.HerSalutation,
            //        FamilyLabel = firstFamRow.FamilyLabel,
            //        HisLabel = firstFamRow.HisLabel,
            //        HerLabel = firstFamRow.HerLabel,
            //        Contacts = FillContacts(famRows),
            //        Addresses = FillAddress(famRows)
            //    };
            //    family.Company = family.Addresses.Any(e => string.IsNullOrEmpty(e.CompanyName))
            //        ? family.Addresses.FirstOrDefault(e => string.IsNullOrEmpty(e.CompanyName)).CompanyName
            //        : string.Empty;
            //    result.Add(family);
            //}
            return result;
        }

        /// <summary>
        /// Build contacts list for family
        /// </summary>
        /// <param name="rows"></param>
        /// <returns></returns>
        IList<ContactReportContactInfo> FillContacts(IList<TransactionsReportDto> rows)
        {
            var contacts = new List<ContactReportContactInfo>();
            foreach (TransactionsReportDto row in rows)
            {
                if (contacts.Any(e => e.IsPrimary == row.IsPrimary && e.NoCall == row.NoCall
                                      && string.Equals(e.PhoneNumber, row.PhoneNumber) && e.PhoneTypeID == row.PhoneTypeID && e.MemberType == row.MemberType)) continue;
                contacts.Add(new ContactReportContactInfo
                {
                    PhoneTypeID = row.PhoneTypeID,
                    PhoneNumber = row.PhoneNumber,
                    NoCall = row.NoCall,
                    IsPrimary = row.IsPrimary,
                    MemberType = row.MemberType
                });
            }
            return contacts;
        }

        /// <summary>
        /// Build address list for family
        /// </summary>
        /// <param name="rows">Row from database sorted obly for one faily</param>
        /// <returns></returns>
        IList<ContactReportAddress> FillAddress(IList<TransactionsReportDto> rows)
        {
            var addresses = new List<ContactReportAddress>();
            addresses.AddRange(rows.GroupBy(r => r.AddressID).Select(e => e.First()).Select(w => new ContactReportAddress
            {
                State = w.State,
                Zip = w.Zip,
                Address = w.Address,
                AddressID = w.AddressID,
                AddrCurrent = w.AddrCurrent,
                Country = w.Country,
                City = w.City,
                CompanyName = w.CompanyName,
                AddrNoMail = w.AddrNoMail,
                AddressTypeID = w.AddressTypeID,
                AddrPrimary = w.AddrPrimary,
                Address2 = w.Address2
            }).Distinct().ToList());
            return addresses;
        }

        /// <summary>
        /// Get rows from database and build correct structure for it
        /// </summary>
        /// <param name="response">Response from database</param>
        /// <param name="reportType">0 - bill 1 - payment, 2 - payment/bills </param>
        /// <param name="unassignedPayments">out params in case if reportType 2 we create onother one list for payments which not assigned to bills</param>
        /// <param name="categoriesIds">Out param for contain categories list</param>
        /// <returns>Return correct list of object for transactions</returns>
        List<TransactionsReportList> FillTransactions(IList<TransactionsReportDto> response, TransFilterType reportType, out List<TransactionsReportList> unassignedPayments, out List<int> categoriesIds)
        {
            unassignedPayments = new List<TransactionsReportList>();
            categoriesIds = new List<int>();
            var sorted = new List<TransactionsReportList>();
            // complette list of reports
            sorted.AddRange(response.AsParallel().GroupBy(e => e.TransactionID).Select(t => t.First()).Select(q => new TransactionsReportList
            {
                Amount = q.Amount,
                BillID = q.BillID,
                TransType = q.TransType,
                FamilyID = q.FamilyID,
                AddressID = q.AddressID,
                AuthTransactionId = q.AuthTransactionId,
                AuthNumber = q.AuthNumber,
                TransactionID = q.TransactionID,
                AmountDue = q.AmountDue,
                CheckNo = q.CheckNo,
                Date = q.Date,
                DepartmentID = q.DepartmentID,

                Details = PrepareTransactionsDetails(q.BillDetails, q.PaymentDetails),
                HonorMemory = q.HonorMemory,
                InvoiceNo = q.InvoiceNo,
                IsReceipt = q.IsReceipt,
                LetterID = q.LetterID,
                MailingID = q.MailingID,
                Note = q.Note,
                PayID = q.PayID,
                PaymentMethodID = q.PaymentMethodID,
                ReceiptNo = q.ReceiptNo,
                ReceiptSent = q.ReceiptSent,
                SolicitorID = q.SolicitorID
            }));

            CalculeteAmountDueForTransactionDetails(sorted);
            //fill categoriesIds
            foreach (List<TransactionDetailReportList> details in sorted.Select(e => e.Details))
            {
                foreach (TransactionDetailReportList det in details)
                {
                    if (det.CategoryID != null && !categoriesIds.Contains(det.CategoryID.Value)) categoriesIds.Add(det.CategoryID.Value);
                }
            }
            // if we have only payment or bill type so we no need to make some addition 
            if (reportType != TransFilterType.Bill) return sorted;

            //create list of payments without wich unasignet to bill
            foreach (TransactionsReportList payment in sorted.Where(r => r.TransType == 1))
            {
                if (payment.Details.All(e => e.BillID != null)) continue;
                unassignedPayments.Add(payment);
                var tmpPayment = unassignedPayments.First(e => e.TransactionID == payment.TransactionID);
                if (tmpPayment.Details.Any(w => w.BillID != null))
                {
                    tmpPayment.Details = payment.Details.Where(r => r.BillID == null).ToList();
                }
            }
            var result = new List<TransactionsReportList>();
            // add to bils detail payment detail
            foreach (TransactionsReportList report in sorted.Where(r => r.TransType == 2))
            {
                result.Add(new TransactionsReportList
                {
                    TransactionID = report.TransactionID,
                    FamilyID = report.FamilyID,
                    BillID = report.BillID,
                    AddressID = report.AddressID,
                    InvoiceNo = report.InvoiceNo,
                    Date = report.Date,
                    Amount = report.Amount,
                    AmountDue = report.AmountDue,
                    CheckNo = report.CheckNo,
                    TransType = report.TransType,
                    PayID = report.PayID,
                    AuthNumber = report.AuthNumber,
                    PaymentMethodID = report.PaymentMethodID,
                    IsReceipt = report.IsReceipt,
                    ReceiptNo = report.ReceiptNo,
                    ReceiptSent = report.ReceiptSent,
                    LetterID = report.LetterID,
                    SolicitorID = report.SolicitorID,
                    DepartmentID = report.DepartmentID,
                    MailingID = report.MailingID,
                    HonorMemory = report.HonorMemory,
                    Note = report.Note,
                    AuthTransactionId = report.AuthTransactionId,
                    Details = report.Details

                });
                foreach (List<TransactionDetailReportList> detailReportLists in sorted.Where(p => p.TransType == 1)
                    .Select(r => r.Details))
                {
                    if (detailReportLists.Any(e => e.BillID == report.BillID))
                        result.First(e => e.TransactionID == report.TransactionID)
                            .Details.AddRange(detailReportLists.Where(e => e.BillID == report.BillID));
                    // report.Details.AddRange(detailReportLists.Where(e => e.BillID == report.BillID));
                }
            }
            //foreach (TransactionsReportList report in sorted.Where(r => r.TransType == 2))
            //{
            //    result.Add(new TransactionsReportList
            //    {
            //        TransactionID = report.TransactionID,
            //        FamilyID = report.FamilyID,
            //        BillID = report.BillID,
            //        AddressID = report.AddressID,
            //        InvoiceNo = report.InvoiceNo,
            //        Date = report.Date,
            //        Amount = report.Amount,
            //        AmountDue = report.AmountDue,
            //        CheckNo = report.CheckNo,
            //        TransType = report.TransType,
            //        PayID = report.PayID,
            //        AuthNumber = report.AuthNumber,
            //        PaymentMethodID = report.PaymentMethodID,
            //        IsReceipt = report.IsReceipt,
            //        ReceiptNo = report.ReceiptNo,
            //        ReceiptSent = report.ReceiptSent,
            //        LetterID = report.LetterID,
            //        SolicitorID = report.SolicitorID,
            //        DepartmentID = report.DepartmentID,
            //        MailingID = report.MailingID,
            //        HonorMemory = report.HonorMemory,
            //        Note = report.Note,
            //        AuthTransactionId = report.AuthTransactionId,
            //        Details = report.Details

            //    });
            //    foreach (List<TransactionDetailReportList> detailReportLists in sorted.Where(p => p.TransType == 1).Select(r => r.Details))
            //    {
            //        if (detailReportLists.Any(e => e.BillID == report.BillID))
            //            result.First(e => e.TransactionID == report.TransactionID).Details.AddRange(detailReportLists.Where(e => e.BillID == report.BillID));
            //        // report.Details.AddRange(detailReportLists.Where(e => e.BillID == report.BillID));
            //    }
            //}

            //return only bills which contains also payment details
            return result;
        }

        /// <summary>
        /// Calculate and fill amount due for details onsode transaction
        /// </summary>
        /// <param name="list"></param>
        private void CalculeteAmountDueForTransactionDetails(List<TransactionsReportList> list)
        {
            //firs grouped by family
            Parallel.ForEach(list.GroupBy(r => r.FamilyID).Select(e => e.Select(t => t)).ToList(), trans =>
            {

                if (!trans.Any(e => e.TransType == 1) || !trans.Any(e => e.TransType == 2)) return;
                //get all transaction grouped by bill id
                var transBils = trans.Where(r => r.TransType == 2 && r.BillID != null)
                    .GroupBy(t => t.BillID)
                    .Select(e => e.Select(q => q))
                    .ToList();
                foreach (var trBill in transBils)
                {
                    int? billId = trBill.First().BillID;
                    if (billId == null || trans.Where(w => w.TransType == 1)
                            .Any(r => r.Details.All(e => e.BillID != billId))) continue;
                    foreach (TransactionsReportList tr in trBill)
                    {
                        foreach (TransactionDetailReportList detail in tr.Details)
                        {
                            var payment = trans.Where(e => e.TransType == 1 &&
                                                           e.Details.Any(
                                                               w => w.TransType == 1 &&
                                                                    w.BillID == detail.BillID &&
                                                                    w.CategoryID == detail.CategoryID &&
                                                                    w.SubcategoryID == detail.SubcategoryID))
                                .Select(q => q.Details.FirstOrDefault(
                                    t => t.TransType == 1 && t.CategoryID == detail.CategoryID &&
                                         t.SubcategoryID == detail.SubcategoryID))
                                .ToList();
                            if (!payment.Any()) continue;
                            detail.AmountDue = detail.Amount + payment.First().Amount;
                        }
                    }

                }

            });
            //foreach (var trans in list.GroupBy(r => r.FamilyID).Select(e => e.Select(t => t)).ToList())
            //{
            //    if (!trans.Any(e => e.TransType == 1) || !trans.Any(e => e.TransType == 2)) continue;
            //    //get all transaction grouped by bill id
            //    var transBils = trans.Where(r => r.TransType == 2 && r.BillID != null)
            //        .GroupBy(t => t.BillID)
            //        .Select(e => e.Select(q => q)).ToList();
            //    foreach (var trBill in transBils)
            //    {
            //        int? billId = trBill.First().BillID;
            //        if (billId == null || trans.Where(w => w.TransType == 1).Any(r => r.Details.All(e => e.BillID != billId))) continue;
            //        foreach (TransactionsReportList tr in trBill)
            //        {
            //            foreach (TransactionDetailReportList detail in tr.Details)
            //            {
            //                var payment = trans.Where(e => e.TransType == 1 &&
            //                                               e.Details.Any(
            //                                                   w => w.TransType == 1 &&
            //                                                        w.BillID == detail.BillID &&
            //                                                        w.CategoryID == detail.CategoryID &&
            //                                                        w.SubcategoryID == detail.SubcategoryID))
            //                    .Select(q => q.Details.FirstOrDefault(t => t.TransType == 1 && t.CategoryID == detail.CategoryID &&
            //                                                               t.SubcategoryID == detail.SubcategoryID)).ToList();
            //                if (!payment.Any()) continue;
            //                detail.AmountDue = detail.Amount + payment.First().Amount;
            //            }
            //        }

            //    }
            //}
        }

        /// <summary>
        /// Parse transaction details from XML
        /// </summary>
        /// <param name="billDetails">Bill details XML string</param>
        /// <param name="paymentDetails">Payment details XML string</param>
        /// <returns></returns>
        private List<TransactionDetailReportList> PrepareTransactionsDetails(string billDetails, string paymentDetails)
        {
            //parse dateils from string
            List<TransactionDetailReportList> details;
            details = string.IsNullOrEmpty(billDetails) && string.IsNullOrEmpty(paymentDetails)
                ? new List<TransactionDetailReportList>()
                : ParseTransactionDetails(string.IsNullOrEmpty(billDetails) ? paymentDetails : billDetails
                        , string.IsNullOrEmpty(billDetails) ? "Payment" : "Bill")
                    .ToList();
            if (!details.Any()) return details;

            details = details.GroupBy(q => new { q.CategoryID, q.SubcategoryID }).Select(w => new TransactionDetailReportList
            {
                Amount = w.Sum(r => r.Amount),
                //in case if havent any payment for bill
                AmountDue = w.Sum(r => r.Amount),
                TransType = w.First().TransType,
                ClassID = w.First().ClassID,
                SubclassID = w.First().SubclassID,
                SubcategoryID = w.First().SubcategoryID,
                CategoryID = w.First().CategoryID,
                BillID = w.First().BillID,
                CardCharged = w.First().CardCharged,
                Category = w.First().Category,
                Class = w.First().Category,
                DateDue = w.First().DateDue,
                Note = w.First().Note,
                Quantity = w.First().Quantity,
                Subcategory = w.First().Subcategory,
                Subclass = w.First().Subclass,
                TransactionDetailID = w.First().TransactionDetailID,
                UnitPrice = w.First().UnitPrice
            }).ToList();


            return details;
        }
        #region Parse xml

        /// <summary>
        /// Convert xml string with transactions detail to object
        /// </summary>
        /// <param name="xmlString">xml string for payment or bill</param>
        /// <param name="nodeName">could be "Payment" or "Bill" </param>
        /// <returns>List of transaction detail objects</returns>
        IList<TransactionDetailReportList> ParseTransactionDetails(string xmlString, string nodeName)
        {

            var details = new List<TransactionDetailReportList>();
            var nodes = _xmlService.GetXMLElements(nodeName, xmlString);
            string transDetId;
            string billId;
            string catId;
            string subCatId;
            string amount;
            string category;
            string subcategory;
            string datedue;
            string quantity;
            string unitPrice;
            string cardCharged;
            string classId;
            string subclassId;
            var culturePointer = CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator;

            foreach (XmlElement node in nodes)
            {
                transDetId = CheckXmlValue(_xmlService.GetTransactionDetailsIds(node.OuterXml));
                billId = CheckXmlValue(_xmlService.GetBillIDs(node.OuterXml));
                catId = CheckXmlValue(_xmlService.GetCategoryID(node.OuterXml));
                classId = CheckXmlValue(_xmlService.GetClassId(node.OuterXml));
                subclassId = CheckXmlValue(_xmlService.GetSubclassId(node.OuterXml));
                subCatId = CheckXmlValue(_xmlService.GetSubcategoryID(node.OuterXml));
                amount = CheckXmlValue(_xmlService.GetAmount(node.OuterXml));
                category = CheckXmlValue(_xmlService.GetCategory(node.OuterXml));
                subcategory = CheckXmlValue(_xmlService.GetSubcategory(node.OuterXml));
                datedue = CheckXmlValue(_xmlService.GetDateDue(node.OuterXml));
                quantity = CheckXmlValue(_xmlService.GetQuantity(node.OuterXml));
                unitPrice = CheckXmlValue(_xmlService.GetUnitPrice(node.OuterXml));
                cardCharged = CheckXmlValue(_xmlService.GetCardCharged(node.OuterXml));
                var det = new TransactionDetailReportList();
                det.TransactionDetailID = string.IsNullOrEmpty(transDetId) ? default(int?) : Convert.ToInt32(transDetId);
                det.BillID = string.IsNullOrEmpty(billId) ? default(int?) : Convert.ToInt32(billId);
                det.CategoryID = string.IsNullOrEmpty(catId) ? default(int?) : Convert.ToInt32(catId);
                det.SubcategoryID = string.IsNullOrEmpty(subCatId) ? default(int?) : Convert.ToInt32(subCatId);
                det.Amount = string.IsNullOrEmpty(amount) ? default(decimal) : decimal.Parse(amount.Replace(",", culturePointer), CultureInfo.InvariantCulture);
                det.Category = category;
                det.Subcategory = subcategory;
                det.ClassID = string.IsNullOrEmpty(classId) ? default(int?) : Convert.ToInt32(classId);
                det.SubclassID = string.IsNullOrEmpty(subclassId) ? default(int?) : Convert.ToInt32(subclassId);
                det.TransType = string.Equals(nodeName, "Payment") ? 1 : 2;
                if (nodeName == "Bill")
                {
                    det.DateDue = string.IsNullOrEmpty(datedue) ? default(DateTime?) : DateTime.Parse(datedue);
                    det.Quantity = string.IsNullOrEmpty(quantity) ? default(int?) : Convert.ToInt32(quantity);
                    det.UnitPrice = string.IsNullOrEmpty(unitPrice) ? default(decimal?) : decimal.Parse(unitPrice.Replace(",", culturePointer), CultureInfo.InvariantCulture);
                    det.CardCharged = string.IsNullOrEmpty(cardCharged) ? default(bool?) : string.Equals(cardCharged, "1");
                }
                details.Add(det);

            }
            return details;
        }

        string CheckXmlValue(XmlNodeList node)
        {
            if (node.Count > 0)
                return node[0].InnerText;
            return string.Empty;
        }
        #endregion
        #endregion

    }
}

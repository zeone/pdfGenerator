using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClasicConsole;
using Newtonsoft.Json;

namespace ClasicConsole.Reports.DTO
{
    public class GroupedContactReport
    {
        public string Name { get; set; }
        public List<ContactReportResultDto> Contacts { get; set; }
    }

    public class ContactReportResultDto
    {
        public int FamilyID { get; set; }
        public string FamilyName { get; set; }
        public string FamilySalutation { get; set; }
        public string HisSalutation { get; set; }
        public string HerSalutation { get; set; }
        public string FamilyLabel { get; set; }
        public string HisLabel { get; set; }
        public string HerLabel { get; set; }
        public IList<ContactReportAddress> Addresses { get; set; }
        public IList<ContactReportMambers> Members { get; set; }
    }

    public class ContactReportMambers : ICloneable
    {
        public int MemberID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Title { get; set; }
        public int TypeID { get; set; }
        public IList<ContactReportContactInfo> Contacts { get; set; }

        public object Clone()
        {
            return new ContactReportMambers
            {
                MemberID = MemberID,
                Contacts = Contacts.Select(r => (ContactReportContactInfo) r.Clone()).ToList(),
                FirstName = FirstName,
                Title = Title,
                TypeID = TypeID,
                LastName = LastName

            };
        }
    }

    public class ContactReportContactInfo : ICloneable
    {
        public string PhoneNumber { get; set; }

        public int PhoneTypeID { get; set; }

        //is primary contact
        public bool? IsPrimary { get; set; }

        //chack if can we call for member
        public bool? NoCall { get; set; }

        public int MemberType { get; set; }

        public object Clone()
        {
            return new ContactReportContactInfo
            {
                PhoneTypeID = this.PhoneTypeID,
                MemberType = this.MemberType,
                PhoneNumber = this.PhoneNumber,
                NoCall = this.NoCall,
                IsPrimary = IsPrimary
            };
        }
    }

    public class ContactReportAddress : ICloneable
    {
        public int AddressID { get; set; }
        public int? AddressTypeID { get; set; }
        public string CompanyName { get; set; }
        public string Address { get; set; }
        public string Address2 { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public bool AddrPrimary { get; set; }
        public bool AddrCurrent { get; set; }
        public bool AddrNoMail { get; set; }

        public object Clone()
        {
            return new ContactReportAddress
            {
                AddressID = this.AddressID,
                AddressTypeID = this.AddressTypeID,
                CompanyName = this.CompanyName,
                Address = this.Address,
                Address2 = this.Address2,
                Country = this.Country,
                City = this.City,
                State = this.State,
                Zip = this.Zip,
                AddrPrimary = this.AddrPrimary,
                AddrCurrent = this.AddrCurrent,
                AddrNoMail = this.AddrNoMail
            };
        }
    }

    public class FilterContactReport
    {
        /// 0 - family, 1 - Member for contacts
        ///  0 - bill 1 - payment, 2 - payment/bills for transactions
        public TransFilterType ReportType { get; set; }

        public bool? GlobalAndOr { get; set; }

        //family categories
        public int?[] FCategoryIds { get; set; }

        public int?[] ExcludeFCategoryIds { get; set; }
        public DateTime? FCatEnrolledDate { get; set; }

        /// false - AND, true - OR
        public bool? FCatAndOr { get; set; }

        public bool? FCatExAndOr { get; set; }

        public bool FCatCurrent { get; set; }

        //member categories
        public int?[] MCategoryIds { get; set; }

        public int?[] ExcludeMCategoryIds { get; set; }
        public DateTime? MCatEnrolledDate { get; set; }

        /// false - AND, true - OR
        public bool? MCatAndOr { get; set; }

        public bool? MCatExAndOr { get; set; }

        public bool MCatCurrent { get; set; }

        //member criterias
        public int?[] RelationsLimit { get; set; }

        public bool? NotBirthDate { get; set; }

        public string Gender { get; set; }

        public DateTime? MinBirthDate { get; set; }

        public DateTime? MaxBirthDate { get; set; }

        /// 0 - not include 1 - include
        public int? MemberType { get; set; }

        // Contacts params

        ///1 - use only primary contacts, 0 - use all contacts
        public bool? PrimaryOnly { get; set; }

        ///0 - use only aloowed contacts, 1 - use all contacts
        public bool? ExcludeNC { get; set; }

        //Address 
        public string Country { get; set; }

        public bool? ExCountry { get; set; }
        public string City { get; set; }
        public bool? ExCity { get; set; }
        public string State { get; set; }
        public bool? ExState { get; set; }
        public string[] Zip { get; set; }
        public bool? ExZip { get; set; }

        /// false - AND, true - OR
        public bool? AddressAndOr { get; set; }

        /// false - all addresses, true - only primary addresses
        public bool AddressPrymary { get; set; }

        /// false - all addresses, true - only addresses marked for mail
        public bool? AddressExNM { get; set; }

        ///0 - any, 1 - home 2 - work
        public int? AddressType { get; set; }

        //   params for complete address
        /// /// Filter by requaired fields
        public bool? AddIsComplete { get; set; }

        /// false - skip, true - must contain
        public bool? AddCPAddress { get; set; }

        /// false - skip, true - must contain
        public bool? AddCPCity { get; set; }

        /// false - skip, true - must contain
        public bool? AddCPState { get; set; }

        /// false - skip, true - must contain
        public bool? AddCPZip { get; set; }

        //Transactions
        public bool? NotTransactions { get; set; }

        public int?[] TransSubCatIDs { get; set; }

        ///1 - payment, 2 - bill
        public int? PaymentOrBill { get; set; }

        ///0-SUM, 1-MAX, 2-LAST
        public int? TransactionSort { get; set; }

        public decimal? MinSum { get; set; }
        public decimal? MaxSum { get; set; }

        public DateTime? TransactionStartDate { get; set; }
        public DateTime? TransactionEndDate { get; set; }

        //Other
        /// sorted member by modify date
        public DateTime? CreateFrom { get; set; }

        public DateTime? CreateTo { get; set; }

        /// sorted member by modify date
        public DateTime? ModifyFrom { get; set; }

        public DateTime? ModifyTo { get; set; }

        /// Sorted member by last name
        public char? StartChar { get; set; }

        public char? EndChar { get; set; }
        public IEnumerable<ReportColumn> Columns { get; set; }

        ///additional info
        [JsonIgnore]
        public string Name { get; set; }

        //// 1 - Contact 2 - transactions
        [JsonIgnore]
        public int Type { get; set; }

        [JsonIgnore]
        public int? CountRows { get; set; }

        public bool ShowAll { get; set; }
        public bool SortReverse { get; set; }
        public string SortType { get; set; }
        public string GroupBy { get; set; }
        public bool SkipTitles { get; set; }
        public bool ShowNC { get; set; }
    }

    public class ReportColumn
    {
        public string Name { get; set; }
        public ReportColumns? Column { get; set; }
        public TransactioReportColumns? TransactionColumn { get; set; }
        public ReportColumnsFilter Filter { get; set; }
        public int Position { get; set; }
        public bool IsChecked { get; set; }
        public bool ColumnOnly { get; set; }
        public bool IsContact { get; set; }
        public ReportTypes ReportType { get; set; }
        public TransFilterType? TransType { get; set; }
        public string Sort { get; set; }
    }


    public enum ReportColumns
    {
        HomePhone = 1,
        WorkPhone = 2,
        MobilePhone = 3,
        OtherPhone = 4,
        Pager = 5,
        Fax = 6,
        Email = 7,
        EmergencyPhone = 8,
        Address = 9,
        City = 10,
        State = 11,
        Zip = 12,
        Country = 13,
        Company = 14
    }

    public enum TransactioReportColumns
    {
        Company = 0,
        Address = 1,
        CatSubcat = 2,
        CheckNum = 3,
        Date = 4,
        ReceiptNum = 5,
        Solicitor = 6,
        Mailing = 7,
        Department = 8,
        InvoiceNum = 9,
        Note = 10,
        Method = 11,
        DateDue = 12,
        Email = 13,
        MobilePhone = 14,
        HomePhone = 15,
        WorkPhone = 16,
        OtherPhone = 17,
        Pager = 18,
        EmergencyPhone = 19,
        Fax = 20,
        Quantity = 21
    }

    public enum ReportColumnsFilter
    {
        All,
        With,
        Without
    }


    public class TransactionReportResultDto
    {
        public List<TransactionReportFamily> Families { get; set; }
        public List<int> CategoriesIDs { get; set; }
        public List<TransactionsReportList> Transactions { get; set; }
        public List<TransactionsReportList> UnassignedPayments { get; set; }
    }

    public class TransactionsReportList
    {
        public int TransactionID { get; set; }
        public int FamilyID { get; set; }
        public int? BillID { get; set; }
        public int? InvoiceNo { get; set; }
        public int? AddressID { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public decimal? AmountDue { get; set; }
        public string CheckNo { get; set; }
        public int TransType { get; set; }
        public int? PayID { get; set; }
        public string AuthNumber { get; set; }
        public int? PaymentMethodID { get; set; }
        public bool? IsReceipt { get; set; }
        public int? ReceiptNo { get; set; }
        public bool? ReceiptSent { get; set; }
        public int? LetterID { get; set; }
        public int? SolicitorID { get; set; }
        public int DepartmentID { get; set; }
        public int? MailingID { get; set; }
        public string HonorMemory { get; set; }
        public string Note { get; set; }
        public string AuthTransactionId { get; set; }
        public List<TransactionDetailReportList> Details { get; set; }
    }

    public class TransactionDetailReportList
    {
        public int? TransactionDetailID { get; set; }
        public int? BillID { get; set; }
        public DateTime? DateDue { get; set; }
        public int? CategoryID { get; set; }
        public int? SubcategoryID { get; set; }
        public int? ClassID { get; set; }
        public int? SubclassID { get; set; }
        public int? Quantity { get; set; }
        public decimal? UnitPrice { get; set; }

        public decimal Amount { get; set; }
        public decimal? AmountDue { get; set; }
        public string Note { get; set; }
        public string Category { get; set; }
        public string Subcategory { get; set; }
        public string Class { get; set; }
        public string Subclass { get; set; }
        public int TransType { get; set; }
        public bool? CardCharged { get; set; }
        public decimal? PaidAmount { get; set; }
    }

    public class TransactionReportFamily
    {
        public int FamilyID { get; set; }
        public string Family { get; set; }
        public int AddressID { get; set; }
        public string FamilySalutation { get; set; }
        public string HisSalutation { get; set; }
        public string HerSalutation { get; set; }
        public string FamilyLabel { get; set; }
        public string HisLabel { get; set; }
        public string HerLabel { get; set; }
        public string Company { get; set; }
        public IList<ContactReportContactInfo> Contacts { get; set; }
        public IList<ContactReportAddress> Addresses { get; set; }

    }


    public class FilterTransactionReport : FilterContactReport
    {
        public bool? ExByTransSubCat { get; set; }

        public DateTime? DateDueFrom { get; set; }
        public DateTime? DateDueTo { get; set; }

        /// <summary>
        /// Billed - use onli billed transactions
        /// Due - use all transactions
        /// </summary>
        public int? DueBilled { get; set; }

        public int? PaidStatus { get; set; }

        /// <summary>
        ///0 false 1 -true used for calculating amount value
        /// </summary>
        public bool? CalcTotalSum { get; set; }

        /// <summary>
        /// Credit cards, cash, check, other
        /// </summary>
        public int?[] MethodTypes { get; set; }

        public int?[] SolicitorIDs { get; set; }

        public bool? ExSolicitors { get; set; }
        public int?[] DepartmentIDs { get; set; }
        public bool? ExByDepartment { get; set; }
        public int?[] ExFamilyIds { get; set; }
        public List<int> FamilyIds { get; set; }

        /// <summary>
        /// used for checking if we need get family members first
        /// </summary>
        public bool HasFamilyCriterias { get; set; }

        /// <summary>
        /// If bill has a card and date due more than current, will skip that bill
        /// </summary>
        public bool? ExcludeBillsWithCards { get; set; }

        public int? ReceiptNumberMin { get; set; }

        public int? ReceiptNumberMax { get; set; }

        //saving addition params
        /// <summary>
        ///transaction report type 
        ///  1- Payment
        ///  2 - Bill
        /// </summary>
        public int? type { get; set; }

        /// <summary>
        /// Show detail or total report
        /// 0 - details
        /// 1 - total
        /// </summary>
        public TransFilterView? view { get; set; }

        /// <summary>
        /// use for matrix view
        /// </summary>
        public string totalOnlyBy { get; set; }

        /// <summary>
        ///use for grouping 
        /// </summary>
        public string subtotalBy { get; set; }

        /// <summary>
        /// if we have some family id from other criteria, it will be work like "OR"
        /// in other case lit will be wor like "AND"
        /// </summary>
        public bool? FamilyOtherCriteria { get; set; }

        public bool ShowDetails { get; set; }
        public bool ShowUnasinged { get; set; }
    }



    public class ReportDto
    {
        FilterTransactionReport _filter;
        public int ReportId { get; set; }
        public ReportTypes ReportType { get; set; }
        public string Name { get; set; }
        public DateTime? Created { get; set; }
        public string Author { get; set; }

        [JsonIgnore]
        public string Filter { get; set; }

        public FilterTransactionReport Criteria
        {
            get
            {
                if (_filter == null && Filter != null)
                    _filter = JsonConvert.DeserializeObject<FilterTransactionReport>(Filter);
                return _filter;
                //return Filter == null ? new FilterContactReport() : JsonConvert.DeserializeObject<FilterContactReport>(Filter);
            }
            set
            {
                _filter = value;

                if (_filter != null) Filter = JsonConvert.SerializeObject(value);
            }
        }
    }

    public enum ReportTypes
    {
        ContactReport = 1,
        TransactionReport = 2
    }

}
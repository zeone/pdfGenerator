using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClasicConsole.Reports.DTO
{
    public class TransactionsReportDto
    {
        public int TransactionID { get; set; }
        public int FamilyID { get; set; }
        public string Family { get; set; }
        public int? MemberID { get; set; }
        public int AddressID { get; set; }
        public int? BillID { get; set; }
        public int? InvoiceNo { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public decimal? AmountDue { get; set; }
        public string CheckNo { get; set; }
        public byte TransType { get; set; }
        public int? PayID { get; set; }
        public string AuthNumber { get; set; }
        public byte? PaymentMethodID { get; set; }
        public bool? IsReceipt { get; set; }
        public int? ReceiptNo { get; set; }
        public bool? ReceiptSent { get; set; }
        public short? LetterID { get; set; }
        public int? SolicitorID { get; set; }
        public int DepartmentID { get; set; }
        public int? MailingID { get; set; }
        public string HonorMemory { get; set; }
        public string Note { get; set; }
        public string AuthTransactionId { get; set; }
        public string BillDetails { get; set; }
        public string PaymentDetails { get; set; }
        public byte? AddressTypeID { get; set; }
        public string CompanyName { get; set; }
        public string Address { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string Country { get; set; }
        public bool AddrPrimary { get; set; }
        public bool AddrCurrent { get; set; }
        public bool AddrNoMail { get; set; }
        public string FamilySalutation { get; set; }
        public string HisSalutation { get; set; }
        public string HerSalutation { get; set; }
        public string FamilyLabel { get; set; }
        public string HisLabel { get; set; }
        public string HerLabel { get; set; }
        public string PhoneNumber { get; set; }
        public byte PhoneTypeID { get; set; }
        public bool IsPrimary { get; set; }
        public bool NoCall { get; set; }
        public int MemberType { get; set; }
    }
}

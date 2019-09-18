using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClasicConsole.Reports.DTO;

namespace ClasicConsole.DTO
{
    public class TransactionGrouped
    {
        public decimal? GrandAmount { get; set; }
        public decimal? GrandDue { get; set; }
        public decimal? GrandPaid { get; set; }
        public decimal? GrandUnasignedAmount { get; set; }
        public List<TransactionReportGroupedDTO> GroupedObj { get; set; }
    }
    public class TransactionReportGroupedDTO : ICloneable
    {
        public string Name { get; set; }
        public string TotalName { get; set; }
        public string DateName { get; set; }
        public int? ID { get; set; }
        public string Company { get; set; }
        public string Category { get; set; }
        public int? CategoryId { get; set; }
        public int? SubcategoryId { get; set; }
        public string Subcategory { get; set; }
        public string Solicitor { get; set; }
        public string Method { get; set; }
        public string Mailing { get; set; }
        public string Account { get; set; }
        public string Department { get; set; }
        public int? Weeks { get; set; }
        public int? Month { get; set; }
        public int? Years { get; set; }
        public decimal Amount { get; set; }
        //public List<TransactionsReportList> SubGroup { get; set; }
        public List<TransactionsReportList> Transactions { get; set; }
        public List<TransactionsReportList> UnassignedPayments { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal BillTotalAmount { get; set; }
        public decimal PaidTotalAmount { get; set; }
        public decimal DueTotalAmount { get; set; }
        public decimal UnasignedAmount { get; set; }
        public decimal GrandAmount { get; set; }
        public decimal GrandPaid { get; set; }
        public decimal GrandDue { get; set; }
        public decimal GrandUnasignedAmount { get; set; }
        public FamilyDetails FamilyDetails { get; set; }
        public List<TransactionReportGroupedDTO> SubGrouped { get; set; }

        public object Clone()
        {
            return new TransactionReportGroupedDTO
            {
                Name = this.Name,
                TotalName = this.TotalName,
                ID = this.ID,
                Company = this.Company,
                Category = this.Category,
                CategoryId = this.CategoryId,
                SubcategoryId = this.SubcategoryId,
                Subcategory = this.Subcategory,
                Solicitor = this.Solicitor,
                Method = this.Method,
                Mailing = this.Mailing,
                Account = this.Account,
                Department = this.Department,
                Weeks = this.Weeks,
                Month = this.Month,
                Years = this.Years,
                Amount = this.Amount,
                Transactions = new List<TransactionsReportList>(),
                UnassignedPayments = new List<TransactionsReportList>(),
                TotalAmount = this.TotalAmount,
                BillTotalAmount = this.BillTotalAmount,
                PaidTotalAmount = this.PaidTotalAmount,
                DueTotalAmount = this.DueTotalAmount,
                UnasignedAmount = this.UnasignedAmount,
                GrandAmount = this.GrandAmount,
                GrandPaid = this.GrandPaid,
                GrandDue = this.GrandDue,
                GrandUnasignedAmount = this.GrandUnasignedAmount,
                FamilyDetails = this.FamilyDetails,
                SubGrouped = new List<TransactionReportGroupedDTO>(),
                DateName = DateName
            };
        }
    }

    public class FamilyDetails
    {
        public IList<ContactReportContactInfo> Contacts { get; set; }
        public IList<ContactReportAddress> Addresses { get; set; }
    }

    public class MatrixDTO: TransactionGrouped
    {
        public List<TransactionMatrixColumn> Columns { get; set; }
        public List<TransactionMatrixRows> Rows { get; set; }
    }

    public class TransactionMatrixColumn
    {
        public string Name { get; set; }
        public int Value { get; set; }
    }

    public class TransactionMatrixRows
    {
        public string Name { get; set; }
        public string TotalName { get; set; }
        public FamilyDetails FamilyDetails { get; set; }
        public List<decimal> Amounts { get; set; }
        public List<TransactioMatrixSubDetails> SubDetails { get; set; }

    }

    public class TransactioMatrixSubDetails
    {
        public string Name { get; set; }
        public List<decimal> Amounts { get; set; }
    }
}


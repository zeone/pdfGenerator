using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClasicConsole.Reports.DAL;
using ClasicConsole.Reports.DTO;

namespace ClasicConsole.Reports.SP
{
    public class SelectTransactionsReportStoredProcedure : StoredProcedureReturningSelectResultQuery<TransactionsReportDto>
    {
        public SelectTransactionsReportStoredProcedure(IQueryProvider provider, PerSchemaSqlDbContext dbContext) : base(provider, dbContext)
        {
        }

        protected override string GetCommandText()
        {
            return "dbo.SelectTransactionReport";
        }

        // 0 - bill 1 - payment, 2 - payment/bills
        public int ReportType
        {
            get { return GetParameter<int>("@ReportType"); }
            set
            {
                SetParameter(paramName: "@ReportType",
                    paramValue: value,
                    isNullable: false,
                    sqlDbType: SqlDbType.Int,
                    direction: ParameterDirection.Input);
            }
        }

        public int?[] TransSubCatIDs
        {
            get { return GetParameter<IntArrayDataTable>("@TransSubCatIDs").GetItems(); }
            set
            {
                var table = new IntArrayDataTable();
                table.AddItems(value);
                SetParameter(paramName: "@TransSubCatIDs",
                    paramValue: table,
                    isNullable: false,
                    sqlDbType: SqlDbType.Structured,
                    direction: ParameterDirection.Input);
            }
        }
        public bool? ExByTransSubCat
        {
            get { return GetParameter<bool?>("@ExByTransSubCat"); }
            set
            {
                SetParameter(paramName: "@ExByTransSubCat",
                    paramValue: value ?? false,
                    isNullable: false,
                    sqlDbType: SqlDbType.Bit,
                    direction: ParameterDirection.Input);
            }
        }

        public DateTime? TransactionStartDate
        {
            get { return GetParameter<DateTime?>("@TransactionStartDate"); }
            set
            {
                SetParameter(paramName: "@TransactionStartDate",
                    paramValue: value,
                    isNullable: true,
                    sqlDbType: SqlDbType.SmallDateTime,
                    direction: ParameterDirection.Input);
            }
        }

        public DateTime? TransactionEndDate
        {
            get { return GetParameter<DateTime?>("@TransactionEndDate"); }
            set
            {
                SetParameter(paramName: "@TransactionEndDate",
                    paramValue: value,
                    isNullable: true,
                    sqlDbType: SqlDbType.SmallDateTime,
                    direction: ParameterDirection.Input);
            }
        }

        public DateTime? DateDueFrom
        {
            get { return GetParameter<DateTime?>("@DateDueFrom"); }
            set
            {
                SetParameter(paramName: "@DateDueFrom",
                    paramValue: value,
                    isNullable: true,
                    sqlDbType: SqlDbType.SmallDateTime,
                    direction: ParameterDirection.Input);
            }
        }

        public DateTime? DateDueTo
        {
            get { return GetParameter<DateTime?>("@DateDueTo"); }
            set
            {
                SetParameter(paramName: "@DateDueTo",
                    paramValue: value,
                    isNullable: true,
                    sqlDbType: SqlDbType.SmallDateTime,
                    direction: ParameterDirection.Input);
            }
        }


        public decimal? MinAmount
        {
            get { return GetParameter<decimal?>("@MinAmount"); }
            set
            {
                SetParameter(paramName: "@MinAmount",
                    paramValue: value,
                    isNullable: true,
                    sqlDbType: SqlDbType.Money,
                    direction: ParameterDirection.Input);
            }
        }

        //public decimal? MaxAmount
        //{
        //    get { return GetParameter<decimal?>("@MaxAmount"); }
        //    set
        //    {
        //        SetParameter(paramName: "@MaxAmount",
        //            paramValue: value,
        //            isNullable: true,
        //            sqlDbType: SqlDbType.Money,
        //            direction: ParameterDirection.Input);
        //    }
        //}

        //public decimal? MinAmountDue
        //{
        //    get { return GetParameter<decimal?>("@MinAmountDue"); }
        //    set
        //    {
        //        SetParameter(paramName: "@MinAmountDue",
        //            paramValue: value,
        //            isNullable: true,
        //            sqlDbType: SqlDbType.Money,
        //            direction: ParameterDirection.Input);
        //    }
        //}

        //public decimal? MaxAmountDue
        //{
        //    get { return GetParameter<decimal?>("@MaxAmountDue"); }
        //    set
        //    {
        //        SetParameter(paramName: "@MaxAmountDue",
        //            paramValue: value,
        //            isNullable: true,
        //            sqlDbType: SqlDbType.Money,
        //            direction: ParameterDirection.Input);
        //    }
        //}

        public int? DueBilled
        {
            get { return GetParameter<int?>("@DueBilled"); }
            set
            {
                SetParameter(paramName: "@DueBilled",
                    paramValue: value ?? 0,
                    isNullable: false,
                    sqlDbType: SqlDbType.Int,
                    direction: ParameterDirection.Input);
            }
        }

        //0 - individual amount 1 - total amount
        public bool? CalcTotalSum
        {
            get { return GetParameter<bool?>("@CalcTotalSum"); }
            set
            {
                SetParameter(paramName: "@CalcTotalSum",
                    paramValue: value ?? false,
                    isNullable: false,
                    sqlDbType: SqlDbType.Bit,
                    direction: ParameterDirection.Input);
            }
        }
        //0 - All 1 - Paid 2- outstanding
        public int? PaidStatus
        {
            get { return GetParameter<int?>("@PaidStatus"); }
            set
            {
                SetParameter(paramName: "@PaidStatus",
                    paramValue: value,
                    isNullable: false,
                    sqlDbType: SqlDbType.Int,
                    direction: ParameterDirection.Input);
            }
        }


        public int?[] MethodTypes
        {
            get { return GetParameter<IntArrayDataTable>("@MethodTypes").GetItems(); }
            set
            {
                var table = new IntArrayDataTable();
                table.AddItems(value);
                SetParameter(paramName: "@MethodTypes",
                    paramValue: table,
                    isNullable: false,
                    sqlDbType: SqlDbType.Structured,
                    direction: ParameterDirection.Input);
            }
        }

        public int?[] SolicitorIDs
        {
            get { return GetParameter<IntArrayDataTable>("@SolicitorIDs").GetItems(); }
            set
            {
                var table = new IntArrayDataTable();
                table.AddItems(value);
                SetParameter(paramName: "@SolicitorIDs",
                    paramValue: table,
                    isNullable: false,
                    sqlDbType: SqlDbType.Structured,
                    direction: ParameterDirection.Input);
            }
        }

        public bool? ExSolicitors
        {
            get { return GetParameter<bool?>("@ExSolicitors"); }
            set
            {
                SetParameter(paramName: "@ExSolicitors",
                    paramValue: value ?? false,
                    isNullable: false,
                    sqlDbType: SqlDbType.Bit,
                    direction: ParameterDirection.Input);
            }
        }

        public int?[] DepartmentIDs
        {
            get { return GetParameter<IntArrayDataTable>("@DepartmentIDs").GetItems(); }
            set
            {
                var table = new IntArrayDataTable();
                table.AddItems(value);
                SetParameter(paramName: "@DepartmentIDs",
                    paramValue: table,
                    isNullable: false,
                    sqlDbType: SqlDbType.Structured,
                    direction: ParameterDirection.Input);
            }
        }

        public bool? ExByDepartment
        {
            get { return GetParameter<bool?>("@ExByDepartment"); }
            set
            {
                SetParameter(paramName: "@ExByDepartment",
                    paramValue: value ?? false,
                    isNullable: false,
                    sqlDbType: SqlDbType.Bit,
                    direction: ParameterDirection.Input);
            }
        }

        public int?[] ExFamilyIds
        {
            get { return GetParameter<IntArrayDataTable>("@ExFamilyIds").GetItems(); }
            set
            {
                var table = new IntArrayDataTable();
                table.AddItems(value);
                SetParameter(paramName: "@ExFamilyIds",
                    paramValue: table,
                    isNullable: false,
                    sqlDbType: SqlDbType.Structured,
                    direction: ParameterDirection.Input);
            }
        }

        public int?[] FamilyIds
        {
            get { return GetParameter<IntArrayDataTable>("@FamilyIds").GetItems(); }
            set
            {
                var table = new IntArrayDataTable();
                table.AddItems(value);
                SetParameter(paramName: "@FamilyIds",
                    paramValue: table,
                    isNullable: false,
                    sqlDbType: SqlDbType.Structured,
                    direction: ParameterDirection.Input);
            }
        }

        public int? CountRows
        {
            get { return GetParameter<int?>("@CountRows"); }
            set
            {
                SetParameter(paramName: "@CountRows",
                    paramValue: value,
                    isNullable: true,
                    sqlDbType: SqlDbType.Int,
                    direction: ParameterDirection.Input);
            }
        }

        public int? ReceiptNumberMin
        {
            get { return GetParameter<int?>("@ReceiptNumberMin"); }
            set
            {
                SetParameter(paramName: "@ReceiptNumberMin",
                    paramValue: value,
                    isNullable: true,
                    sqlDbType: SqlDbType.Int,
                    direction: ParameterDirection.Input);
            }
        }

        public int? ReceiptNumberMax
        {
            get { return GetParameter<int?>("@ReceiptNumberMax"); }
            set
            {
                SetParameter(paramName: "@ReceiptNumberMax",
                    paramValue: value,
                    isNullable: true,
                    sqlDbType: SqlDbType.Int,
                    direction: ParameterDirection.Input);
            }
        }

        public bool? FamilyOtherCriteria
        {
            get { return GetParameter<bool?>("@FamilyOtherCriteria"); }
            set
            {
                SetParameter(paramName: "@FamilyOtherCriteria",
                    paramValue: value ?? false,
                    isNullable: false,
                    sqlDbType: SqlDbType.Bit,
                    direction: ParameterDirection.Input);
            }
        }
    }
}

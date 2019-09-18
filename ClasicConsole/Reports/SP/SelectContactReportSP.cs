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
    public class SelectContactReportStoredProcedure : StoredProcedureReturningSelectResultQuery<ContactReportDto>
    {
        public SelectContactReportStoredProcedure(IQueryProvider provider, PerSchemaSqlDbContext dbContext)
            : base(provider, dbContext)
        { }

        protected override string GetCommandText()
        {
            return "dbo.SelectContactReport";
        }

        // 0 - family, 1 - Member
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
        public bool? GlobalAndOr
        {
            get { return GetParameter<bool?>("@GlobalAndOr"); }
            set
            {
                SetParameter(paramName: "@GlobalAndOr",
                    paramValue: value ?? false,
                    isNullable: true,
                    sqlDbType: SqlDbType.Bit,
                    direction: ParameterDirection.Input);
            }
        }

        //family categories
        public int?[] FCategoryIds
        {
            get { return GetParameter<IntArrayDataTable>("@FCategoryIds").GetItems(); }
            set
            {
                var table = new IntArrayDataTable();
                table.AddItems(value);
                SetParameter(paramName: "@FCategoryIds",
                    paramValue: table,
                    isNullable: false,
                    sqlDbType: SqlDbType.Structured,
                    direction: ParameterDirection.Input);
            }
        }
        public int?[] ExcludeFCategoryIds
        {
            get { return GetParameter<IntArrayDataTable>("@ExcludeFCategoryIds").GetItems(); }
            set
            {
                var table = new IntArrayDataTable();
                table.AddItems(value);
                SetParameter(paramName: "@ExcludeFCategoryIds",
                    paramValue: table,
                    isNullable: false,
                    sqlDbType: SqlDbType.Structured,
                    direction: ParameterDirection.Input);
            }
        }
        public DateTime? FCatEnrolledDate
        {
            get { return GetParameter<DateTime?>("@FCatEnrolledDate"); }
            set
            {
                SetParameter(paramName: "@FCatEnrolledDate",
                    paramValue: value,
                    isNullable: true,
                    sqlDbType: SqlDbType.SmallDateTime,
                    direction: ParameterDirection.Input);
            }
        }
        // false - AND, true - OR
        public bool? FCatAndOr
        {
            get { return GetParameter<bool?>("@FCatAndOr"); }
            set
            {
                SetParameter(paramName: "@FCatAndOr",
                    paramValue: value ?? false,
                    isNullable: true,
                    sqlDbType: SqlDbType.Bit,
                    direction: ParameterDirection.Input);
            }
        }
        public bool? FCatExAndOr
        {
            get { return GetParameter<bool?>("@FCatExAndOr"); }
            set
            {
                SetParameter(paramName: "@FCatExAndOr",
                    paramValue: value ?? false,
                    isNullable: true,
                    sqlDbType: SqlDbType.Bit,
                    direction: ParameterDirection.Input);
            }
        }

        //member categories
        public int?[] MCategoryIds
        {
            get { return GetParameter<IntArrayDataTable>("@MCategoryIds").GetItems(); }
            set
            {
                var table = new IntArrayDataTable();
                table.AddItems(value);
                SetParameter(paramName: "@MCategoryIds",
                    paramValue: table,
                    isNullable: false,
                    sqlDbType: SqlDbType.Structured,
                    direction: ParameterDirection.Input);
            }
        }
        public int?[] ExcludeMCategoryIds
        {
            get { return GetParameter<IntArrayDataTable>("@ExcludeMCategoryIds").GetItems(); }
            set
            {
                var table = new IntArrayDataTable();
                table.AddItems(value);
                SetParameter(paramName: "@ExcludeMCategoryIds",
                    paramValue: table,
                    isNullable: false,
                    sqlDbType: SqlDbType.Structured,
                    direction: ParameterDirection.Input);
            }
        }
        public DateTime? MCatEnrolledDate
        {
            get { return GetParameter<DateTime?>("@MCatEnrolledDate"); }
            set
            {
                SetParameter(paramName: "@MCatEnrolledDate",
                    paramValue: value,
                    isNullable: true,
                    sqlDbType: SqlDbType.SmallDateTime,
                    direction: ParameterDirection.Input);
            }
        }
        // false - AND, true - OR
        public bool? MCatAndOr
        {
            get { return GetParameter<bool?>("@MCatAndOr"); }
            set
            {
                SetParameter(paramName: "@MCatAndOr",
                    paramValue: value ?? false,
                    isNullable: true,
                    sqlDbType: SqlDbType.Bit,
                    direction: ParameterDirection.Input);
            }
        }
        public bool? MCatExAndOr
        {
            get { return GetParameter<bool?>("@MCatExAndOr"); }
            set
            {
                SetParameter(paramName: "@MCatExAndOr",
                    paramValue: value ?? false,
                    isNullable: true,
                    sqlDbType: SqlDbType.Bit,
                    direction: ParameterDirection.Input);
            }
        }

        //member criterias
        public int?[] RelationsLimit
        {
            get { return GetParameter<IntArrayDataTable>("@RelationsLimit").GetItems(); }
            set
            {
                var table = new IntArrayDataTable();
                table.AddItems(value);
                SetParameter(paramName: "@RelationsLimit",
                    paramValue: table,
                    isNullable: false,
                    sqlDbType: SqlDbType.Structured,
                    direction: ParameterDirection.Input);
            }
        }

        public bool? NotBirthDate
        {
            get { return GetParameter<bool?>("@NotBirthDate"); }
            set
            {
                SetParameter(paramName: "@NotBirthDate",
                    paramValue: value ?? false,
                    isNullable: true,
                    sqlDbType: SqlDbType.Bit,
                    direction: ParameterDirection.Input);
            }
        }

        public string Gender
        {
            get { return GetParameter<string>("@Gender"); }
            set
            {
                SetParameter(paramName: "@Gender",
                    paramValue: value,
                    isNullable: true,
                    sqlDbType: SqlDbType.VarChar,
                    direction: ParameterDirection.Input);
            }
        }

        public DateTime? MinBirthDate
        {
            get { return GetParameter<DateTime?>("@MinBirthDate"); }
            set
            {
                SetParameter(paramName: "@MinBirthDate",
                    paramValue: value,
                    isNullable: true,
                    sqlDbType: SqlDbType.DateTime,
                    direction: ParameterDirection.Input);
            }
        }

        public DateTime? MaxBirthDate
        {
            get { return GetParameter<DateTime?>("@MaxBirthDate"); }
            set
            {
                SetParameter(paramName: "@MaxBirthDate",
                    paramValue: value,
                    isNullable: true,
                    sqlDbType: SqlDbType.DateTime,
                    direction: ParameterDirection.Input);
            }
        }

        //  0 - all 1 - primary 2 - non primary 
        public int? MemberType
        {
            get { return GetParameter<int?>("@MemberType"); }
            set
            {
                SetParameter(paramName: "@MemberType",
                    paramValue: value,
                    isNullable: true,
                    sqlDbType: SqlDbType.Int,
                    direction: ParameterDirection.Input);
            }
        }

        // Contacts params

        //1 - use only primary contacts, 0 - use all contacts
        public bool? PrimaryOnly
        {
            get { return GetParameter<bool?>("@PrimaryOnly"); }
            set
            {
                SetParameter(paramName: "@PrimaryOnly",
                    paramValue: value ?? false,
                    isNullable: true,
                    sqlDbType: SqlDbType.Bit,
                    direction: ParameterDirection.Input);
            }
        }

        //0 - use only aloowed contacts, 1 - use all contacts
        public bool? ExcludeNC
        {
            get { return GetParameter<bool?>("@ExcludeNC"); }
            set
            {
                SetParameter(paramName: "@ExcludeNC",
                    paramValue: value ?? false,
                    isNullable: true,
                    sqlDbType: SqlDbType.Bit,
                    direction: ParameterDirection.Input);
            }
        }

        //Address 
        public string Country
        {
            get { return GetParameter<string>("@Country"); }
            set
            {
                SetParameter(paramName: "@Country",
                    paramValue: value,
                    isNullable: true,
                    sqlDbType: SqlDbType.NVarChar,
                    direction: ParameterDirection.Input);
            }
        }
        public bool? ExCountry
        {
            get { return GetParameter<bool?>("@ExCountry"); }
            set
            {
                SetParameter(paramName: "@ExCountry",
                    paramValue: value ?? false,
                    isNullable: true,
                    sqlDbType: SqlDbType.Bit,
                    direction: ParameterDirection.Input);
            }
        }
        public string City
        {
            get { return GetParameter<string>("@City"); }
            set
            {
                SetParameter(paramName: "@City",
                    paramValue: value,
                    isNullable: true,
                    sqlDbType: SqlDbType.NVarChar,
                    direction: ParameterDirection.Input);
            }
        }
        public bool? ExCity
        {
            get { return GetParameter<bool?>("@ExCity"); }
            set
            {
                SetParameter(paramName: "@ExCity",
                    paramValue: value ?? false,
                    isNullable: true,
                    sqlDbType: SqlDbType.Bit,
                    direction: ParameterDirection.Input);
            }
        }
        public string State
        {
            get { return GetParameter<string>("@State"); }
            set
            {
                SetParameter(paramName: "@State",
                    paramValue: value,
                    isNullable: true,
                    sqlDbType: SqlDbType.NVarChar,
                    direction: ParameterDirection.Input);
            }
        }
        public bool? ExState
        {
            get { return GetParameter<bool?>("@ExState"); }
            set
            {
                SetParameter(paramName: "@ExState",
                    paramValue: value ?? false,
                    isNullable: true,
                    sqlDbType: SqlDbType.Bit,
                    direction: ParameterDirection.Input);
            }
        }
        public string[] Zip
        {
            get { return GetParameter<StringArrayDataTable>("@Zip").GetItems(); }
            set
            {
                var table = new StringArrayDataTable();
                table.AddItems(value);
                SetParameter(paramName: "@Zip",
                    paramValue: table,
                    isNullable: false,
                    sqlDbType: SqlDbType.Structured,
                    direction: ParameterDirection.Input);
            }
        }
        public bool? ExZip
        {
            get { return GetParameter<bool?>("@ExZip"); }
            set
            {
                SetParameter(paramName: "@ExZip",
                    paramValue: value ?? false,
                    isNullable: true,
                    sqlDbType: SqlDbType.Bit,
                    direction: ParameterDirection.Input);
            }
        }
        // false - AND, true - OR
        public bool? AddressAndOr
        {
            get { return GetParameter<bool?>("@AddressAndOr"); }
            set
            {
                SetParameter(paramName: "@AddressAndOr",
                    paramValue: value ?? false,
                    isNullable: true,
                    sqlDbType: SqlDbType.Bit,
                    direction: ParameterDirection.Input);
            }
        }
        public bool? AddressPrymary
        {
            get { return GetParameter<bool?>("@AddressPrymary"); }
            set
            {
                SetParameter(paramName: "@AddressPrymary",
                    paramValue: value ?? false,
                    isNullable: true,
                    sqlDbType: SqlDbType.Bit,
                    direction: ParameterDirection.Input);
            }
        }
        public bool? AddressExNM
        {
            get { return GetParameter<bool?>("@AddressExNM"); }
            set
            {
                SetParameter(paramName: "@AddressExNM",
                    paramValue: value ?? false,
                    isNullable: true,
                    sqlDbType: SqlDbType.Bit,
                    direction: ParameterDirection.Input);
            }
        }
        public int? AddressType
        {
            get { return GetParameter<int?>("@AddressType"); }
            set
            {
                SetParameter(paramName: "@AddressType",
                    paramValue: value ?? 0,
                    isNullable: true,
                    sqlDbType: SqlDbType.Bit,
                    direction: ParameterDirection.Input);
            }
        }
        public bool? AddIsComplete
        {
            get { return GetParameter<bool?>("@AddIsComplete"); }
            set
            {
                SetParameter(paramName: "@AddIsComplete",
                    paramValue: value ?? false,
                    isNullable: true,
                    sqlDbType: SqlDbType.Bit,
                    direction: ParameterDirection.Input);
            }
        }
        public bool? AddCPAddress
        {
            get { return GetParameter<bool?>("@AddCPAddress"); }
            set
            {
                SetParameter(paramName: "@AddCPAddress",
                    paramValue: value ?? false,
                    isNullable: true,
                    sqlDbType: SqlDbType.Bit,
                    direction: ParameterDirection.Input);
            }
        }
        public bool? AddCPCity
        {
            get { return GetParameter<bool?>("@AddCPCity"); }
            set
            {
                SetParameter(paramName: "@AddCPCity",
                    paramValue: value ?? false,
                    isNullable: true,
                    sqlDbType: SqlDbType.Bit,
                    direction: ParameterDirection.Input);
            }
        }
        public bool? AddCPState
        {
            get { return GetParameter<bool?>("@AddCPState"); }
            set
            {
                SetParameter(paramName: "@AddCPState",
                    paramValue: value ?? false,
                    isNullable: true,
                    sqlDbType: SqlDbType.Bit,
                    direction: ParameterDirection.Input);
            }
        }
        public bool? AddCPZip
        {
            get { return GetParameter<bool?>("@AddCPZip"); }
            set
            {
                SetParameter(paramName: "@AddCPZip",
                    paramValue: value ?? false,
                    isNullable: true,
                    sqlDbType: SqlDbType.Bit,
                    direction: ParameterDirection.Input);
            }
        }
        //Transactions
        public bool? NotTransactions
        {
            get { return GetParameter<bool?>("@NotTransactions"); }
            set
            {
                SetParameter(paramName: "@NotTransactions",
                    paramValue: value ?? false,
                    isNullable: true,
                    sqlDbType: SqlDbType.Bit,
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
        //1 - payment, 2 - bill
        public int? PaymentOrBill
        {
            get { return GetParameter<int?>("@PaymentOrBill"); }
            set
            {
                SetParameter(paramName: "@PaymentOrBill",
                    paramValue: value,
                    isNullable: true,
                    sqlDbType: SqlDbType.Int,
                    direction: ParameterDirection.Input);
            }
        }
        //0-SUM, 1-MAX, 2-LAST
        public int? TransactionSort
        {
            get { return GetParameter<int?>("@TransactionSort"); }
            set
            {
                SetParameter(paramName: "@TransactionSort",
                    paramValue: value,
                    isNullable: true,
                    sqlDbType: SqlDbType.Int,
                    direction: ParameterDirection.Input);
            }
        }
        public decimal? MinSum
        {
            get { return GetParameter<decimal?>("@MinSum"); }
            set
            {
                SetParameter(paramName: "@MinSum",
                    paramValue: value,
                    isNullable: true,
                    sqlDbType: SqlDbType.Money,
                    direction: ParameterDirection.Input);
            }
        }
        public decimal? MaxSum
        {
            get { return GetParameter<decimal?>("@MaxSum"); }
            set
            {
                SetParameter(paramName: "@MaxSum",
                    paramValue: value,
                    isNullable: true,
                    sqlDbType: SqlDbType.Money,
                    direction: ParameterDirection.Input);
            }
        }
        public DateTime? TransactionStartDate
        {
            get { return GetParameter<DateTime?>("@TransStartDate"); }
            set
            {
                SetParameter(paramName: "@TransStartDate",
                    paramValue: value,
                    isNullable: true,
                    sqlDbType: SqlDbType.SmallDateTime,
                    direction: ParameterDirection.Input);
            }
        }
        public DateTime? TransactionEndDate
        {
            get { return GetParameter<DateTime?>("@TransEndDate"); }
            set
            {
                SetParameter(paramName: "@TransEndDate",
                    paramValue: value,
                    isNullable: true,
                    sqlDbType: SqlDbType.SmallDateTime,
                    direction: ParameterDirection.Input);
            }
        }

        //Other
        // sorted member by modify date
        public DateTime? CreateFrom
        {
            get { return GetParameter<DateTime?>("@CreateFrom"); }
            set
            {
                SetParameter(paramName: "@CreateFrom",
                    paramValue: value,
                    isNullable: true,
                    sqlDbType: SqlDbType.DateTime,
                    direction: ParameterDirection.Input);
            }
        }

        public DateTime? CreateTo
        {
            get { return GetParameter<DateTime?>("@CreateTo"); }
            set
            {
                SetParameter(paramName: "@CreateTo",
                    paramValue: value,
                    isNullable: true,
                    sqlDbType: SqlDbType.DateTime,
                    direction: ParameterDirection.Input);
            }
        }

        // sorted member by modify date
        public DateTime? ModifyFrom
        {
            get { return GetParameter<DateTime?>("@ModifyFrom"); }
            set
            {
                SetParameter(paramName: "@ModifyFrom",
                    paramValue: value,
                    isNullable: true,
                    sqlDbType: SqlDbType.DateTime,
                    direction: ParameterDirection.Input);
            }
        }

        public DateTime? ModifyTo
        {
            get { return GetParameter<DateTime?>("@ModifyTo"); }
            set
            {
                SetParameter(paramName: "@ModifyTo",
                    paramValue: value,
                    isNullable: true,
                    sqlDbType: SqlDbType.DateTime,
                    direction: ParameterDirection.Input);
            }
        }

        // Sorted member by last name
        public char? StartChar
        {
            get { return GetParameter<char?>("@StartChar"); }
            set
            {
                SetParameter(paramName: "@StartChar",
                    paramValue: value,
                    isNullable: true,
                    sqlDbType: SqlDbType.NChar,
                    direction: ParameterDirection.Input);
            }
        }

        public char? EndChar
        {
            get { return GetParameter<char?>("@EndChar"); }
            set
            {
                SetParameter(paramName: "@EndChar",
                    paramValue: value,
                    isNullable: true,
                    sqlDbType: SqlDbType.NChar,
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

    }
}

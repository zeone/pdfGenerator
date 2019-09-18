using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClasicConsole.DTO;
using ClasicConsole.Reports.DAL;

namespace ClasicConsole.Reports.SP
{
    public class SelectBillDetailsStoredProcedure : StoredProcedureReturningSelectResultQuery<BillInvoiceDto>
    {
        public SelectBillDetailsStoredProcedure(IQueryProvider provider, PerSchemaSqlDbContext dbContext)
            : base(provider, dbContext)
        {
            CommandText = "dbo.SelectBillForInvoice";
        }

        protected string CommandText { get; set; }

        protected override string GetCommandText()
        {
            return CommandText;
        }

        protected override void ConfigureQuery(CustomQueryConfiguration config)
        {
            base.ConfigureQuery(config);
            config.ModelBuilderType = typeof(TransactionBillInvoiceModelBuilder);
        }

        public int? FamilyId
        {
            get { return GetParameter<int?>("@FamilyID"); }
            set
            {
                SetParameter(paramName: "@FamilyID",
                    paramValue: value,
                    sqlDbType: SqlDbType.Int,
                    isNullable: true,
                    direction: ParameterDirection.Input);
            }
        }

        public int? TransactionId
        {
            get { return GetParameter<int?>("@TransactionID"); }
            set
            {
                SetParameter(paramName: "@TransactionID",
                    paramValue: value,
                    sqlDbType: SqlDbType.Int,
                    isNullable: true,
                    direction: ParameterDirection.Input);
            }
        }
    }
}

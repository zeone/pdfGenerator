using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClasicConsole.Reports.DAL
{
    public abstract class DataSelectCustomQuery : CustomQuery
    {
        protected DataSelectCustomQuery(IQueryProvider provider,
            PerSchemaSqlDbContext dbContext)
            : base(provider, dbContext)
        {
        }

        /// <summary>
        /// Command type of this query
        /// </summary>
        public sealed override CommandType CurrentCommandType
        {
            get
            {
                // the command is always CommandType.Text
                return CommandType.Text;
            }
        }

        /// <summary>
        /// Query returns a single record
        /// </summary>
        public bool ReturnsSingleRecord
        {
            get
            {
                //
                return Config.QueryReturnsSingleRecord;
            }
        }
    }
}

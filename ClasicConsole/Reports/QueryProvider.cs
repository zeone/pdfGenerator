using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClasicConsole.Reports.DAL;
using Ninject;
using Ninject.Parameters;
using System.Configuration;

namespace ClasicConsole.Reports
{
    public class QueryProvider : IQueryProvider
    {


        private readonly StandardKernel _injectKernel;
        private string _baseConnectionString;
        private string _sharedConnectionString;
        // public string SharedSchema = Schemas.SharedSchema;

        public QueryProvider(StandardKernel injectKernel)
        {
            _injectKernel = injectKernel;
        }

        /// <summary>
        /// In schema-based application stands for the base DB connection string
        /// </summary>
        public string BaseConnectionString
        {
            get
            {
                return _baseConnectionString;
            }
            set
            {
                _baseConnectionString = value;
            }
        }
        public string SharedConnectionString
        {
            get
            {
                return _sharedConnectionString;
            }
            set
            {
                _sharedConnectionString = value;
            }
        }


        /// <summary>
        /// Create a Query object with Schema specified
        /// </summary>
        /// <typeparam name="TQuery"></typeparam>
        /// <param name="schema"></param>
        /// <returns></returns>
        public TQuery CreateQuery<TQuery>(string schema) where TQuery : BaseQuery
        {

            // create the DB context per current schema
            // var perSchemaSqlDbContext = new PerSchemaSqlDbContext(schema.Equals(SharedSchema) ? SharedConnectionString : BaseConnectionString, schema);
            string conStr = ConfigurationManager.AppSettings["connStr"];
            var perSchemaSqlDbContext = new PerSchemaSqlDbContext(conStr, schema);

            ConstructorArgument[] ctorArgs = new[]{
                new ConstructorArgument("dbContext", perSchemaSqlDbContext),
                new ConstructorArgument("provider", this)
            };

            // create a query with injected DB context
            return _injectKernel.Get<TQuery>(ctorArgs);
        }

        //public TQuery CreateQueryToSharedSchema<TQuery>() where TQuery : BaseQuery
        //{
        //    return CreateQuery<TQuery>(SharedSchema);
        //}


        public BaseQuery CreateQuery(Type queryType, string schema)
        {


            // schema.Equals(SharedSchema) ? BaseConnectionString : SharedConnectionString is a hack this should be done in a better way
            // var perSchemaSqlDbContext = new PerSchemaSqlDbContext(schema.Equals(SharedSchema) ? SharedConnectionString : BaseConnectionString, schema);
            string conStr = ConfigurationManager.AppSettings["connStr"];
            var perSchemaSqlDbContext = new PerSchemaSqlDbContext(conStr, schema);
            ConstructorArgument[] ctorArgs = new[]{
                new ConstructorArgument("dbContext", perSchemaSqlDbContext),
                new ConstructorArgument("provider", this)
            };

            return (BaseQuery)_injectKernel.Get(queryType, ctorArgs);
        }
    }
}

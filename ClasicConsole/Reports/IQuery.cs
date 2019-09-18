using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClasicConsole.Reports.DAL;

namespace ClasicConsole.Reports
{
    public interface IQueryProvider
    {
        /// <summary>
        /// Creates a Query object specified for a particular Schema (database catalog)
        /// </summary>
        /// <typeparam name="TQuery">Type of Query object to create defined explicitly</typeparam>
        /// <param name="schema">Schama (DB catalog) for which a Query object should be created</param>
        /// <returns></returns>
        TQuery CreateQuery<TQuery>(string schema) where TQuery : BaseQuery;

        /// <summary>
        /// Creates a Query object specified for Shared Schema
        /// </summary>
        /// <typeparam name="TQuery">Type of Query object to create defined explicitly</typeparam>
        /// <returns>Return created query object</returns>
       // TQuery CreateQueryToSharedSchema<TQuery>() where TQuery : BaseQuery;

        /// <summary>
        /// Creates a Query object of the specified type for a particular Schema (database catalog)
        /// </summary>
        /// <param name="queryType"></param>
        /// <param name="schema"></param>
        /// <returns></returns>
        BaseQuery CreateQuery(Type queryType, string schema);
    }

    public interface IModelBuilder<out TModel>
    {
        /// <summary>
        /// Build a model from the data reader
        /// </summary>
        /// <param name="dbDataReader"></param>
        /// <returns></returns>
        TModel Build(IDataReader dbDataReader);

        /// <summary>
        /// Build a model with default state
        /// </summary>
        /// <returns></returns>
        TModel BuildDefaultModel();
        /// <summary>
        /// Event that is fired after the model has been built
        /// </summary>
        event ModelMaterializedEventHandler<TModel> ModelReady;
    }
}

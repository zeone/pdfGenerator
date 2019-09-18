using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClasicConsole.Reports.DAL
{
    public class QueryParameterConfig
    {
        protected bool Equals(QueryParameterConfig other)
        {
            return DbType == other.DbType &&
                   Size == other.Size &&
                   IsStoredProcedureParameter == other.IsStoredProcedureParameter &&
                   NoQuotes == other.NoQuotes &&
                   IsNullable == other.IsNullable &&
                   Direction == other.Direction;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (obj.GetType() != GetType())
                return false;
            return Equals((QueryParameterConfig)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int)DbType;
                hashCode = (hashCode * 397) ^ Size;
                hashCode = (hashCode * 397) ^ IsStoredProcedureParameter.GetHashCode();
                hashCode = (hashCode * 397) ^ NoQuotes.GetHashCode();
                hashCode = (hashCode * 397) ^ IsNullable.GetHashCode();
                hashCode = (hashCode * 397) ^ (int)Direction;
                return hashCode;
            }
        }

        /// <summary>
        /// Name of the DB parameter
        /// </summary>
        public string ParameterName { get; set; }

        /// <summary>
        /// SQL DB type
        /// </summary>
        public SqlDbType DbType { get; set; }

        /// <summary>
        /// Size/scale of a DB parameter value
        /// </summary>
        public int Size { get; set; }

        /// <summary>
        /// Mark this parameter as a stored procedure parameter
        /// </summary>
        public bool IsStoredProcedureParameter { get; set; }


        /// <summary>
        /// No quotes needed when un-parameterizing query parameters
        /// </summary>
        /// <summary>
        /// Value of this parameter should not be quoted 
        /// </summary>
        public bool NoQuotes { get; set; }

        /// <summary>
        /// This parameter allows NULL values
        /// </summary>
        /// <summary>
        /// Parameter can set NULL values
        /// </summary>
        public bool IsNullable { get; set; }

        /// <summary>
        /// Direction of a parameter (input, output, return value)
        /// </summary>
        /// <summary>
        /// Direction of a DB parameter (input, output)
        /// </summary>
        public ParameterDirection Direction { get; set; }

    }

    /// <summary>
    /// Signature of the callback that is run after the model is 
    /// materialized from the executed Query object
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    /// <param name="model"></param>
    public delegate void ModelMaterializedEventHandler<in TModel>(TModel model);

    /// <summary>
    /// Callback function that is fired after the model is populated
    /// </summary>
    /// <param name="model"></param>
    public delegate void ModelmaterializedEventHandler(object model);

    public sealed class CustomQueryConfiguration
    {

        private Delegate _modelReadyCallback;

        /// <summary>
        /// Usee default handling of query parameters
        /// </summary>
        public bool UseDefaultParameterHandling { get; set; }

        /// <summary>
        /// Boolean value that defines what exactly result 
        /// is returned from the executed query: single record or multiple records
        /// </summary>
        public bool QueryReturnsSingleRecord { get; set; }

        /// <summary>
        /// Type of a model builder uses to construct models
        /// by reading results from the SqlDataReader
        /// </summary>
        public Type ModelBuilderType { get; set; }

        /// <summary>
        /// Set the callback
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="callback"></param>
        public void SetOnModelReadyCallback<TModel>(ModelMaterializedEventHandler<TModel> callback)
        {

            _modelReadyCallback = callback;
        }

        /// <summary>
        /// Get the callback function
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <returns></returns>
        public ModelMaterializedEventHandler<TModel> GetModelReadyCallback<TModel>()
        {
            if (_modelReadyCallback == null)
                return null;

            return (ModelMaterializedEventHandler<TModel>)_modelReadyCallback;
        }


        /// <summary>
        /// Check if the event handler is set to the [ModelReady] event
        /// </summary>
        /// <returns></returns>
        public bool ModelReadyEventSet()
        {
            // if NULL
            return (_modelReadyCallback != null);
        }
    }
}

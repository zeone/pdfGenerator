using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Ninject.Infrastructure.Language;

namespace ClasicConsole.Reports.DAL
{

    internal class QueryParameterEntry
    {
        /// <summary>
        /// Database parameter 
        /// </summary>
        public SqlParameter Parameter { get; set; }

        /// <summary>
        /// Parameter configuration
        /// </summary>
        public QueryParameterConfig Config { get; set; }
    }


    public abstract class BaseQuery
    {
        private const int DeadLockErrorCode = 1205;

        protected readonly PerSchemaSqlDbContext DbContext;
        protected readonly IQueryProvider Provider;
        private int _execTimes;
        private bool _lastExecFailed;
        private object _cachedResult;
        private bool _isExecuted;

       



        protected BaseQuery(IQueryProvider provider, PerSchemaSqlDbContext dbContext)
        {
            if (provider == null)
                throw new ArgumentNullException("provider", "Query provider is not defined for query");
            if (dbContext == null)
                throw new ArgumentNullException("dbContext", "DB context is not defined for query");

            DbContext = dbContext;
            Provider = provider;
        }

        /// <summary>
        /// Count of executions this query was run
        /// </summary>
        public int ExecutedTimes
        {
            get
            {
                return _execTimes;
            }
            private set
            {
                _execTimes = value;
            }
        }


        /// <summary>
        /// Current Schema (database catalog) the query is running at
        /// </summary>
        public virtual string Schema
        {
            get
            {
                return DbContext.Schema;
            }
            set
            {
                // empty 
            }
        }


        /// <summary>
        /// Last execution failed?
        /// </summary>
        public bool LastExecutionFailed
        {
            get
            {
                return _lastExecFailed;
            }
            private set
            {
                _lastExecFailed = value;
            }
        }



        /// <summary>
        /// Property used to hold result after query executed
        /// </summary>
        public object CachedResult
        {
            get
            {
                return _cachedResult;
            }
            private set
            {
                _cachedResult = value;
            }
        }

        /// <summary>
        /// Property that defines status of the query. 
        /// </summary>
        protected bool IsExecuted
        {
            get
            {
                return _isExecuted;
            }
            private set
            {
                _isExecuted = value;
            }
        }



        /// <summary>
        /// Execute the query base entry point
        /// </summary>
        /// <returns></returns>
        public object Execute()
        {
            // reset tracking properties
            CachedResult = null;
            IsExecuted = false;
            LastExecutionFailed = false;

            try
            {

                // delegate execution of the query to derived queries
                object execResult = ExecuteInternal();
                CachedResult = execResult;
                IsExecuted = true;
                

                // increase times the query was executed
                ExecutedTimes++;
                return execResult;
            }
            catch (SqlException sqlEx)
            {
                LastExecutionFailed = true;
                // DEADLOCK issue, still need fixes
                if (sqlEx.Number == DeadLockErrorCode)
                {
                    // handle the SQL Server deadlock issue here
                    return HandleDeadLock();
                }

                throw;
            }
            catch (Exception)
            {
                LastExecutionFailed = true;
                throw;
            }
        }


        private object HandleDeadLock()
        {
            const int maxRetries = 10;

            int reTry = 0;
            object result = null;

            while (reTry < maxRetries)
            {
                try
                {
                    result = ExecuteInternal();
                    CachedResult = result;
                    LastExecutionFailed = false;
                    return result;
                }
                catch (SqlException sqlEx)
                {
                    if (sqlEx.Number == DeadLockErrorCode)
                    {
                        /* wait a second + extra time each loop step */
                        Thread.Sleep(1000 + (50 * reTry));
                        reTry++;
                    }
                    else
                    {

                        throw;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Executes the query against the database
        /// </summary>
        /// <returns></returns>
        protected abstract object ExecuteInternal();
    }


    public abstract class CustomQuery : BaseQuery
    {
        // parameters map that hold a collection of parameters needed to run the query
        protected readonly Hashtable ParametersMap = new Hashtable();
        protected readonly CustomQueryConfiguration Config = new CustomQueryConfiguration();



        protected CustomQuery(IQueryProvider provider,
            PerSchemaSqlDbContext dbContext)
            : base(provider, dbContext)
        {

        }

        protected virtual void ConfigureQuery(CustomQueryConfiguration config)
        {
            // default behavior is to handle parameters 
            // by inserting them into SqlCommand objects
            config.UseDefaultParameterHandling = true;
        }

        #region === ABSTRACT PROPERTIES & METHODS ===

        protected abstract object ProcessDbCommand(SqlCommand dbCommand);

        /// <summary>
        /// Get a string represenation of a command text (name of a stored procedure, text of an SQL query)
        /// </summary>
        protected abstract string GetCommandText(
            /*
             *  SELECT fm.*
             *  FROM FamiliesMembers fm
             *  WHERE (fm.MemberId = @MemberId) && (fm.FamilyId = @FamilyId)
             */
        );

        /// <summary>
        /// Get the type of current command/query: StoredProcedure/Text/TableDirect
        /// </summary>
        public abstract CommandType CurrentCommandType { get; }

        #endregion



        private string UnParameterizeSqlQuery(string sql)
        {
            foreach (var parameterKey in ParametersMap.Keys)
            {
                // take all entries
                QueryParameterEntry entry = (QueryParameterEntry)ParametersMap[parameterKey];

                // in case the parameter is set with NoQuotes
                bool noQuotes = (entry.Parameter.Value is string) && entry.Config.NoQuotes;

                // 
                sql = sql.Replace(entry.Parameter.ParameterName, ValueToString(entry.Parameter.Value, noQuotes));
            }

            return sql;
        }

        private string ValueToString(object parameterValue, bool noQuotes = false)
        {
            if (DBNull.Value.Equals(parameterValue) || (parameterValue == null))
            {
                // return the NULL literal for parameters with no value  
                return ("NULL");
            }

            if (IsQuotedType(parameterValue) && !noQuotes)
            {
                // enclose the value with quote symbols 
                return string.Format("'{0}'", parameterValue);
            }

            return parameterValue.ToString();
        }

        /// <summary>
        /// Method needed to re-create parameters on each new execution of 
        /// the query cause 
        /// </summary>
        private void ResetParameters()
        {
            foreach (var entryKey in ParametersMap.Keys)
            {
                QueryParameterEntry entry = (QueryParameterEntry)ParametersMap[entryKey];
                // create a new SQL parameter
                SqlParameter newDbParameter = CreateParameterFromEntry(entry);
                // set the new parameter
                entry.Parameter = newDbParameter;
            }
        }

        private SqlParameter CreateParameterFromEntry(QueryParameterEntry entry)
        {

            SqlParameter newDbParameter = DbContext.CreateParameter();

            newDbParameter.ParameterName = entry.Config.ParameterName;
            newDbParameter.SqlDbType = entry.Config.DbType;
            newDbParameter.IsNullable = entry.Config.IsNullable;
            newDbParameter.Size = entry.Config.Size;
            newDbParameter.Direction = entry.Config.Direction;
            newDbParameter.Value = entry.Parameter.Value;

            return newDbParameter;
        }

        /// <summary>
        /// Quoted types are those that need additional quote symbols around them 
        /// set [Name] = 'Jim Anderson',
        /// set [Id] = '3FA6861B-4D8D-48F8-ABF7-0764B23C256F',
        /// set [StartDate] = '09/03/1972 12:31:45'
        /// </summary>
        /// <param name="paramValue">Parameter value</param>
        /// <returns></returns>
        private bool IsQuotedType(object paramValue)
        {
            return (paramValue is string) ||
                   (paramValue is char) ||
                   (paramValue is Guid) ||
                   (paramValue is DateTime);
        }

        /// <summary>
        /// Appends the Schema [USE] statement
        /// </summary>
        /// <param name="commandText"></param>
        /// <returns></returns>
        private string AppendSchemaUse(string commandText)
        {
            Console.WriteLine(commandText, "SQL query is empty");

            // is the SQL Select statement;
            string newSqlCommandText = string.Format("USE {0}; ", Schema);
            newSqlCommandText += commandText;

            return newSqlCommandText;
        }



        protected override object ExecuteInternal()
        {

            if (ExecutedTimes > 0)
            {
                // if this query was previously run,
                // then we must re-create parameters in order
                // to use this query multiple times
                ResetParameters();
            }

            // configure the query 
            ConfigureQuery(this.Config);

            // SQL text command to execute
            // For SPs this stands for the stored procedure
            // name. (without Schema). 
            string commandText = GetCommandText();


            if (ReflectionUtils.InheritsFromType<DataSelectCustomQuery>(this.GetType()))
            {
                // append the "USE {Schema};" statement if needed
                commandText = AppendSchemaUse(commandText);
            }

            commandText = Regex.Replace(commandText, @"\s+", " ");


            using (SqlConnection dbConnection = DbContext.CreateConnection())
            {
                using (SqlCommand dbCommand = DbContext.CreateSqlCommand(commandText, dbConnection, CurrentCommandType))
                {
                    //inject parameters into command when needed
                    if (Config.UseDefaultParameterHandling)
                    {
                        // use DEFAULT parameters handling
                        // insert DB paraneters into DB commands
                        InsertParametersIntoDbCommand(dbCommand);
                    }
                    else if (!Config.UseDefaultParameterHandling &&
                             ReflectionUtils.InheritsFromType<DataSelectCustomQuery>(this.GetType()))
                    {
                        // replace any parameter placeholders with 
                        // actual values
                        string sqlNoParameters = UnParameterizeSqlQuery(commandText);

                        // assign a parameter-less SQL text to be executed
                        // by the SQL command query
                        dbCommand.CommandText = sqlNoParameters;
                    }


                    // open connection to the database
                    dbCommand.Connection.Open();
                    return ProcessDbCommand(dbCommand);
                }
            }
        }




        public bool UsesDefaultParameterHandling
        {

            get
            {
                // use default parameter handling of ADO.NET
                // This setting is used by default to make SQL queries append actual
                // DB parameters with values rather than add strings
                return Config.UseDefaultParameterHandling;
            }
        }

        private void InsertParametersIntoDbCommand(SqlCommand dbCommand)
        {
            // clear any previous parameters
            dbCommand.Parameters.Clear();

            // take all parameters and insert them into DB command
            foreach (var parameterKey in ParametersMap.Keys)
            {
                QueryParameterEntry entry = (QueryParameterEntry)ParametersMap[parameterKey];
                dbCommand.Parameters.Add(entry.Parameter);
            }
        }


        protected virtual TParamType GetParameter<TParamType>(string paramName)
        {
            if (string.IsNullOrEmpty(paramName))
                throw new ArgumentException("Parameter name is not defined!", "paramName");

            QueryParameterEntry entry = (QueryParameterEntry)ParametersMap[paramName];
            if (entry == null)
            {
                // parameter entry was not found by parameter name
                string errorMsg = string.Format("Parameter [{0}] was not found within query parameters!", paramName);
                throw new InvalidOperationException(errorMsg);
            }

            object parameterValue = entry.Parameter.Value;
            return ((parameterValue != null) && !DBNull.Value.Equals(parameterValue))
                ? (TParamType)parameterValue
                : default(TParamType);
        }

        protected bool RemoveParameter(string paramName)
        {
            if (string.IsNullOrEmpty(paramName))
                throw new ArgumentException("Parameter name is not defined!", "paramName");
            QueryParameterEntry entry = (QueryParameterEntry)ParametersMap[paramName];

            if (entry == null)
                return false;

            ParametersMap.Remove(paramName);
            return true;
        }


        protected virtual void SetParameter(string paramName, object paramValue, QueryParameterConfig config)
        {
            if (string.IsNullOrEmpty(paramName))
                throw new ArgumentNullException("paramName");
            if (config == null)
                throw new ArgumentNullException("config", "Parameter configuration is not defined!");




            // get a parameter from the parameters hashmap
            QueryParameterEntry entry = (QueryParameterEntry)ParametersMap[paramName];

            if ((entry != null) && (entry.Config != null) && entry.Config.Equals(config))
            {
                // entry is found for parameter
                // just create a new DB parameter
                // leave the parameter config untouched if it did not change
                if (ExecutedTimes > 0)
                {
                    SqlParameter newDbParameter = config.IsStoredProcedureParameter
                        ? DbContext.CreateStoredProcParameter(
                            paramName: paramName, paramValue: paramValue,
                            direction: config.Direction, sqlDbType: config.DbType,
                            isNullable: config.IsNullable, size: config.Size)
                        : DbContext.CreateParameter(paramName, paramValue);

                    // reset parameter with a one with updated values
                    entry.Parameter = newDbParameter;
                    return;
                }

                // just reset the parameter value 
                // without re-creating parameter
                entry.Parameter.Value = paramValue ?? DBNull.Value;
            }
            else
            {

                SqlParameter newDbParameter = config.IsStoredProcedureParameter
                    ? DbContext.CreateStoredProcParameter(
                        paramName: paramName,
                        paramValue: paramValue,
                        direction: config.Direction,
                        sqlDbType: config.DbType,
                        isNullable: config.IsNullable,
                        size: config.Size)
                    : DbContext.CreateParameter(paramName, paramValue);

                // create a new parameter entry, contating the parameter itself and its configuration
                QueryParameterEntry newEntry = new QueryParameterEntry
                {
                    Parameter = newDbParameter,
                    Config = config
                };

                // save entry in the parameters map
                ParametersMap[paramName] = newEntry;
            }
        }

        /// <summary>
        /// Check if the parameter is set in a query
        /// </summary>
        /// <param name="paramName"></param>
        /// <returns></returns>
        public bool IsParameterSet(string paramName)
        {
            if (string.IsNullOrEmpty(paramName))
                throw new ArgumentException("Parameter name is not defined!", "paramName");

            QueryParameterEntry entry = (QueryParameterEntry)ParametersMap[paramName];

            return (entry != null) &&
                   (entry.Parameter != null) &&
                   string.Equals(entry.Parameter.ParameterName, paramName);
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class OutParameterAttribute : System.Attribute
    {
        public string ParamName { get; private set; }
        public SqlDbType SqlType { get; private set; }
        public OutParameterAttribute(string paramName, SqlDbType type)
        {
            ParamName = paramName;
            SqlType = type;
        }
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

    public abstract class StoredProcedureBase<TResult> : CustomQuery where TResult : new()
    {
        protected StoredProcedureBase(IQueryProvider provider,
            PerSchemaSqlDbContext dbContext)
            : base(provider, dbContext)
        {

            // append the @ReturnVal OUTPUT parameter
            // so it's always known what code stored procedure
            // returns after execution
            SetParameter(paramName: "@ReturnVal",
                paramValue: default(int),
                sqlDbType: SqlDbType.Int,
                isNullable: false,
                direction: ParameterDirection.ReturnValue);

            // initialise out parameters which were tagged by OutTransactionAttribute to parameters map
            InitOutParameters();
        }

        private void InitOutParameters()
        {
            foreach (var result in GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.HasAttribute(typeof(OutParameterAttribute))))
            {
                var targetType = result.PropertyType;
                var instance = result.GetCustomAttribute<OutParameterAttribute>();
                SetParameter(paramName: instance.ParamName,
                    paramValue: targetType.IsValueType ? Activator.CreateInstance(targetType) : null,
                    sqlDbType: instance.SqlType,
                    isNullable: false,
                    direction: ParameterDirection.Output);
            }
        }

        protected int ReturnCode(SqlCommand dbCommand)
        {
            if (dbCommand == null)
                throw new ArgumentNullException("dbCommand", "SQL DB command is not defined!");
            SqlParameter returnVal = dbCommand.Parameters["@ReturnVal"];
            return (int)returnVal.Value;
        }


        protected virtual IModelBuilder<TResult> CreateBuilder()
        {
            IModelBuilder<TResult> builder = (IModelBuilder<TResult>)Activator.CreateInstance(Config.ModelBuilderType);

            if (Config.ModelReadyEventSet())
            {
                // append the event handler here
                builder.ModelReady += Config.GetModelReadyCallback<TResult>();
            }

            return builder;
        }


        protected override void ConfigureQuery(CustomQueryConfiguration config)
        {
            base.ConfigureQuery(config);

            config.ModelBuilderType = IsComplexDataModel(typeof(TResult)) ?
                typeof(DefaultModelBuilder<TResult>) :
                typeof(ScalarValueBuilder<TResult>);
        }

        protected bool IsComplexDataModel(Type modelType)
        {
         

            // figure out if the current type is complex. 
            // Complex type is a type having properties that make up entity

            // note: StringAdapter type is primitive cause it encapsulates a plain string
            return modelType.IsClass &&
                   !modelType.IsPrimitive &&
                   !typeof(StringAdapter).IsAssignableFrom(modelType);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public new SpExecResult Execute()
        {
            // execute the store procedure 
            object baseResult = base.Execute();

            // cast result to the SpExecResult
            return (SpExecResult)baseResult;
        }

        /// <summary>
        /// Method to execute current stored procedure in asynchronous mode
        /// </summary>
        /// <returns></returns>
        public Task<SpExecResult> ExecuteAsync()
        {
            return Task.Run(() => Execute());
        }

        protected void SetParameter(string paramName,
            object paramValue,
            SqlDbType sqlDbType,
            bool isNullable,
            ParameterDirection direction,
            int size = 0)
        {
            if (string.IsNullOrEmpty(paramName))
                throw new ArgumentNullException("paramName", "Name of parameter is not defined!");

            QueryParameterConfig config = new QueryParameterConfig
            {
                ParameterName = paramName,
                IsStoredProcedureParameter = true, // TRUE since it's always an SP parameter
                IsNullable = isNullable,
                Size = size,
                Direction = direction,
                DbType = sqlDbType
            };

            // set parameter in the parent class
            SetParameter(paramName, paramValue, config);
        }



        public sealed override CommandType CurrentCommandType
        {
            get
            {
                // type of SQL command is always [Stored Procedure]
                return CommandType.StoredProcedure;
            }
        }

        /// <summary>
        /// Return code set after stored procedure executed
        /// </summary>
        public int OutReturnCode
        {
            get
            {
                return GetParameter<int>("@ReturnVal");
            }
        }
    }

    public abstract class StoredProcedureReturningSelectResultQuery<TResult> : StoredProcedureBase<TResult> where TResult : new()
    {

        protected StoredProcedureReturningSelectResultQuery(IQueryProvider provider, PerSchemaSqlDbContext dbContext) :
            base(provider, dbContext)
        {
        }


        protected override object ProcessDbCommand(SqlCommand dbCommand)
        {
            SpExecResult result = new SpExecResult();

            SqlDataReader reader = null;
            try
            {

                // set stored procedure name
                result.StoredProcedureName = DbContext.GetFullStoredProcName(GetCommandText());

                // execute the stored procedure
                Stopwatch sw = Stopwatch.StartNew();
                reader = dbCommand.ExecuteReader();
                sw.Stop();
                TimeSpan ts = sw.Elapsed;

                // Format and display the TimeSpan value.
                string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                    ts.Hours, ts.Minutes, ts.Seconds,
                    ts.Milliseconds / 10);
                Console.WriteLine($"response to DB: {elapsedTime}");
                if (!reader.HasRows)
                {
                    // if the stored procedure did not
                    // return any row
                    reader.Close();
                    result.ReturnCode = OutReturnCode;
                    return result;
                }

                IModelBuilder<TResult> builder = CreateBuilder();

                while (reader.Read())
                {
                    TResult newModel = builder.Build(reader);
                    result.AppendRow(newModel);
                }

                // close the reader
                reader.Close();

                // set return code
                result.ReturnCode = OutReturnCode;
                return result;
            }
            catch (Exception e)
            {
                throw;
            }
            finally
            {
                if ((reader != null) && !reader.IsClosed)
                {
                    // close the data reader if it was not closed
                    reader.Close();
                }
            }
        }
    }
}

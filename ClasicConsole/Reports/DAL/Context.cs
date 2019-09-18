using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClasicConsole.Reports.DAL
{
    /// <summary>
    /// DB context that creates SQL-Server objects depending on the Schema
    /// </summary>
    public sealed class PerSchemaSqlDbContext
    {
        /// <summary>
        /// Default Schema name, DEV database catalog
        /// </summary>
        public const string DefaultSchema = "DEV";


        // connection string
        private readonly string _connectionString;

        // current schema
        private string _schema;




        public PerSchemaSqlDbContext(string connectionString)
        {
            Console.WriteLine(connectionString, "Connection string is not defined");

            // build a connection string 
            _connectionString = connectionString;
        }

        public PerSchemaSqlDbContext(string connectionString, string schema) : this(connectionString)
        {
            Console.WriteLine(schema, "Schema is not defined");
            // set schema
            Schema = schema;
        }



        /// <summary>
        /// Schema of the DB context. Simply, the database name assigned for organization
        /// DEV - is default schema
        /// </summary>
        public string Schema
        {
            get
            {
                return _schema;
            }
            set
            {
                Console.WriteLine(value, "Schema is not defined");
                _schema = value;
            }
        }

        /// <summary>
        /// Appends a Schema part to the stored procedure name
        /// </summary>
        /// <param name="spName"></param>
        /// <returns></returns>
        public string GetFullStoredProcName(string spName)
        {
            Console.WriteLine(spName, "Name of stored procedure is not defined");
            Console.WriteLine(Schema, "Schema should be defined");

            // join the string parts
            return String.Concat(Schema, ".", spName);
        }

        /// <summary>
        /// Get the result connection string
        /// </summary>
        public string ConnectionString
        {
            get
            {
                return _connectionString;
            }
        }

        /// <summary>
        /// Create a new SQL connection to the database 
        /// </summary>
        /// <returns></returns>
        public SqlConnection CreateConnection()
        {
            if (Schema == null)
                throw new InvalidOperationException("Schema is not defined for DB connection!");
            // build an SQL connection using a specified
            return new SqlConnection(ConnectionString);
        }


        /// <summary>
        /// Create just a new Sql parameter without any link to the schema
        /// </summary>
        /// <returns></returns>
        public SqlParameter CreateParameter()
        {

            return new SqlParameter();
        }

        /// <summary>
        /// Creates a new SQL named parameter with initial value
        /// </summary>
        /// <param name="paramName"></param>
        /// <param name="paramValue"></param>
        /// <returns></returns>
        public SqlParameter CreateParameter(string paramName, object paramValue)
        {

            // create a new named SQL parameter with initial value
            return new SqlParameter(paramName, paramValue ?? DBNull.Value);
        }

        /// <summary>
        /// Create an SQL stored procedure parameter 
        /// </summary>
        /// <param name="paramName"></param>
        /// <param name="paramValue"></param>
        /// <param name="direction"></param>
        /// <param name="sqlDbType"></param>
        /// <param name="isNullable"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public SqlParameter CreateStoredProcParameter(string paramName,
            object paramValue,
            ParameterDirection direction,
            SqlDbType sqlDbType,
            bool isNullable,
            int size = 0)
        {
            SqlParameter dbParameter = new SqlParameter
            {
                ParameterName = paramName,
                SqlDbType = sqlDbType,
                IsNullable = isNullable,
                Direction = direction,
                Size = size,
                Value = (object)paramValue ?? DBNull.Value
            };


            // fix any parameter issues here
            FixParameter(dbParameter);

            return dbParameter;
        }

        private void FixParameter(SqlParameter parameter)
        {
            bool isText = parameter.SqlDbType == SqlDbType.VarChar || parameter.SqlDbType == SqlDbType.NVarChar;

            if (isText && parameter.Size == default(int))
            {
                // for nvarchar(MAX) and varchar(MAX) is property [Size]
                // equals ZERO, it's advisable to set it (-1)
                parameter.Size = (-1);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="spName"></param>
        /// <returns></returns>
        public SqlCommand CreateStoredProc(string spName)
        {
            if (string.IsNullOrEmpty(spName))
                throw new ArgumentException("Stored procedure name is not defined", "spName");
            if (Schema == null)
                throw new InvalidOperationException("Schema is not defined for the context");

            string fullSpName = string.Concat(Schema, ".", spName);
            SqlCommand spCommand = new SqlCommand(fullSpName);

            // make this SQL command a stored procedure
            spCommand.CommandType = CommandType.StoredProcedure;
            return spCommand;
        }


        public SqlCommand CreateSqlCommand(string commandText, SqlConnection connection, CommandType commandType)
        {
            if (Schema == null)
                throw new InvalidOperationException("Schema is not defined for DB context!");

            if (string.IsNullOrEmpty(commandText))
                throw new ArgumentException("Command text is not defined!", "commandText");

            if (connection == null)
                throw new ArgumentNullException("connection", "SQL Connection object is not defined!");

            if (string.IsNullOrEmpty(connection.ConnectionString))
            {
                // in case the connection string was not defined
                // for the SQL connection object
                connection.ConnectionString = this.ConnectionString;
            }


            SqlCommand dbCommand = new SqlCommand(commandText, connection);

            if (commandType == CommandType.StoredProcedure)
            {
                // if a new SP parameter is being created, then
                // properly format its full object name
                dbCommand.CommandType = CommandType.StoredProcedure;
                dbCommand.CommandText = GetFullStoredProcName(commandText);
            }

            return dbCommand;
        }
    }
}

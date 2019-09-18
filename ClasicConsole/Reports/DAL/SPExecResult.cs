using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClasicConsole.Utils;

namespace ClasicConsole.Reports.DAL
{
    public class SpExecResult
    {
        /// <summary>
        /// Return code returned if the stored procedure executed without errors
        /// </summary>
        public const int NoErrorsCode = 0;


        /// <summary>
        /// by default, the SP return code is (-1), not defined
        /// </summary>
        private int _spReturnCode = (-1);
        private List<object> _rows;
        private string _errorMessage;
        private string _spName;



        public SpExecResult()
        {

        }

        public SpExecResult(int returnCode, string spName, string errorMessage)
        {
            _spReturnCode = returnCode;
            _spName = spName;
            _errorMessage = errorMessage;
        }

        internal void AppendRow(object dataRow)
        {

            if (_rows == null)
            {
                // lazily create the list for data rows
                _rows = new List<object>();
            }

            _rows.Add(dataRow);
        }



        /// <summary>
        /// Materialized data rows
        /// </summary>
        public object[] DataRows
        {
            get
            {
                if (_rows == null)
                    return ReflectionUtils.Null<object[]>();

                return _rows.ToArray();
            }
        }




        /// <summary>
        /// Check if the result has any data rows
        /// </summary>
        public bool HasNoDataRows
        {
            get
            {
                // 
                return (_rows == null) || !_rows.Any();
            }
        }

        /// <summary>
        /// Cast to the list of generic models
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <returns></returns>
        public TModel[] ResultToArray<TModel>()
        {
            if (DataRows == null)
                return ReflectionUtils.Null<TModel[]>();

            if (typeof(string).IsAssignableFrom(typeof(TModel)))
            {
                StringAdapter[] adapters = DataRows.Cast<StringAdapter>()
                    .ToArray();
                return (TModel[])((object)adapters.ConvertToStrings());
            }

            return DataRows.Cast<TModel>()
                .ToArray();
        }

        /// <summary>
        /// Get the 
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <returns></returns>
        public TModel FirstRow<TModel>()
        {
            if (DataRows == null)
                return typeof(TModel).IsClass ? (TModel)ReflectionUtils.Null<object>()
                    : default(TModel);

            // select the first row
            return (TModel)DataRows.FirstOrDefault();
        }



        /// <summary>
        /// Return code from the stored procedure
        /// </summary>
        public int ReturnCode
        {
            get
            {
                return _spReturnCode;
            }
            internal set
            {
                _spReturnCode = value;
            }
        }


        /// <summary>
        /// Full name of the stored procedure that just executed
        /// </summary>
        public string StoredProcedureName
        {
            get
            {
                return _spName;
            }
            internal set
            {
                _spName = value;
            }
        }

        /// <summary>
        /// Error message from stored procedure taken from parameter @ErrorMessage
        /// </summary>
        public string ErrorMessage
        {
            get
            {
                return _errorMessage;
            }
            internal set
            {
                _errorMessage = value;
            }
        }

        /// <summary>
        /// Cheeck if 
        /// </summary>
        public bool NonZeroReturnCode
        {
            get
            {
                return ReturnCode != NoErrorsCode;
            }
        }
    }

    /// <summary>
    /// Generic execution result from the stored procedure
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SpExecResult<T> : SpExecResult
    {

        public new T[] DataRows
        {
            get
            {
                if (base.DataRows == null)
                    return ReflectionUtils.Null<T[]>();

                // ReSharper disable once RedundantEnumerableCastCall
                var rows = base.DataRows.OfType<T>();
                return rows.ToArray();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClasicConsole.Utils
{
    public static class DataReaderExtensions
    {
        public static TParam ReadColumn<TParam>(this IDataReader dataReader, string columnName)
        {
            object columnValue = dataReader[columnName];
            return !DBNull.Value.Equals(columnValue) ? (TParam)columnValue : default(TParam);
        }

        public static TParam ReadColumn<TParam>(this IDataReader dataReader, int columnIndex)
        {
            object columnValue = dataReader[columnIndex];
            return !DBNull.Value.Equals(columnValue) ? (TParam)columnValue : default(TParam);
        }

        public static object ReadColumn(this IDataReader dataReader, int columnIndex)
        {
            object columnValue = dataReader[columnIndex];
            return !DBNull.Value.Equals(columnValue) ? columnValue : null;
        }

        public static object ReadColumn(this IDataReader dataReader, string columnName)
        {
            object columnValue = dataReader[columnName];
            return !DBNull.Value.Equals(columnValue) ? columnValue : null;
        }

        public static bool ColumnExists(this IDataReader dataReader, string columnName)
        {

            for (var i = 0; i < dataReader.FieldCount; i++)
            {
                if (string.Equals(dataReader.GetName(i), columnName))
                    return true;
            }

            return false;
        }
    }
}

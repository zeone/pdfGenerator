using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClasicConsole.Reports
{
    public class StringArrayDataTable : DataTable
    {
        public StringArrayDataTable()
        {
            Columns.Add("Item", typeof(string)).AllowDBNull = true;
        }
        protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
        {
            return new StringArrayDataRow(builder);
        }

        protected override Type GetRowType()
        {
            return typeof(StringArrayDataRow);
        }

        public void AddItems(string[] items)
        {
            if (items == null) return;
            Rows.Clear();
            foreach (string t in items)
            {
                var row = NewRow() as StringArrayDataRow;
                row.Item = t;
                Rows.Add(row);
            }
        }

        public string[] GetItems()
        {
            var items = new string[Rows.Count];
            for (int i = 0; i < Rows.Count; i++)
                items[i] = ((StringArrayDataRow)Rows[i]).Item;
            return items;
        }
    }

    public class StringArrayDataRow : DataRow
    {
        public StringArrayDataRow(DataRowBuilder builder) : base(builder) { }

        public string Item
        {
            get { return this["Item"] == DBNull.Value ? (string)this["Item"] : null; }
            set { this["Item"] = (object)value ?? DBNull.Value; }
        }
    }
}

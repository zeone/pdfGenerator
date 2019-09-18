using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClasicConsole.Reports
{
    public class IntArrayDataTable : DataTable
    {
        public IntArrayDataTable()
        {
            Columns.Add("Item", typeof(int)).AllowDBNull = true;
        }
        protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
        {
            return new IntArrayDataRow(builder);
        }

        protected override Type GetRowType()
        {
            return typeof(IntArrayDataRow);
        }

        public void AddItems(int?[] items)
        {
            if (items == null) return;
            Rows.Clear();
            foreach (int? t in items)
            {
                var row = NewRow() as IntArrayDataRow;
                row.Item = t;
                Rows.Add(row);
            }
        }

        public int?[] GetItems()
        {
            var items = new int?[Rows.Count];
            for (int i = 0; i < Rows.Count; i++)
                items[i] = ((IntArrayDataRow)Rows[i]).Item;
            return items;
        }
    }

    public class IntArrayDataRow : DataRow
    {
        public IntArrayDataRow(DataRowBuilder builder) : base(builder) { }

        public int? Item
        {
            get { return this["Item"] == DBNull.Value ? (int?)this["Item"] : null; }
            set { this["Item"] = (object)value ?? DBNull.Value; }
        }
    }
}

using ClasicConsole.Reports.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using ClasicConsole.DTO;
using ClasicConsole.Properties;
using ClasicConsole.Utils;

namespace ClasicConsole.CSV
{
    public static class CSVService
    {
        static int hisHomePhoneLength = 1;
        static int herHomePhoneLength = 1;
        static int hisWorkPhoneLength = 1;
        static int herWorkPhoneLength = 1;
        static int hisMobilePhoneLength = 1;
        static int herMobilePhoneLength = 1;
        static int hisOtherPhoneLength = 1;
        static int herOtherPhoneLength = 1;
        static int hisPagerLength = 1;
        static int herPagerLength = 1;
        static int hisFaxLength = 1;
        static int herFaxLength = 1;
        static int hisEmailLength = 1;
        static int herEmailLength = 1;
        static int hisEmergencyPhoneLength = 1;
        static int herEmergencyPhoneLength = 1;

        public static string GetCsvString(FilterTransactionReport filter, TransactionReportResultDto transDto)
        {
            InitializeValues();
            TransactionGrouped grouped = new TransactionGrouped();

            if (filter.view == TransFilterView.Details)
                if (!string.Equals(filter.GroupBy, "NameCompany", StringComparison.InvariantCultureIgnoreCase))
                    filter.GroupBy = "NameCompany";
            grouped = Grouping.TotalBy(transDto, filter);
            if (filter.view == TransFilterView.Details)
                return CsvForDetails(filter, grouped);
            if (filter.view == TransFilterView.Total)

                if (filter.totalOnlyBy != "totalOnly")
                    return CsvForTotal(filter, grouped, (MatrixDTO)grouped);
                else
                    return CsvForTotal(filter, grouped, null);

            return string.Empty;
        }

        static string CsvForTotal(FilterTransactionReport filter, TransactionGrouped grouped, MatrixDTO matrix)
        {
            StringBuilder str = new StringBuilder();
            if (string.Equals(filter.totalOnlyBy, "totalOnly", StringComparison.CurrentCultureIgnoreCase))
            {
                //csvString = "\"FamilyId\",";
                str.Append("\"Name\",");
                if (filter.ReportType == TransFilterType.Payment) str.Append("\"Amount\",");
                if (filter.ReportType == TransFilterType.Bill)
                {
                    str.Append("\"BillAmount\",");
                    str.Append("\"PaidAmount\",");
                    str.Append("\"DueAmount\",");
                }
            }
            else
            {
                str.Append("\"Name\",");
                foreach (TransactionMatrixColumn col in matrix.Columns)
                {
                    str.Append("\"" + col.Name + "\",");
                }
            }
            List<ReportColumn> checkedColumns = new List<ReportColumn>();
            if (filter.GroupBy == "NameCompany")
            {
                checkedColumns = filter.Columns.Where(e => e.IsChecked && e.IsContact).ToList();
                //address and CompanyName columns not marked like @is contact@ so I check it and add manually
                if (filter.Columns.Any(e => e.IsChecked && string.Equals(e.Sort, "Address", StringComparison.InvariantCultureIgnoreCase)))
                    checkedColumns.Add(filter.Columns.First(e => e.IsChecked && string.Equals(e.Sort, "Address", StringComparison.InvariantCultureIgnoreCase)));
                if (filter.Columns.Any(e => e.IsChecked && string.Equals(e.Sort, "Address", StringComparison.InvariantCultureIgnoreCase)))
                    checkedColumns.Add(filter.Columns.First(e => e.IsChecked && string.Equals(e.Sort, "CompanyName", StringComparison.InvariantCultureIgnoreCase)));
                checkedColumns = checkedColumns.OrderBy(e => e.TransactionColumn).ToList();
                bool settedColAmount;
                FillCheckedColumns(str, checkedColumns, filter, out settedColAmount, grouped: grouped, matrix: matrix);
            }
            str.Append("\n");
            if (filter.totalOnlyBy == "totalOnly")
            {
                foreach (TransactionReportGroupedDTO trans in grouped.GroupedObj)
                {
                    var name = GetMainInfoByCol("Name", filter, trans);
                    var contacts = "";
                    var companyName = "";
                    var address = "";
                    if (filter.GroupBy == "NameCompany")
                    {
                        foreach (ReportColumn col in checkedColumns)
                        {
                            if (col.Sort == "Address")
                            {
                                //var addr = getMainInfoByCol(col.sort, trans);
                                //if (addr) contacts += addr;
                            }
                            else if (col.Sort == "CompanyName")
                            {
                                var compName = GetMainInfoByCol(col.Sort, filter, trans);
                                if (!string.IsNullOrEmpty(compName)) companyName += compName;
                            }
                            else
                            {
                                var colValue = GetContactsByCol(col.Sort, trans.FamilyDetails, filter);
                                if (!string.IsNullOrEmpty(colValue)) contacts += colValue;
                            }
                        }
                    }
                    if (checkedColumns.Any(col => col.Sort == "Address"))
                    {
                        var addr = GetMainInfoByCol("Address", filter, trans);
                        if (!string.IsNullOrEmpty(addr)) address += addr;
                    }
                    StringBuilder transData = new StringBuilder();
                    if (filter.ReportType == TransFilterType.Payment)
                        GetColByGroupedTransaction(transData, "TotalAmount", filter, trans);
                    if (filter.ReportType == TransFilterType.Bill)
                    {
                        GetColByGroupedTransaction(transData, "BillTotalAmount", filter, trans);
                        GetColByGroupedTransaction(transData, "PaidTotalAmount", filter, trans);
                        GetColByGroupedTransaction(transData, "DueTotalAmount", filter, trans);
                    }

                    str.Append(name + transData + companyName + address + contacts + "\n");
                }
            }
            else
            {
                for (var index = 0; index < matrix.Rows.Count; index++)
                {
                    TransactionMatrixRows row = matrix.Rows[index];
                    // last value is a total for row so we must to skip it
                    if (index < matrix.Rows.Count - 1)
                    {
                        var name = "\"" + row.Name + "\",";
                        var amounts = "";
                        var contacts = "";
                        if (filter.GroupBy == "NameCompany")
                        {
                            foreach (ReportColumn col in checkedColumns)
                            {
                                if (col.Sort == "Address")
                                {
                                    var addr = GetMainInfoByCol(col.Sort, filter, row);
                                    if (!string.IsNullOrEmpty(addr)) contacts += addr;
                                }
                                else if (col.Sort == "CompanyName")
                                {
                                    var addr = GetMainInfoByCol(col.Sort, filter, row);
                                    if (!string.IsNullOrEmpty(addr)) contacts += addr;
                                }
                                else
                                {
                                    var colValue = GetContactsByCol(col.Sort, row.FamilyDetails, filter);
                                    if (!string.IsNullOrEmpty(colValue)) contacts += colValue;
                                }
                            }
                        }
                        foreach (decimal am in row.Amounts)
                        {
                            amounts += "\"" + am.ToMoneyString() + "\",";
                        }
                        str.Append(name + amounts + contacts + "\n");
                    }
                }
            }
            return str.ToString();
        }
        static string CsvForDetails(FilterTransactionReport filter, TransactionGrouped grouped)
        {
            StringBuilder str = new StringBuilder();
            str.Append("\"FamilyId\",");
            str.Append("\"TransactionId\",");
            str.Append("\"BillId\",");
            if (filter.ShowDetails)
            {
                str.Append("\"TransactionDetailsId\",");
                str.Append("\"Type\",");
            }
            str.Append("\"Name\",");
            var checkedColumns = filter.Columns.Where(e => e.IsChecked).OrderBy(e => e.TransactionColumn).ToList();
            var checkedContackts = filter.Columns.Any(e => e.IsChecked && e.IsContact);
            var settedColAmount = false;
            FillCheckedColumns(str, checkedColumns, filter, out settedColAmount, checkedContackts: checkedContackts, grouped: grouped);
            if (!checkedContackts)
            {
                if (filter.ReportType == TransFilterType.Payment || filter.ShowDetails) str.Append("\"Amount\",");
                if (filter.ReportType == TransFilterType.Bill && !filter.ShowDetails)
                {
                    str.Append("\"BillAmount\",");
                    str.Append("\"PaidAmount\",");
                    str.Append("\"DueAmount\",");
                }
            }
            str.Append("\n");
            foreach (TransactionReportGroupedDTO trans in grouped.GroupedObj)
            {
                var transLine = "";
                var contactsLine = "";
                for (var index = 0; index < checkedColumns.Count; index++)
                {
                    ReportColumn col = checkedColumns[index];
                    if (index == 0) transLine += GetMainInfoByCol("Name", filter, trans);
                    if (col.Sort != "Address")
                    {
                        var res = GetMainInfoByCol(col.Sort, filter, trans);
                        if (!string.IsNullOrEmpty(res)) transLine += res;
                    }
                    var contact = GetContactsByCol(col.Sort, trans.FamilyDetails, filter);
                    if (!string.IsNullOrEmpty(contact)) contactsLine += contact;
                }
                foreach (TransactionsReportList tr in trans.Transactions)
                {
                    StringBuilder firstPart = new StringBuilder();
                    StringBuilder transData = new StringBuilder();
                    StringBuilder colValue = new StringBuilder();
                    var famId = GetColFromTransactions("FamilyId", filter, tr);
                    var transId = GetColFromTransactions("TransactionId", filter, tr);
                    var billId = GetColFromTransactions("BillId", filter, tr);
                    var address = "";

                    if (filter.ShowDetails)
                    {
                        foreach (TransactionDetailReportList transDet in tr.Details)
                        {

                            address = "";
                            transData.Clear();
                            firstPart.Clear();
                            firstPart.Append(famId + transId + billId);
                            GetColByTransactionDetails(firstPart, "TransactionDetailsId", filter, transDet, tr);
                            GetColByTransactionDetails(firstPart, "Type", filter, transDet, tr);

                            foreach (ReportColumn col in checkedColumns)
                            {
                                if (col.Sort == "Address" && checkedColumns.Any(s => s.Sort == "Address"))
                                {
                                    var addr = GetMainInfoByCol(col.Sort, filter, trans, tr.AddressID);
                                    if (!string.IsNullOrEmpty(addr)) address += addr;
                                }
                                colValue.Clear();
                                GetColByTransactionDetails(colValue, col.Sort, filter, transDet, tr);
                                if (!string.IsNullOrEmpty(colValue.ToString()))
                                    transData.Append(colValue);
                            }
                            GetColByTransactionDetails(transData, "Amount", filter, transDet, tr);

                            str.Append(firstPart + transLine + address + transData + contactsLine + "\n");
                        }
                    }
                    else
                    {
                        firstPart.Append(famId + transId + billId);
                        foreach (ReportColumn col in checkedColumns)
                        {

                            if (col.Sort == "Address" && checkedColumns.Any(s => s.Sort == "Address"))
                            {
                                var addr = GetMainInfoByCol(col.Sort, filter, trans, tr.AddressID);
                                if (!string.IsNullOrEmpty(addr)) address += addr;
                            }

                            var value = GetColFromTransactions(col.Sort, filter, tr);
                            if (!string.IsNullOrEmpty(value)) transData.Append(value);

                        }

                        if (filter.ReportType == TransFilterType.Payment) transData.Append(GetColFromTransactions("Amount", filter, tr));
                        if (filter.ReportType == TransFilterType.Bill)
                        {
                            transData.Append(GetColFromTransactions("BillAmount", filter, tr));
                            transData.Append(GetColFromTransactions("PaidAmount", filter, tr));
                            transData.Append(GetColFromTransactions("DueAmount", filter, tr));
                        }
                        str.Append(firstPart + transLine + address + transData + contactsLine + "\n");
                    }
                }
            }
            return str.ToString();
        }


        /// <summary>
        /// Create header for contacts and checked columns
        /// </summary>
        /// <param name="str">Main string builder</param>
        /// <param name="cols">Collection with all cheked columns</param>
        /// <param name="filter">Filter for getting report</param>
        /// <param name="checkedContackts">cols with checked contacts</param>
        /// <param name="settedColAmount">out variable for cols amounts</param>
        /// <param name="grouped">collection of grouped items</param>
        /// <param name="matrix">collection of grouped items for matrix view</param>
        static void FillCheckedColumns(StringBuilder str, List<ReportColumn> cols, FilterTransactionReport filter,
              out bool settedColAmount, bool? checkedContackts = null, TransactionGrouped grouped = null, MatrixDTO matrix = null)
        {
            settedColAmount = false;
            foreach (ReportColumn col in cols)
            {
                if (checkedContackts != null && (bool)checkedContackts && col.IsContact && !settedColAmount)
                {
                    if (filter.ReportType == TransFilterType.Payment || filter.ShowDetails)
                        str.Append("\"Amount\",");
                    if (filter.ReportType == TransFilterType.Bill && !filter.ShowDetails)
                    {
                        str.Append("\"BillAmount\",");
                        str.Append("\"PaidAmount\",");
                        str.Append("\"DueAmount\",");
                    }
                    settedColAmount = true;
                }
                if (string.Equals(col.Sort, "Address", StringComparison.InvariantCultureIgnoreCase))
                {
                    str.Append("\"Address\",");
                    str.Append("\"Address2\",");
                    str.Append("\"City\",");
                    str.Append("\"State\",");
                    str.Append("\"ZipCode\",");
                    str.Append("\"Country\",");
                }
                else if (string.Equals(col.Sort, "Categoryes", StringComparison.InvariantCultureIgnoreCase))
                {
                    str.Append("\"Category\",");
                    str.Append("\"Subcategory\",");
                }
                else if (string.Equals(col.Sort, "Email", StringComparison.InvariantCultureIgnoreCase))
                {
                    MaxColSize(7, filter, out hisEmailLength, out herEmailLength, grouped, matrix);

                    if (hisEmailLength > 1)
                    {
                        for (var i = 0; i < hisEmailLength; i++)
                        {
                            str.Append("\"new_report_HisEmail" + (i + 1) + "\",");
                        }
                    }
                    else
                    {
                        str.Append("\"new_report_HisEmail\",");
                    }
                    if (herEmailLength > 1)
                    {
                        for (var i = 0; i < herEmailLength; i++)
                        {
                            str.Append("\"new_report_HerEmail') " + (i + 1) + "\",");
                        }
                    }
                    else
                    {
                        str.Append("\"new_report_HerEmail\",");
                    }

                }
                else if (string.Equals(col.Sort, "MobilePhone", StringComparison.InvariantCultureIgnoreCase))
                {
                    MaxColSize(3, filter, out hisMobilePhoneLength, out herMobilePhoneLength, grouped, matrix);

                    if (hisMobilePhoneLength > 1)
                    {
                        for (var i = 0; i < hisMobilePhoneLength; i++)
                        {
                            str.Append("\"new_report_HisMobilePhone') " + (i + 1) + "\",");
                        }
                    }
                    else
                    {
                        str.Append("\"new_report_HisMobilePhone\",");
                    }
                    if (herMobilePhoneLength > 1)
                    {
                        for (var i = 0; i < herMobilePhoneLength; i++)
                        {
                            str.Append("\"new_report_HerMobilePhone') " + (i + 1) + "\",");
                        }
                    }
                    else
                    {
                        str.Append("\"new_report_HerMobilePhone\",");
                    }
                }
                else if (string.Equals(col.Sort, "HomePhone", StringComparison.InvariantCultureIgnoreCase))
                {
                    MaxColSize(1, filter, out hisHomePhoneLength, out herHomePhoneLength, grouped, matrix);
                    if (hisHomePhoneLength > 1)
                    {
                        for (var i = 0; i < hisHomePhoneLength; i++)
                        {
                            str.Append("\"new_report_HisHomePhone') " + (i + 1) + "\",");
                        }
                    }
                    else
                    {
                        str.Append("\"new_report_HisHomePhone\",");
                    }
                    if (herHomePhoneLength > 1)
                    {
                        for (var i = 0; i < herHomePhoneLength; i++)
                        {
                            str.Append("\"new_report_HerHomePhone') " + (i + 1) + "\",");
                        }
                    }
                    else
                    {
                        str.Append("\"new_report_HerHomePhone\",");
                    }
                }
                else if (string.Equals(col.Sort, "WorkPhone", StringComparison.InvariantCultureIgnoreCase))
                {
                    MaxColSize(2, filter, out hisWorkPhoneLength, out herWorkPhoneLength, grouped, matrix);

                    if (hisWorkPhoneLength > 1)
                    {
                        for (var i = 0; i < hisWorkPhoneLength; i++)
                        {
                            str.Append("\"new_report_HisWorkPhone') " + (i + 1) + "\",");
                        }
                    }
                    else
                    {
                        str.Append("\"new_report_HisWorkPhone\",");
                    }
                    if (herWorkPhoneLength > 1)
                    {
                        for (var i = 0; i < herWorkPhoneLength; i++)
                        {
                            str.Append("\"new_report_HerWorkPhone') " + (i + 1) + "\",");
                        }
                    }
                    else
                    {
                        str.Append("\"new_report_HerWorkPhone\",");
                    }

                }
                else if (string.Equals(col.Sort, "OtherPhone", StringComparison.InvariantCultureIgnoreCase))
                {
                    MaxColSize(4, filter, out hisOtherPhoneLength, out herOtherPhoneLength, grouped, matrix);

                    if (hisOtherPhoneLength > 1)
                    {
                        for (var i = 0; i < hisOtherPhoneLength; i++)
                        {
                            str.Append("\"new_report_HisOtherPhone') " + (i + 1) + "\",");
                        }
                    }
                    else
                    {
                        str.Append("\"new_report_HisOtherPhone\",");
                    }
                    if (herOtherPhoneLength > 1)
                    {
                        for (var i = 0; i < herOtherPhoneLength; i++)
                        {
                            str.Append("\"new_report_HerOtherPhone') " + (i + 1) + "\",");
                        }
                    }
                    else
                    {
                        str.Append("\"new_report_HerOtherPhone\",");
                    }
                }
                else if (string.Equals(col.Sort, "Pager", StringComparison.InvariantCultureIgnoreCase))
                {
                    MaxColSize(5, filter, out hisPagerLength, out herPagerLength, grouped, matrix);
                    if (hisPagerLength > 1)
                    {
                        for (var i = 0; i < hisPagerLength; i++)
                        {
                            str.Append("\"new_report_HisPager') " + (i + 1) + "\",");
                        }
                    }
                    else
                    {
                        str.Append("\"new_report_HisPager\",");
                    }
                    if (herPagerLength > 1)
                    {
                        for (var i = 0; i < herPagerLength; i++)
                        {
                            str.Append("\"new_report_HerPager') " + (i + 1) + "\",");
                        }
                    }
                    else
                    {
                        str.Append("\"new_report_HerPager\",");
                    }
                }
                else if (string.Equals(col.Sort, "EmergencyPhone", StringComparison.InvariantCultureIgnoreCase))
                {
                    MaxColSize(8, filter, out hisEmergencyPhoneLength, out herEmergencyPhoneLength, grouped, matrix);
                    if (hisEmergencyPhoneLength > 1)
                    {
                        for (var i = 0; i < hisEmergencyPhoneLength; i++)
                        {
                            str.Append("\"new_report_HisEmergencyPhone') " + (i + 1) + "\",");
                        }
                    }
                    else
                    {
                        str.Append("\"new_report_HisEmergencyPhone\",");
                    }
                    if (herEmergencyPhoneLength > 1)
                    {
                        for (var i = 0; i < herEmergencyPhoneLength; i++)
                        {
                            str.Append("\"new_report_HerEmergencyPhone') " + (i + 1) + "\",");
                        }
                    }
                    else
                    {
                        str.Append("\"new_report_HerEmergencyPhone\",");
                    }
                }
                else if (string.Equals(col.Sort, "Fax", StringComparison.InvariantCultureIgnoreCase))
                {
                    MaxColSize(6, filter, out hisFaxLength, out herFaxLength, grouped, matrix);
                    if (hisFaxLength > 1)
                    {
                        for (var i = 0; i < hisFaxLength; i++)
                        {
                            str.Append("\"new_report_HisFax') " + (i + 1) + "\",");
                        }
                    }
                    else
                    {
                        str.Append("\"new_report_HisFax\",");
                    }
                    if (herFaxLength > 1)
                    {
                        for (var i = 0; i < herFaxLength; i++)
                        {
                            str.Append("\"new_report_HerFax') " + (i + 1) + "\",");
                        }
                    }
                    else
                    {
                        str.Append("\"new_report_HerFax\",");
                    }
                }
                else if (string.Equals(col.Sort, "DateDue", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (filter.ShowDetails) str.Append("\"" + col.Name + "\",");
                }
                else
                {
                    str.Append("\"" + col.Name + "\",");
                }

            }
        }

        static void MaxColSize(int phoneTypeId, FilterTransactionReport filter, out int hisVal, out int herVal, TransactionGrouped grouped = null, MatrixDTO matrix = null)
        {
            hisVal = 1;
            herVal = 1;
            if (filter.view == TransFilterView.Details || string.Equals(filter.totalOnlyBy, "totalOnly", StringComparison.InvariantCultureIgnoreCase))
            {
                foreach (var trans in grouped.GroupedObj)
                {
                    var hisCount = 0;
                    var herCount = 0;
                    foreach (var cn in trans.FamilyDetails.Contacts)
                    {
                        if (cn.MemberType == 1 && cn.PhoneTypeID == phoneTypeId) hisCount++;
                        if (cn.MemberType == 2 && cn.PhoneTypeID == phoneTypeId) herCount++;
                    }
                    if (hisVal < hisCount) hisVal = hisCount;
                    if (herVal < herCount) herVal = herCount;
                }
            }
            else
            {
                foreach (TransactionMatrixRows trans in matrix.Rows)
                {
                    var hisCount = 0;
                    var herCount = 0;
                    if (trans.FamilyDetails?.Contacts != null && trans.FamilyDetails.Contacts.Any())
                        foreach (ContactReportContactInfo cn in trans.FamilyDetails.Contacts)
                        {
                            if (cn.MemberType == 1 && cn.PhoneTypeID == phoneTypeId) hisCount++;
                            if (cn.MemberType == 2 && cn.PhoneTypeID == phoneTypeId) herCount++;
                        }
                    if (hisVal < hisCount) hisVal = hisCount;
                    if (herVal < herCount) herVal = herCount;
                }
            }
        }
        /// <summary>
        /// Get main info for columns
        /// </summary>
        /// <param name="colName">Column name</param>
        /// <param name="filter">report filter</param>
        /// <param name="trans">grouped transactions</param>
        /// <param name="addressId">optional value</param>
        /// <returns></returns>
        static string GetMainInfoByCol(string colName, FilterTransactionReport filter, TransactionReportGroupedDTO trans, int? addressId = null)
        {
            if (string.Equals(colName, "Name", StringComparison.InvariantCultureIgnoreCase))
            {
                return "\"" + trans.Name + "\",";
            }
            if (string.Equals(colName, "CompanyName", StringComparison.InvariantCultureIgnoreCase))
            {
                if (filter.view == TransFilterView.Details)
                {
                    var companyName = trans.Company;
                    return !string.IsNullOrEmpty(companyName) ? "\"" + companyName + "\"," : "\"\",";
                }
                if (filter.view == TransFilterView.Total && string.Equals(filter.totalOnlyBy, "totalOnly", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (trans.FamilyDetails?.Addresses != null && trans.FamilyDetails.Addresses.Any())
                    {
                        var companyName = trans.FamilyDetails.Addresses[0].CompanyName;
                        return !string.IsNullOrEmpty(companyName) ? "\"" + companyName + "\"," : "\"\",";
                    }
                }
                return "\"\",";
            }
            if (string.Equals(colName, "Address", StringComparison.InvariantCultureIgnoreCase))
            {
                var addresStr = "";
                ContactReportAddress addres = new ContactReportAddress();
                if (trans.FamilyDetails?.Addresses != null && trans.FamilyDetails.Addresses.Any())
                {
                    addres = addressId != null && trans.FamilyDetails.Addresses.Any(ad => ad.AddressID == addressId) ? trans.FamilyDetails.Addresses.FirstOrDefault(ad => ad.AddressID == addressId)
                        : trans.FamilyDetails.Addresses[0];
                    addresStr += !string.IsNullOrEmpty(addres.Address) ? "\"" + addres.Address + "\"," : "\"\",";
                    addresStr += !string.IsNullOrEmpty(addres.Address2) ? "\"" + addres.Address2 + "\"," : "\"\",";
                    addresStr += !string.IsNullOrEmpty(addres.City) ? "\"" + addres.City + "\"," : "\"\",";
                    addresStr += !string.IsNullOrEmpty(addres.State) ? "\"" + addres.State + "\"," : "\"\",";
                    addresStr += !string.IsNullOrEmpty(addres.Zip) ? "\"" + addres.Zip + "\"," : "\"\",";
                    addresStr += !string.IsNullOrEmpty(addres.Country) ? "\"" + addres.Country + "\"," : "\"\",";
                    return addresStr;
                }
                else
                {
                    return "\"\",\"\",\"\",\"\",\"\",\"\",";
                }
            }
            return string.Empty;
        }

        static string GetMainInfoByCol(string colName, FilterTransactionReport filter, TransactionMatrixRows row, int? addressId = null)
        {
            if (string.Equals(colName, "Name", StringComparison.InvariantCultureIgnoreCase))
            {
                return "\"" + row.Name + "\",";
            }
            if (string.Equals(colName, "CompanyName", StringComparison.InvariantCultureIgnoreCase))
            {
                if (row.FamilyDetails?.Addresses != null && row.FamilyDetails.Addresses.Any())
                {
                    var companyName = row.FamilyDetails.Addresses[0].CompanyName;
                    return !string.IsNullOrEmpty(companyName) ? "\"" + companyName + "\"," : "\"\",";
                }

                return "\"\",";
            }
            if (string.Equals(colName, "Address", StringComparison.InvariantCultureIgnoreCase))
            {
                var addresStr = "";
                ContactReportAddress addres = new ContactReportAddress();
                if (row.FamilyDetails?.Addresses != null && row.FamilyDetails.Addresses.Any())
                {
                    addres = addressId != null && row.FamilyDetails.Addresses.Any(ad => ad.AddressID == addressId) ? row.FamilyDetails.Addresses.FirstOrDefault(ad => ad.AddressID == addressId)
                        : row.FamilyDetails.Addresses[0];
                    addresStr += !string.IsNullOrEmpty(addres.Address) ? "\"" + addres.Address + "\"," : "\"\",";
                    addresStr += !string.IsNullOrEmpty(addres.Address2) ? "\"" + addres.Address2 + "\"," : "\"\",";
                    addresStr += !string.IsNullOrEmpty(addres.City) ? "\"" + addres.City + "\"," : "\"\",";
                    addresStr += !string.IsNullOrEmpty(addres.State) ? "\"" + addres.State + "\"," : "\"\",";
                    addresStr += !string.IsNullOrEmpty(addres.Zip) ? "\"" + addres.Zip + "\"," : "\"\",";
                    addresStr += !string.IsNullOrEmpty(addres.Country) ? "\"" + addres.Country + "\"," : "\"\",";
                    return addresStr;
                }
                else
                {
                    return "\"\",\"\",\"\",\"\",\"\",\"\",";
                }
            }
            return string.Empty;
        }
        /// <summary>
        /// return string for transaction columns
        /// </summary>
        /// <param name="colName">Column name</param>
        /// <param name="filter">Report filter</param>
        /// <param name="trans">Transaction</param>
        /// <param name="detail">Transaction detail</param>
        /// <param name="transGroped">Grouped obj with transaction and other data</param>
        /// <returns></returns>
        static string GetColFromTransactions(string colName, FilterTransactionReport filter, TransactionsReportList trans = null)
        {
            string res;
            if (string.Equals(colName, "FamilyId", StringComparison.InvariantCultureIgnoreCase) && trans != null)
            {

                return "\"" + trans.FamilyID + "\",";

            }
            if (string.Equals(colName, "TransactionId", StringComparison.InvariantCultureIgnoreCase) && trans != null)
            {
                return "\"" + trans.TransactionID + "\",";

            }
            if (string.Equals(colName, "BillId", StringComparison.InvariantCultureIgnoreCase) && trans != null)
            {
                return trans.BillID != null ? "\"" + trans.BillID + "\"," : "\"\",";

            }

            if (string.Equals(colName, "CheckNum", StringComparison.InvariantCultureIgnoreCase) && trans != null)
            {
                return trans.CheckNo != null ? "\"" + trans.CheckNo + "\"," : "\"\",";

            }
            if (string.Equals(colName, "Date", StringComparison.InvariantCultureIgnoreCase) && trans != null)
            {
                return "\"" + trans.Date + "\",";
            }
            if (string.Equals(colName, "Method", StringComparison.InvariantCultureIgnoreCase) && trans != null)
            {
                res = GetMethodName(trans.PaymentMethodID);
                return !string.IsNullOrEmpty(res) ? "\"" + res + "\"," : "\"\",";

            }
            if (string.Equals(colName, "Solicitor", StringComparison.InvariantCultureIgnoreCase) && trans != null)
            {
                res = GetSolicitorName(trans.SolicitorID);
                return !string.IsNullOrEmpty(res) ? "\"" + res + "\"," : "\"\",";

            }
            if (string.Equals(colName, "Mailing", StringComparison.InvariantCultureIgnoreCase) && trans != null)
            {
                res = GetMailingName(trans.MailingID);
                if (string.Equals(res, "no_mailing", StringComparison.InvariantCultureIgnoreCase)) res = GetTranslation(res);
                return !string.IsNullOrEmpty(res) ? "\"" + res + "\"," : "\"\",";

            }
            if (string.Equals(colName, "Categoryes", StringComparison.InvariantCultureIgnoreCase) && trans != null)
            {
                res = GetCatSubcatName(trans.Details);

                // return !string.IsNullOrEmpty(res) ? "\"" + res.Split(':')[0] + "\"," + "\"" + res.Split(':')[1] + "\"," : "\"\",";
                return !string.IsNullOrEmpty(res) ? "\"!!!" + res + "\"," +
                                                    "\"!!!" + res + "\"," : "\"\",\"\",";
            }
            if (string.Equals(colName, "Department", StringComparison.InvariantCultureIgnoreCase) && trans != null)
            {
                res = GetDepartmentName(trans.DepartmentID);
                return !string.IsNullOrEmpty(res) ? "\"" + res + "\"," : "\"\",\"\",";

            }
            if (string.Equals(colName, "ReceiptNum", StringComparison.InvariantCultureIgnoreCase) && trans != null)
            {
                return trans.ReceiptNo != null ? "\"" + trans.ReceiptNo + "\"," : "\"\",";
            }
            if (string.Equals(colName, "InvoiceNum", StringComparison.InvariantCultureIgnoreCase) && trans != null)
            {
                return trans.InvoiceNo != null ? "\"" + trans.InvoiceNo + "\"," : "\"\",";
            }
            if (string.Equals(colName, "Note", StringComparison.InvariantCultureIgnoreCase) && trans != null)
            {
                return !string.IsNullOrEmpty(trans.Note) ? "\"" + trans.Note + "\"," : "\"\",";

            }
            if (string.Equals(colName, "Amount", StringComparison.InvariantCultureIgnoreCase) && trans != null)
            {
                return "\"$" + trans.Amount + "\",";

            }

            if (string.Equals(colName, "BillAmount", StringComparison.InvariantCultureIgnoreCase) && trans != null)
            {
                res = (-trans.Amount).ToMoneyString();
                return !string.IsNullOrEmpty(res) ? "\"" + res + "\"," : "\"$0\",";

            }
            if (string.Equals(colName, "PaidAmount", StringComparison.InvariantCultureIgnoreCase) && trans != null)
            {
                res = CalcPayedAmount(trans.Amount, trans.AmountDue);
                return !string.IsNullOrEmpty(res) ? "\"" + res + "\"," : "\"$0\",";

            }
            if (string.Equals(colName, "DueAmount", StringComparison.InvariantCultureIgnoreCase) && trans != null)
            {
                res = trans.AmountDue == null ? 0M.ToMoneyString() : Math.Abs((decimal)trans.AmountDue).ToMoneyString();
                return !string.IsNullOrEmpty(res) ? "\"" + res + "\"," : "\"$0\",";

            }


            if (string.Equals(colName, "Type", StringComparison.InvariantCultureIgnoreCase) && trans != null)
            {
                return trans.TransType == 1
                     ? "\"" + GetTranslation("transaction_report_BillType") + "\","
                     : "\"" + GetTranslation("transaction_report_PaymentType") + "\",";
            }
            return null;
        }

        static void GetColByTransactionDetails(StringBuilder str, string colName, FilterTransactionReport filter, TransactionDetailReportList detail, TransactionsReportList trans)
        {
            string res;
            if (string.Equals(colName, "Categoryes", StringComparison.InvariantCultureIgnoreCase) && detail != null)
            {

                str.Append("\"" + GetCategoryName(detail.CategoryID) + "\"," +
                           "\"" + GetSubCategoryName(detail.SubcategoryID) + "\",");


            }
            if (string.Equals(colName, "Amount", StringComparison.InvariantCultureIgnoreCase) && detail != null)
            {
                str.Append("\"$" + detail.Amount + "\",");
                return;
            }
            if (string.Equals(colName, "DateDue", StringComparison.InvariantCultureIgnoreCase) && detail != null)
            {
                if (filter.ShowDetails)
                    str.Append(detail.DateDue != null ? "\"" + detail.DateDue + "\"," : "\"\",");
                return;
            }
            if (string.Equals(colName, "Date", StringComparison.InvariantCultureIgnoreCase) && trans != null)
            {
                str.Append("\"" + trans.Date + "\",");
            }
            if (string.Equals(colName, "CheckNum", StringComparison.InvariantCultureIgnoreCase) && trans != null)
            {
                str.Append(trans.CheckNo != null ? "\"" + trans.CheckNo + "\"," : "\"\",");

            }
            if (string.Equals(colName, "Solicitor", StringComparison.InvariantCultureIgnoreCase) && trans != null)
            {
                res = GetSolicitorName(trans.SolicitorID);
                str.Append(!string.IsNullOrEmpty(res) ? "\"" + res + "\"," : "\"\",");

            }
            if (string.Equals(colName, "Mailing", StringComparison.InvariantCultureIgnoreCase) && trans != null)
            {
                res = GetMailingName(trans.MailingID);
                if (string.Equals(res, "no_mailing", StringComparison.InvariantCultureIgnoreCase)) res = GetTranslation(res);
                str.Append(!string.IsNullOrEmpty(res) ? "\"" + res + "\"," : "\"\",");

            }
            if (string.Equals(colName, "InvoiceNum", StringComparison.InvariantCultureIgnoreCase) && trans != null)
            {
                str.Append(trans.InvoiceNo != null ? "\"" + trans.InvoiceNo + "\"," : "\"\",");
            }
            if (string.Equals(colName, "Department", StringComparison.InvariantCultureIgnoreCase) && trans != null)
            {
                res = GetDepartmentName(trans.DepartmentID);
                str.Append(!string.IsNullOrEmpty(res) ? "\"" + res + "\"," : "\"\",\"\",");
            }
            if (string.Equals(colName, "Note", StringComparison.InvariantCultureIgnoreCase) && trans != null)
            {
                str.Append(!string.IsNullOrEmpty(trans.Note) ? "\"" + trans.Note + "\"," : "\"\",");

            }
            if (string.Equals(colName, "TransactionDetailsId", StringComparison.InvariantCultureIgnoreCase) && detail != null)
            {
                str.Append("\"" + detail.TransactionDetailID + "\",");
            }
            if (string.Equals(colName, "Type", StringComparison.InvariantCultureIgnoreCase) && detail != null)
            {
                str.Append(detail.TransType == 1
                    ? "\"" + GetTranslation("transaction_report_BillType") + "\","
                    : "\"" + GetTranslation("transaction_report_PaymentType") + "\",");
            }
        }

        static void GetColByGroupedTransaction(StringBuilder str, string colName, FilterTransactionReport filter,
            TransactionReportGroupedDTO transGroped)
        {
            string res;
            if (string.Equals(colName, "TotalAmount", StringComparison.InvariantCultureIgnoreCase) && transGroped != null)
            {
                res = Math.Abs(transGroped.TotalAmount).ToMoneyString();
                str.Append(!string.IsNullOrEmpty(res) ? "\"" + res + "\"," : "\"$0\",");
                return;
            }
            if (string.Equals(colName, "BillTotalAmount", StringComparison.InvariantCultureIgnoreCase) && transGroped != null)
            {
                res = Math.Abs(transGroped.TotalAmount).ToMoneyString();
                str.Append(!string.IsNullOrEmpty(res) ? "\"" + res + "\"," : "\"$0\",");
                return;
            }
            if (string.Equals(colName, "PaidTotalAmount", StringComparison.InvariantCultureIgnoreCase) && transGroped != null)
            {
                res = Math.Abs(transGroped.PaidTotalAmount).ToMoneyString();
                str.Append(!string.IsNullOrEmpty(res) ? "\"" + res + "\"," : "\"$0\",");
                return;
            }
            if (string.Equals(colName, "DueTotalAmount", StringComparison.InvariantCultureIgnoreCase) && transGroped != null)
            {
                res = Math.Abs(transGroped.DueTotalAmount).ToMoneyString();
                str.Append(!string.IsNullOrEmpty(res) ? "\"" + res + "\"," : "\"$0\",");
            }
        }
        static string GetContactsByCol(string colName, FamilyDetails famDetails, FilterTransactionReport filter)
        {
            if (colName == "Email")
            {
                return GetContactsString(7, famDetails.Contacts.ToList(), hisEmailLength, herEmailLength);
            }
            if (colName == "MobilePhone")
            {
                return GetContactsString(3, famDetails.Contacts.ToList(), hisMobilePhoneLength, herMobilePhoneLength);
            }
            if (colName == "HomePhone")
            {
                return GetContactsString(1, famDetails.Contacts.ToList(), hisHomePhoneLength, herHomePhoneLength);
            }
            if (colName == "WorkPhone")
            {
                return GetContactsString(2, famDetails.Contacts.ToList(), hisWorkPhoneLength, herWorkPhoneLength);
            }
            if (colName == "OtherPhone")
            {
                return GetContactsString(4, famDetails.Contacts.ToList(), hisOtherPhoneLength, herOtherPhoneLength);
            }
            if (colName == "Pager")
            {
                return GetContactsString(5, famDetails.Contacts.ToList(), hisPagerLength, herPagerLength);
            }
            if (colName == "EmergencyPhone")
            {
                return GetContactsString(8, famDetails.Contacts.ToList(), hisEmergencyPhoneLength, herEmergencyPhoneLength);
            }
            if (colName == "Fax")
            {
                return GetContactsString(6, famDetails.Contacts.ToList().ToList(), hisFaxLength, herFaxLength);
            }
            return string.Empty;
        }
        static string GetContactsString(int contId, List<ContactReportContactInfo> contactList, int hisLength, int herLength)
        {
            var hisContact = "";
            var herContact = "";
            if (contactList != null && contactList.Any())
            {
                var contacts = contactList.Where(ct => ct.PhoneTypeID == contId).ToList();
                var hisContactExist = contacts.Any(ct => ct.MemberType == 1);
                var herContactExist = contacts.Any(ct => ct.MemberType == 2);
                for (var i = 0; i < hisLength; i++)
                {

                    if (hisContactExist && contacts.First(ct => ct.MemberType == 1) != null)
                    {
                        hisContact += "\"" +
                                      contacts.First(ct => ct.MemberType == 1).PhoneNumber +
                            "\",";
                    }
                    else
                    {
                        hisContact += "\"\",";
                    }
                }
                for (var j = 0; j < herLength; j++)
                {

                    if (herContactExist && contacts.First(ct => ct.MemberType == 2) != null)
                    {
                        herContact += "\"" +
                                      contacts.First(ct => ct.MemberType == 2).PhoneNumber +
                            "\",";
                    }
                    else
                    {
                        herContact += "\"\",";
                    }
                }
            }
            else
            {
                for (var i = 0; i < hisLength; i++)
                {
                    hisContact += "\"\",";
                }
                for (var j = 0; j < herLength; j++)
                {
                    herContact += "\"\",";
                }
            }
            return hisContact + herContact;
        }


        static void InitializeValues()
        {
            hisHomePhoneLength = 1;
            herHomePhoneLength = 1;
            hisWorkPhoneLength = 1;
            herWorkPhoneLength = 1;
            hisMobilePhoneLength = 1;
            herMobilePhoneLength = 1;
            hisOtherPhoneLength = 1;
            herOtherPhoneLength = 1;
            hisPagerLength = 1;
            herPagerLength = 1;
            hisFaxLength = 1;
            herFaxLength = 1;
            hisEmailLength = 1;
            herEmailLength = 1;
            hisEmergencyPhoneLength = 1;
            herEmergencyPhoneLength = 1;
        }



        #region names
        public static string GetTranslation(string translateFrom)
        {
            if (string.IsNullOrEmpty(translateFrom))
                return string.Empty;
            ResourceManager resource = new ResourceManager(typeof(Resources));
            var result = resource.GetString(translateFrom);
            return string.IsNullOrEmpty(result) ? translateFrom : result;
        }

        public static string GetSolicitorName(int? solId)
        {
            //TODO implement
            return solId == null ? GetTranslation("report_no_solicitors") : $"Some sol name {solId}";
        }

        public static string GetCategoryName(int? catId)
        {
            //TODO implement
            return catId == null ? GetTranslation("report_no_category") : $"Some cat name {catId}";
        }

        public static string GetSubCategoryName(int? subcatId)
        {
            //TODO implement
            return subcatId == null ? GetTranslation("report_no_subcategory") : $"Some subcat name {subcatId}";
        }

        public static string GetCatSubcatName(List<TransactionDetailReportList> transDetails)
        {
            //todo 
            return "catsubcatName";
        }
        public static string GetCatSubcatName(int catId, int subcatId)
        {
            //todo 
            return $"catName {catId}/ {subcatId}";
        }
        public static string GetMethodName(int? metId)
        {
            //TODO implement
            return metId == null ? "" : $"Some method name {metId}";
        }
        public static string GetMailingName(int? mailingId)
        {
            //TODO implement
            return mailingId == null ? GetTranslation("report_no_mailingDesc") : $"Some mailing name {mailingId}";
        }

        public static string GetDepartmentName(int? depId)
        {
            return depId == null ? GetTranslation("report_no_department") : $"Some department name {depId}";
        }
        private static string CalcPayedAmount(decimal transAmount, decimal? transAmountDue)
        {
            if (transAmountDue == null) return 0M.ToMoneyString();
            return (Math.Abs(transAmount) - Math.Abs((decimal)transAmountDue)).ToMoneyString();
        }
        #endregion
    }


}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClasicConsole.Reports.DTO;

namespace ClasicConsole.CSV
{
    public static class CSVContactService
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


        public static string GetCsvString(FilterContactReport filter, List<ContactReportResultDto> contacts)
        {
            StringBuilder str = new StringBuilder();
            bool colAmount;
            // FillColumns(str, filter);
            str.Append("sep=,\n");
            str.Append("\"Name\",");
            FillAdressColumns(str, filter.Columns.Where(r => (int)r.Column >= 8 && (int)r.Column <= 13 && r.IsChecked).OrderBy(e => e.Column).ToList());
            FillContactsColumns(str, filter.Columns.Where(r => r.IsChecked).OrderBy(e => e.Column).ToList(), out colAmount, contacts);
            str.Append("\n");
            FillRows(str, filter, contacts);
            return str.ToString();
        }




        private static void FillRows(StringBuilder str, FilterContactReport filter, List<ContactReportResultDto> contacts)
        {
            foreach (var contact in contacts)
            {
                var contactStr = "";
                if (filter.ReportType == TransFilterType.Family)
                {
                    if (contact.Addresses != null && contact.Addresses.Any())
                    {
                        foreach (ContactReportAddress addr in contact.Addresses)
                        {
                            FillRow(str, contact.FamilyName, filter, addr, contact.Members.SelectMany(r => r.Contacts.Select(t => { t.MemberType = r.TypeID; return t; })).ToList());
                        }
                    }
                    else
                    {
                        FillRow(str, contact.FamilyName, filter, null, contact.Members.SelectMany(r => r.Contacts.Select(t => { t.MemberType = r.TypeID; return t; })).ToList());
                    }
                }
                if (filter.ReportType == TransFilterType.Member)
                {
                    if (contact.Addresses != null && contact.Addresses.Any())
                    {
                        foreach (ContactReportAddress addr in contact.Addresses)
                        {

                            foreach (ContactReportMambers member in contact.Members)
                            {
                                var memberName = (filter.SkipTitles ? "" : member.Title) + member.FirstName + " " +
                                                 member.LastName;
                                FillRow(str, memberName, filter, addr, member.Contacts.Select(t => { t.MemberType = member.TypeID; return t; }).ToList());
                            }
                        }
                    }
                    else
                    {
                        foreach (ContactReportMambers member in contact.Members)
                        {
                            var memberName = (filter.SkipTitles ? "" : member.Title) + member.FirstName + " " +
                                             member.LastName;
                            FillRow(str, memberName, filter, null, member.Contacts.Select(t => { t.MemberType = member.TypeID; return t; }).ToList());
                        }
                    }
                }
            }
        }

        static void FillRow(StringBuilder str, string name, FilterContactReport filter, ContactReportAddress addr, List<ContactReportContactInfo> contactsList)
        {
            var contactStr = "";
            str.Append($"\"{name}\",");
            if (addr != null)
                GetAddressString(str, addr, filter);
            else
                GetEmptyAddressString(str, filter);
            // var contactsList = contact.Members.SelectMany(r => r.Contacts.Select(t => { t.MemberType = r.TypeID; return t; })).ToList();
            foreach (ReportColumn col in filter.Columns.Where(e => (int)e.Column <= 7 && e.IsChecked).OrderBy(e => e.Column))
            {
                var colValue = GetContactsByCol(col.Sort, contactsList);
                if (!string.IsNullOrEmpty(colValue))
                    contactStr += colValue;
            }
            str.Append(contactStr);
            str.Append("\n");
        }

        static void GetAddressString(StringBuilder str, ContactReportAddress address, FilterContactReport filter)
        {
            if (filter.Columns.Any(e => e.Column == ReportColumns.Address && e.IsChecked))
                str.Append($"\"{address.Address}\",");
            if (filter.Columns.Any(e => e.Column == ReportColumns.City && e.IsChecked))
                str.Append($"\"{address.City}\",");
            if (filter.Columns.Any(e => e.Column == ReportColumns.State && e.IsChecked))
                str.Append($"\"{address.State}\",");
            if (filter.Columns.Any(e => e.Column == ReportColumns.Zip && e.IsChecked))
                str.Append($"\"{address.Zip}\",");
            if (filter.Columns.Any(e => e.Column == ReportColumns.Country && e.IsChecked))
                str.Append($"\"{address.Country}\",");
            if (filter.Columns.Any(e => e.Column == ReportColumns.Company && e.IsChecked))
                str.Append($"\"{address.CompanyName}\",");
        }
        static void GetEmptyAddressString(StringBuilder str, FilterContactReport filter)
        {
            if (filter.Columns.Any(e => e.Column == ReportColumns.Address && e.IsChecked))
                str.Append("\"\",");
            if (filter.Columns.Any(e => e.Column == ReportColumns.City && e.IsChecked))
                str.Append("\"\",");
            if (filter.Columns.Any(e => e.Column == ReportColumns.State && e.IsChecked))
                str.Append("\"\",");
            if (filter.Columns.Any(e => e.Column == ReportColumns.Zip && e.IsChecked))
                str.Append("\"\",");
            if (filter.Columns.Any(e => e.Column == ReportColumns.Country && e.IsChecked))
                str.Append("\"\",");
            if (filter.Columns.Any(e => e.Column == ReportColumns.Company && e.IsChecked))
                str.Append("\"\",");
        }

        static string GetTranslation(string trans)
        {
            return trans;
        }

        private static void FillAdressColumns(StringBuilder str, List<ReportColumn> cols)
        {
            foreach (var col in cols)
            {
                str.Append($"\"{GetTranslation(col.Name)}\",");
            }
        }

        static void FillContactsColumns(StringBuilder str, List<ReportColumn> cols,
            out bool settedColAmount, List<ContactReportResultDto> contacts)
        {
            settedColAmount = false;
            foreach (ReportColumn col in cols)
            {
                if (string.Equals(col.Sort, "Email", StringComparison.InvariantCultureIgnoreCase))
                {
                    MaxColSize(7, out hisEmailLength, out herEmailLength, contacts);

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
                    MaxColSize(3, out hisMobilePhoneLength, out herMobilePhoneLength, contacts);

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
                    MaxColSize(1, out hisHomePhoneLength, out herHomePhoneLength, contacts);
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
                    MaxColSize(2, out hisWorkPhoneLength, out herWorkPhoneLength, contacts);

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
                    MaxColSize(4, out hisOtherPhoneLength, out herOtherPhoneLength, contacts);

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
                    MaxColSize(5, out hisPagerLength, out herPagerLength, contacts);
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
                    MaxColSize(8, out hisEmergencyPhoneLength, out herEmergencyPhoneLength, contacts);
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
                    MaxColSize(6, out hisFaxLength, out herFaxLength, contacts);
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
            }
        }

        static void MaxColSize(int phoneTypeId, out int hisVal, out int herVal, List<ContactReportResultDto> contacts)
        {
            hisVal = 1;
            herVal = 1;

            foreach (var trans in contacts)
            {
                var hisCount = 0;
                var herCount = 0;
                foreach (var cn in trans.Members.SelectMany(r => r.Contacts))
                {
                    if (cn.MemberType == 1 && cn.PhoneTypeID == phoneTypeId) hisCount++;
                    if (cn.MemberType == 2 && cn.PhoneTypeID == phoneTypeId) herCount++;
                }
                if (hisVal < hisCount) hisVal = hisCount;
                if (herVal < herCount) herVal = herCount;
            }


        }

        static string GetContactsByCol(string colName, List<ContactReportContactInfo> Contacts)
        {
            if (colName == "Email")
            {
                return GetContactsString(7, Contacts.ToList(), hisEmailLength, herEmailLength);
            }
            if (colName == "MobilePhone")
            {
                return GetContactsString(3, Contacts.ToList(), hisMobilePhoneLength, herMobilePhoneLength);
            }
            if (colName == "HomePhone")
            {
                return GetContactsString(1, Contacts.ToList(), hisHomePhoneLength, herHomePhoneLength);
            }
            if (colName == "WorkPhone")
            {
                return GetContactsString(2, Contacts.ToList(), hisWorkPhoneLength, herWorkPhoneLength);
            }
            if (colName == "OtherPhone")
            {
                return GetContactsString(4, Contacts.ToList(), hisOtherPhoneLength, herOtherPhoneLength);
            }
            if (colName == "Pager")
            {
                return GetContactsString(5, Contacts.ToList(), hisPagerLength, herPagerLength);
            }
            if (colName == "EmergencyPhone")
            {
                return GetContactsString(8, Contacts.ToList(), hisEmergencyPhoneLength, herEmergencyPhoneLength);
            }
            if (colName == "Fax")
            {
                return GetContactsString(6, Contacts.ToList(), hisFaxLength, herFaxLength);
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
    }

}

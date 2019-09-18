using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using ClasicConsole.DTO;
using ClasicConsole.Utils;

namespace ClasicConsole.Reports.DAL
{
    public class TransactionBillModelBuilder : BaseModelBuilder<BillDto>
    {
        protected override BillDto BuildSingleModel(IDataReader dbDataReader, PropertyInfo[] properties)
        {
            BillDto model;
            var xmlString = dbDataReader.ReadColumn<string>("BillDetails");
            if (xmlString != null)
            {
                var serializer = new XmlSerializer(typeof(BillDto),
                    new XmlRootAttribute("BillDetails"));
                using (var stream =
                    new MemoryStream(Encoding.Unicode.GetBytes(xmlString)))
                    model = serializer.Deserialize(stream) as BillDto;
            }
            else
                model = new BillDto();

            model.TransactionId = dbDataReader.ReadColumn<int>("TransactionId");
            model.BillId = dbDataReader.ReadColumn<int?>("BillId");
            model.Family = dbDataReader.ReadColumn<string>("Family");
            model.FamilyId = dbDataReader.ReadColumn<int>("FamilyId");
            model.MemberId = dbDataReader.ReadColumn<int?>("MemberId");
            model.AddressId = dbDataReader.ReadColumn<int>("AddressId");
            model.Date = dbDataReader.ReadColumn<DateTime>("Date");
            model.InvoiceNo = dbDataReader.ReadColumn<int?>("InvoiceNo");
            model.Amount = dbDataReader.ReadColumn<decimal>("Amount");
            model.AmountDue = dbDataReader.ReadColumn<decimal?>("AmountDue");
            model.PayId = dbDataReader.ReadColumn<int?>("PayId");
            model.LetterId = dbDataReader.ReadColumn<short?>("LetterId");
            model.SolicitorId = dbDataReader.ReadColumn<int?>("SolicitorId");
            model.DepartmentId = dbDataReader.ReadColumn<int>("DepartmentId");
            model.MailingId = dbDataReader.ReadColumn<int?>("MailingId");
            model.HonorMemory = dbDataReader.ReadColumn<string>("HonnorMemory");
            model.Note = dbDataReader.ReadColumn<string>("Note");

            return model;
        }
    }
}

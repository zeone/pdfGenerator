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
    public class TransactionBillInvoiceModelBuilder : BaseModelBuilder<BillInvoiceDto>
    {
        protected override BillInvoiceDto BuildSingleModel(IDataReader dbDataReader, PropertyInfo[] properties)
        {
            BillInvoiceDto model;
            var xmlString = dbDataReader.ReadColumn<string>("BillDetails");
            if (xmlString != null)
            {
                var serializer = new XmlSerializer(typeof(BillInvoiceDto),
                    new XmlRootAttribute("BillDetails"));
                using (var stream =
                    new MemoryStream(Encoding.Unicode.GetBytes(xmlString)))
                    model = serializer.Deserialize(stream) as BillInvoiceDto;
            }
            else
                model = new BillInvoiceDto();

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
            model.Address = dbDataReader.ReadColumn<string>("Address");
            model.CompanyName = dbDataReader.ReadColumn<string>("CompanyName");
            model.City = dbDataReader.ReadColumn<string>("City");
            model.Country = dbDataReader.ReadColumn<string>("Country");
            model.State = dbDataReader.ReadColumn<string>("State");
            model.Zip = dbDataReader.ReadColumn<string>("Zip");

            return model;
        }
    }
}

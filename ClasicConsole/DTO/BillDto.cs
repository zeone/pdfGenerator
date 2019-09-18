using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.ComponentModel;

namespace ClasicConsole.DTO
{
    public class BillDto
    {
        public int? TransactionId { get; set; }

        public int? BillId { get; set; }


        public int FamilyId { get; set; }

        public string Family { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? MemberId { get; set; }


        public int AddressId { get; set; }


        public DateTime Date { get; set; }

        public int? InvoiceNo { get; set; }


        public decimal Amount { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public decimal? AmountDue { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public short? LetterId { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? SolicitorId { get; set; }


        public int DepartmentId { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? MailingId { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string HonorMemory { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Note { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? PayId { get; set; }


        [XmlElement("Bill")]
        public BillDetailsDto[] BillDetails { get; set; }
    }

    [XmlRoot("Bill")]
    public class BillDetailsDto
    {
        [XmlElement("TransactionDetailID")]
        public int? TransactionDetailId { get; set; }

        [XmlElement("BillID")]
        public int? BillId { get; set; }

        [XmlElement("DateDue")]
        public DateTime DateDue { get; set; }

        [XmlElement("CategoryID")]
        public int CategoryId { get; set; }

        [XmlElement("Category")]
        public string Category { get; set; }

        [XmlElement("SubcategoryID")]
        public int SubcategoryId { get; set; }

        [XmlElement("Subcategory")]
        public string Subcategory { get; set; }

        [XmlElement("ItemID")]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? ItemId { get; set; }

        [XmlElement("Item")]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Item { get; set; }

        [XmlElement("ClassID")]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? ClassId { get; set; }

        [XmlElement("Class")]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Class { get; set; }

        [XmlElement("SubclassID")]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? SubclassId { get; set; }

        [XmlElement("Subclass")]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Subclass { get; set; }

        [XmlElement("Quantity")]
        public short Quantity { get; set; }

        [XmlElement("UnitPrice")]
        public decimal UnitPrice { get; set; }

        [XmlElement("Discount")]
        public decimal Discount { get; set; }

        [XmlElement("Amount")]
        public decimal Amount { get; set; }

        [XmlElement("DueAmount")]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public decimal? DueAmount { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public decimal? PaidAmount { get { return Amount - DueAmount; } }

        [XmlElement("Note")]
        public string Note { get; set; }

        [XmlElement("CardCharged")]
        public bool CardCharged { get; set; }

        [DefaultValue(DtoClientState.Unchanged)]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public byte Status { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ChargeMsg { get; set; }

        [DefaultValue(ChargeStatuses.None)]
        public int ChargeStatus { get; set; }

        public int? TransType { get; set; }
    }

    public class BillInvoiceDto : BillDto
    {
        public string Address { get; set; }
        public string CompanyName { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
    }
    [Flags]
    public enum ChargeStatuses : int
    {
        None = 0,
        Success = 1,
        CanceledByUser = 2,
        Error = 4
    }
    public enum DtoClientState
    {
        Unchanged = 0,
        Changed,
        New,
        Deleted
    }
}

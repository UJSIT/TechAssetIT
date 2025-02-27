using System;
using System.Collections.Generic;
using System.Data;
using System.Web;
using System.Web.Mvc;

namespace slbfeHardware.Models
{
    public class InvoicenGRNModel
    {
        public string Invoice_Reference { get; set; }
        public string Invoice_No { get; set; }
        public string Invoice_Date { get; set; }
        public string PO_No { get; set; }
        public string Name_of_Supplier { get; set; }
        public decimal Total_Amount { get; set; }
        public decimal Dicount_Amount { get; set; }
        public decimal Vat { get; set; }
        public string Invoice_Image { get; set; }


        public string GRN_No { get; set; }
        public string GRN_Date { get; set; }
        public string Remarks { get; set; }
        public DateTime sysDate { get; set; }
        public string sysUser { get; set; }

        public byte[] Doc { get; set; }

        public DataTable dt { get; set; }
        public DataTable dt7 { get; set; }
        public string invno { get; set; }

        public string Reference { get; set; }

        public string status { get; set; }

        public List<SelectListItem> CompanySelectList { get; set; }


        public HttpPostedFileBase InvoiceImageFile { get; set; }
        public string InvoiceImagePath { get; set; }

        public DataTable FullInvoice { get; set; }

        public string ErrorMessage { get; set; }

        
    }
}
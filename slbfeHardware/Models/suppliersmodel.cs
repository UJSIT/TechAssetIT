using System;
using System.Collections.Generic;
using System.Data;
using System.Web.Mvc;

namespace slbfeHardware.Models
{
    public class suppliersmodel
    {
        public string Name_of_Supplier { get; set; }
        public string Registration_Status { get; set; }
        public string Registration_Year { get; set; }
        public string Address_of_Dealer { get; set; }
        public string email { get; set; }
        public string fax { get; set; }
        public string Telephone { get; set; }
        public string Contact_Person1 { get; set; }
        public string Contact_Person1_Phone { get; set; }
        public string Contact_Person2 { get; set; }
        public string Contact_Person2_Phone { get; set; }
        public string status { get; set; }
        public string DStatus { get; set; }

        public string RedirectType { get; set; }

        public string EditSuppliers { get; set; }

        public DateTime sysDate { get; set; }
        public string sysUser { get; set; }

        public DataTable dt3 { get; set; }

        public List<SelectListItem> CompanySelectList { get; set; }

        public string CCategory { get; set; }

        public string Suppliers { get; set; }
        public string SupplierID { get; set; }

        public string CCategory1 { get; set; }

        public string CCategory2 { get; set; }

        public string CCategory3 { get; set; }

        public string ErrorMessage { get; set; }


    }
}
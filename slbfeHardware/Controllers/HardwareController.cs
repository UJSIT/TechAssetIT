 using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using slbfeHardware.DAL;
using slbfeHardware.Models;


namespace slbfeHardware.Controllers
{
    [Authorize]
    public class HardwareController : Controller
    {

        public ActionResult Dashboard()
        {

            EnterItemsModel model = new EnterItemsModel
            {
                PendingHandoverCount = HardwareModule.GetPendingHandoverCount(),
                InITCount = HardwareModule.GetInITCount(),
                ProcessOutsideCount = HardwareModule.GetProcessOutsideCount(),
                ProcessInITCount = HardwareModule.GetProcessInITCount(),
                DisposeCount = HardwareModule.GetDisposeCount()
            };

            return View(model);
        }

        public ActionResult CreateInvoice()
        {

            InvoicenGRNModel Mod = new InvoicenGRNModel
            {
                Invoice_Reference = HardwareModule.GetReference(),
                CompanySelectList = HardwareModule.GetCompaniesSelectList()
            };
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            return View(Mod);
        }

        public ActionResult DataPushToDB(string index)
        {
            InvoicenGRNModel mod = new InvoicenGRNModel();
            _ = mod.Invoice_Reference;

            return View(mod);
        }

        public ActionResult InvDataView()
        {
            InvoicenGRNModel InMod = new InvoicenGRNModel();

            try
            {
                string sql = "SELECT [Invoice_Reference],[Invoice_No],CONVERT(VARCHAR,[Invoice_Date], 23),[PO_No],[Name_of_Supplier],[GRN_No],CONVERT(VARCHAR,[GRN_Date], 23) FROM [TechAssets].[dbo].[Hardware_Master_Invoice] WHERE [Status]='1' ORDER BY [Invoice_Reference] DESC";

                InMod.dt = HardwareModule.Get_Any_DT(sql);
            }
            catch (Exception)
            {
                InMod.ErrorMessage = "An error occurred while retrieving data. Please try again later.";
            }
            return View(InMod);
        }


        public ActionResult InvData_FullView(string id)
        {
            _ = new InvoicenGRNModel();

            string sql = "SELECT [Invoice_Reference],[Invoice_No],CONVERT(VARCHAR,[Invoice_Date], 23),[PO_No],[Name_of_Supplier],[Vat],[Dicount_Amount],[Total_Amount],[Invoice_Image],[GRN_No],CONVERT(VARCHAR,[GRN_Date], 23),[Remarks] FROM [TechAssets].[dbo].[Hardware_Master_Invoice] where Invoice_Reference='" + id + "'";
            InvoicenGRNModel mod = HardwareModule.GetFullInvoiceInfo(sql);
            mod.FullInvoice = HardwareModule.Get_Any_DT(sql);
            ViewBag.invoiceFull = mod;

            return View(mod);
        }


        public ActionResult Submit_Invoice_Data(InvoicenGRNModel mod, HttpPostedFileBase InvImgfile)
        {
            if (InvImgfile != null)
            {
                HttpPostedFileBase document = InvImgfile;
                string fileExtension = Path.GetExtension(document.FileName).ToUpper();

                if (fileExtension == ".PDF")
                {
                    string uniqueFileName = $"Invoice_{mod.Invoice_Reference}.pdf";
                    string path = Path.Combine(Server.MapPath("~/Upload/Invoice"), uniqueFileName);
                    document.SaveAs(path);

                }
                else
                {
                    mod.Doc = null;
                    mod.status = "No valid file was uploaded!";
                }

                string sql = "INSERT INTO [TechAssets].[dbo].[Hardware_Master_Invoice]([Invoice_Reference],[Invoice_No],[Invoice_Date],[PO_No],[Name_of_Supplier],[Total_Amount],[Dicount_Amount],[Vat],[GRN_No],[GRN_Date],[Remarks],[sysDate],[sysUser],[Status]) VALUES ('" + mod.Invoice_Reference + "','" + mod.Invoice_No + "','" + mod.Invoice_Date + "','" + mod.PO_No + "','" + mod.Name_of_Supplier + "'," + mod.Total_Amount + "," + mod.Dicount_Amount + "," + mod.Vat + ",'" + mod.GRN_No + "','" + mod.GRN_Date + "','" + mod.Remarks + "','" + DateTime.Now + "','" + Constants.sysUser + "','1')";

                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
                {
                    SqlCommand cmd = new SqlCommand(sql, con);
                    con.Open();
                    _ = cmd.ExecuteNonQuery();
                    con.Close();
                }
            }

            return RedirectToAction("InvDataView");
        }


        public FileResult ViewInvoiceImage(string id)
        {
            InvoicenGRNModel mod = new InvoicenGRNModel
            {
                Invoice_Reference = id
            };
            mod = HardwareModule.Get_Invoice_Documents(mod);
            return File(mod.Doc, "application/pdf");
        }



        public ActionResult EditInvoice(string Invoice_Reference)
        {
            InvoicenGRNModel mod = HardwareModule.EditInvoice(Invoice_Reference);
            mod.CompanySelectList = HardwareModule.GetCompaniesSelectList();
            return View(mod);
        }

        //public ActionResult UpdateInvoice(InvoicenGRNModel mod, string Invoice_Reference)
        //{
        //    string sql = "UPDATE [TechAssets].[dbo].[Hardware_Master_Invoice] SET Invoice_Reference = @Invoice_Reference, Invoice_No = @Invoice_No, Invoice_Date = @Invoice_Date, PO_No = @PO_No, Name_of_Supplier = @Name_of_Supplier, Total_Amount = @Total_Amount, Dicount_Amount = @Dicount_Amount, Vat = @Vat, GRN_No = @GRN_No, GRN_Date = @GRN_Date, Remarks = @Remarks, sysDate = @sysDate, sysUser = @sysUser, Status = @Status WHERE Name_of_Supplier = @Name_of_Supplier";

        //    string sysuser = Constants.sysUser;
        //    using (var conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
        //    {
        //        conn_uat.Open();
        //        SqlCommand sqlcmd = new SqlCommand(sql, conn_uat);
        //        sqlcmd.Parameters.AddWithValue("@Invoice_Reference", mod.Invoice_Reference);
        //        sqlcmd.Parameters.AddWithValue("@Invoice_No", mod.Invoice_No);
        //        sqlcmd.Parameters.AddWithValue("@Invoice_Date", mod.Invoice_Date);
        //        sqlcmd.Parameters.AddWithValue("@PO_No", mod.PO_No);
        //        sqlcmd.Parameters.AddWithValue("@Name_of_Supplier", mod.Name_of_Supplier);
        //        sqlcmd.Parameters.AddWithValue("@Total_Amount", mod.Total_Amount);
        //        sqlcmd.Parameters.AddWithValue("@Dicount_Amount", mod.Dicount_Amount);
        //        sqlcmd.Parameters.AddWithValue("@Vat", mod.Vat);
        //        sqlcmd.Parameters.AddWithValue("@GRN_No", mod.GRN_No);
        //        sqlcmd.Parameters.AddWithValue("@GRN_Date", mod.GRN_Date);
        //        sqlcmd.Parameters.AddWithValue("@Remarks", mod.Remarks ?? (object)DBNull.Value); // Handle null value
        //        sqlcmd.Parameters.AddWithValue("@sysDate", DateTime.Now);
        //        sqlcmd.Parameters.AddWithValue("@sysUser", sysuser);
        //        sqlcmd.Parameters.AddWithValue("@Status", mod.status);
        //        sqlcmd.Parameters.AddWithValue("@Company_Name", Invoice_Reference);

        //        sqlcmd.ExecuteNonQuery();
        //        conn_uat.Close();
        //    }

        //    return RedirectToAction("InvDataView", "Hardware");
        //}


        public ActionResult UpdateInvoice(InvoicenGRNModel mod, string Invoice_Reference)
        {
            string sql = "UPDATE [TechAssets].[dbo].[Hardware_Master_Invoice] SET Invoice_Reference = @Invoice_Reference, Invoice_No = @Invoice_No, Invoice_Date = @Invoice_Date, PO_No = @PO_No, Name_of_Supplier = @Name_of_Supplier, Total_Amount = @Total_Amount, Dicount_Amount = @Dicount_Amount, Vat = @Vat, GRN_No = @GRN_No, GRN_Date = @GRN_Date, Remarks = @Remarks, sysDate = @sysDate, sysUser = @sysUser, Status = @Status WHERE Invoice_Reference = @Invoice_Reference";

            string sysuser = Constants.sysUser;

            try
            {
                using (SqlConnection conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
                {
                    conn_uat.Open();
                    SqlCommand sqlcmd = new SqlCommand(sql, conn_uat);
                    _ = sqlcmd.Parameters.AddWithValue("@Invoice_Reference", mod.Invoice_Reference);
                    _ = sqlcmd.Parameters.AddWithValue("@Invoice_No", mod.Invoice_No);
                    _ = sqlcmd.Parameters.AddWithValue("@Invoice_Date", mod.Invoice_Date);
                    _ = sqlcmd.Parameters.AddWithValue("@PO_No", mod.PO_No);
                    _ = sqlcmd.Parameters.AddWithValue("@Name_of_Supplier", mod.Name_of_Supplier);
                    _ = sqlcmd.Parameters.AddWithValue("@Total_Amount", mod.Total_Amount);
                    _ = sqlcmd.Parameters.AddWithValue("@Dicount_Amount", mod.Dicount_Amount);
                    _ = sqlcmd.Parameters.AddWithValue("@Vat", mod.Vat);
                    _ = sqlcmd.Parameters.AddWithValue("@GRN_No", mod.GRN_No);
                    _ = sqlcmd.Parameters.AddWithValue("@GRN_Date", mod.GRN_Date);
                    _ = sqlcmd.Parameters.AddWithValue("@Remarks", mod.Remarks ?? (object)DBNull.Value); // Handle null value
                    _ = sqlcmd.Parameters.AddWithValue("@sysDate", DateTime.Now);
                    _ = sqlcmd.Parameters.AddWithValue("@sysUser", sysuser);
                    _ = sqlcmd.Parameters.AddWithValue("@Status", mod.status);
                    _ = sqlcmd.Parameters.AddWithValue("@Company_Name", Invoice_Reference);

                    _ = sqlcmd.ExecuteNonQuery();
                }

                TempData["SuccessMessage"] = "Invoice updated successfully.";
                return RedirectToAction("InvDataView", "Hardware");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while updating the invoice: " + ex.Message;
                return RedirectToAction("InvDataView", "Hardware");
            }
        }



        public ActionResult AddItems()
        {
            EnterItemsModel InMod = new EnterItemsModel();
            string sql = "SELECT [Invoice_Reference],[Invoice_No],[PO_No],[Name_of_Supplier],CONVERT(VARCHAR,[Invoice_Date], 23),[Total_Amount],[Invoice_Image],[GRN_No],[GRN_Date],[Remarks] FROM [TechAssets].[dbo].[Hardware_Master_Invoice] where [Status]='1'";

            InMod.dt2 = HardwareModule.Get_Any_DT(sql);
            return View(InMod);
        }

        public ActionResult ItemsDataView()
        {
            EnterItemsModel InMod = new EnterItemsModel();
            string sql = "SELECT [Invoice_Reference],[Invoice_No],[PO_No],[Name_of_Supplier],[Address_of_Dealer],[Conact_Person],[Telephone],[fax],[email],CONVERT(VARCHAR,[Invoice_Date], 23),[Total_Amount],[Invoice_Image],[GRN_No],[GRN_Date],[Remarks] FROM [TechAssets].[dbo].[Hardware_Master_Invoice] where [Status]='1'";

            InMod.dt2 = HardwareModule.Get_Any_DT(sql);

            return View(InMod);
        }

        public ActionResult View_Invoice(string id)
        {
            InvoicenGRNModel jsm = new InvoicenGRNModel
            {
                Invoice_Reference = id.Trim()
            };

            byte[] Docs = HardwareModule.Get_Invoice_Documents(jsm).Doc;

            if (Docs == null)
            {
                return RedirectToAction("NodataFound", "Hardware");
            }
            else
            {
                Response.AppendHeader("Content-Disposition", "inline; filename=Document.pdf");
                return File(Docs, "application/pdf");
            }

        }


        public int GetRemainingQuantity(string descriptionCode, int totalQuantity)
        {
            string countQuery = "SELECT COUNT([DescriptionCode]) AS DescriptionCodeCount FROM [TechAssets].[dbo].[Hardware_Devices] WHERE [DescriptionCode] = @DescriptionCode";
            int currentDeviceCount = 0;

            using (SqlConnection conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
            {
                SqlCommand countCommand = new SqlCommand(countQuery, conn_uat);
                _ = countCommand.Parameters.AddWithValue("@DescriptionCode", descriptionCode);
                conn_uat.Open();
                currentDeviceCount = (int)countCommand.ExecuteScalar();
                conn_uat.Close();
            }

            // Calculate remaining quantity
            return totalQuantity - currentDeviceCount;
        }


        public int GetPartsQuantity(string descriptionCode, int totalPartQuantity)
        {
            string countQuery = "SELECT COUNT([DescriptionCode]) AS DescriptionCodeCount FROM [TechAssets].[dbo].[Hardware_Stores] WHERE [DescriptionCode] = @DescriptionCode";
            int currentPartCount = 0;

            using (SqlConnection conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
            {
                SqlCommand countCommand = new SqlCommand(countQuery, conn_uat);
                _ = countCommand.Parameters.AddWithValue("@DescriptionCode", descriptionCode);
                conn_uat.Open();
                currentPartCount = (int)countCommand.ExecuteScalar();
                conn_uat.Close();
            }

            return totalPartQuantity - currentPartCount;
        }





        public ActionResult View_Invoice_Full(string invno, string DescriptionCode)
        {
            EnterItemsModel mod = new EnterItemsModel();
            _ = new EnterItemsModel();

            mod.Item_Type_Code = HardwareModule.GetItemTypeCode();
            mod.ItemTypeList = HardwareModule.GetItemTypeListByDatabase();
            //mod = HardwareModule.EditInvoiceDescriptionData(DescriptionCode);
            List<SelectListItem> model = new List<SelectListItem>();
            List<SelectListItem> brand = new List<SelectListItem>();
            mod.BrandList = brand;
            mod.ModelList = model;

            mod.invno = invno;

            EnterItemsModel temp = HardwareModule.GetInvoiceInfo(invno);

            mod.Invoice_Ref = temp.Invoice_Ref;
            mod.Invoice_No = temp.Invoice_No;
            mod.Invoice_Date = temp.Invoice_Date;
            mod.Name_of_Supplier = temp.Name_of_Supplier;


            string sql = "SELECT HI.[Invoice_Ref],HI.[DescriptionCode],IT.[Item_Type],B.[BrandName],HMN.[Model_Name],HI.[Warranty_Period],HI.[description],HI.[quantity],HI.[unitprice],HI.[sys_Date],HI.[sys_User],HI.[status],HI.[Asset]" +
                    "FROM [TechAssets].[dbo].[Hardware_Inovice_Items] HI " +
                    "INNER JOIN [TechAssets].[dbo].[Hardware_ModelNew] HMN ON HI.[ModelName] = HMN.[Model_Code] " +
                    "INNER JOIN [TechAssets].[dbo].[Hardware_Item_Types] IT ON HI.[Item_Type] = IT.[Item_Type_Code] " +
                    "INNER JOIN [TechAssets].[dbo].[Hardware_Brand] B ON HI.[BrandName] = B.[BrandCode] " +
                    "WHERE HI.Invoice_Ref = @invno";

            using (SqlConnection conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
            {
                conn_uat.Open();
                SqlDataAdapter da = new SqlDataAdapter(sql, conn_uat);
                DataSet ds = new DataSet();
                _ = da.SelectCommand.Parameters.AddWithValue("@invno", invno);
                _ = da.Fill(ds);
                mod.dt1 = ds.Tables[0];
                conn_uat.Close();
            }

            // Add a new column for RemainingQuantity to the DataTable
            if (!mod.dt1.Columns.Contains("RemainingQuantity"))
            {
                _ = mod.dt1.Columns.Add("RemainingQuantity", typeof(int));
            }

            // Calculate the remaining quantity for each row using the helper method
            foreach (DataRow row in mod.dt1.Rows)
            {
                string descriptionCode = row["DescriptionCode"].ToString();
                int totalQuantity = Convert.ToInt32(row["quantity"]);

                // Call the helper method to get the remaining quantity
                int remainingQuantity = GetRemainingQuantity(descriptionCode, totalQuantity);
                row["RemainingQuantity"] = remainingQuantity;
            }

            return View(mod);
        }



        public JsonResult EditInvoiceDescriptionData(string DescriptionCode)
        {
            EnterItemsModel data = new EnterItemsModel();
            string sql = "SELECT [Invoice_Ref],[DescriptionCode],[Item_Type],[BrandName],[ModelName],[Warranty_Period],[description],[quantity],[unitprice] FROM [TechAssets].[dbo].[Hardware_Inovice_Items] WHERE [DescriptionCode] = @DescriptionCode";

            using (SqlConnection conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
            {
                conn_uat.Open();
                SqlCommand sqlcmd2 = new SqlCommand(sql, conn_uat);
                _ = sqlcmd2.Parameters.AddWithValue("@DescriptionCode", DescriptionCode);
                SqlDataReader dr = sqlcmd2.ExecuteReader();

                if (dr.Read())
                {
                    data.invno = dr["Invoice_Ref"].ToString();
                    data.invID = dr["DescriptionCode"].ToString();
                    data.Item_Type = dr["Item_Type"].ToString();
                    data.BrandName = dr["BrandName"].ToString();
                    data.ModelName = dr["ModelName"].ToString();
                    data.Warranty_Period = dr["Warranty_Period"].ToString();
                    data.description = dr["description"].ToString();
                    data.quantity = dr["quantity"].ToString();
                    data.unitprice = dr["unitprice"].ToString();
                }
            }

            return Json(data, JsonRequestBehavior.AllowGet);
        }



        public ActionResult UpdateInvoiceDescriptionData(EnterItemsModel mod, string invID)
        {
            string sql = "UPDATE [TechAssets].[dbo].[Hardware_Inovice_Items] SET Invoice_Ref = '" + mod.invno + "', DescriptionCode = '" + mod.invID + "', Item_Type ='" + mod.Item_Type + "', BrandName='" + mod.BrandName + "' , ModelName ='" + mod.ModelName + "', Warranty_Period='" + mod.Warranty_Period + "', description='" + mod.description + "',quantity='" + mod.quantity + "', unitprice='" + mod.unitprice + "',sys_Date = '" + DateTime.Now + "', sys_User='" + Constants.sysUser + "', status ='" + mod.status + "' WHERE DescriptionCode = '" + invID + "' ";
            using (SqlConnection conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
            {
                conn_uat.Open();
                SqlCommand sqlcmd = new SqlCommand(sql, conn_uat);
                _ = sqlcmd.ExecuteNonQuery();
                conn_uat.Close();
            }

            return RedirectToAction("View_Invoice_Full", "Hardware", new { mod.invno });
        }



        public ActionResult Submit_Inv_items(EnterItemsModel mod)
        {
            mod.sys_Date = DateTime.Now;

            string sql = "INSERT INTO [TechAssets].[dbo].[Hardware_Inovice_Items] ([Invoice_Ref],[DescriptionCode],[Item_Type],[BrandName],[ModelName],[Warranty_Period],[description],[quantity],[unitprice],[sys_Date],[sys_User],[status],[Asset]) VALUES ('" + mod.invno + "','" + mod.DescriptionCode + "','" + mod.Item_Type + "','" + mod.BrandName + "','" + mod.ModelName + "','" + mod.Warranty_Period + "','" + mod.description + "','" + mod.quantity + "','" + mod.unitprice + "','" + DateTime.Now + "','" + Constants.sysUser + "','" + mod.status + "','" + mod.Asset + "')";
            using (SqlConnection conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
            {
                conn_uat.Open();
                SqlCommand sqlcmd = new SqlCommand(sql, conn_uat);
                _ = sqlcmd.ExecuteNonQuery();
                conn_uat.Close();
            }
            return RedirectToAction("View_Invoice_Full", "Hardware", new { mod.invno });
        }



        public ActionResult DeleteInvoice(InvoicenGRNModel mod, string Invoice_Reference)
        {
            string sql3 = "UPDATE [TechAssets].[dbo].[Hardware_Master_Invoice] SET Status = '2' WHERE Invoice_Reference = '" + mod.Invoice_Reference + "'";
            using (SqlConnection conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
            {
                conn_uat.Open();
                SqlCommand sqlCmd3 = new SqlCommand(sql3, conn_uat);
                _ = sqlCmd3.ExecuteNonQuery();
                conn_uat.Close();
            }

            return RedirectToAction("InvDataView", mod);
        }




        public ActionResult Suppliers(string RedirectType, suppliersmodel mod, string Name_of_Supplier)
        {
            mod.CompanySelectList = HardwareModule.GetCompaniesSelectList();

            mod.RedirectType = RedirectType;
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            if (string.IsNullOrEmpty(mod.status))
            {
                mod.status = "ACT";
            }

            return View(mod);
        }


        public ActionResult EditSupplierData(string Name_of_Supplier, string SupplierID)
        {
            _ = new suppliersmodel();
            suppliersmodel mod = HardwareModule.EditSuppliers(Name_of_Supplier);
            mod.SupplierID = SupplierID;
            return View(mod);
        }

        public ActionResult UpdateSupplierData(suppliersmodel mod, string Name_of_Supplier)
        {
            string sql = "UPDATE [TechAssets].[dbo].[Hardware_Companies] SET Company_Name = '" + mod.Name_of_Supplier + "', Registration_Status = '" + mod.Registration_Status + "', Registration_Year ='" + mod.Registration_Year + "', Address_of_Dealer='" + mod.Address_of_Dealer + "' , email='" + mod.email + "',fax='" + mod.fax + "',Telephone='" + mod.Telephone + "',Contact_Person1='" + mod.Contact_Person1 + "',Contact_Person1_Phone='" + mod.Contact_Person1_Phone + "',Contact_Person2='" + mod.Contact_Person2 + "',Contact_Person2_Phone='" + mod.Contact_Person2_Phone + "', status='" + mod.status + "',DStatus=1,sysDate='" + DateTime.Now + "',sysUser='" + Constants.sysUser + "' ,CVendor='" + mod.CCategory1 + "', CCourier='" + mod.CCategory2 + "', CRepair='" + mod.CCategory3 + "' WHERE id = '" + mod.SupplierID + "' ";
            using (SqlConnection conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
            {
                conn_uat.Open();
                SqlCommand sqlcmd = new SqlCommand(sql, conn_uat);
                _ = sqlcmd.ExecuteNonQuery();
                conn_uat.Close();
            }

            return RedirectToAction("SuppDataView", "Hardware");
        }



        public ActionResult SuppDataSubmit(suppliersmodel mod)
        {
            mod.sysDate = DateTime.Now;

            int CCategory1 = mod.CCategory1 == "1" ? 1 : 0;
            int CCategory2 = mod.CCategory2 == "1" ? 1 : 0;
            int CCategory3 = mod.CCategory3 == "1" ? 1 : 0;

            string sql = @"
        INSERT INTO [TechAssets].[dbo].[Hardware_Companies]
        ([Company_Name],[Registration_Status],[Registration_Year],[Address_of_Dealer],
        [email],[fax],[Telephone],[Contact_Person1],[Contact_Person1_Phone],
        [Contact_Person2],[Contact_Person2_Phone],[status],[DStatus],[sysDate],
        [sysUser],[CVendor],[CCourier],[CRepair])
        VALUES 
        (@Name_of_Supplier, @Registration_Status, @Registration_Year, @Address_of_Dealer,
        @Email, @Fax, @Telephone, @Contact_Person1, @Contact_Person1_Phone,
        @Contact_Person2, @Contact_Person2_Phone, @Status, 1, @SysDate,
        @SysUser, @CCategory1, @CCategory2, @CCategory3)";

            using (SqlConnection conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
            {
                conn_uat.Open();
                using (SqlCommand sqlcmd = new SqlCommand(sql, conn_uat))
                {
                    _ = sqlcmd.Parameters.AddWithValue("@Name_of_Supplier", mod.Name_of_Supplier ?? (object)DBNull.Value);
                    _ = sqlcmd.Parameters.AddWithValue("@Registration_Status", mod.Registration_Status ?? (object)DBNull.Value);
                    _ = sqlcmd.Parameters.AddWithValue("@Registration_Year", mod.Registration_Year ?? (object)DBNull.Value);
                    _ = sqlcmd.Parameters.AddWithValue("@Address_of_Dealer", mod.Address_of_Dealer ?? (object)DBNull.Value);
                    _ = sqlcmd.Parameters.AddWithValue("@Email", mod.email ?? (object)DBNull.Value);
                    _ = sqlcmd.Parameters.AddWithValue("@Fax", mod.fax ?? (object)DBNull.Value);
                    _ = sqlcmd.Parameters.AddWithValue("@Telephone", mod.Telephone ?? (object)DBNull.Value);
                    _ = sqlcmd.Parameters.AddWithValue("@Contact_Person1", mod.Contact_Person1 ?? (object)DBNull.Value);
                    _ = sqlcmd.Parameters.AddWithValue("@Contact_Person1_Phone", mod.Contact_Person1_Phone ?? (object)DBNull.Value);
                    _ = sqlcmd.Parameters.AddWithValue("@Contact_Person2", mod.Contact_Person2 ?? (object)DBNull.Value);
                    _ = sqlcmd.Parameters.AddWithValue("@Contact_Person2_Phone", mod.Contact_Person2_Phone ?? (object)DBNull.Value);
                    _ = sqlcmd.Parameters.AddWithValue("@Status", mod.status ?? (object)DBNull.Value);
                    _ = sqlcmd.Parameters.AddWithValue("@SysDate", mod.sysDate);
                    _ = sqlcmd.Parameters.AddWithValue("@SysUser", Constants.sysUser);
                    _ = sqlcmd.Parameters.AddWithValue("@CCategory1", CCategory1);
                    _ = sqlcmd.Parameters.AddWithValue("@CCategory2", CCategory2);
                    _ = sqlcmd.Parameters.AddWithValue("@CCategory3", CCategory3);

                    _ = sqlcmd.ExecuteNonQuery();
                }
                conn_uat.Close();
            }

            TempData["SuccessMessage"] = "Supplier data submitted successfully!";

            return mod.RedirectType == "InvoiceNewSupplier"
                ? RedirectToAction("CreateInvoice", "Hardware")
                : (ActionResult)RedirectToAction("Suppliers", "Hardware");
        }




        public ActionResult SuppDataView()
        {
            suppliersmodel InMod = new suppliersmodel();
            string sql = "SELECT [Company_Name],[Registration_Year],[Address_of_Dealer],[Telephone],[status],id FROM [TechAssets].[dbo].[Hardware_Companies] WHERE DStatus = '1'";

            InMod.dt3 = HardwareModule.Get_Any_DT(sql);

            return View(InMod);
        }

        public ActionResult DeleteSupplier(suppliersmodel mod, string Name_of_Supplier, string supplierID)
        {
            string sql3 = "UPDATE [TechAssets].[dbo].[Hardware_Companies] SET DStatus = '2', status= 'Inactive' WHERE [id] = '" + supplierID + "'";
            using (SqlConnection conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
            {
                conn_uat.Open();
                SqlCommand sqlCmd3 = new SqlCommand(sql3, conn_uat);
                _ = sqlCmd3.ExecuteNonQuery();
                conn_uat.Close();
            }

            return RedirectToAction("SuppDataView", mod);
        }

        public ActionResult suppliersFullView(string id)
        {
            _ = new suppliersmodel();
            string mmm = "SELECT [Company_Name],[Registration_Status],[Registration_Year],[Address_of_Dealer],[email],[fax],[Telephone],[Contact_Person1],[Contact_Person1_Phone],[Contact_Person2],[Contact_Person2_Phone],[status] FROM [TechAssets].[dbo].[Hardware_Companies] where company_name = '" + id + "'";

            suppliersmodel InMod = HardwareModule.GetSupplierInfo(mmm);
            ViewBag.Supplier = InMod;

            return View(InMod);
        }

        public ActionResult AddItemType(string RedirectType, string invno)
        {
            EnterItemsModel Mod = new EnterItemsModel
            {
                Item_Type_Code = HardwareModule.GetItemTypeCode(),
                BrandList = HardwareModule.GetBrandListByDatabase(),
                ModelList = HardwareModule.GetModelListByDatabase()
            };
            ViewBag.RedirectType = RedirectType;
            Mod.invno = invno;

            return View(Mod);
        }

        public ActionResult DataPushToDB1(string index)
        {
            EnterItemsModel mod = new EnterItemsModel();
            _ = mod.Item_Type_Code;

            return View(mod);
        }

        public ActionResult SubmitAddItemType(EnterItemsModel mod, string invno, string brand, string model, string itemtype, string itemtypecode)
        {
            mod.sys_Date = DateTime.Now;

            string brandCode = HardwareModule.AddNewBrand(brand, itemtypecode);
            HardwareModule.AddNewModel(model, brandCode);

            string sql = "INSERT INTO [TechAssets].[dbo].[Hardware_Item_Types]([Item_Type_Code],[Item_Type],[brand],[model],[sysDate],[sysUser]) VALUES ('" + itemtypecode + "','" + itemtype + "','','','" + DateTime.Now + "','" + Constants.sysUser + "')";

            using (SqlConnection conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
            {
                conn_uat.Open();
                SqlCommand sqlcmd = new SqlCommand(sql, conn_uat);

                _ = sqlcmd.ExecuteNonQuery();

                conn_uat.Close();
            }

            return mod.RedirectType == "NewItemType"
                ? RedirectToAction("View_Invoice_Full", new { invno })
                : (ActionResult)RedirectToAction("AddItemType", "Hardware");

        }

        public ActionResult ItemTypeDataView()
        {
            EnterItemsModel Inmod = new EnterItemsModel();
            string sql = "SELECT  [Item_Type_Code],[Item_Type] FROM [TechAssets].[dbo].[Hardware_Item_Types]";
            Inmod.dt4 = HardwareModule.Get_Any_DT(sql);

            return View(Inmod);
        }

        public ActionResult AddBrand(string RedirectType, string invno)
        {
            EnterItemsModel Mod = new EnterItemsModel
            {
                BrandCode = HardwareModule.GetBrandCode(),
                BrandList = HardwareModule.GetBrandListByDatabase()
            };
            ViewBag.RedirectType = RedirectType;
            Mod.invno = invno;

            return View(Mod);
        }

        public ActionResult DataPushToDB2(string index)
        {
            EnterItemsModel mod = new EnterItemsModel();
            _ = mod.BrandCode;

            return View(mod);
        }

        public ActionResult SubmitAddBrand(EnterItemsModel mod, string invno)
        {
            string sql = " INSERT INTO [TechAssets].[dbo].[Hardware_Brand] ([BrandCode],[BrandName],[sysDate],[sysUser]) VALUES ('" + mod.BrandCode + "','" + mod.BrandName + "','" + DateTime.Now + "','" + Constants.sysUser + "')";
            _ = "INSERT INTO [TechAssets].[dbo].[Hardware_Item_Brand] (Item_Type_Code,BrandCode) VALUES ('" + mod.Item_Type + "','" + mod.BrandCode + "')";
            using (SqlConnection conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
            {
                conn_uat.Open();

                SqlCommand sqlcmd = new SqlCommand(sql, conn_uat);
                _ = sqlcmd.ExecuteNonQuery();
                conn_uat.Close();
            }

            return mod.RedirectType == "NewBrand"
                ? RedirectToAction("View_Invoice_Full", new { invno })
                : (ActionResult)RedirectToAction("AddBrand", "Hardware");
        }

        public ActionResult BrandDataView()
        {
            EnterItemsModel Inmod = new EnterItemsModel();
            string sql = " SELECT [BrandCode],[BrandName] FROM [TechAssets].[dbo].[Hardware_Brand]";
            Inmod.dt5 = HardwareModule.Get_Any_DT(sql);

            return View(Inmod);
        }

        public ActionResult model(string RedirectType, string invno, string ItmTypeCode)
        {
            EnterItemsModel Mod = new EnterItemsModel
            {
                ItemTypeList = HardwareModule.GetItemTypeListByDatabase(),

                BrandList = HardwareModule.GetBrandListByDatabase(),

                Item_Type_Code = HardwareModule.GetItemTypeCode(),
                BrandCode = HardwareModule.GetBrandCode(),
                ModelCode = HardwareModule.GetModelCode()
            };

            ViewBag.RedirectType = RedirectType;

            Mod.invno = invno;
            return View(Mod);
        }

        public ActionResult DataPushToDB3(string index)
        {
            EnterItemsModel mod = new EnterItemsModel();
            _ = mod.ModelCode;

            return View(mod);
        }

        public ActionResult submitModel(EnterItemsModel mod)
        {
            string sql3 = "INSERT INTO [TechAssets].[dbo].[Hardware_ModelNew] ([Model_Code],[Model_Name],[Item_Code],[Brand_Code],[sysDate],[sysUser]) VALUES ('" + mod.ModelCode + "','" + mod.ModelName + "','" + mod.Item_Type_Code + "','" + mod.BrandCode + "','" + DateTime.Now + "','" + Constants.sysUser + "')";
            using (SqlConnection conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
            {
                conn_uat.Open();
                SqlCommand sqlCmd3 = new SqlCommand(sql3, conn_uat);
                _ = sqlCmd3.ExecuteNonQuery();
                conn_uat.Close();
            }

            return RedirectToAction("model", "Hardware");

        }

        public ActionResult NItemSubmit(EnterItemsModel mod, string Item_Type_Code, string Item_Type)
        {
            string sql = "INSERT INTO [TechAssets].[dbo].[Hardware_Item_Types] ([Item_Type_Code],[Item_Type],[sysDate],[sysUser],[ShortCode]) VALUES ('" + mod.Item_Type_Code + "','" + mod.Item_Type + "','" + DateTime.Now + "','" + Constants.sysUser + "', '" + mod.ShortCode + "')";
            using (SqlConnection conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
            {

                conn_uat.Open();
                SqlCommand sqlcmd = new SqlCommand(sql, conn_uat);
                _ = sqlcmd.ExecuteNonQuery();
                conn_uat.Close();

            }
            return RedirectToAction("model", "Hardware");

        }

        public ActionResult NBrandSubmit(EnterItemsModel mod)
        {

            string sql = "INSERT INTO [TechAssets].[dbo].[Hardware_Brand] ([BrandCode],[BrandName],[sysDate],[sysUser]) VALUES ('" + mod.BrandCode + "','" + mod.BrandName + "','" + DateTime.Now + "','" + Constants.sysUser + "')";

            using (SqlConnection conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
            {

                conn_uat.Open();
                SqlCommand sqlcmd = new SqlCommand(sql, conn_uat);


                _ = sqlcmd.ExecuteNonQuery();


                conn_uat.Close();

            }
            return RedirectToAction("model", "Hardware");
        }

        public ActionResult NModelSubmit(EnterItemsModel mod, string ModelCode, string ModelName, string Item_Type_Code, string BrandCode)
        {
            string sql = "update [TechAssets].[dbo].[Hardware_ModelNew] set [Model_Code]='" + mod.ModelCode + "', [Model_Name] = '" + mod.ModelName + "' where [Item_Code] = '" + mod.Item_Type_Code + "' and [Brand_Code] = '" + mod.BrandCode + "'";

            using (SqlConnection conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
            {

                conn_uat.Open();
                SqlCommand sqlcmd = new SqlCommand(sql, conn_uat);
                _ = sqlcmd.ExecuteNonQuery();
                conn_uat.Close();

            }
            return RedirectToAction("model", "Hardware");
        }

        public ActionResult modelDataView()
        {
            EnterItemsModel Inmod = new EnterItemsModel();
            string sql = " SELECT [ModelCode],[ModelName],[BrandCode],[Item_Type_Code] FROM [TechAssets].[dbo].[Hardware_Models]";
            Inmod.dt6 = HardwareModule.Get_Any_DT(sql);

            return View(Inmod);
        }

        public JsonResult getTypeInfrom(string itemCode)
        {
            _ = new List<SelectListItem>();
            List<SelectListItem> result = HardwareModule.GetItemData(itemCode);


            return Json(new { result }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetModelInfo(string brandCode, string itemCode)
        {
            _ = new List<SelectListItem>();

            List<SelectListItem> result = HardwareModule.GetModelData(brandCode, itemCode);

            return Json(new { result }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult Devices(string invno, string invID, string invoiceNo, string Item, int count)
        {
            if (!(TempData["EnterItemsModel"] is EnterItemsModel mod))
            {
                mod = new EnterItemsModel();
            }

            int remain = GetRemainingQuantity(invID, count);

            string sql = "SELECT TOP 1 d.[IT_No] FROM [TechAssets].[dbo].[Hardware_Devices] d JOIN [TechAssets].[dbo].[Hardware_Inovice_Items] i ON d.[DescriptionCode] = i.[DescriptionCode] INNER JOIN [TechAssets].[dbo].[Hardware_Item_Types] it ON i.Item_Type = it.Item_Type_Code INNER JOIN [TechAssets].[dbo].[Hardware_DeviceStatus] ds ON d.[Current_Status] = ds.[StatusID] LEFT JOIN [EMMSDB].[dbo].[Staff_Division] sd ON d.[Purchased_DivisionID] = sd.[div_index] LEFT JOIN [EMMSDB].[dbo].[Staff_Division] sd_loc ON d.[Current_LocationID] = sd_loc.[div_index] WHERE it.Item_Type = @Item ORDER BY d.[IT_No] DESC";

            using (SqlConnection conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
            {
                conn_uat.Open();
                SqlCommand sqlcmd = new SqlCommand(sql, conn_uat);
                _ = sqlcmd.Parameters.AddWithValue("@Item", Item);

                object result = sqlcmd.ExecuteScalar();
                mod.StickerLastNo = result != null ? result.ToString() : "No existing devices on this type";

                conn_uat.Close();
            }

            mod.invno = invno;
            mod.invID = invID;
            mod.invoiceNo = invoiceNo;
            mod.Item = Item;
            mod.count = count;
            mod.remain = remain;

            mod.DivisionList = HardwareModule.GetDivisionListByDatabase();

            if (TempData["ErrorMessage"] != null)
            {
                ViewBag.ErrorMessage = TempData["ErrorMessage"];
            }

            if (TempData["SuccessMessage"] != null)
            {
                ViewBag.SuccessMessage = TempData["SuccessMessage"];
            }

            return View(mod);
        }



        public ActionResult DataPushToDB4(string index)
        {
            EnterItemsModel mod = new EnterItemsModel();
            return View(mod);
        }

        public ActionResult submitDevices(string invno, string invID, string invoiceNo, int count, string Item, string IT_No, string Serial_No, string INV_No, string QR, string Purchased_DivisionID, string Current_LocationID, string Remarks1)
        {
            try
            {


                string sql = string.IsNullOrEmpty(Purchased_DivisionID)
        ? "INSERT INTO [TechAssets].[dbo].[Hardware_Devices]([Current_Status], [Invoice_Reference], [DescriptionCode], [Invoice_No],[IT_No], [Serial_No], [INV_No], [QR], [Purchased_DivisionID],[Current_LocationID],[Remarks1], [activeStatus], [sysDate], [sysUser])" +
            " VALUES (1, @Invoice_Reference, @DescriptionCode, @InvoiceNo, @IT_No, @Serial_No, @INV_No, @QR, @Purchased_DivisionID, @Current_LocationID,@Remarks1, 1, @SysDate, @SysUser)"
        : "INSERT INTO [TechAssets].[dbo].[Hardware_Devices]([Current_Status], [Invoice_Reference], [DescriptionCode], [Invoice_No],[IT_No], [Serial_No], [INV_No], [QR], [Purchased_DivisionID],[Current_LocationID], [Remarks1], [activeStatus], [sysDate], [sysUser])" +
            " VALUES (2, @Invoice_Reference, @DescriptionCode, @InvoiceNo, @IT_No, @Serial_No, @INV_No, @QR, @Purchased_DivisionID, @Current_LocationID, @Remarks1, 1, @SysDate, @SysUser)";
                using (SqlConnection conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
                {
                    conn_uat.Open();
                    using (SqlCommand sqlcmd = new SqlCommand(sql, conn_uat))
                    {
                        _ = sqlcmd.Parameters.AddWithValue("@Invoice_Reference", invno);
                        _ = sqlcmd.Parameters.AddWithValue("@DescriptionCode", invID);
                        _ = sqlcmd.Parameters.AddWithValue("@InvoiceNo", invoiceNo);
                        _ = sqlcmd.Parameters.AddWithValue("@IT_No", IT_No);
                        _ = sqlcmd.Parameters.AddWithValue("@Serial_No", Serial_No);
                        _ = sqlcmd.Parameters.AddWithValue("@INV_No", INV_No);
                        _ = sqlcmd.Parameters.AddWithValue("@QR", QR);

                        if (string.IsNullOrEmpty(Purchased_DivisionID))
                        {
                            _ = sqlcmd.Parameters.AddWithValue("@Purchased_DivisionID", Current_LocationID);
                            _ = sqlcmd.Parameters.AddWithValue("@Current_LocationID", Current_LocationID);
                        }
                        else 
                        {
                            _ = sqlcmd.Parameters.AddWithValue("@Purchased_DivisionID", Purchased_DivisionID);
                            _ = sqlcmd.Parameters.AddWithValue("@Current_LocationID", "NRMDIV019");
                        }


                        _ = sqlcmd.Parameters.AddWithValue("@Remarks1", Remarks1);
                        _ = sqlcmd.Parameters.AddWithValue("@SysDate", DateTime.Now);
                        _ = sqlcmd.Parameters.AddWithValue("@SysUser", Constants.sysUser);
                        _ = sqlcmd.ExecuteNonQuery();

                    }
                }
                TempData["SuccessMessage"] = "Successfully added";
            }
            catch (SqlException ex)
            {
                TempData["ErrorMessage"] = ex.Number == 2627 ? "A device with this key already exists." : (object)("An error occurred while inserting the device: " + ex.Message);
            }

            return string.IsNullOrEmpty(Purchased_DivisionID)
                ? RedirectToAction("AddExsitingDEV", "Hardware", new
                {
                    invno,
                    invID,
                    invoiceNo,
                    count,
                    Item
                })
                : (ActionResult)RedirectToAction("Devices", "Hardware", new
                {
                    invno,
                    invID,
                    invoiceNo,
                    count,
                    Item
                });


        }



        public JsonResult GetEmployeeName(string empNo)
        {
            string employeeName = null;

            try
            {
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
                {
                    string query = "SELECT sed.[Name_with_ini] FROM [WebDB].[dbo].[staff] JOIN [EMMSDB].[dbo].[Staff_employee_Details] AS sed ON [WebDB].[dbo].[staff].[EmpNo] = sed.[Emp_No] WHERE status_code = '1'AND [WebDB].[dbo].[staff].[EmpNo] = @EmpNo";

                    using (SqlCommand command = new SqlCommand(query, conn))
                    {
                        _ = command.Parameters.AddWithValue("@EmpNo", empNo);

                        conn.Open();

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                employeeName = reader["Name_with_ini"].ToString();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }

            return Json(employeeName, JsonRequestBehavior.AllowGet);
        }



        public JsonResult getEmpName(string empNo)
        {
            string empName = HardwareModule.GetEmpName(empNo);
            return Json(new { empName }, JsonRequestBehavior.AllowGet);
        }


        public JsonResult invoiceItemNumber(string invoiceRef)
        {
            string newItemNo = HardwareModule.CreateInvoiceItemNumber(invoiceRef);
            return Json(new { newItemNo }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DevicesView(string DescriptionCode)
        {
            _ = new EnterItemsModel();
            string sql = "SELECT d.[IT_No], d.[Serial_No], d.[INV_No], s.division as Purchased_Division, st.Status as Current_Status, s1.division as Current_Location, it.Item_Type, d.Invoice_Reference, d.Invoice_No FROM [TechAssets].[dbo].[Hardware_Devices] d JOIN [TechAssets].[dbo].[Hardware_Inovice_Items] i ON d.[DescriptionCode] = i.[DescriptionCode] INNER JOIN [TechAssets].[dbo].[Hardware_Item_Types] it ON i.Item_Type = it.Item_Type_Code INNER JOIN [EMMSDB].[dbo].[Staff_Division] s ON d.Purchased_DivisionID = s.div_index INNER JOIN [EMMSDB].[dbo].[Staff_Division] s1 ON d.Current_LocationID = s1.div_index INNER JOIN [TechAssets].[dbo].[Hardware_DeviceStatus] st ON d.Current_Status = st.StatusID WHERE d.[DescriptionCode] = '" + DescriptionCode + "'";
            EnterItemsModel Inmod = HardwareModule.GetInvoiceInfo2(DescriptionCode);
            Inmod.dt8 = HardwareModule.Get_Any_DT(sql);


            Inmod.Invoice_Ref = Inmod.Invoice_Ref;
            Inmod.Invoice_No = Inmod.Invoice_No;
            Inmod.DescriptionCode = DescriptionCode;


            return View(Inmod);
        }


        public ActionResult DevicesFullView(string itNO)
        {
            _ = new EnterItemsModel();

            EnterItemsModel mod = HardwareModule.GetFUllDevicesInfo(itNO);


            string sql = "SELECT hh.[id], DS_HO.[Status] AS [HOstatus], [IT_No], CL.[division] AS Current_Location, SD_HO.[division] AS Handover_Location, [Authorized_Officer], CONVERT(VARCHAR, HH.HORecDate, 23) AS Handover_Request_Date, [HORecRemarks], [sysRe_Date], [sysRe_User], [sysApp_User], [sysApp_Date], [AppReReson], [HOComDate], [HOUser], [HOComRemarks], [sysCom_User], [sysCom_Date], [HOReceiver], CONVERT(VARCHAR, HH.ConDdate, 23) AS ConDdate_Date, [ConDRemaks], [SysConDUser], [SysConDate] FROM [TechAssets].[dbo].[Hardware_Handover] HH INNER JOIN [TechAssets].[dbo].[Hardware_DeviceStatus] DS ON HH.[Current_Status] = DS.[StatusID] INNER JOIN [TechAssets].[dbo].[Hardware_DeviceStatus] DS_HO ON HH.[HOstatus] = DS_HO.[StatusID] LEFT JOIN [EMMSDB].[dbo].[Staff_Division] SD_Current ON HH.[Current_LocationID] = SD_Current.[div_index] LEFT JOIN [EMMSDB].[dbo].[Staff_Division] SD_HO ON HH.[HOLocationID] = SD_HO.[div_index] LEFT JOIN [EMMSDB].[dbo].[Staff_Division] CL ON HH.[Current_LocationID] = CL.[div_index] WHERE [IT_No] = '" + itNO + "' ORDER BY hh.[id] DESC";
            mod.HOHistory = HardwareModule.Get_Any_DT(sql);


            string sql1 = "SELECT hd.[IT_No], hd.[Serial_No], hd.[INV_No], hd.[QR], sd1.[division] AS Purchased_Division, sd2.[division] AS Current_Location, hd.[Authorized_Officer], CONVERT(VARCHAR, hd.[Installation_Date], 23) AS Installation_Date, ds.[Status] AS CurrentStatus, hd.[activeStatus] FROM [TechAssets].[dbo].[Hardware_Devices] hd LEFT JOIN [EMMSDB].[dbo].[Staff_Division] sd1 ON hd.[Purchased_DivisionID] = sd1.[div_index] LEFT JOIN [EMMSDB].[dbo].[Staff_Division] sd2 ON hd.[Current_LocationID] = sd2.[div_index] LEFT JOIN [TechAssets].[dbo].[Hardware_DeviceStatus] ds ON hd.[Current_Status] = ds.[StatusID] WHERE [IT_No] = '" + itNO + "'";
            mod.Dinfor = HardwareModule.Get_Any_DT(sql1);

            string sql4 = "SELECT d.[Invoice_Reference], d.[DescriptionCode], d.[IT_No], m.[Invoice_No], CONVERT(VARCHAR, m.Invoice_Date, 23) , m.[PO_No], m.[Name_of_Supplier], m.[GRN_No], m.[GRN_Date], i.[Warranty_Period], i.[unitprice] FROM [TechAssets].[dbo].[Hardware_Devices] d LEFT JOIN [TechAssets].[dbo].[Hardware_Master_Invoice] m ON d.[Invoice_Reference] = m.[Invoice_Reference] LEFT JOIN [TechAssets].[dbo].[Hardware_Inovice_Items] i ON d.[DescriptionCode] = i.[DescriptionCode] WHERE d.[IT_No] = '" + itNO + "'";
            mod.InvTbl = HardwareModule.Get_Any_DT(sql4);

            string sql10 = "SELECT [PurchaseDate], [PurchasingPrice] FROM [TechAssets].[dbo].[Hardware_UpdateHistory] WHERE IT_No = '" + itNO + "'";
            mod.UPHTbl = HardwareModule.Get_Any_DT(sql10);

            string Cinforsql2 = "SELECT hc.[id], s.[Status] AS CourierCurrentStatus, hc.[CourierStatus], hc.[IT_No], hc.[HoID], CONVERT(VARCHAR, hc.[CReqDate], 23), div.division AS CourierLocation, hc.[CReqRemarks], hc.[CsysReqUser], hc.[CsysReqDate], hc.[CsysAppUser], CONVERT(VARCHAR, hc.[CsysAppDate], 23), hc.[CAppReReson], hc.[CompanyName], CONVERT(VARCHAR, hc.[StickerDate], 23), hc.[StickerNo], hc.[StickerRemaks], CONVERT(VARCHAR, hc.[PickupDate], 23), hc.[PickupRemaks], CONVERT(VARCHAR, hc.[CompleteDate], 23) , hc.[CompleteRemaks], hc.[ConfirmUser], hc.[DutyID], hc.[THoId], hc.[ConfirmDate] FROM [TechAssets].[dbo].[Hardware_Courier] hc JOIN [TechAssets].[dbo].[Hardware_DeviceStatus] s ON s.StatusID = hc.CourierStatus LEFT JOIN [EMMSDB].[dbo].[Staff_Division] div ON div.div_index = hc.CReqLocationID WHERE hc.[IT_No] = '" + itNO + "' ORDER BY hc.[id] DESC";
            mod.Cinfor = HardwareModule.Get_Any_DT(Cinforsql2);

            string sql2 = "SELECT th.[IT_No], s.[Status], s1.[Status] AS HandoverType, sd.[division], th.[THRequestDate], th.[DeviceUser], th.[Userfeedback], th.[ContactDetails], th.[SysTHReqUser], th.[SysTHReqDate], th.[SysTHAppUser], th.[SysTHAppDate], th.[CenterRejectReson], th.[ItemSentDate], th.[ItemSentType], th.[ItemSentRemaks], th.[SysTSentUser], th.[SysTSentDate], th.[ItemReciveRemaks], th.[ItemReciveDate], th.[SysTReciveUser], th.[SysTReciveDate], th.[ReturnDate], th.[CollectOfficer], th.[CollectRemarks], th.[SysCoUser], th.[SysCoDate], th.[THandoverStatus], th.[id] FROM [TechAssets].[dbo].[Hardware_THandover] AS th JOIN [TechAssets].[dbo].[Hardware_DeviceStatus] AS s ON th.THandoverStatus = s.StatusID JOIN [TechAssets].[dbo].[Hardware_DeviceStatus] AS s1 ON th.THandoverType = s1.StatusID JOIN [EMMSDB].[dbo].[Staff_Division] AS sd ON th.THandoverLocation = sd.[div_index] WHERE [THandoverStatus] IN (35, 36, 38) AND [IT_No] = '" + itNO + "'";
            mod.VThandoverTBL = HardwareModule.Get_Any_DT(sql2);

            string sql5 = "SELECT d.[IT_No], ity.[Item_Type], ity.[ShortCode], br.[BrandName], mo.[Model_Name], sp.[Processor], sp.[ProcessorSpeed], sp.[Memory], sp.[Storage], sp.[StorageType], sp.[OperatingSystem], sp.[ScreenSize], sp.[PanelType], sp.[SpeRemarks] FROM [TechAssets].[dbo].[Hardware_Devices] d LEFT JOIN [TechAssets].[dbo].[Hardware_DeviceSpecification] sp ON d.IT_No = sp.IT_No LEFT JOIN [TechAssets].[dbo].[Hardware_Master_Invoice] m ON d.[Invoice_Reference] = m.[Invoice_Reference] LEFT JOIN [TechAssets].[dbo].[Hardware_Inovice_Items] i ON d.[DescriptionCode] = i.[DescriptionCode] LEFT JOIN [TechAssets].[dbo].[Hardware_Item_Types] ity ON i.[Item_Type] = ity.[Item_Type_Code] LEFT JOIN [TechAssets].[dbo].[Hardware_Brand] br ON i.[BrandName] = br.[BrandCode] LEFT JOIN [TechAssets].[dbo].[Hardware_ModelNew] mo ON i.ModelName = mo.[Model_Code] WHERE d.[IT_No]  = '" + itNO + "'";
            mod.SpecTbl = HardwareModule.Get_Any_DT(sql5);

            string sql6 = "SELECT DA.[DutyID], DS1.[Status], DS.[Status] AS Reson, CONVERT(VARCHAR, DA.[DutyAssignDate], 23) AS DutyAssignDate, DA.[DutyAssignOfficer], DA.[DutyAssignOfficerSP], DA.[DutyAssignRemaks], DA.[SysRAssiUser] AS SysAssingUser, CONVERT(VARCHAR, DA.[SysRAssiDate], 23), DA.[FaultTypeIDs] AS TechnicalAssesmentIDs, DA.[TechnicalAssesment], DA.[SolutionTypeIDs], DA.[Solutions], CONVERT(VARCHAR, DA.[SysRCompleteDate], 23) AS SysRCompleteDate, CONVERT(VARCHAR, DA.[OutsideRReqDate], 23) AS OutsideRReqDate, DA.[IT_No], DA.[THandoverID], Th.[ItemReciveDate], Th.[SysTReciveUser] FROM [TechAssets].[dbo].[Hardware_DutyAssign] DA LEFT JOIN [TechAssets].[dbo].[Hardware_DeviceStatus] DS ON DA.THandoverType = DS.StatusID LEFT JOIN [TechAssets].[dbo].[Hardware_DeviceStatus] DS1 ON DA.DutyStatus = DS1.StatusID LEFT JOIN [TechAssets].[dbo].[Hardware_THandover] Th ON Th.id = DA.[THandoverID] WHERE DA.[IT_No] = '" + itNO + "' ORDER BY DA.[DutyID] DESC";
            mod.TaskTbl = HardwareModule.Get_Any_DT(sql6);

            string sql9 = "SELECT D.[DisposeID], D.[IT_No], S.Status, D.[DisposeReason], D.[sysReqUser], CONVERT(VARCHAR, D.[sysReqDate], 23) AS ReqDate, D.[sysRecUser], CONVERT(VARCHAR, D.[sysRecDate], 23) AS RecDate, D.[RecRemarks], D.[sysAppUser], CONVERT(VARCHAR, D.[sysAppDate], 23) AS sysAppDate, D.[AppRemarks], D.[RLetterDate], D.[RLetterRemarks], D.[DisposeDate], D.[BulkNo], DB.[DisposeCCompany], D.[ITDisposeNo], D.[FinalRemarks], D.[DisposeRemarks], D.[DisposeCode] FROM [TechAssets].[dbo].[Hardware_Dispose] D LEFT JOIN [TechAssets].[dbo].[Hardware_DeviceStatus] S ON D.[DisposeStatus] = S.StatusID LEFT JOIN [TechAssets].[dbo].[Hardware_DisposeBulk] DB ON D.[BulkNo] = DB.[DisposeBulkID] WHERE D.[IT_No] = '" + itNO + "' ORDER BY D.[DisposeID] DESC";
            mod.DisTbl = HardwareModule.Get_Any_DT(sql9);

            string sql7 = "SELECT RO.[id], DS.[Status], CONVERT(VARCHAR, RO.[OutsideRReqDate], 23) AS OutsideRReqDate, RO.[ORReson], GP.[CompanyID], CONVERT(VARCHAR, GP.[SentDate], 23) AS SentDate  , CONVERT(VARCHAR, RO.[RepairCompleteDate], 23) AS RepairCompleteDate , GP.OfficerName, GP.Remarks, GP.SysUser, RO.[DeviceRemaks], RO.[EstimateRecDate], RO.[Solution], RO.[RepairPrice], RO.[Warranty], RO.[EstimateRemaks], RO.[ApprovelLetterDate], RO.[ApproveStatus], RO.[RejectReson], RO.[ApproveInformDate], RO.[RepairCompleteDate], RO.[RepairRemaks], RO.[RepairOrNot], RO.[SysUser], RO.[SysDate], [InvoiceNo], [InvoiceDate], [PayVoucherDate], [VoucherNo], [DutyID], [IT_No] FROM [TechAssets].[dbo].[Hardware_RepairOut] RO LEFT JOIN [TechAssets].[dbo].[Hardware_DeviceStatus] DS ON RO.[ROutsideStatus] = DS.StatusID LEFT JOIN [TechAssets].[dbo].[Hardware_GatePass] GP ON GP.[ORBulkNo] = RO.[GatepassNo] WHERE RO.[IT_No] = '" + itNO + "' ORDER BY RO.[id] DESC";
            mod.OutReTBL = HardwareModule.Get_Any_DT(sql7);

            string sql3 = "SELECT da.[THandoverID], da.[DutyStatus], da.[DutyAssignDate], da.[IT_No], da.[DutyAssignOfficer], da.[DutyAssignEmpNo], da.[DutyAssignOfficerSP], da.[DutyAssignEmpNoSP], da.[DutyAssignRemaks], da.[SysRAssiUser], da.[SysRAssiDate], da.[TechnicalAssesment], da.[FaultTypeIDs], da.[SysTAEnterDate], da.[Solutions], da.[SolutionsO], da.[SysSoEnterDate], da.[SysRCompleteDate], da.[OutsideRReqDate], da.[DutyID] FROM [TechAssets].[dbo].[Hardware_DutyAssign] AS da WHERE da.[DutyStatus] IN (35, 47) AND [IT_No] = '" + itNO + "'";

            mod.VDutyATBL = HardwareModule.Get_Any_DT(sql3);
           
            ViewBag.ItNO = itNO;
            return View(mod);

        }




        public JsonResult Get_Brand_List(string x)
        {
            EnterItemsModel mod = new EnterItemsModel
            {
                BrandList = HardwareModule.GetBrandListByDatabase2(x)
            };

            return Json(mod.BrandList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Get_Model_List(string x, string y)
        {
            EnterItemsModel mod = new EnterItemsModel
            {
                ModelList = HardwareModule.GetModelListByDatabase2(x, y)
            };

            return Json(mod.ModelList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CheckSupplierName(string supplierName)
        {
            bool exists = false;
            string sql = "SELECT COUNT(*) FROM [TechAssets].[dbo].[Hardware_Companies] WHERE [Company_Name] = @supplierName AND status = 'ACT'";

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(sql, conn);
                _ = cmd.Parameters.AddWithValue("@supplierName", supplierName);

                exists = (int)cmd.ExecuteScalar() > 0;
            }

            return Json(new { exists }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult FindDevices(string status)
        {
            string sql = null;

            if (status == null)
            {
                sql = "SELECT it.Item_Type, d.[Invoice_Reference], d.[IT_No], d.[Serial_No], d.[INV_No], d.[QR], d.Current_Status, ds.[Status] AS Current_Status_Text, d.[activeStatus], sd.[division] AS Purchased_Division_Name, sd_loc.[division] AS Current_Location_Name FROM [TechAssets].[dbo].[Hardware_Devices] d JOIN [TechAssets].[dbo].[Hardware_Inovice_Items] i ON d.[DescriptionCode] = i.[DescriptionCode] INNER JOIN [TechAssets].[dbo].[Hardware_Item_Types] it ON i.Item_Type = it.Item_Type_Code INNER JOIN [TechAssets].[dbo].[Hardware_DeviceStatus] ds ON d.[Current_Status] = ds.[StatusID] LEFT JOIN [EMMSDB].[dbo].[Staff_Division] sd ON d.[Purchased_DivisionID] = sd.[div_index] LEFT JOIN [EMMSDB].[dbo].[Staff_Division] sd_loc ON d.[Current_LocationID] = sd_loc.[div_index] WHERE d.[activeStatus] = 1";
            }
            else if (status == "All")
            {

                sql = "SELECT it.Item_Type, d.[Invoice_Reference], d.[IT_No], d.[Serial_No], d.[INV_No], d.[QR], d.Current_Status, ds.[Status] AS Current_Status_Text, d.[activeStatus], sd.[division] AS Purchased_Division_Name, sd_loc.[division] AS Current_Location_Name FROM [TechAssets].[dbo].[Hardware_Devices] d JOIN [TechAssets].[dbo].[Hardware_Inovice_Items] i ON d.[DescriptionCode] = i.[DescriptionCode] INNER JOIN [TechAssets].[dbo].[Hardware_Item_Types] it ON i.Item_Type = it.Item_Type_Code INNER JOIN [TechAssets].[dbo].[Hardware_DeviceStatus] ds ON d.[Current_Status] = ds.[StatusID] LEFT JOIN [EMMSDB].[dbo].[Staff_Division] sd ON d.[Purchased_DivisionID] = sd.[div_index] LEFT JOIN [EMMSDB].[dbo].[Staff_Division] sd_loc ON d.[Current_LocationID] = sd_loc.[div_index] WHERE d.[activeStatus] = 1";
            }
            else if (status == "55")
            {
                sql = "SELECT it.Item_Type, d.[Invoice_Reference], d.[IT_No], d.[Serial_No], d.[INV_No], d.[QR], d.Current_Status, ds.[Status] AS Current_Status_Text, d.[activeStatus], sd.[division] AS Purchased_Division_Name, sd_loc.[division] AS Current_Location_Name FROM [TechAssets].[dbo].[Hardware_Devices] d JOIN [TechAssets].[dbo].[Hardware_Inovice_Items] i ON d.[DescriptionCode] = i.[DescriptionCode] INNER JOIN [TechAssets].[dbo].[Hardware_Item_Types] it ON i.Item_Type = it.Item_Type_Code INNER JOIN [TechAssets].[dbo].[Hardware_DeviceStatus] ds ON d.[Current_Status] = ds.[StatusID] LEFT JOIN [EMMSDB].[dbo].[Staff_Division] sd ON d.[Purchased_DivisionID] = sd.[div_index] LEFT JOIN [EMMSDB].[dbo].[Staff_Division] sd_loc ON d.[Current_LocationID] = sd_loc.[div_index] WHERE d.[Current_Status] = 55";
            }
            else if (status == status)
            {
                sql = "SELECT it.Item_Type, d.[Invoice_Reference], d.[IT_No], d.[Serial_No], d.[INV_No], d.[QR], d.Current_Status, ds.[Status] AS Current_Status_Text, d.[activeStatus], sd.[division] AS Purchased_Division_Name, sd_loc.[division] AS Current_Location_Name FROM [TechAssets].[dbo].[Hardware_Devices] d JOIN [TechAssets].[dbo].[Hardware_Inovice_Items] i ON d.[DescriptionCode] = i.[DescriptionCode] INNER JOIN [TechAssets].[dbo].[Hardware_Item_Types] it ON i.Item_Type = it.Item_Type_Code INNER JOIN [TechAssets].[dbo].[Hardware_DeviceStatus] ds ON d.[Current_Status] = ds.[StatusID] LEFT JOIN [EMMSDB].[dbo].[Staff_Division] sd ON d.[Purchased_DivisionID] = sd.[div_index] LEFT JOIN [EMMSDB].[dbo].[Staff_Division] sd_loc ON d.[Current_LocationID] = sd_loc.[div_index] WHERE d.[activeStatus] = 1 AND d.[Current_Status] = '" + status + "'";
            }


            EnterItemsModel Inmod = new EnterItemsModel
            {
                dt9 = HardwareModule.Get_Any_DT(sql),
                DStatus = status
            };
            ViewBag.CurrentStatus = status;
            return View(Inmod);
        }





        public ActionResult HandOverForm(string itNo)
        {
            _ = new EnterItemsModel();
            EnterItemsModel mod = HardwareModule.GetFUllDevicesInfo(itNo);
            mod.sysRe_User = Constants.sysUser;
            mod.DivisionList = HardwareModule.GetDivisionListByDatabase();

            return View(mod);
        }

        public ActionResult DataPushToDB5(string index)
        {
            EnterItemsModel mod = new EnterItemsModel();
            return View(mod);
        }

        public ActionResult UpdateHandOver(EnterItemsModel mod)
        {
            string sql1 = "INSERT INTO [TechAssets].[dbo].[Hardware_Handover] ([Current_Status],[HOstatus],[IT_No],[Current_LocationID],[HOLocationID],[Authorized_Officer],[HORecDate],[HORecRemarks],[sysRe_User],[sysRe_Date]) VALUES (2,10,'" + mod.IT_No + "','" + mod.Current_LocationID + "','" + mod.HOLocation + "', '" + mod.Authorized_Officer + "', '" + mod.Installation_Date + "','" + mod.Remarks2 + "','" + Constants.sysUser + "', '" + DateTime.Now + "')";

            string sql = "UPDATE [TechAssets].[dbo].[Hardware_Devices] SET Current_Status = 10, [Authorized_Officer] = '" + mod.Authorized_Officer + "' WHERE IT_No = '" + mod.IT_No + "'";

            using (SqlConnection conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
            {
                conn_uat.Open();
                SqlCommand sqlcmd = new SqlCommand(sql, conn_uat);
                SqlCommand sqlcmd1 = new SqlCommand(sql1, conn_uat);

                _ = sqlcmd.ExecuteNonQuery();
                _ = sqlcmd1.ExecuteNonQuery();
                conn_uat.Close();
            }

            return RedirectToAction("DevicesInIT", "Hardware");
        }

        public ActionResult PendingAppHandover()
        {
            EnterItemsModel InMod = new EnterItemsModel();
            string sql = "SELECT it.Item_Type, h.IT_No, div.division AS Purchased_Division, div1.division AS Current_Location, div2.division AS Handover_Location, ho.Authorized_Officer, ho.HORecRemarks, CONVERT(VARCHAR, ho.HORecDate, 23) AS RecomendcDate, ho.sysRe_User, ho.id FROM [TechAssets].[dbo].[Hardware_Devices] h JOIN [TechAssets].[dbo].[Hardware_Inovice_Items] i ON h.DescriptionCode = i.DescriptionCode JOIN [TechAssets].[dbo].[Hardware_Item_Types] it ON i.item_type = it.Item_Type_Code LEFT JOIN [EMMSDB].[dbo].[Staff_Division] div ON h.Purchased_DivisionID = div.div_index LEFT JOIN [EMMSDB].[dbo].[Staff_Division] div1 ON h.Current_LocationID = div1.div_index LEFT JOIN [TechAssets].[dbo].[Hardware_Handover] ho ON h.IT_No = ho.IT_No LEFT JOIN [EMMSDB].[dbo].[Staff_Division] div2 ON ho.HOLocationID = div2.div_index WHERE ho.HOstatus = '10'";
            InMod.dt10 = HardwareModule.Get_Any_DT(sql);

            return View(InMod);
        }



        public ActionResult HandoverFilterView(string status)
        {
            string sql = null;


            if (status == null)
            {
                sql = "SELECT it.Item_Type, h.IT_No, h.Serial_No, h.INV_No, h.QR, div.division AS Purchased_Division, div1.division AS Current_Location, div2.division AS Handover_Location, ho.Authorized_Officer, ho.HORecRemarks, ho.HOstatus, st.Status, ho.HOLocationID FROM [TechAssets].[dbo].[Hardware_Devices] h JOIN [TechAssets].[dbo].[Hardware_Inovice_Items] i ON h.DescriptionCode = i.DescriptionCode JOIN [TechAssets].[dbo].[Hardware_Item_Types] it ON i.item_type = it.Item_Type_Code LEFT JOIN [EMMSDB].[dbo].[Staff_Division] div ON h.Purchased_DivisionID = div.div_index LEFT JOIN [EMMSDB].[dbo].[Staff_Division] div1 ON h.Current_LocationID = div1.div_index LEFT JOIN [TechAssets].[dbo].[Hardware_Handover] ho ON h.IT_No = ho.IT_No JOIN [TechAssets].[dbo].[Hardware_DeviceStatus] st ON ho.HOstatus = st.StatusID LEFT JOIN [EMMSDB].[dbo].[Staff_Division] div2 ON ho.HOLocationID = div2.div_index WHERE ho.HOstatus IN (CAST(3 AS INT), CAST(4 AS INT), CAST(10 AS INT))";
            }

            else if (status == "All")
            {
                sql = "SELECT it.Item_Type, h.IT_No, h.Serial_No, h.INV_No, h.QR, div.division AS Purchased_Division, div1.division AS Current_Location, div2.division AS Handover_Location, ho.Authorized_Officer, ho.HORecRemarks, ho.HOstatus, st.Status, ho.HOLocationID FROM [TechAssets].[dbo].[Hardware_Devices] h JOIN [TechAssets].[dbo].[Hardware_Inovice_Items] i ON h.DescriptionCode = i.DescriptionCode JOIN [TechAssets].[dbo].[Hardware_Item_Types] it ON i.item_type = it.Item_Type_Code LEFT JOIN [EMMSDB].[dbo].[Staff_Division] div ON h.Purchased_DivisionID = div.div_index LEFT JOIN [EMMSDB].[dbo].[Staff_Division] div1 ON h.Current_LocationID = div1.div_index LEFT JOIN [TechAssets].[dbo].[Hardware_Handover] ho ON h.IT_No = ho.IT_No JOIN [TechAssets].[dbo].[Hardware_DeviceStatus] st ON ho.HOstatus = st.StatusID LEFT JOIN [EMMSDB].[dbo].[Staff_Division] div2 ON ho.HOLocationID = div2.div_index WHERE ho.HOstatus IN (CAST(3 AS INT), CAST(4 AS INT), CAST(10 AS INT))";

            }

            else if (status == status)
            {
                sql = "SELECT it.Item_Type, h.IT_No, h.Serial_No, h.INV_No, h.QR, div.division AS Purchased_Division, div1.division AS Current_Location, div2.division AS Handover_Location, ho.Authorized_Officer, ho.HORecRemarks, ho.HOstatus, st.Status, ho.HOLocationID FROM [TechAssets].[dbo].[Hardware_Devices] h JOIN [TechAssets].[dbo].[Hardware_Inovice_Items] i ON h.DescriptionCode = i.DescriptionCode JOIN [TechAssets].[dbo].[Hardware_Item_Types] it ON i.item_type = it.Item_Type_Code LEFT JOIN [EMMSDB].[dbo].[Staff_Division] div ON h.Purchased_DivisionID = div.div_index LEFT JOIN [EMMSDB].[dbo].[Staff_Division] div1 ON h.Current_LocationID = div1.div_index LEFT JOIN [TechAssets].[dbo].[Hardware_Handover] ho ON h.IT_No = ho.IT_No JOIN [TechAssets].[dbo].[Hardware_DeviceStatus] st ON ho.HOstatus = st.StatusID LEFT JOIN [EMMSDB].[dbo].[Staff_Division] div2 ON ho.HOLocationID = div2.div_index WHERE ho.HOstatus = '" + status + "'";

                //WHERE h.[Current_Status] = '" + status + "'"
            }


            EnterItemsModel Inmod = new EnterItemsModel
            {
                CCategory = HardwareModule.GetCourierCompaniesSelectList(),
                CFilter = HardwareModule.Get_Any_DT(sql),
                HStatus = status
            };
            ViewBag.CurrentStatus = status;
            return View(Inmod);
        }


        public ActionResult EditDevices(string itNO)
        {
            _ = new EnterItemsModel();
            EnterItemsModel mod = HardwareModule.EditDevicesData(itNO);
            mod.DivisionList = HardwareModule.GetDivisionListByDatabase();
            return View(mod);
        }


        public ActionResult UpdateDevicesData(EnterItemsModel mod, string IT_No, string invno, string invID, string invoiceNo)
        {
            string sql = "UPDATE [TechAssets].[dbo].[Hardware_Devices] SET Invoice_Reference = '" + mod.invno + "', DescriptionCode = '" + mod.invID + "', Invoice_No ='" + mod.invoiceNo + "', IT_No='" + mod.IT_No + "' , Serial_No='" + mod.Serial_No + "', INV_No ='" + mod.INV_No + "', QR ='" + mod.QR + "', Purchased_Division ='" + mod.Purchased_Division + "', Current_Location ='" + mod.Current_Location + "', Current_Status='" + mod.Current_Status + "', Authorized_Officer ='" + mod.Authorized_Officer + "', Installation_Date =  " + (mod.Installation_Date == null ? "NULL" : "'" + mod.Installation_Date + "'") + " , Remarks1 = '" + mod.Remarks1 + "', Remarks2 = '" + mod.Remarks2 + "' , activeStatus = 1 , sysDate = '" + DateTime.Now + "' , sysUser = '" + Constants.sysUser + "'  WHERE IT_No = '" + IT_No + "' ";
            using (SqlConnection conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
            {
                conn_uat.Open();
                SqlCommand sqlcmd = new SqlCommand(sql, conn_uat);
                _ = sqlcmd.ExecuteNonQuery();
                conn_uat.Close();
            }

            return RedirectToAction("SuppDataView", "Hardware");
        }


        [HttpPost]
        public JsonResult ApproveHandover(string IT_No, string New_HO_Location, string Authorized_Officer, EnterItemsModel mod, int? Ho_Id)
        {
            try
            {
                string sql2 = "UPDATE [TechAssets].[dbo].[Hardware_Handover] SET HOstatus = '4', sysApp_User = @sysApp_User, sysApp_Date = @SysApp_Date WHERE id = @Ho_Id";
                string sql1 = "UPDATE [TechAssets].[dbo].[Hardware_Devices] SET Current_Status = '4' WHERE IT_No = @IT_No";

                using (SqlConnection conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
                {
                    conn_uat.Open();


                    using (SqlCommand sqlcmd = new SqlCommand(sql1, conn_uat))
                    {
                        _ = sqlcmd.Parameters.AddWithValue("@IT_No", IT_No);
                        _ = sqlcmd.ExecuteNonQuery();
                    }


                    using (SqlCommand sqlcmd1 = new SqlCommand(sql2, conn_uat))
                    {
                        _ = sqlcmd1.Parameters.AddWithValue("@IT_No", IT_No);
                        _ = sqlcmd1.Parameters.AddWithValue("@Ho_Id", Ho_Id);
                        _ = sqlcmd1.Parameters.AddWithValue("@sysApp_User", Constants.sysUser);
                        _ = sqlcmd1.Parameters.AddWithValue("@SysApp_Date", DateTime.Now);

                        _ = sqlcmd1.ExecuteNonQuery();
                    }
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }




        [HttpPost]
        public ActionResult RejectHandover(string IT_No, string Remarks3, int? Ho_Id)
        {

            string sql2 = "UPDATE [TechAssets].[dbo].[Hardware_Handover] SET HOstatus = '5', sysApp_User = '" + Constants.sysUser + "', sysApp_Date = @SysApp_Date, AppReReson = @Remarks3 WHERE id = @Ho_Id";
            string sql = "UPDATE [TechAssets].[dbo].[Hardware_Devices] SET Current_Status = '2' WHERE IT_No = @IT_No";


            using (SqlConnection conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
            {
                conn_uat.Open();

                using (SqlCommand sqlcmd = new SqlCommand(sql, conn_uat))
                {
                    _ = sqlcmd.Parameters.AddWithValue("@IT_No", IT_No);
                    _ = sqlcmd.ExecuteNonQuery();
                }

                using (SqlCommand sqlcmd2 = new SqlCommand(sql2, conn_uat))
                {
                    _ = sqlcmd2.Parameters.AddWithValue("@Ho_Id", Ho_Id);
                    _ = sqlcmd2.Parameters.AddWithValue("@Remarks3", Remarks3);
                    _ = sqlcmd2.Parameters.AddWithValue("@SysApp_Date", DateTime.Now);
                    _ = sqlcmd2.ExecuteNonQuery();
                }

                conn_uat.Close();
            }

            return Json(new { success = true });
        }


        public ActionResult Empty()
        {
            return View();
        }


        public ActionResult Dispose(string itNO)
        {
            _ = new EnterItemsModel();

            EnterItemsModel mod = HardwareModule.GetFUllDevicesInfo(itNO);
            ViewBag.DevicesFull = mod;

            return View(mod);

        }

        public ActionResult DataPushToDB6(string index)
        {
            EnterItemsModel mod = new EnterItemsModel();
            return View(mod);
        }



        public ActionResult RequestDispose(string itno, string dutyID, string reason)
        {
            try
            {

                string sql1 = "INSERT INTO [TechAssets].[dbo].[Hardware_Dispose] ([IT_No],[DutyID],[sysReqUser],[sysReqDate],[DisposeStatus],[DisposeReason]) VALUES (@IT_No, @DutyID, @sysReqUser, @sysReqDate, @DisposeStatus,@DisposeReason)";
                string sql2 = "UPDATE [TechAssets].[dbo].[Hardware_Devices] SET Current_Status='6' WHERE IT_No = @IT_No";
                string sql3 = "UPDATE [TechAssets].[dbo].[Hardware_DutyAssign] SET [DutyStatus] ='6' WHERE DutyID = @DutyID";

                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
                {
                    conn.Open();


                    using (SqlCommand cmdInsert = new SqlCommand(sql1, conn))
                    {
                        _ = cmdInsert.Parameters.AddWithValue("@IT_No", itno);
                        _ = cmdInsert.Parameters.AddWithValue("@DutyID", dutyID);
                        _ = cmdInsert.Parameters.AddWithValue("@sysReqUser", Constants.sysUser);
                        _ = cmdInsert.Parameters.AddWithValue("@sysReqDate", DateTime.Now);
                        _ = cmdInsert.Parameters.AddWithValue("@DisposeStatus", 6);
                        _ = cmdInsert.Parameters.AddWithValue("@DisposeReason", reason);
                        _ = cmdInsert.ExecuteNonQuery();
                    }


                    using (SqlCommand cmdUpdate = new SqlCommand(sql2, conn))
                    {
                        _ = cmdUpdate.Parameters.AddWithValue("@IT_No", itno);
                        _ = cmdUpdate.ExecuteNonQuery();
                    }

                    using (SqlCommand cmdUpdate1 = new SqlCommand(sql3, conn))
                    {
                        _ = cmdUpdate1.Parameters.AddWithValue("@DutyID", dutyID);
                        _ = cmdUpdate1.ExecuteNonQuery();
                    }
                }

                return Json(new { success = true, message = "Successfully submitted!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }




        public ActionResult PendingRecommendDispose(EnterItemsModel InMod)
        {
            string sql = "SELECT h.IT_No, pd.[division] AS Purchased_Division, cl.[division] AS Current_Location, h.DutyID, h.sysReqUser, CONVERT(VARCHAR, h.[sysReqDate], 23) AS [sysReqDate], it.Item_Type, ds.Status AS Current_Status, h.DisposeID, h.DisposeReason FROM [TechAssets].[dbo].[Hardware_Dispose] h JOIN [TechAssets].[dbo].[Hardware_Devices] d ON h.IT_No = d.IT_No JOIN [TechAssets].[dbo].[Hardware_Inovice_Items] i ON d.DescriptionCode = i.DescriptionCode JOIN [TechAssets].[dbo].[Hardware_Item_Types] it ON i.Item_Type = it.Item_Type_Code JOIN [TechAssets].[dbo].[Hardware_DeviceStatus] ds ON d.Current_Status = ds.StatusID LEFT JOIN [EMMSDB].[dbo].[Staff_Division] pd ON d.Purchased_DivisionID = pd.div_index LEFT JOIN [EMMSDB].[dbo].[Staff_Division] cl ON d.[Current_LocationID] = cl.[div_index] WHERE d.Current_Status = '6'";
            InMod.dt11 = HardwareModule.Get_Any_DT(sql);

            return View(InMod);
        }

        [HttpPost]
        public JsonResult RecommendedDispose(string IT_No, string DisposeID, string dutyID)
        {
            try
            {
                string sql1 = "UPDATE [TechAssets].[dbo].[Hardware_Devices] SET Current_Status = '7' WHERE IT_No = @IT_No";
                string sql2 = "UPDATE [TechAssets].[dbo].[Hardware_Dispose] SET DisposeStatus = '7',  sysRecUser = '" + Constants.sysUser + "', sysRecDate = GETDATE() WHERE DisposeID = @DisposeID";
                string sql3 = "UPDATE [TechAssets].[dbo].[Hardware_DutyAssign] SET Current_Status = '7' WHERE DutyID = @dutyID";
                using (SqlConnection conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
                {
                    conn_uat.Open();

                    using (SqlCommand sqlcmd = new SqlCommand(sql1, conn_uat))
                    {
                        _ = sqlcmd.Parameters.AddWithValue("@IT_No", IT_No);
                        _ = sqlcmd.ExecuteNonQuery();
                    }

                    using (SqlCommand sqlcmd1 = new SqlCommand(sql2, conn_uat))
                    {
                        _ = sqlcmd1.Parameters.AddWithValue("@DisposeID", DisposeID);
                        _ = sqlcmd1.Parameters.AddWithValue("@SysApp_Date", DateTime.Now);
                        _ = sqlcmd1.ExecuteNonQuery();
                    }

                    using (SqlCommand sqlcmd2 = new SqlCommand(sql3, conn_uat))
                    {
                        _ = sqlcmd2.Parameters.AddWithValue("@dutyID", dutyID);
                        _ = sqlcmd2.ExecuteNonQuery();
                    }
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                // Handle exception (log it, etc.)
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult ApprovedDispose(string IT_No, string DisposeID, string dutyID)
        {
            try
            {
                string sql1 = "UPDATE [TechAssets].[dbo].[Hardware_Devices] SET Current_Status = '8' WHERE IT_No = @IT_No";
                string sql2 = "UPDATE [TechAssets].[dbo].[Hardware_Dispose] SET DisposeStatus = '8',  sysAppUser = '" + Constants.sysUser + "', sysAppDate = GETDATE() WHERE DisposeID = @DisposeID";
                string sql3 = "UPDATE [TechAssets].[dbo].[Hardware_DutyAssign] SET Current_Status = '8' WHERE DutyID = @dutyID";
                using (SqlConnection conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
                {
                    conn_uat.Open();

                    using (SqlCommand sqlcmd = new SqlCommand(sql1, conn_uat))
                    {
                        _ = sqlcmd.Parameters.AddWithValue("@IT_No", IT_No);
                        _ = sqlcmd.ExecuteNonQuery();
                    }

                    using (SqlCommand sqlcmd1 = new SqlCommand(sql2, conn_uat))
                    {
                        _ = sqlcmd1.Parameters.AddWithValue("@DisposeID", DisposeID);
                        _ = sqlcmd1.Parameters.AddWithValue("@SysApp_Date", DateTime.Now);
                        _ = sqlcmd1.ExecuteNonQuery();
                    }

                    using (SqlCommand sqlcmd2 = new SqlCommand(sql3, conn_uat))
                    {
                        _ = sqlcmd2.Parameters.AddWithValue("@dutyID", dutyID);
                        _ = sqlcmd2.ExecuteNonQuery();
                    }
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                // Handle exception (log it, etc.)
                return Json(new { success = false, message = ex.Message });
            }
        }

        public ActionResult PendingApprovalDispose(EnterItemsModel InMod)
        {
            string sql = "SELECT h.IT_No, pd.[division] AS Purchased_Division,  h.DutyID,h.sysReqUser, CONVERT(VARCHAR, h.sysReqDate, 23) AS sysReqDate, h.sysRecUser, CONVERT(VARCHAR, h.sysRecDate, 23) AS sysRecDate, it.Item_Type, ds.Status AS Current_Status, h.DisposeID, h.DisposeReason FROM [TechAssets].[dbo].[Hardware_Dispose] h JOIN [TechAssets].[dbo].[Hardware_Devices] d ON h.IT_No = d.IT_No JOIN [TechAssets].[dbo].[Hardware_Inovice_Items] i ON d.DescriptionCode = i.DescriptionCode JOIN [TechAssets].[dbo].[Hardware_Item_Types] it ON i.Item_Type = it.Item_Type_Code JOIN [TechAssets].[dbo].[Hardware_DeviceStatus] ds ON d.Current_Status = ds.StatusID LEFT JOIN [EMMSDB].[dbo].[Staff_Division] pd ON d.Purchased_DivisionID = pd.div_index WHERE d.Current_Status = '7'";
            InMod.dt12 = HardwareModule.Get_Any_DT(sql);

            return View(InMod);

        }

        public ActionResult ApprovedDispose(EnterItemsModel InMod)
        {
            //string sql = "SELECT  it.Item_Type, h.IT_No, d.Purchased_Division, d.Current_Location, CONVERT(VARCHAR, h.ReqDisposeDate, 23) AS ReqDisposeDate, CONVERT(VARCHAR, h.sysRecDate, 23) AS sysRecDate, CONVERT(VARCHAR, h.sysAppDate, 23) AS sysAppDate, CONVERT(VARCHAR, h.ReqDisposeDate, 23) AS sysRecDate FROM [TechAssets].[dbo].[Hardware_Dispose] h JOIN [TechAssets].[dbo].[Hardware_Devices] d ON h.IT_No = d.IT_No JOIN [TechAssets].[dbo].[Hardware_Inovice_Items] i ON d.DescriptionCode = i.DescriptionCode JOIN [TechAssets].[dbo].[Hardware_Item_Types] it ON i.Item_Type = it.Item_Type_Code WHERE d.Current_Status = '8'";
            string sql = "SELECT it.Item_Type, h.IT_No, d.Purchased_Division, d.Current_Location, CONVERT(VARCHAR, h.ReqDisposeDate, 23) AS ReqDisposeDate, CONVERT(VARCHAR, h.sysRecDate, 23) AS sysRecDate, CONVERT(VARCHAR, h.sysAppDate, 23) AS sysAppDate, CONVERT(VARCHAR, h.ReqDisposeDate, 23) AS sysRecDate, ds.Status AS Current_Status FROM [TechAssets].[dbo].[Hardware_Dispose] h JOIN [TechAssets].[dbo].[Hardware_Devices] d ON h.IT_No = d.IT_No JOIN [TechAssets].[dbo].[Hardware_Inovice_Items] i ON d.DescriptionCode = i.DescriptionCode JOIN [TechAssets].[dbo].[Hardware_Item_Types] it ON i.Item_Type = it.Item_Type_Code JOIN [TechAssets].[dbo].[Hardware_DeviceStatus] ds ON d.Current_Status = ds.StatusID WHERE d.Current_Status = '8'";
            InMod.dt13 = HardwareModule.Get_Any_DT(sql);
            return View(InMod);
        }

        //public string DisposeYear()
        //{
        //    int currentYear = DateTime.Now.Year;
        //    int yearLastTwo = currentYear % 100;

        //    string disYear = 'D' + yearLastTwo.ToString() + '-';


        //    return disYear;
        //}
        public ActionResult StoringDispose()
        {
            string sql = "SELECT d.[Invoice_Reference], d.[IT_No], d.[Serial_No], d.[INV_No], pd.[division] AS Purchased_Division, ds.[Status], cl.[division] AS Current_Location, it.[Item_Type], CONVERT(VARCHAR, hd.RLetterDate, 23) AS RLetterDate, hd.DisposeID , hd.[ITDisposeNo], it.[ShortCode], hd.[DisposeReason], d.[QR] FROM [TechAssets].[dbo].[Hardware_Devices] d INNER JOIN [TechAssets].[dbo].[Hardware_Inovice_Items] i ON d.[DescriptionCode] = i.[DescriptionCode] INNER JOIN [TechAssets].[dbo].[Hardware_Item_Types] it ON i.[Item_Type] = it.[Item_Type_Code] INNER JOIN [TechAssets].[dbo].[Hardware_Dispose] hd ON d.[IT_No] = hd.[IT_No] INNER JOIN [TechAssets].[dbo].[Hardware_DeviceStatus] ds ON d.[Current_Status] = ds.[StatusID] LEFT JOIN [EMMSDB].[dbo].[Staff_Division] pd ON d.[Purchased_DivisionID] = pd.[div_index] LEFT JOIN [EMMSDB].[dbo].[Staff_Division] cl ON d.[Current_LocationID] = cl.[div_index] WHERE d.[Current_Status] = '8' AND d.[activeStatus] = 1";
            EnterItemsModel mod = new EnterItemsModel
            {
                DApproval = HardwareModule.Get_Any_DT(sql),
                DisposeBulkNo = HardwareModule.GetDisposeBulkNo()
            };

            return View(mod);
        }


        public JsonResult GetDisposeBulk()
        {
            List<string> orBulkNos = new List<string>();

            try
            {
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
                {
                    string query = "SELECT [DisposeBulkID] FROM [TechAssets].[dbo].[Hardware_DisposeBulk] WHERE [DBulkStatus] = 1";

                    using (SqlCommand command = new SqlCommand(query, conn))
                    {
                        conn.Open();

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                orBulkNos.Add(reader["DisposeBulkID"].ToString());
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }

            return Json(orBulkNos, JsonRequestBehavior.AllowGet);
        }



        [HttpPost]
        public ActionResult AddDisposeSticker(string BulkNo1, string ITDisposeNo, string itno, string dpno, string scode, string newITdisposeNo)
        {
            try
            {
                newITdisposeNo = BulkNo1 + "-" + scode + ITDisposeNo;

                string query1 = "UPDATE[TechAssets].[dbo].[Hardware_Dispose] SET BulkNo = @BulkNo, ITDisposeNo = @ITDisposeNo, DisposeStatus = '9' WHERE DisposeID = @DisposeID";
                string query = "UPDATE[TechAssets].[dbo].[Hardware_Devices] SET Current_Status = '9' , DisposeBulkNo = @DisposeBulkNo WHERE IT_No = @IT_No";


                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
                {
                    conn.Open();

                    using (SqlCommand command1 = new SqlCommand(query1, conn))
                    {
                        _ = command1.Parameters.AddWithValue("@BulkNo", BulkNo1);
                        _ = command1.Parameters.AddWithValue("@ITDisposeNo", newITdisposeNo);
                        _ = command1.Parameters.AddWithValue("@DisposeID", dpno);
                        _ = command1.ExecuteNonQuery();
                    }

                    using (SqlCommand command = new SqlCommand(query, conn))
                    {
                        _ = command.Parameters.AddWithValue("@IT_No", itno);
                        _ = command.Parameters.AddWithValue("@DisposeBulkNo", BulkNo1);
                        _ = command.ExecuteNonQuery();
                    }

                    conn.Close();
                }

                TempData["SuccessMessage"] = "Sticker added successfully.";
                return RedirectToAction("StoringDispose");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error: " + ex.Message;
                return RedirectToAction("StoringDispose");
            }
        }






        public ActionResult StoringDisposeBulk(EnterItemsModel mod, string NewBulkID)
        {
            try
            {
                if (string.IsNullOrEmpty(mod.DisposeBulkNo) || string.IsNullOrEmpty(mod.DisposeBulkL))
                {
                    return Json(new { success = false, error = "DisposeBulkNo or DisposeBulkL is missing." });
                }

                NewBulkID = mod.DisposeBulkNo + mod.DisposeBulkL;

                string sql = @"INSERT INTO [TechAssets].[dbo].[Hardware_DisposeBulk] ([DisposeBulkID],[DBulkStatus],[CreateDate]) 
                       VALUES (@DisposeBulkID, @DBulkStatus, @CreateDate)";

                using (SqlConnection conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
                {
                    conn_uat.Open();
                    using (SqlCommand sqlCmd3 = new SqlCommand(sql, conn_uat))
                    {
                        _ = sqlCmd3.Parameters.AddWithValue("@DisposeBulkID", NewBulkID);
                        _ = sqlCmd3.Parameters.AddWithValue("@DBulkStatus", 1);
                        _ = sqlCmd3.Parameters.AddWithValue("@CreateDate", DateTime.Now);
                        _ = sqlCmd3.ExecuteNonQuery();
                    }
                }

                return Json(new { success = true, message = "Dispose Bulk generated successfully!" });
            }
            catch (Exception ex)
            {
                // Return error details
                return Json(new { success = false, error = ex.Message });
            }
        }








        public ActionResult UserLogin()
        {
            return View();
        }

        public ActionResult AddViewUser()
        {
            return View();
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult UserRoleAssign()
        {
            return View();
        }


        public ActionResult DisposeFilterView(string status)
        {
            string sql;
            if (status == null)
            {
                status = "2";
                sql = "SELECT d.[Invoice_Reference], d.[IT_No], d.[Serial_No], d.[INV_No], pd.[division] AS Purchased_Division, cl.[division] AS Current_Location, d.[Current_Status], it.[Item_Type], CONVERT(VARCHAR, hd.RLetterDate, 23) AS RLetterDate FROM [TechAssets].[dbo].[Hardware_Devices] d INNER JOIN [TechAssets].[dbo].[Hardware_Inovice_Items] i ON d.[DescriptionCode] = i.[DescriptionCode] INNER JOIN [TechAssets].[dbo].[Hardware_Item_Types] it ON i.[Item_Type] = it.[Item_Type_Code] INNER JOIN [TechAssets].[dbo].[Hardware_Dispose] hd ON d.[IT_No] = hd.[IT_No] LEFT JOIN [EMMSDB].[dbo].[Staff_Division] pd ON d.[Purchased_DivisionID] = pd.[div_index] LEFT JOIN [EMMSDB].[dbo].[Staff_Division] cl ON d.[Current_LocationID] = cl.[div_index] WHERE d.[Current_Status] = '" + status + "' AND d.[activeStatus] = 1";

            }
            else
            {
                sql = status == "All"
                    ? "SELECT d.[Invoice_Reference], d.[IT_No], d.[Serial_No], d.[INV_No], pd.[division] AS Purchased_Division, d.[Current_Status], it.[Item_Type], hd.[RLetterDate] FROM [TechAssets].[dbo].[Hardware_Devices] d INNER JOIN [TechAssets].[dbo].[Hardware_Inovice_Items] i ON d.[DescriptionCode] = i.[DescriptionCode] INNER JOIN [TechAssets].[dbo].[Hardware_Item_Types] it ON i.[Item_Type] = it.[Item_Type_Code] INNER JOIN [TechAssets].[dbo].[Hardware_Dispose] hd ON d.[IT_No] = hd.[IT_No] INNER JOIN [TechAssets].[dbo].[Hardware_DeviceStatus] ds ON d.[Current_Status] = ds.[StatusID] LEFT JOIN [EMMSDB].[dbo].[Staff_Division] pd ON d.[Purchased_DivisionID] = pd.[div_index] WHERE d.[activeStatus] = 1"
                    : "SELECT d.[Invoice_Reference], d.[IT_No], d.[Serial_No], d.[INV_No], pd.[division] AS Purchased_Division, ds.[Status], cl.[division] AS Current_Location, it.[Item_Type], CONVERT(VARCHAR, hd.RLetterDate, 23) AS RLetterDate, hd.DisposeID FROM [TechAssets].[dbo].[Hardware_Devices] d INNER JOIN [TechAssets].[dbo].[Hardware_Inovice_Items] i ON d.[DescriptionCode] = i.[DescriptionCode] INNER JOIN [TechAssets].[dbo].[Hardware_Item_Types] it ON i.[Item_Type] = it.[Item_Type_Code] INNER JOIN [TechAssets].[dbo].[Hardware_Dispose] hd ON d.[IT_No] = hd.[IT_No] INNER JOIN [TechAssets].[dbo].[Hardware_DeviceStatus] ds ON d.[Current_Status] = ds.[StatusID] LEFT JOIN [EMMSDB].[dbo].[Staff_Division] pd ON d.[Purchased_DivisionID] = pd.[div_index] LEFT JOIN [EMMSDB].[dbo].[Staff_Division] cl ON d.[Current_LocationID] = cl.[div_index] WHERE d.[Current_Status] = '" + status + "' AND d.[activeStatus] = 1";
            }

            EnterItemsModel Inmod = new EnterItemsModel
            {
                DLetterdt = HardwareModule.Get_Any_DT(sql)
            };

            return View(Inmod);

        }

        public ActionResult DRequestLetterUpdate(EnterItemsModel mod, string dpNumber)
        {
            if (string.IsNullOrEmpty(mod.RLetterRemarks))
            {
                mod.RLetterRemarks = "";
            }

            string sql = "UPDATE [TechAssets].[dbo].[Hardware_Dispose] SET RLetterDate = @RLetterDate, RLetterRemarks = @RLetterRemarks WHERE DisposeID = @DisposeID";

            using (SqlConnection conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
            {
                conn_uat.Open();
                SqlCommand sqlcmd = new SqlCommand(sql, conn_uat);

                _ = sqlcmd.Parameters.AddWithValue("@RLetterDate", mod.RLetterDate);
                _ = sqlcmd.Parameters.AddWithValue("@RLetterRemarks", mod.RLetterRemarks);
                _ = sqlcmd.Parameters.AddWithValue("@DisposeID", dpNumber);

                _ = sqlcmd.ExecuteNonQuery();
                conn_uat.Close();
            }

            return RedirectToAction("StoringDispose", "Hardware");

        }


        public ActionResult ViewDisposeBulk(string status)
        {

            string sql = null;

            if (status == "All")
            {
                sql = "";
            }
            else if (status == status)
            {
                sql = "SELECT HDB.[id], HDB.[DisposeBulkID], HDB.[DBulkStatus], CONVERT(VARCHAR, HDB.[CreateDate], 23) AS [CreateDate], (SELECT COUNT(BulkNo) FROM [TechAssets].[dbo].[Hardware_Dispose] WHERE BulkNo = HDB.DisposeBulkID) AS [NumberOfItems], CONVERT(VARCHAR, HDB.[SysTakenADate], 23) AS [SysTakenADate],[InformDate],[InformRemaks],[DisposeCCompany],[DisposeCRemaks],[CommonRemaks] FROM [TechAssets].[dbo].[Hardware_DisposeBulk] HDB WHERE DBulkStatus = '" + status + "'";
            }
            EnterItemsModel mod = new EnterItemsModel
            {
                DisposeBulkTBL = HardwareModule.Get_Any_DT(sql),
                DisStatus = status
            };
            ViewBag.DisCurrentStatus = status;
            return View(mod);
        }

        public ActionResult FullViewDisposeItems(string DispodeBulk)
        {
            string sql = "SELECT it.[Item_Type] , d.[IT_No], d.[Serial_No], d.[INV_No], d.[QR] , cl.[division] AS Current_Location, hd.[DisposeReason], hd.[BulkNo], hd.[ITDisposeNo], it.ShortCode, hd.DisposeID FROM [TechAssets].[dbo].[Hardware_Devices] d INNER JOIN [TechAssets].[dbo].[Hardware_Inovice_Items] i ON d.[DescriptionCode] = i.[DescriptionCode] INNER JOIN [TechAssets].[dbo].[Hardware_Item_Types] it ON i.[Item_Type] = it.[Item_Type_Code] INNER JOIN [TechAssets].[dbo].[Hardware_Dispose] hd ON d.[IT_No] = hd.[IT_No] INNER JOIN [TechAssets].[dbo].[Hardware_DeviceStatus] ds ON d.[Current_Status] = ds.[StatusID] LEFT JOIN [EMMSDB].[dbo].[Staff_Division] pd ON d.[Purchased_DivisionID] = pd.[div_index] LEFT JOIN [EMMSDB].[dbo].[Staff_Division] cl ON d.[Current_LocationID] = cl.[div_index] WHERE hd.[BulkNo]= '" + DispodeBulk + "'";

            EnterItemsModel mod = new EnterItemsModel
            {
                DispodeBulk = DispodeBulk,
                DisposeMoreD = HardwareModule.Get_Any_DT(sql)
            };

            return View(mod);
        }



        [HttpPost]
        public JsonResult UpdateDdetails(EnterItemsModel model)
        {
            if (model == null)
            {
                return Json(new { success = false, message = "Invalid data received." });
            }

            try
            {
                string sql = "UPDATE [TechAssets].[dbo].[Hardware_DisposeBulk] SET [InformDate]=@InformDate, [InformRemaks]=@InformRemaks, [SysTakenADate]=@SysTakenADate, [DisposeCCompany]=@DisposeCCompany, [DisposeCRemaks]=@DisposeCRemaks, [CommonRemaks]=@CommonRemaks, [SysDetailUpdateUser]=@SysDetailUpdateUser, SysDetailUpdateDate=@SysDetailUpdateDate WHERE DisposeBulkID = @DisposeBulkID";

                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
                {
                    conn.Open();

                    using (SqlCommand cmdDDetail = new SqlCommand(sql, conn))
                    {
                        _ = cmdDDetail.Parameters.AddWithValue("@DisposeBulkID", model.Dbid ?? (object)DBNull.Value);
                        _ = cmdDDetail.Parameters.AddWithValue("@InformDate", model.Dinfdate ?? (object)DBNull.Value);
                        _ = cmdDDetail.Parameters.AddWithValue("@InformRemaks", model.Dinfremaks ?? (object)DBNull.Value);
                        _ = cmdDDetail.Parameters.AddWithValue("@SysTakenADate", model.Dtakendate ?? (object)DBNull.Value);
                        _ = cmdDDetail.Parameters.AddWithValue("@DisposeCCompany", model.Dtakencom ?? (object)DBNull.Value);
                        _ = cmdDDetail.Parameters.AddWithValue("@DisposeCRemaks", model.Dtakeremaks ?? (object)DBNull.Value);
                        _ = cmdDDetail.Parameters.AddWithValue("@CommonRemaks", model.Dcomremaks ?? (object)DBNull.Value);
                        _ = cmdDDetail.Parameters.AddWithValue("@SysDetailUpdateUser", Constants.sysUser);
                        _ = cmdDDetail.Parameters.AddWithValue("@SysDetailUpdateDate", DateTime.Now);

                        _ = cmdDDetail.ExecuteNonQuery();
                    }
                }

                return Json(new { success = true, message = "Updated successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }


        public ActionResult PendingCourierApprove(EnterItemsModel mod)
        {
            string sql = "SELECT hc.HoID, it.Item_Type, h.IT_No, CONVERT(VARCHAR, HC.CReqDate, 23) AS CourierReqDate, div.division, HC.CReqRemarks, HC.CsysReqUser, DS.Status AS CourierStatus,hc.id,hc.DutyID,hc.THoId FROM [TechAssets].[dbo].[Hardware_Devices] h JOIN [TechAssets].[dbo].[Hardware_Inovice_Items] i ON h.DescriptionCode = i.DescriptionCode JOIN [TechAssets].[dbo].[Hardware_Item_Types] it ON i.item_type = it.Item_Type_Code JOIN [TechAssets].[dbo].[Hardware_Courier] HC ON h.IT_No = HC.IT_No JOIN [EMMSDB].[dbo].[Staff_Division] div ON HC.CReqLocationID = div.div_index INNER JOIN [TechAssets].[dbo].[Hardware_DeviceStatus] DS ON HC.CourierStatus = DS.StatusID WHERE HC.CourierStatus = 11";
            mod.CRecommend = HardwareModule.Get_Any_DT(sql);

            return View(mod);
        }


        [HttpPost]
        public JsonResult ApprovedCourier(string iD, string itNo, string dutyId, string thoId)
        {
            try
            {

                string sql2 = "UPDATE [TechAssets].[dbo].[Hardware_Courier] SET CourierStatus = 12,  CsysAppUser = '" + Constants.sysUser + "', CsysAppDate = @CsysAppDate WHERE id = @iD";
                string sql1 = "UPDATE [TechAssets].[dbo].[Hardware_Devices] SET Current_Status = 12 WHERE IT_No = @IT_No";


                using (SqlConnection conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
                {
                    conn_uat.Open();

                    using (SqlCommand sqlcmd = new SqlCommand(sql1, conn_uat))
                    {
                        _ = sqlcmd.Parameters.AddWithValue("@IT_No", itNo);
                        _ = sqlcmd.ExecuteNonQuery();
                    }

                    using (SqlCommand sqlcmd1 = new SqlCommand(sql2, conn_uat))
                    {
                        _ = sqlcmd1.Parameters.AddWithValue("@iD", iD);
                        _ = sqlcmd1.Parameters.AddWithValue("@CsysAppDate", DateTime.Now);
                        _ = sqlcmd1.ExecuteNonQuery();
                    }
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                // Handle exception (log it, etc.)
                return Json(new { success = false, message = ex.Message });
            }
        }



        [HttpPost]
        public ActionResult RejectCourier(string IT_No, string CAppReReson, string CId, string hoId, string thoId, string dutyId)
        {
            string device;
            string courier;
            string handover;
            string thandover;
            string duty;


            courier = "UPDATE [TechAssets].[dbo].[Hardware_Courier] SET CourierStatus = 15, CsysAppUser = @CsysAppUser, CsysAppDate = @CsysAppDate, CAppReReson = @CAppReReson WHERE id = @cId";
            handover = "UPDATE [TechAssets].[dbo].[Hardware_Handover] SET HOstatus = 4 WHERE id = @hoId";
            thandover = "UPDATE [TechAssets].[dbo].[Hardware_THandover] SET THandoverStatus = @THandoverStatus WHERE id = @thoId";
            duty = "UPDATE [TechAssets].[dbo].[Hardware_DutyAssign] SET DutyStatus = @DutyStatus WHERE DutyID = @DutyID";

            device = "UPDATE [TechAssets].[dbo].[Hardware_Devices] SET Current_Status = @Current_Status WHERE IT_No = @IT_No";


            using (SqlConnection conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
            {
                conn_uat.Open();

                try
                {


                    if (!string.IsNullOrEmpty(hoId)) // Check if hoId is null or empty
                    {
                        using (SqlCommand sqlcmd = new SqlCommand(device, conn_uat))
                        {
                            _ = sqlcmd.Parameters.AddWithValue("@Current_Status", 4);
                            _ = sqlcmd.Parameters.AddWithValue("@IT_No", IT_No);
                            _ = sqlcmd.ExecuteNonQuery();
                        }

                        using (SqlCommand sqlcmd1 = new SqlCommand(courier, conn_uat))
                        {
                            _ = sqlcmd1.Parameters.AddWithValue("@cId", CId);
                            _ = sqlcmd1.Parameters.AddWithValue("@CAppReReson", CAppReReson);
                            _ = sqlcmd1.Parameters.AddWithValue("@CsysAppDate", DateTime.Now);
                            _ = sqlcmd1.Parameters.AddWithValue("@CsysAppUser", Constants.sysUser);
                            _ = sqlcmd1.ExecuteNonQuery();
                        }

                        using (SqlCommand sqlcmd2 = new SqlCommand(handover, conn_uat))
                        {
                            _ = sqlcmd2.Parameters.AddWithValue("@hoId", hoId);
                            _ = sqlcmd2.ExecuteNonQuery();
                        }
                    }

                    else
                    {
                        using (SqlCommand sqlcmd = new SqlCommand(device, conn_uat))
                        {
                            _ = sqlcmd.Parameters.AddWithValue("@Current_Status", 36);
                            _ = sqlcmd.Parameters.AddWithValue("@IT_No", IT_No);
                            _ = sqlcmd.ExecuteNonQuery();
                        }

                        using (SqlCommand sqlcmd1 = new SqlCommand(courier, conn_uat))
                        {
                            _ = sqlcmd1.Parameters.AddWithValue("@cId", CId);
                            _ = sqlcmd1.Parameters.AddWithValue("@CAppReReson", CAppReReson);
                            _ = sqlcmd1.Parameters.AddWithValue("@CsysAppDate", DateTime.Now);
                            _ = sqlcmd1.Parameters.AddWithValue("@CsysAppUser", Constants.sysUser);
                            _ = sqlcmd1.ExecuteNonQuery();
                        }

                        using (SqlCommand sqlcmd3 = new SqlCommand(thandover, conn_uat))
                        {
                            _ = sqlcmd3.Parameters.AddWithValue("@thoId", thoId);
                            _ = sqlcmd3.Parameters.AddWithValue("@THandoverStatus", 35);
                            _ = sqlcmd3.ExecuteNonQuery();
                        }

                        using (SqlCommand sqlcmd4 = new SqlCommand(duty, conn_uat))
                        {
                            _ = sqlcmd4.Parameters.AddWithValue("@DutyID", dutyId);
                            _ = sqlcmd4.Parameters.AddWithValue("@DutyStatus", 36);
                            _ = sqlcmd4.ExecuteNonQuery();
                        }




                    }



                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = ex.Message });
                }
                finally
                {
                    conn_uat.Close();
                }
            }
            return Json(new { success = true });
        }


        public ActionResult SubmitCourier(EnterItemsModel mod, string IT_No, string CReqDate, string hOLocationID, string CReqRemarks, int id)
        {
            string sqlInsert = "INSERT INTO [TechAssets].[dbo].[Hardware_Courier] (CourierStatus, IT_No, HoID, CReqDate, CReqLocationID, CReqRemarks, CsysReqUser, CsysReqDate) " +
                               "VALUES (@CourierStatus, @IT_No, @HoID, @CReqDate, @CReqLocationID, @CReqRemarks, @CsysReqUser, @CsysReqDate)";

            string sqlUpdate1 = "UPDATE [TechAssets].[dbo].[Hardware_Handover] SET HOstatus = 11 WHERE id = @id";

            string sqlUpdate = "UPDATE [TechAssets].[dbo].[Hardware_Devices] SET Current_Status = 11 WHERE IT_No = @IT_No";



            using (SqlConnection conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
            {
                conn_uat.Open();


                using (SqlCommand insertCmd = new SqlCommand(sqlInsert, conn_uat))
                {
                    _ = insertCmd.Parameters.AddWithValue("@CourierStatus", 11);
                    _ = insertCmd.Parameters.AddWithValue("@IT_No", IT_No);
                    _ = insertCmd.Parameters.AddWithValue("@HoID", id);
                    _ = insertCmd.Parameters.AddWithValue("@CReqDate", CReqDate);
                    _ = insertCmd.Parameters.AddWithValue("@CReqLocationID", hOLocationID);
                    _ = insertCmd.Parameters.AddWithValue("@CReqRemarks", CReqRemarks);
                    _ = insertCmd.Parameters.AddWithValue("@CsysReqUser", Constants.sysUser);
                    _ = insertCmd.Parameters.AddWithValue("@CsysReqDate", DateTime.Now);
                    _ = insertCmd.ExecuteNonQuery();
                }


                using (SqlCommand updateCmd = new SqlCommand(sqlUpdate, conn_uat))
                {
                    _ = updateCmd.Parameters.AddWithValue("@IT_No", IT_No);
                    _ = updateCmd.ExecuteNonQuery();
                }


                using (SqlCommand updateCmd = new SqlCommand(sqlUpdate1, conn_uat))
                {
                    _ = updateCmd.Parameters.AddWithValue("@id", id);
                    _ = updateCmd.ExecuteNonQuery();
                }
            }

            return RedirectToAction("FindDevices", "Hardware", new { status = "2" });
        }





        [HttpPost]
        public JsonResult AORejectDispose(string IT_No, string RecRemarks)
        {
            try
            {
                string sql1 = "UPDATE [TechAssets].[dbo].[Hardware_Devices] SET Current_Status = '8' WHERE IT_No = @IT_No";
                string sql2 = "UPDATE [TechAssets].[dbo].[Hardware_Dispose] SET DisposeStatus = '16',  sysRecUser = '" + Constants.sysUser + "', sysRecDate = GETDATE(), RecRemarks = @RecRemarks WHERE IT_No = @IT_No";

                using (SqlConnection conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
                {
                    conn_uat.Open();

                    using (SqlCommand sqlcmd = new SqlCommand(sql1, conn_uat))
                    {
                        _ = sqlcmd.Parameters.AddWithValue("@IT_No", IT_No);
                        _ = sqlcmd.ExecuteNonQuery();
                    }

                    using (SqlCommand sqlcmd1 = new SqlCommand(sql2, conn_uat))
                    {
                        _ = sqlcmd1.Parameters.AddWithValue("@IT_No", IT_No);
                        _ = sqlcmd1.Parameters.AddWithValue("@sysRecDate", DateTime.Now);
                        _ = sqlcmd1.Parameters.AddWithValue("@RecRemarks", RecRemarks);
                        _ = sqlcmd1.ExecuteNonQuery();
                    }
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                // Handle exception (log it, etc.)
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult MGRRejectDispose(string IT_No, string AppRemarks)
        {
            try
            {
                string sql1 = "UPDATE [TechAssets].[dbo].[Hardware_Devices] SET Current_Status = '8' WHERE IT_No = @IT_No";
                string sql2 = "UPDATE [TechAssets].[dbo].[Hardware_Dispose] SET DisposeStatus = '16',  sysAppUser = '" + Constants.sysUser + "', sysAppDate = GETDATE(), AppRemarks = @AppRemarks WHERE IT_No = @IT_No";

                using (SqlConnection conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
                {
                    conn_uat.Open();

                    using (SqlCommand sqlcmd = new SqlCommand(sql1, conn_uat))
                    {
                        _ = sqlcmd.Parameters.AddWithValue("@IT_No", IT_No);
                        _ = sqlcmd.ExecuteNonQuery();
                    }

                    using (SqlCommand sqlcmd1 = new SqlCommand(sql2, conn_uat))
                    {
                        _ = sqlcmd1.Parameters.AddWithValue("@IT_No", IT_No);
                        _ = sqlcmd1.Parameters.AddWithValue("@SysApp_Date", DateTime.Now);
                        _ = sqlcmd1.Parameters.AddWithValue("@AppRemarks", AppRemarks);
                        _ = sqlcmd1.ExecuteNonQuery();
                    }
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                // Handle exception (log it, etc.)
                return Json(new { success = false, message = ex.Message });
            }
        }

        public ActionResult CourierFilterView(string status)
        {
            string sql = null;


            if (status == null)
            {
                sql = "";
            }

            else if (status == "All")
            {
                sql = "SELECT it.Item_Type, d.[IT_No], d.[Serial_No], d.[INV_No], d.[QR], SD1.division AS Purchased_Division, SD.division AS Courier_Division, ds.Status AS Current_Status, ds.StatusID, Co.HoID, Co.id ,Co.DutyID, Co.THoId ,Co.CReqLocationID FROM [TechAssets].[dbo].[Hardware_Devices] d JOIN [TechAssets].[dbo].[Hardware_Inovice_Items] i ON d.[DescriptionCode] = i.[DescriptionCode] INNER JOIN [TechAssets].[dbo].[Hardware_Item_Types] it ON i.Item_Type = it.Item_Type_Code INNER JOIN [TechAssets].[dbo].[Hardware_DeviceStatus] ds ON d.[Current_Status] = ds.[StatusID] JOIN [TechAssets].[dbo].[Hardware_Courier] Co ON d.IT_No = Co.IT_No JOIN [EMMSDB].[dbo].[Staff_Division] SD1 ON d.Purchased_DivisionID = SD1.div_index JOIN [EMMSDB].[dbo].[Staff_Division] SD ON Co.CReqLocationID = SD.div_index WHERE d.[activeStatus] = 1 AND d.[Current_Status] IN (CAST(11 AS INT), CAST(12 AS INT), CAST(13 AS INT))";
            }

            else if (status == status)
            {
                //sql = "SELECT it.Item_Type, d.[IT_No], d.[Serial_No], d.[INV_No], d.[QR], SD1.division AS Purchased_Division, SD.division AS Courier_Division, ds.Status AS Current_Status, ds.StatusID, Co.HoID, Co.id ,Co.DutyID, Co.THoId ,Co.CReqLocationID FROM [TechAssets].[dbo].[Hardware_Devices] d JOIN [TechAssets].[dbo].[Hardware_Inovice_Items] i ON d.[DescriptionCode] = i.[DescriptionCode] INNER JOIN [TechAssets].[dbo].[Hardware_Item_Types] it ON i.Item_Type = it.Item_Type_Code INNER JOIN [TechAssets].[dbo].[Hardware_DeviceStatus] ds ON d.[Current_Status] = ds.[StatusID] JOIN [TechAssets].[dbo].[Hardware_Courier] Co ON d.IT_No = Co.IT_No JOIN [EMMSDB].[dbo].[Staff_Division] SD1 ON d.Purchased_DivisionID = SD1.div_index JOIN [EMMSDB].[dbo].[Staff_Division] SD ON Co.CReqLocationID = SD.div_index WHERE d.[activeStatus] = 1 AND d.[Current_Status] = '" + status + "'";
                sql = "SELECT it.Item_Type, d.[IT_No], d.[Serial_No], d.[INV_No], d.[QR], SD1.division AS Purchased_Division, SD.division AS Courier_Division, ds.Status AS Current_Status, ds.StatusID, Co.HoID, Co.id, Co.DutyID, Co.THoId, Co.CReqLocationID FROM [TechAssets].[dbo].[Hardware_Devices] d JOIN [TechAssets].[dbo].[Hardware_Inovice_Items] i ON d.[DescriptionCode] = i.[DescriptionCode] INNER JOIN [TechAssets].[dbo].[Hardware_Item_Types] it ON i.Item_Type = it.Item_Type_Code INNER JOIN [TechAssets].[dbo].[Hardware_DeviceStatus] ds ON d.[Current_Status] = ds.[StatusID] JOIN [TechAssets].[dbo].[Hardware_Courier] Co ON d.IT_No = Co.IT_No JOIN [EMMSDB].[dbo].[Staff_Division] SD1 ON d.Purchased_DivisionID = SD1.div_index JOIN [EMMSDB].[dbo].[Staff_Division] SD ON Co.CReqLocationID = SD.div_index WHERE d.[activeStatus] = 1 AND d.[Current_Status] = '" + status + "' AND Co.[CourierStatus] = '" + status + "'";
            }


            EnterItemsModel Inmod = new EnterItemsModel
            {
                CCategory = HardwareModule.GetCourierCompaniesSelectList(),
                CFilter = HardwareModule.Get_Any_DT(sql),
                CStatus = status
            };
            ViewBag.CurrentStatus = status;
            return View(Inmod);
        }



        public JsonResult GetCourierData(string itNo)
        {
            var result = new
            {
                CourierLocation = "",
                ItemType = "",
                IT_No = "",
                INV_No = "",
                Serial_No = "",
                QR = "",
                HOLocationID = "",
                Current_Status = "",
                id = ""
            };

            string query = "SELECT it.Item_Type, h.IT_No, d.INV_No, d.Serial_No, d.QR, div.division AS CourierLocation, h.HOLocationID, h.id, d.Current_Status FROM [TechAssets].[dbo].[Hardware_Handover] h JOIN [TechAssets].[dbo].[Hardware_Devices] d ON h.IT_No = d.IT_No JOIN [TechAssets].[dbo].[Hardware_Inovice_Items] i ON d.DescriptionCode = i.DescriptionCode JOIN [TechAssets].[dbo].[Hardware_Item_Types] it ON i.item_type = it.Item_Type_Code JOIN [EMMSDB].[dbo].[Staff_Division] div ON h.HOLocationID = div.div_index WHERE HOstatus = '4' AND h.IT_No = @IT_No";

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                _ = cmd.Parameters.AddWithValue("@IT_No", itNo);
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        result = new
                        {
                            CourierLocation = reader["CourierLocation"].ToString(),
                            ItemType = reader["Item_Type"].ToString(),
                            IT_No = reader["IT_No"].ToString(),
                            INV_No = reader["INV_No"].ToString(),
                            Serial_No = reader["Serial_No"].ToString(),
                            QR = reader["QR"].ToString(),
                            HOLocationID = reader["HOLocationID"].ToString(),
                            Current_Status = reader["Current_Status"].ToString(),
                            id = reader["id"].ToString()

                        };
                    }
                }
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }


        public ActionResult ShippingCourier(EnterItemsModel mod, string IT_No, string id)
        {
            string sql1 = "UPDATE [TechAssets].[dbo].[Hardware_Courier] SET CourierStatus = '17', CompanyName = @CompanyName, StickerDate = @StickerDate, StickerNo = @StickerNo, StickerRemaks = @StickerRemaks WHERE id = @id";
            string sql = "UPDATE [TechAssets].[dbo].[Hardware_Devices] SET Current_Status='17' WHERE IT_No = @IT_No";


            using (SqlConnection conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
            {
                conn_uat.Open();

                SqlCommand sqlcmd = new SqlCommand(sql, conn_uat);
                _ = sqlcmd.Parameters.AddWithValue("@IT_No", IT_No);
                _ = sqlcmd.ExecuteNonQuery();

                SqlCommand sqlcmd1 = new SqlCommand(sql1, conn_uat);
                _ = sqlcmd1.Parameters.AddWithValue("@IT_No", IT_No);
                _ = sqlcmd1.Parameters.AddWithValue("@StickerDate", mod.StickerDate ?? (object)DBNull.Value);
                _ = sqlcmd1.Parameters.AddWithValue("@StickerNo", mod.StickerNo ?? (object)DBNull.Value);
                _ = sqlcmd1.Parameters.AddWithValue("@StickerRemaks", mod.CRemarks2 ?? (object)DBNull.Value);
                _ = sqlcmd1.Parameters.AddWithValue("@CompanyName", mod.SelectedCompany ?? (object)DBNull.Value);
                _ = sqlcmd1.Parameters.AddWithValue("@id", id);
                _ = sqlcmd1.ExecuteNonQuery();

                conn_uat.Close();
            }

            return RedirectToAction("CourierFilterView", "Hardware", new { status = "12" });
        }

        public ActionResult PickupCourier(EnterItemsModel mod, string IT_No, int id, string clocationid, string thoIdPi, string hoIdPi)
        {
            string sql1 = "UPDATE [TechAssets].[dbo].[Hardware_Courier] SET CourierStatus = @CourierStatus, PickupDate = @PickupDate, PickupRemaks = @PickupRemaks WHERE id = @id";
            string sql = "UPDATE [TechAssets].[dbo].[Hardware_Devices] SET Current_Status=@Current_Status, CoID=@CoID, HoIDst=@HoIDst, HoIDth=@HoIDth, Current_LocationID=@Current_LocationID WHERE IT_No = @IT_No";
            string sql2 = "UPDATE [TechAssets].[dbo].[Hardware_THandover] SET THandoverStatus=@THandoverStatus WHERE id = @id";
            string sql3 = "UPDATE [TechAssets].[dbo].[Hardware_Handover] SET HOstatus=@HOstatus WHERE id = @id";



            using (SqlConnection conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
            {
                conn_uat.Open();
                SqlCommand sqlcmd1 = new SqlCommand(sql1, conn_uat);
                _ = sqlcmd1.Parameters.AddWithValue("@IT_No", IT_No);
                _ = sqlcmd1.Parameters.AddWithValue("@CourierStatus", 18);
                _ = sqlcmd1.Parameters.AddWithValue("@PickupDate", mod.PickupDate ?? (object)DBNull.Value);
                _ = sqlcmd1.Parameters.AddWithValue("@PickupRemaks", mod.AdditionalInfo1 ?? (object)DBNull.Value);
                _ = sqlcmd1.Parameters.AddWithValue("@id", id);
                _ = sqlcmd1.ExecuteNonQuery();


                SqlCommand sqlcmd = new SqlCommand(sql, conn_uat);
                _ = sqlcmd.Parameters.AddWithValue("@IT_No", IT_No);
                _ = sqlcmd.Parameters.AddWithValue("@Current_LocationID", clocationid);
                _ = sqlcmd.Parameters.AddWithValue("@HoIDst", string.IsNullOrEmpty(hoIdPi) ? (object)DBNull.Value : hoIdPi);
                _ = sqlcmd.Parameters.AddWithValue("@HoIDth", string.IsNullOrEmpty(thoIdPi) ? (object)DBNull.Value : thoIdPi);
                _ = sqlcmd.Parameters.AddWithValue("@Current_Status", 18);
                _ = sqlcmd.Parameters.AddWithValue("@CoID", id);
                _ = sqlcmd.ExecuteNonQuery();

                if (!string.IsNullOrEmpty(thoIdPi))
                {
                    SqlCommand sqlcmd2 = new SqlCommand(sql2, conn_uat);
                    _ = sqlcmd2.Parameters.AddWithValue("@id", thoIdPi);
                    _ = sqlcmd2.Parameters.AddWithValue("@THandoverStatus", 18);
                    _ = sqlcmd2.ExecuteNonQuery();
                }
                else {
                    SqlCommand sqlcmd2 = new SqlCommand(sql3, conn_uat);
                    _ = sqlcmd2.Parameters.AddWithValue("@id", hoIdPi);
                    _ = sqlcmd2.Parameters.AddWithValue("@HOstatus", 18);
                    _ = sqlcmd2.ExecuteNonQuery();

                }


                conn_uat.Close();
            }

            return RedirectToAction("CourierFilterView", "Hardware", new { status = "17" });
        }



        public ActionResult CompleteHandover(EnterItemsModel mod, string IT_No, string DCurrentLocation, string DAuthorized, string HOLid, string hNumberRe)
        {

            string sql1 = "UPDATE [TechAssets].[dbo].[Hardware_Handover] SET Current_Status = '1', HOstatus = '19', HOComDate = @HOComDate, HOComRemarks = @HOComRemarks, sysCom_User = '" + Constants.sysUser + "', sysCom_Date = @SysApp_Date, HOReceiver=@EmployeeName  WHERE IT_No = @IT_No";
            string sql = "UPDATE [TechAssets].[dbo].[Hardware_Devices] SET Current_Status = '1', Installation_Date = @HOComDate, Authorized_Officer = @DAuthorized, Current_LocationID = @HOLid WHERE IT_No = @IT_No";

            using (SqlConnection conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
            {
                conn_uat.Open();

                using (SqlCommand sqlcmd1 = new SqlCommand(sql1, conn_uat))
                {
                    _ = sqlcmd1.Parameters.AddWithValue("@IT_No", IT_No);
                    _ = sqlcmd1.Parameters.AddWithValue("@HOComDate", mod.HOComDate ?? (object)DBNull.Value);
                    _ = sqlcmd1.Parameters.AddWithValue("@HOComRemarks", mod.HOComRemarks ?? (object)DBNull.Value);
                    _ = sqlcmd1.Parameters.AddWithValue("@SysApp_Date", DateTime.Now);
                    _ = sqlcmd1.Parameters.AddWithValue("@EmployeeName", mod.EmployeeName);


                    _ = sqlcmd1.ExecuteNonQuery();
                }

                using (SqlCommand sqlcmd = new SqlCommand(sql, conn_uat))
                {
                    _ = sqlcmd.Parameters.AddWithValue("@IT_No", IT_No);
                    _ = sqlcmd.Parameters.AddWithValue("@HOLid", HOLid ?? (object)DBNull.Value);

                    _ = sqlcmd.ExecuteNonQuery();
                }

                conn_uat.Close();
            }

            // Redirect to the HandoverFilterView after completion
            return RedirectToAction("HandoverFilterView", "Hardware", new { status = "4" });
        }



        public ActionResult ReceptionPendingItem(string status)
        {
            EnterItemsModel mod = new EnterItemsModel();

            string sql = null;


            if (status == null)
            {
                sql = "";
            }

            else if (status == "All")
            {
                sql = "SELECT it.[Item_Type], TH.[IT_No], D.[Serial_No], D.[INV_No], D.[QR], div.[division], TH.[DeviceUser], TH.[ContactDetails], TH.[ItemSentDate], TH.[ItemSentType], TH.[ItemSentRemaks], ST.[Status], TH.[id], TH.[THandoverStatus], TH.[THandoverType] FROM [TechAssets].[dbo].[Hardware_THandover] AS TH JOIN [TechAssets].[dbo].[Hardware_DeviceStatus] ST ON ST.StatusID = TH.THandoverType JOIN [EMMSDB].[dbo].[Staff_Division] div ON TH.[THandoverLocation] = div.div_index JOIN [TechAssets].[dbo].[Hardware_Devices] D ON TH.IT_No = D.IT_No JOIN [TechAssets].[dbo].[Hardware_Inovice_Items] i ON D.DescriptionCode = i.DescriptionCode JOIN [TechAssets].[dbo].[Hardware_Item_Types] it ON i.item_type = it.Item_Type_Code WHERE TH.[THandoverStatus] = '33'";
            }
            else if (status == status)
            {
                sql = "SELECT it.[Item_Type], TH.[IT_No], D.[Serial_No], D.[INV_No], D.[QR], div.[division], TH.[DeviceUser], TH.[ContactDetails], TH.[ItemSentDate], TH.[ItemSentType], TH.[ItemSentRemaks], ST.[Status], TH.[id], TH.[THandoverStatus] , TH.[THandoverType] FROM [TechAssets].[dbo].[Hardware_THandover] AS TH JOIN [TechAssets].[dbo].[Hardware_DeviceStatus] ST ON ST.StatusID = TH.THandoverType JOIN [EMMSDB].[dbo].[Staff_Division] div ON TH.[THandoverLocation] = div.div_index JOIN [TechAssets].[dbo].[Hardware_Devices] D ON TH.IT_No = D.IT_No JOIN [TechAssets].[dbo].[Hardware_Inovice_Items] i ON D.DescriptionCode = i.DescriptionCode JOIN [TechAssets].[dbo].[Hardware_Item_Types] it ON i.item_type = it.Item_Type_Code WHERE TH.[THandoverStatus] = '33' AND TH.[THandoverType]='" + status + "'";
            }

            mod.ReceptionPTable = HardwareModule.Get_Any_DT(sql);
            ViewBag.CurrentStatus = status;
            return View(mod);

        }


        [HttpGet]
        public ActionResult GetRepairAssignment()
        {
            List<object> result = new List<object>();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
            {
                conn.Open();


                string query = @"
                    DECLARE @EmpNo INT;

                    SET @EmpNo = COALESCE(
                        (SELECT DutyAssignEmpNo 
                         FROM [TechAssets].[dbo].[Hardware_DutyAssign]
                         WHERE DutyID = (SELECT MAX(DutyID) FROM [TechAssets].[dbo].[Hardware_DutyAssign])
                        ),
                        (SELECT TOP 1 b.EmpNo 
                         FROM [TechAssets].[dbo].[Hardware_UserCredential] AS b 
                         JOIN [EMMSDB].[dbo].[Staff_employee_Details] AS a ON a.Emp_no = b.EmpNo 
                         WHERE b.UserType = 'Technical Officer' AND b.UserStatus = 'Active' 
                         ORDER BY b.EmpNo ASC
                        )
                    );

                    WITH OrderedEmployees AS (
                        SELECT 
                            ROW_NUMBER() OVER (ORDER BY b.EmpNo ASC) AS OrderID,
                            b.EmpNo
                        FROM 
                            [TechAssets].[dbo].[Hardware_UserCredential] AS b
                        JOIN 
                            [EMMSDB].[dbo].[Staff_employee_Details] AS a 
                        ON 
                            a.Emp_no = b.EmpNo
                        WHERE 
                            b.UserType = 'Technical Officer' 
                            AND b.UserStatus = 'Active'
                    )
                    SELECT 
                        oe.EmpNo,
                        sed.Name_with_ini
                    FROM 
                        OrderedEmployees AS oe
                    JOIN 
                        [EMMSDB].[dbo].[Staff_employee_Details] AS sed
                    ON 
                        sed.Emp_No = oe.EmpNo
                    WHERE 
                        oe.OrderID = 
                            CASE 
                                WHEN (SELECT EmpNo FROM OrderedEmployees WHERE EmpNo = @EmpNo) IS NOT NULL 
                                     AND (SELECT COUNT(*) FROM OrderedEmployees) = 
                                         (SELECT COUNT(*) FROM OrderedEmployees WHERE EmpNo <= @EmpNo)
                                THEN 1 
                                ELSE (SELECT OrderID + 1 FROM OrderedEmployees WHERE EmpNo = @EmpNo) 
                            END;";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(new
                            {
                                EmpNo = reader["EmpNo"],
                                NameWithIni = reader["Name_with_ini"]
                            });
                        }
                    }
                }
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetTechnicalOfficer()
        {
            List<object> employees = new List<object>();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
            {
                string query = "SELECT a.Name_with_ini, b.EmpNo FROM [TechAssets].[dbo].[Hardware_UserCredential] AS b JOIN [EMMSDB].[dbo].[Staff_employee_Details] AS a ON a.Emp_no = b.EmpNo WHERE b.UserType = 'Technical Officer' AND b.UserStatus = 'Active' ORDER BY b.EmpNo ASC";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            employees.Add(new
                            {
                                EmpNo = reader["EmpNo"],
                                Name_with_ini = reader["Name_with_ini"]
                            });
                        }
                    }
                }
            }

            return Json(employees, JsonRequestBehavior.AllowGet);
        }


        public JsonResult GetNextTechOfficer(int empNo)
        {
            List<EmployeeInfo> result = new List<EmployeeInfo>();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
            {
                conn.Open();

                string sqlQuery = @"
                WITH OrderedEmployees AS (
                    SELECT 
                        ROW_NUMBER() OVER (ORDER BY b.EmpNo ASC) AS OrderID,
                        b.EmpNo
                    FROM 
                        [TechAssets].[dbo].[Hardware_UserCredential] AS b
                    JOIN 
                        [EMMSDB].[dbo].[Staff_employee_Details] AS a 
                    ON 
                        a.Emp_no = b.EmpNo
                    WHERE 
                        b.UserType = 'Technical Officer' 
                        AND b.UserStatus = 1
                )
                SELECT 
                    oe.EmpNo,
                    sed.Name_with_ini
                FROM 
                    OrderedEmployees AS oe
                JOIN 
                    [EMMSDB].[dbo].[Staff_employee_Details] AS sed
                ON 
                    sed.Emp_No = oe.EmpNo
                WHERE 
                    oe.OrderID = 
                        CASE 
                            WHEN (SELECT EmpNo FROM OrderedEmployees WHERE EmpNo = @EmpNo) IS NOT NULL 
                                 AND (SELECT COUNT(*) FROM OrderedEmployees) = 
                                     (SELECT COUNT(*) FROM OrderedEmployees WHERE EmpNo <= @EmpNo)
                            THEN 1  -- Return the first employee if the last employee (1982) is entered
                            ELSE (SELECT OrderID + 1 FROM OrderedEmployees WHERE EmpNo = @EmpNo)  -- Otherwise return the next employee
                        END;";

                using (SqlCommand cmd = new SqlCommand(sqlQuery, conn))
                {
                    _ = cmd.Parameters.AddWithValue("@EmpNo", empNo);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            EmployeeInfo employee = new EmployeeInfo
                            {
                                EmpNo = reader.GetInt32(0),
                                NameWithIni = reader.GetString(1)
                            };
                            result.Add(employee);
                        }
                    }
                }
            }

            return Json(result, JsonRequestBehavior.AllowGet);


        }

        public class EmployeeInfo
        {
            public int EmpNo { get; set; }
            public string NameWithIni { get; set; }
        }

        [HttpPost]
        public ActionResult THreceivedModal(string itNo, string ItemReciveRemaks, string ItemReciveDate, string hoId, string hoType, string employeeNoTextbox, string employeeNameTextbox)
        {
            try
            {
                string sql2 = "UPDATE [TechAssets].[dbo].[Hardware_THandover] SET THandoverStatus = '35', ItemReciveDate = @ItemReciveDate, ItemReciveRemaks = @ItemReciveRemaks, SysTReciveDate=@SysTReciveDate, SysTReciveUser=@SysTReciveUser WHERE id = @id";
                string sql1 = "UPDATE [TechAssets].[dbo].[Hardware_Devices] SET Current_Status = '35' WHERE IT_No = @IT_No";
                string sql3 = "INSERT INTO [TechAssets].[dbo].[Hardware_DutyAssign] ([DutyStatus], [THandoverID], [THandoverType], [DutyAssignDate], [IT_No],[DutyAssignOfficer], [DutyAssignEmpNo]) VALUES ('35','" + hoId + "','" + hoType + "','" + DateTime.Now + "' , '" + itNo + "','" + employeeNameTextbox + "', '" + employeeNoTextbox + "')";

                using (SqlConnection conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
                {
                    conn_uat.Open();

                    using (SqlCommand sqlcmd = new SqlCommand(sql1, conn_uat))
                    {
                        _ = sqlcmd.Parameters.AddWithValue("@IT_No", itNo);
                        _ = sqlcmd.ExecuteNonQuery();
                    }

                    using (SqlCommand sqlcmd1 = new SqlCommand(sql2, conn_uat))
                    {
                        _ = sqlcmd1.Parameters.AddWithValue("@id", hoId);
                        _ = sqlcmd1.Parameters.AddWithValue("@ItemReciveDate", ItemReciveDate);
                        _ = sqlcmd1.Parameters.AddWithValue("@ItemReciveRemaks", ItemReciveRemaks);
                        _ = sqlcmd1.Parameters.AddWithValue("@SysTReciveDate", DateTime.Now);
                        _ = sqlcmd1.Parameters.AddWithValue("@SysTReciveUser", Constants.sysUser);
                        _ = sqlcmd1.ExecuteNonQuery();
                    }

                    using (SqlCommand sqlcmd3 = new SqlCommand(sql3, conn_uat))
                    {
                        _ = sqlcmd3.ExecuteNonQuery();
                    }
                }
                return Json(new { success = true, message = "Repair Added Successfully" });
            }

            catch (SqlException sqlEx) // Catch specific SQL exceptions
            {
                return Json(new { success = false, message = "SQL Error: " + sqlEx.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }

        }

        public ActionResult SpecialTaskAssign(string status)
        {
            EnterItemsModel mod = new EnterItemsModel();
            string sql = null;

            if (status == null)
            {
                sql = "SELECT it.[Item_Type], DA.[IT_No], D.[INV_No], CONVERT(varchar, DA.[DutyAssignDate], 103) AS DutyAssignDate, div.[division], ST.[Status], DA.[DutyAssignOfficer], DA.[DutyAssignOfficerSP], DA.[THandoverID], DA.[DutyStatus], DA.[THandoverType], DA.[DutyID] FROM [TechAssets].[dbo].[Hardware_DutyAssign] AS DA JOIN [TechAssets].[dbo].[Hardware_DeviceStatus] ST ON ST.StatusID = DA.THandoverType JOIN [TechAssets].[dbo].[Hardware_Devices] D ON DA.IT_No = D.IT_No JOIN [EMMSDB].[dbo].[Staff_Division] div ON div.div_index = D.[Current_LocationID] JOIN [TechAssets].[dbo].[Hardware_Inovice_Items] i ON D.DescriptionCode = i.DescriptionCode JOIN [TechAssets].[dbo].[Hardware_Item_Types] it ON i.item_type = it.Item_Type_Code WHERE DA.[DutyStatus] = '35'";
            }
            else if (status == "All")
            {
                sql = "SELECT it.[Item_Type], DA.[IT_No], D.[INV_No], CONVERT(varchar, DA.[DutyAssignDate], 103) AS DutyAssignDate, div.[division], ST.[Status], DA.[DutyAssignOfficer], DA.[DutyAssignOfficerSP], DA.[THandoverID], DA.[DutyStatus], DA.[THandoverType], DA.[DutyID] FROM [TechAssets].[dbo].[Hardware_DutyAssign] AS DA JOIN [TechAssets].[dbo].[Hardware_DeviceStatus] ST ON ST.StatusID = DA.THandoverType JOIN [TechAssets].[dbo].[Hardware_Devices] D ON DA.IT_No = D.IT_No JOIN [EMMSDB].[dbo].[Staff_Division] div ON div.div_index = D.[Current_LocationID] JOIN [TechAssets].[dbo].[Hardware_Inovice_Items] i ON D.DescriptionCode = i.DescriptionCode JOIN [TechAssets].[dbo].[Hardware_Item_Types] it ON i.item_type = it.Item_Type_Code WHERE DA.[DutyStatus] = '35'";
            }
            else if (status == status)
            {
                sql = "SELECT it.[Item_Type], DA.[IT_No], D.[INV_No], CONVERT(varchar, DA.[DutyAssignDate], 103) AS DutyAssignDate, div.[division], ST.[Status], DA.[DutyAssignOfficer], DA.[DutyAssignOfficerSP], DA.[THandoverID], DA.[DutyStatus], DA.[THandoverType], DA.[DutyID] FROM [TechAssets].[dbo].[Hardware_DutyAssign] AS DA JOIN [TechAssets].[dbo].[Hardware_DeviceStatus] ST ON ST.StatusID = DA.THandoverType JOIN [TechAssets].[dbo].[Hardware_Devices] D ON DA.IT_No = D.IT_No JOIN [EMMSDB].[dbo].[Staff_Division] div ON div.div_index = D.[Current_LocationID] JOIN [TechAssets].[dbo].[Hardware_Inovice_Items] i ON D.DescriptionCode = i.DescriptionCode JOIN [TechAssets].[dbo].[Hardware_Item_Types] it ON i.item_type = it.Item_Type_Code WHERE DA.[DutyStatus] = '35' AND DA.[THandoverType]='" + status + "'";
            }

            mod.DutyAssingTBL = HardwareModule.Get_Any_DT(sql);
            ViewBag.CurrentStatus = status;
            return View(mod);
        }

        [HttpPost]
        public ActionResult SPOfficerModal(string itNo, string dutyId, string DutyAssignEmpNoSP, string DutyAssignOfficerSP, string DutyAssignRemaks)
        {
            try
            {
                string sql2 = "UPDATE [TechAssets].[dbo].[Hardware_DutyAssign] SET DutyAssignOfficerSP = @DutyAssignOfficerSP, DutyAssignEmpNoSP = @DutyAssignEmpNoSP , DutyAssignRemaks = @DutyAssignRemaks, SysRAssiUser = @SysRAssiUser, SysRAssiDate = @SysRAssiDate WHERE DutyID = @DutyID";

                using (SqlConnection conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
                {
                    conn_uat.Open();

                    using (SqlCommand sqlcmd1 = new SqlCommand(sql2, conn_uat))
                    {
                        _ = sqlcmd1.Parameters.AddWithValue("@DutyAssignOfficerSP", DutyAssignOfficerSP);
                        _ = sqlcmd1.Parameters.AddWithValue("@DutyAssignEmpNoSP", DutyAssignEmpNoSP);
                        _ = sqlcmd1.Parameters.AddWithValue("@DutyAssignRemaks", DutyAssignRemaks);
                        _ = sqlcmd1.Parameters.AddWithValue("@SysRAssiDate", DateTime.Now);
                        _ = sqlcmd1.Parameters.AddWithValue("@SysRAssiUser", Constants.sysUser);
                        _ = sqlcmd1.Parameters.AddWithValue("@dutyId", dutyId);
                        _ = sqlcmd1.ExecuteNonQuery();
                    }


                }
                return Json(new { success = true, message = "Special Officer Assigned" });
            }

            catch (SqlException sqlEx) // Catch specific SQL exceptions
            {
                return Json(new { success = false, message = "SQL Error: " + sqlEx.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }

        }

        public ActionResult Task()
        {
            EnterItemsModel mod = new EnterItemsModel();

            string sql = "SELECT it.[Item_Type], DA.[IT_No], div.[division], ST.[Status], TH.[DeviceUser], TH.[Userfeedback], TH.[ContactDetails], CONVERT(varchar, DA.[DutyAssignDate], 103) AS DutyAssignDate, DA.[DutyAssignOfficer], DA.[DutyAssignEmpNo], DA.[DutyAssignOfficerSP], DA.[DutyAssignEmpNoSP], DA.[DutyAssignRemaks], DA.[TechnicalAssesment], DA.[Solutions], DA.[DutyID], DA.[DutyStatus], DA.[THandoverID], DA.[THandoverType], DA.[FaultTypeIDs] , it.Item_Type_Code,DA.[SolutionTypeIDs] FROM [TechAssets].[dbo].[Hardware_DutyAssign] AS DA JOIN [TechAssets].[dbo].[Hardware_THandover] AS TH ON DA.[THandoverID] = TH.[ID] JOIN [TechAssets].[dbo].[Hardware_DeviceStatus] ST ON ST.StatusID = DA.THandoverType JOIN [TechAssets].[dbo].[Hardware_Devices] D ON DA.IT_No = D.IT_No JOIN [EMMSDB].[dbo].[Staff_Division] div ON div.div_index = D.[Current_LocationID] JOIN [TechAssets].[dbo].[Hardware_Inovice_Items] i ON D.DescriptionCode = i.DescriptionCode JOIN [TechAssets].[dbo].[Hardware_Item_Types] it ON i.item_type = it.Item_Type_Code WHERE DA.[DutyAssignEmpNoSP] = '" + Constants.sysEmpNo + "' AND DA.[DutyStatus]='35'";
            
            string sql1 = "SELECT it.[Item_Type], DA.[IT_No], div.[division], ST.[Status], TH.[DeviceUser], TH.[Userfeedback], TH.[ContactDetails], CONVERT(varchar, DA.[DutyAssignDate], 103) AS DutyAssignDate, DA.[DutyAssignOfficer], DA.[DutyAssignEmpNo], DA.[DutyAssignOfficerSP], DA.[DutyAssignEmpNoSP], DA.[DutyAssignRemaks], DA.[TechnicalAssesment], DA.[Solutions], DA.[DutyID], DA.[DutyStatus], DA.[THandoverID], DA.[THandoverType], DA.[FaultTypeIDs] , it.Item_Type_Code,DA.[SolutionTypeIDs] FROM [TechAssets].[dbo].[Hardware_DutyAssign] AS DA JOIN [TechAssets].[dbo].[Hardware_THandover] AS TH ON DA.[THandoverID] = TH.[ID] JOIN [TechAssets].[dbo].[Hardware_DeviceStatus] ST ON ST.StatusID = DA.THandoverType JOIN [TechAssets].[dbo].[Hardware_Devices] D ON DA.IT_No = D.IT_No JOIN [EMMSDB].[dbo].[Staff_Division] div ON div.div_index = D.[Current_LocationID] JOIN [TechAssets].[dbo].[Hardware_Inovice_Items] i ON D.DescriptionCode = i.DescriptionCode JOIN [TechAssets].[dbo].[Hardware_Item_Types] it ON i.item_type = it.Item_Type_Code WHERE DA.[DutyAssignEmpNo] = '" + Constants.sysEmpNo + "' AND DA.[DutyStatus]='35'";

            string sql2 = "SELECT it.[Item_Type], DA.[IT_No], div.[division], ST.[Status], TH.[DeviceUser], TH.[Userfeedback], TH.[ContactDetails], CONVERT(varchar, DA.[DutyAssignDate], 103) AS DutyAssignDate, DA.[DutyAssignOfficer], DA.[DutyAssignEmpNo], DA.[DutyAssignOfficerSP], DA.[DutyAssignEmpNoSP], DA.[DutyAssignRemaks], DA.[TechnicalAssesment], DA.[Solutions], DA.[DutyID], DA.[DutyStatus], DA.[THandoverID], DA.[THandoverType], DA.[FaultTypeIDs], it.Item_Type_Code, DA.[SolutionTypeIDs], DS.[Status] AS Current_Status FROM [TechAssets].[dbo].[Hardware_DutyAssign] AS DA JOIN [TechAssets].[dbo].[Hardware_THandover] AS TH ON DA.[THandoverID] = TH.[ID] JOIN [TechAssets].[dbo].[Hardware_DeviceStatus] ST ON ST.StatusID = DA.THandoverType JOIN [TechAssets].[dbo].[Hardware_Devices] D ON DA.IT_No = D.IT_No JOIN [EMMSDB].[dbo].[Staff_Division] div ON div.div_index = D.[Current_LocationID] JOIN [TechAssets].[dbo].[Hardware_Inovice_Items] i ON D.DescriptionCode = i.DescriptionCode JOIN [TechAssets].[dbo].[Hardware_Item_Types] it ON i.item_type = it.Item_Type_Code JOIN [TechAssets].[dbo].[Hardware_DeviceStatus] DS ON D.Current_Status = DS.StatusID WHERE DA.[DutyAssignEmpNo] = '" + Constants.sysEmpNo + "' AND (DA.[DutyStatus] = '36' OR DA.[DutyStatus] = '52' OR DA.[DutyStatus] = '50' OR DA.[DutyStatus] = '37' OR DA.[DutyStatus] = '38' OR DA.[DutyStatus] = '51')";

            
            mod.CardTBL = HardwareModule.Get_Any_DT(sql);
            mod.CardTBL1 = HardwareModule.Get_Any_DT(sql1);
            mod.CardTBL2 = HardwareModule.Get_Any_DT(sql2);



            string faultSolutionSql = "SELECT [FSid], [Fault_Solution] FROM [TechAssets].[dbo].[Hardware_Fault&Solution] WHERE [Type] = 'Fault'";
            DataTable faultSolutions = HardwareModule.Get_Any_DT(faultSolutionSql);


            ViewBag.FaultTypeID = faultSolutions.AsEnumerable().Select(row => Convert.ToInt32(row["FSid"])).ToList();

            ViewBag.FaultType = faultSolutions.AsEnumerable().Select(row => row["Fault_Solution"].ToString()).ToList();


            return View(mod);
        }


        [HttpPost]
        public JsonResult GetFaultList(string itemType, string dutyID, string FaultTypeIDs, string itemTypeCode)
        {
            try
            {
                List<SelectListItem> faultList = new List<SelectListItem>();
                string query = "SELECT [FSid], [Fault_Solution] FROM [TechAssets].[dbo].[Hardware_Fault&Solution] WHERE [Type] = 'Fault' AND [Item_Type] = @ItemType";
                string query1 = "SELECT [FaultTypeIDs] FROM [TechAssets].[dbo].[Hardware_DutyAssign] WHERE DutyID = @dutyID";
                string faultTypeIds = null;

                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        _ = cmd.Parameters.Add(new SqlParameter("@ItemType", SqlDbType.NVarChar) { Value = itemTypeCode });

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                faultList.Add(new SelectListItem
                                {
                                    Value = reader["FSid"].ToString().Trim(),
                                    Text = reader["Fault_Solution"].ToString().Trim()
                                });
                            }
                        }
                    }


                    using (SqlCommand cmd1 = new SqlCommand(query1, conn))
                    {
                        _ = cmd1.Parameters.AddWithValue("@dutyID", dutyID);
                        object result = cmd1.ExecuteScalar();
                        faultTypeIds = result?.ToString();
                    }
                }

                return Json(new { success = true, data = faultList, faultTypeIds }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while retrieving the fault list: " + ex.Message });
            }
        }



        [HttpPost]
        public ActionResult TASubmit(string DutyID, string TechnicalAssesment, string SFaultTypeIDs)
        {
            try
            {
                string sqlUpdateDutyAssign = "UPDATE [TechAssets].[dbo].[Hardware_DutyAssign] SET TechnicalAssesment = @TechnicalAssesment, SysTAEnterDate = @SysTAEnterDate , FaultTypeIDs=@FaultTypeIDs WHERE DutyID = @DutyID";

                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
                {
                    conn.Open();

                    using (SqlCommand cmdTAssesment = new SqlCommand(sqlUpdateDutyAssign, conn))
                    {
                        _ = cmdTAssesment.Parameters.AddWithValue("@TechnicalAssesment", TechnicalAssesment);
                        _ = cmdTAssesment.Parameters.AddWithValue("@SysTAEnterDate", DateTime.Now);
                        _ = cmdTAssesment.Parameters.AddWithValue("@DutyID", DutyID);
                        _ = cmdTAssesment.Parameters.AddWithValue("@FaultTypeIDs", SFaultTypeIDs);

                        _ = cmdTAssesment.ExecuteNonQuery();
                    }
                }

                return RedirectToAction("Task", "Hardware");
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }


        }




        [HttpPost]
        public ActionResult TaskComplete(string itno, string dutyID)
        {
            try
            {
                string sqlUpdateDutyAssign = "UPDATE [TechAssets].[dbo].[Hardware_DutyAssign] SET DutyStatus =@DutyStatus , SysRCompleteDate = @SysRCompleteDate WHERE DutyID = @DutyID";
                string sqlUpdateDevices = "UPDATE [TechAssets].[dbo].[Hardware_Devices] SET Current_Status = @Current_Status WHERE IT_No = @IT_No";


                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
                {
                    conn.Open();

                    using (SqlCommand cmdDutyAssign = new SqlCommand(sqlUpdateDutyAssign, conn))
                    {
                        _ = cmdDutyAssign.Parameters.AddWithValue("@DutyID", dutyID);
                        _ = cmdDutyAssign.Parameters.AddWithValue("@DutyStatus", 36);
                        _ = cmdDutyAssign.Parameters.AddWithValue("@SysRCompleteDate", DateTime.Now);
                        _ = cmdDutyAssign.ExecuteNonQuery();
                    }

                    using (SqlCommand cmdDevices = new SqlCommand(sqlUpdateDevices, conn))
                    {
                        _ = cmdDevices.Parameters.AddWithValue("@IT_No", itno);
                        _ = cmdDevices.Parameters.AddWithValue("@Current_Status", 36);
                        _ = cmdDevices.ExecuteNonQuery();
                    }
                }

                return Json(new { success = true, message = "Successfully submitted!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }



        [HttpPost]
        public ActionResult RequestTransferIT(string itno, string dutyID)
        {
            try
            {
                string sqlUpdateDutyAssign = "UPDATE [TechAssets].[dbo].[Hardware_DutyAssign] SET DutyStatus =@DutyStatus , SysRCompleteDate = @SysRCompleteDate WHERE DutyID = @DutyID";
                string sqlUpdateDevices = "UPDATE [TechAssets].[dbo].[Hardware_Devices] SET Current_Status = @Current_Status, Current_LocationID = @Current_LocationID WHERE IT_No = @IT_No";


                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
                {
                    conn.Open();

                    using (SqlCommand cmdDutyAssign = new SqlCommand(sqlUpdateDutyAssign, conn))
                    {
                        _ = cmdDutyAssign.Parameters.AddWithValue("@DutyID", dutyID);
                        _ = cmdDutyAssign.Parameters.AddWithValue("@DutyStatus", 47);
                        _ = cmdDutyAssign.Parameters.AddWithValue("@SysRCompleteDate", DateTime.Now);
                        _ = cmdDutyAssign.ExecuteNonQuery();
                    }

                    using (SqlCommand cmdDevices = new SqlCommand(sqlUpdateDevices, conn))
                    {
                        _ = cmdDevices.Parameters.AddWithValue("@IT_No", itno);
                        _ = cmdDevices.Parameters.AddWithValue("@Current_Status", 2);
                        _ = cmdDevices.Parameters.AddWithValue("@Current_LocationID", "NRMDIV019");
                        _ = cmdDevices.ExecuteNonQuery();
                    }
                }

                return Json(new { success = true, message = "Successfully submitted!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }



        public ActionResult FaultnSolution()
        {
            EnterItemsModel InMod = new EnterItemsModel
            {
                ItemTypeList = HardwareModule.GetItemTypeListByDatabase()
            };
            string sql = "SELECT HF.[FSid], HF.[Fault_Solution], HF.[Type], IT.[Item_Type] FROM [TechAssets].[dbo].[Hardware_Fault&Solution] AS HF INNER JOIN [TechAssets].[dbo].[Hardware_Item_Types] AS IT ON HF.[Item_Type] = IT.[Item_Type_Code] WHERE HF.[FSStatus] = 1";

            InMod.faultSolutions = HardwareModule.Get_Any_DT(sql);

            return View(InMod);
        }


        public ActionResult SubmitNewFnS(string FaultType, string FaultItem, string Fault_Solution)
        {
            string sql = "INSERT INTO [TechAssets].[dbo].[Hardware_Fault&Solution] ([Fault_Solution], [Type], [Item_Type], [FSStatus]) VALUES (@Fault_Solution, @FaultType, @FaultItem, 1)";

            using (SqlConnection conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
            {
                conn_uat.Open();

                using (SqlCommand sqlcmd = new SqlCommand(sql, conn_uat))
                {

                    _ = sqlcmd.Parameters.AddWithValue("@Fault_Solution", Fault_Solution);
                    _ = sqlcmd.Parameters.AddWithValue("@FaultType", FaultType);
                    _ = sqlcmd.Parameters.AddWithValue("@FaultItem", FaultItem);

                    _ = sqlcmd.ExecuteNonQuery();
                }
            }

            return RedirectToAction("FaultnSolution", "Hardware");
        }

        public ActionResult DeleteFnS(string FSid)
        {
            string sql = "UPDATE [TechAssets].[dbo].[Hardware_Fault&Solution] SET FSStatus = '2' WHERE FSid = @FSid";
            using (SqlConnection conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
            {
                conn_uat.Open();

                SqlCommand sqlcmd = new SqlCommand(sql, conn_uat);
                _ = sqlcmd.Parameters.AddWithValue("@FSid", FSid);
                _ = sqlcmd.ExecuteNonQuery();
                conn_uat.Close();
            }

            return RedirectToAction("FaultnSolution", "Hardware");



        }


        public ActionResult AddExsitingDEV(string invno, string invID, string invoiceNo, string Item, int count)
        {
            EnterItemsModel mod = new EnterItemsModel();
            _ = new EnterItemsModel();

            mod.Item_Type_Code = HardwareModule.GetItemTypeCode();
            mod.ItemTypeList = HardwareModule.GetItemTypeListByDatabase();
            mod.DivisionList = HardwareModule.GetDivisionListByDatabase();
            List<SelectListItem> model = new List<SelectListItem>();
            List<SelectListItem> brand = new List<SelectListItem>();
            mod.BrandList = brand;
            mod.ModelList = model;

            mod.invno = invno;

            EnterItemsModel temp = HardwareModule.GetInvoiceInfo(invno);

            mod.Invoice_Ref = temp.Invoice_Ref;
            mod.Invoice_No = temp.Invoice_No;
            mod.Invoice_Date = temp.Invoice_Date;
            mod.Name_of_Supplier = temp.Name_of_Supplier;

            string sql = "SELECT HI.[Invoice_Ref],HI.[DescriptionCode],IT.[Item_Type],B.[BrandName],HMN.[Model_Name],HI.[Warranty_Period],HI.[description],HI.[quantity],HI.[unitprice],HI.[sys_Date],HI.[sys_User],HI.[status],HI.[Asset]" +
                "FROM [TechAssets].[dbo].[Hardware_Inovice_Items] HI " +
                "INNER JOIN [TechAssets].[dbo].[Hardware_ModelNew] HMN ON HI.[ModelName] = HMN.[Model_Code] " +
                "INNER JOIN [TechAssets].[dbo].[Hardware_Item_Types] IT ON HI.[Item_Type] = IT.[Item_Type_Code] " +
                "INNER JOIN [TechAssets].[dbo].[Hardware_Brand] B ON HI.[BrandName] = B.[BrandCode] " +
                "WHERE HI.Invoice_Ref = @invno";

            using (SqlConnection conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
            {
                conn_uat.Open();
                SqlDataAdapter da = new SqlDataAdapter(sql, conn_uat);
                DataSet ds = new DataSet();
                _ = da.SelectCommand.Parameters.AddWithValue("@invno", invno);
                _ = da.Fill(ds);
                mod.dt1 = ds.Tables[0];
                conn_uat.Close();
            }

            // Add a new column for RemainingQuantity to the DataTable
            if (!mod.dt1.Columns.Contains("RemainingQuantity"))
            {
                _ = mod.dt1.Columns.Add("RemainingQuantity", typeof(int));
            }

            // Calculate the remaining quantity for each row using the helper method
            foreach (DataRow row in mod.dt1.Rows)
            {
                string descriptionCode = row["DescriptionCode"].ToString();
                int totalQuantity = Convert.ToInt32(row["quantity"]);

                // Call the helper method to get the remaining quantity
                int remainingQuantity = GetRemainingQuantity(descriptionCode, totalQuantity);
                row["RemainingQuantity"] = remainingQuantity;
            }

            if (TempData["ErrorMessage"] != null)
            {
                ViewBag.ErrorMessage = TempData["ErrorMessage"];
            }

            if (TempData["SuccessMessage"] != null)
            {
                ViewBag.SuccessMessage = TempData["SuccessMessage"];
            }

            return View(mod);
        }

        public ActionResult DataPushToDB47(string index)
        {
            EnterItemsModel mod = new EnterItemsModel();
            return View(mod);
        }


        public ActionResult Stores(string invno, string invID, string invoiceNo, string Item, int count)
        {
            int remainP = GetPartsQuantity(invID, count);


            if (!(TempData["EnterItemsModel"] is EnterItemsModel mod))
            {
                mod = new EnterItemsModel();
            }

            mod.invno = invno;
            mod.invID = invID;
            mod.invoiceNo = invoiceNo;
            mod.Item = Item;
            mod.count = count;
            mod.remainP = remainP;

            mod.DivisionList = HardwareModule.GetDivisionListByDatabase();

            if (TempData["ErrorMessage"] != null)
            {
                ViewBag.ErrorMessage = TempData["ErrorMessage"];
            }

            if (TempData["SuccessMessage"] != null)
            {
                ViewBag.SuccessMessage = TempData["SuccessMessage"];
            }


            return View(mod);
        }



        public ActionResult SubmitStores(EnterItemsModel mod, string invno, string invID, string invoiceNo, int count, string Item)
        {
            _ = GetPartsQuantity(invID, count);


            string sql = "INSERT INTO [TechAssets].[dbo].[Hardware_Stores] ([Current_Status],[Invoice_Reference],[DescriptionCode],[Invoice_No],[ItemType],[IT_No],[Serial_No],[INV_No],[QR],[SsysUser],[SsysDate]) VALUES ('1','" + mod.invno + "','" + mod.invID + "','" + mod.invoiceNo + "','" + Item + "','" + mod.IT_No + "','" + mod.Serial_No + "','" + mod.INV_No + "','" + mod.QR + "','" + Constants.sysUser + "','" + DateTime.Now + "')";
            using (SqlConnection conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
            {
                conn_uat.Open();
                SqlCommand sqlcmd = new SqlCommand(sql, conn_uat);

                _ = sqlcmd.ExecuteNonQuery();
                conn_uat.Close();

            }
            return RedirectToAction("Stores", "Hardware", new { invno, invID, invoiceNo, count, Item });
        }


        [HttpPost]
        public JsonResult GetSolutionList(string itemType1, string dutyID1, string solutionTypeIDs, string itemTypeCode1)
        {
            try
            {
                List<SelectListItem> solutionList = new List<SelectListItem>();
                string query = "SELECT [FSid], [Fault_Solution] FROM [TechAssets].[dbo].[Hardware_Fault&Solution] WHERE [Type] = 'Solution' AND [Item_Type] = @ItemTypeCode";
                string query1 = "SELECT [SolutionTypeIDs] FROM [TechAssets].[dbo].[Hardware_DutyAssign] WHERE DutyID = @dutyID";
                string solutionTypeIds = null;

                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
                {
                    conn.Open();


                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        _ = cmd.Parameters.Add(new SqlParameter("@ItemTypeCode", SqlDbType.NVarChar) { Value = itemTypeCode1 });

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                solutionList.Add(new SelectListItem
                                {
                                    Value = reader["FSid"].ToString().Trim(),
                                    Text = reader["Fault_Solution"].ToString().Trim()
                                });
                            }
                        }
                    }

                    using (SqlCommand cmd1 = new SqlCommand(query1, conn))
                    {
                        _ = cmd1.Parameters.AddWithValue("@DutyID", dutyID1);
                        object result = cmd1.ExecuteScalar();
                        solutionTypeIds = result?.ToString();
                    }
                }

                return Json(new { success = true, data = solutionList, solutionTypeIds }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while retrieving the fault list: " + ex.Message });
            }
        }


        [HttpPost]
        public ActionResult SoSubmit(string DutyID1, string Solution, string SSolutionTypeIDs)
        {
            try
            {
                string sqlUpdateDutyAssign = "UPDATE [TechAssets].[dbo].[Hardware_DutyAssign] SET Solutions = @Solutions, SysSoEnterDate = @SysSoEnterDate , SolutionTypeIDs = @SolutionTypeIDs WHERE DutyID = @DutyID";

                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
                {
                    conn.Open();

                    using (SqlCommand cmdTAssesment = new SqlCommand(sqlUpdateDutyAssign, conn))
                    {
                        _ = cmdTAssesment.Parameters.AddWithValue("@Solutions", Solution);
                        _ = cmdTAssesment.Parameters.AddWithValue("@SysSoEnterDate", DateTime.Now);
                        _ = cmdTAssesment.Parameters.AddWithValue("@DutyID", DutyID1);
                        _ = cmdTAssesment.Parameters.AddWithValue("@SolutionTypeIDs", SSolutionTypeIDs);

                        _ = cmdTAssesment.ExecuteNonQuery();
                    }
                }

                return RedirectToAction("Task", "Hardware");
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }



        }

        public ActionResult ReceptionHandover()
        {
            string sql = "SELECT it.Item_Type, h.IT_No, h.Serial_No, h.INV_No, h.QR, div.division AS Purchased_Division, div1.division AS Current_Location, div2.division AS Handover_Location, ho.Authorized_Officer, ho.HORecRemarks, ho.HOstatus, st.Status, ho.HOLocationID, ho.id FROM [TechAssets].[dbo].[Hardware_Devices] h JOIN [TechAssets].[dbo].[Hardware_Inovice_Items] i ON h.DescriptionCode = i.DescriptionCode JOIN [TechAssets].[dbo].[Hardware_Item_Types] it ON i.item_type = it.Item_Type_Code LEFT JOIN [EMMSDB].[dbo].[Staff_Division] div ON h.Purchased_DivisionID = div.div_index LEFT JOIN [EMMSDB].[dbo].[Staff_Division] div1 ON h.Current_LocationID = div1.div_index LEFT JOIN [TechAssets].[dbo].[Hardware_Handover] ho ON h.IT_No = ho.IT_No JOIN [TechAssets].[dbo].[Hardware_DeviceStatus] st ON ho.HOstatus = st.StatusID LEFT JOIN [EMMSDB].[dbo].[Staff_Division] div2 ON ho.HOLocationID = div2.div_index WHERE ho.HOstatus IN (CAST(4 AS INT))";
            EnterItemsModel Inmod = new EnterItemsModel
            {
                RHFilter = HardwareModule.Get_Any_DT(sql)
            };

            return View(Inmod);
        }

        public ActionResult ReceptionHandoverNew(EnterItemsModel mod, string IT_No, string DCurrentLocation, string DAuthorized, string HOLid, string hno)
        {

            string Handover = "UPDATE [TechAssets].[dbo].[Hardware_Handover] SET Current_Status = @Current_Status, HOstatus = @HOstatus, HOComDate = @HOComDate, HOComRemarks = @HOComRemarks, sysCom_User = @sysCom_User, sysCom_Date = @SysApp_Date, HOReceiver=@EmployeeName  WHERE id = @hno";
            string Device = "UPDATE [TechAssets].[dbo].[Hardware_Devices] SET Current_Status = @Current_Status, Current_LocationID = @Current_LocationID, HoIDst=@HoIDst WHERE IT_No = @IT_No";

            using (SqlConnection conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
            {
                conn_uat.Open();

                using (SqlCommand sqlcmd1 = new SqlCommand(Handover, conn_uat))
                {
                    _ = sqlcmd1.Parameters.AddWithValue("@hno", hno);
                    _ = sqlcmd1.Parameters.AddWithValue("@Current_Status", 56);
                    _ = sqlcmd1.Parameters.AddWithValue("@HOstatus", 56);
                    _ = sqlcmd1.Parameters.AddWithValue("@HOComDate", mod.HOComDate ?? (object)DBNull.Value);
                    _ = sqlcmd1.Parameters.AddWithValue("@HOComRemarks", mod.HOComRemarks ?? (object)DBNull.Value);
                    _ = sqlcmd1.Parameters.AddWithValue("@EmployeeName", mod.EmployeeName ?? (object)DBNull.Value);
                    _ = sqlcmd1.Parameters.AddWithValue("@sysCom_User", Constants.sysUser);
                    _ = sqlcmd1.Parameters.AddWithValue("@SysApp_Date", DateTime.Now);
                   
                    _ = sqlcmd1.ExecuteNonQuery();
                }

                using (SqlCommand sqlcmd = new SqlCommand(Device, conn_uat))
                {
                    _ = sqlcmd.Parameters.AddWithValue("@IT_No", IT_No);
                    _ = sqlcmd.Parameters.AddWithValue("@Current_Status", 56);
                    _ = sqlcmd.Parameters.AddWithValue("@HoIDst", hno);
                    _ = sqlcmd.Parameters.AddWithValue("@Current_LocationID", HOLid ?? (object)DBNull.Value);
                    _ = sqlcmd.ExecuteNonQuery();
                }

                conn_uat.Close();
            }
            return RedirectToAction("ReceptionHandover", "Hardware");
        }


        public ActionResult ReceptionCHandover(EnterItemsModel mod, string IT_No, string Duty_Id, string Ho_Id)
        {

            string sql = "UPDATE [TechAssets].[dbo].[Hardware_Devices] SET Current_Status = @Current_Status, HoIDth = @HoIDth WHERE IT_No = @IT_No";
            string sql1 = "UPDATE [TechAssets].[dbo].[Hardware_THandover] SET THandoverStatus = @THandoverStatus, ReturnDate = @ReturnDate, CollectOfficer = @CollectOfficer, CollectRemarks = @CollectRemarks, SysCoUser = @SysCoUser, SysCoDate=@SysCoDate, ReturnBy='Return By Hand' WHERE id = @Ho_Id";
            string sql2 = "UPDATE [TechAssets].[dbo].[Hardware_DutyAssign] SET DutyStatus =@DutyStatus WHERE DutyID = @DutyID";

            using (SqlConnection conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
            {
                conn_uat.Open();

                using (SqlCommand sqlcmd = new SqlCommand(sql, conn_uat))
                {
                    _ = sqlcmd.Parameters.AddWithValue("@HoIDth", Ho_Id);
                    _ = sqlcmd.Parameters.AddWithValue("@Current_Status", 56);
                    _ = sqlcmd.Parameters.AddWithValue("@IT_No", IT_No ?? (object)DBNull.Value);
                    _ = sqlcmd.ExecuteNonQuery();
                }

                using (SqlCommand sqlcmd1 = new SqlCommand(sql1, conn_uat))
                {
                    _ = sqlcmd1.Parameters.AddWithValue("@Ho_Id", Ho_Id ?? (object)DBNull.Value);
                    _ = sqlcmd1.Parameters.AddWithValue("@THandoverStatus", 56);
                    _ = sqlcmd1.Parameters.AddWithValue("@ReturnDate", mod.HOComDate ?? (object)DBNull.Value);
                    _ = sqlcmd1.Parameters.AddWithValue("@CollectOfficer", mod.EmployeeName ?? (object)DBNull.Value);
                    _ = sqlcmd1.Parameters.AddWithValue("@CollectRemarks", mod.HOComRemarks ?? (object)DBNull.Value);
                    _ = sqlcmd1.Parameters.AddWithValue("@SysCoUser", Constants.sysUser);
                    _ = sqlcmd1.Parameters.AddWithValue("@SysCoDate", DateTime.Now);
                    _ = sqlcmd1.ExecuteNonQuery();
                }

                using (SqlCommand sqlcmd2 = new SqlCommand(sql2, conn_uat))

                {
                    _ = sqlcmd2.Parameters.AddWithValue("@DutyStatus", 47);
                    _ = sqlcmd2.Parameters.AddWithValue("@DutyID", Duty_Id ?? (object)DBNull.Value);
                    _ = sqlcmd2.ExecuteNonQuery();
                }

                conn_uat.Close();
            }
            return RedirectToAction("RepairedHandover", "Hardware");
        }


        public ActionResult CourierReqNewItem()
        {
            string sql = "SELECT it.Item_Type, h.IT_No, h.Serial_No, h.INV_No, h.QR, div.division AS Purchased_Division, div1.division AS Current_Location, div2.division AS Handover_Location, ho.Authorized_Officer, ho.HORecRemarks, ho.HOstatus, st.Status, ho.HOLocationID FROM [TechAssets].[dbo].[Hardware_Devices] h JOIN [TechAssets].[dbo].[Hardware_Inovice_Items] i ON h.DescriptionCode = i.DescriptionCode JOIN [TechAssets].[dbo].[Hardware_Item_Types] it ON i.item_type = it.Item_Type_Code LEFT JOIN [EMMSDB].[dbo].[Staff_Division] div ON h.Purchased_DivisionID = div.div_index LEFT JOIN [EMMSDB].[dbo].[Staff_Division] div1 ON h.Current_LocationID = div1.div_index LEFT JOIN [TechAssets].[dbo].[Hardware_Handover] ho ON h.IT_No = ho.IT_No JOIN [TechAssets].[dbo].[Hardware_DeviceStatus] st ON ho.HOstatus = st.StatusID LEFT JOIN [EMMSDB].[dbo].[Staff_Division] div2 ON ho.HOLocationID = div2.div_index WHERE ho.HOstatus IN (CAST(4 AS INT))";
            EnterItemsModel Inmod = new EnterItemsModel
            {
                CNFilter = HardwareModule.Get_Any_DT(sql)
            };

            return View(Inmod);
        }


        public ActionResult NewSubmitCourier(EnterItemsModel mod, string IT_No, string CReqDate, string hOLocationID, string CReqRemarks, int id)
        {
            string sqlInsert = "INSERT INTO [TechAssets].[dbo].[Hardware_Courier] (CourierStatus, IT_No, HoID, CReqDate, CReqLocationID, CReqRemarks, CsysReqUser, CsysReqDate) " +
                               "VALUES (@CourierStatus, @IT_No, @HoID, @CReqDate, @CReqLocationID, @CReqRemarks, @CsysReqUser, @CsysReqDate)";

            string sqlUpdate1 = "UPDATE [TechAssets].[dbo].[Hardware_Handover] SET HOstatus = 57 WHERE id = @id";

            string sqlUpdate = "UPDATE [TechAssets].[dbo].[Hardware_Devices] SET Current_Status = 11 WHERE IT_No = @IT_No";



            using (SqlConnection conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
            {
                conn_uat.Open();


                using (SqlCommand insertCmd = new SqlCommand(sqlInsert, conn_uat))
                {
                    _ = insertCmd.Parameters.AddWithValue("@CourierStatus", 11);
                    _ = insertCmd.Parameters.AddWithValue("@IT_No", IT_No);
                    _ = insertCmd.Parameters.AddWithValue("@HoID", id);
                    _ = insertCmd.Parameters.AddWithValue("@CReqDate", CReqDate);
                    _ = insertCmd.Parameters.AddWithValue("@CReqLocationID", hOLocationID);
                    _ = insertCmd.Parameters.AddWithValue("@CReqRemarks", CReqRemarks);
                    _ = insertCmd.Parameters.AddWithValue("@CsysReqUser", Constants.sysUser);
                    _ = insertCmd.Parameters.AddWithValue("@CsysReqDate", DateTime.Now);
                    _ = insertCmd.ExecuteNonQuery();
                }


                using (SqlCommand updateCmd = new SqlCommand(sqlUpdate, conn_uat))
                {
                    _ = updateCmd.Parameters.AddWithValue("@IT_No", IT_No);
                    _ = updateCmd.ExecuteNonQuery();
                }


                using (SqlCommand updateCmd = new SqlCommand(sqlUpdate1, conn_uat))
                {
                    _ = updateCmd.Parameters.AddWithValue("@id", id);
                    _ = updateCmd.ExecuteNonQuery();
                }
            }

            return RedirectToAction("CourierReqNewItem", "Hardware", new { status = "2" });
        }


        public ActionResult RepairedHandover()
        {
            string sql = "SELECT it.[Item_Type], Du.[IT_No], Di.Serial_No, Di.INV_No, Di.QR, div1.division AS Location, Du.[DutyAssignOfficer], Du.DutyAssignOfficerSP, Du.[SysRCompleteDate], Di.Authorized_Officer, Du.[DutyID], Du.[DutyStatus], Du.[THandoverID], Du.[THandoverType],div1.div_index FROM [TechAssets].[dbo].[Hardware_DutyAssign] AS Du JOIN [TechAssets].[dbo].[Hardware_Devices] Di ON Du.[IT_No] = Di.IT_No JOIN [TechAssets].[dbo].[Hardware_Inovice_Items] i ON Di.DescriptionCode = i.DescriptionCode JOIN [TechAssets].[dbo].[Hardware_Item_Types] it ON i.item_type = it.Item_Type_Code JOIN [EMMSDB].[dbo].[Staff_Division] div1 ON Di.Current_LocationID = div1.div_index WHERE [DutyStatus] = 36";
            EnterItemsModel Inmod = new EnterItemsModel
            {
                ReHFilter = HardwareModule.Get_Any_DT(sql)
            };

            return View(Inmod);
        }



        public ActionResult CourierRepairedItem()
        {
            string sql = "SELECT it.[Item_Type], Du.[IT_No], Di.Serial_No, Di.INV_No, Di.QR, div1.division AS Location, Du.[DutyAssignOfficer], Du.DutyAssignOfficerSP, Du.[SysRCompleteDate], Di.Authorized_Officer, Du.[DutyID], Du.[DutyStatus], Du.[THandoverID], Du.[THandoverType], Di.Current_LocationID FROM [TechAssets].[dbo].[Hardware_DutyAssign] AS Du JOIN [TechAssets].[dbo].[Hardware_Devices] Di ON Du.[IT_No] = Di.IT_No JOIN [TechAssets].[dbo].[Hardware_Inovice_Items] i ON Di.DescriptionCode = i.DescriptionCode JOIN [TechAssets].[dbo].[Hardware_Item_Types] it ON i.item_type = it.Item_Type_Code JOIN [EMMSDB].[dbo].[Staff_Division] div1 ON Di.Current_LocationID = div1.div_index WHERE [DutyStatus] = 36";
            EnterItemsModel Inmod = new EnterItemsModel
            {
                QReHFilter = HardwareModule.Get_Any_DT(sql)
            };

            return View(Inmod);
        }

        public ActionResult RepairSubmitCourier(EnterItemsModel mod, string itNumberRe, string CReqDate, string hOLocationID, string CReqRemarks, int DutyIdRe, int HoIdRe)
        {
            string sqlInsert = "INSERT INTO [TechAssets].[dbo].[Hardware_Courier] (CourierStatus, IT_No, DutyID, THoId, CReqDate, CReqLocationID, CReqRemarks, CsysReqUser, CsysReqDate) " +
                               "VALUES (@CourierStatus, @IT_No, @DutyID, @THoId, @CReqDate, @CReqLocationID, @CReqRemarks, @CsysReqUser, @CsysReqDate)";

            string sqlUpdate1 = "UPDATE [TechAssets].[dbo].[Hardware_THandover] SET THandoverStatus = 11 WHERE id = @THoId";

            string sqlUpdate2 = "UPDATE [TechAssets].[dbo].[Hardware_DutyAssign] SET DutyStatus = 11 WHERE DutyID = @DutyID";

            string sqlUpdate = "UPDATE [TechAssets].[dbo].[Hardware_Devices] SET Current_Status = 11 WHERE IT_No = @IT_No";



            using (SqlConnection conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
            {
                conn_uat.Open();


                using (SqlCommand insertCmd = new SqlCommand(sqlInsert, conn_uat))
                {
                    _ = insertCmd.Parameters.AddWithValue("@CourierStatus", 11);
                    _ = insertCmd.Parameters.AddWithValue("@IT_No", itNumberRe);
                    _ = insertCmd.Parameters.AddWithValue("@DutyID", DutyIdRe);
                    _ = insertCmd.Parameters.AddWithValue("@THoId", HoIdRe);
                    _ = insertCmd.Parameters.AddWithValue("@CReqDate", CReqDate);
                    _ = insertCmd.Parameters.AddWithValue("@CReqLocationID", hOLocationID);
                    _ = insertCmd.Parameters.AddWithValue("@CReqRemarks", CReqRemarks);
                    _ = insertCmd.Parameters.AddWithValue("@CsysReqUser", Constants.sysUser);
                    _ = insertCmd.Parameters.AddWithValue("@CsysReqDate", DateTime.Now);
                    _ = insertCmd.ExecuteNonQuery();
                }


                using (SqlCommand updateCmd = new SqlCommand(sqlUpdate, conn_uat))
                {
                    _ = updateCmd.Parameters.AddWithValue("@IT_No", itNumberRe);
                    _ = updateCmd.ExecuteNonQuery();
                }


                using (SqlCommand updateCmd = new SqlCommand(sqlUpdate1, conn_uat))
                {
                    _ = updateCmd.Parameters.AddWithValue("@THoId", HoIdRe);
                    _ = updateCmd.ExecuteNonQuery();
                }

                using (SqlCommand updateCmd = new SqlCommand(sqlUpdate2, conn_uat))
                {
                    _ = updateCmd.Parameters.AddWithValue("@DutyID", DutyIdRe);
                    _ = updateCmd.ExecuteNonQuery();
                }
            }

            return RedirectToAction("CourierRepairedItem", "Hardware");
        }

        public ActionResult CreateAssestReqForm(string status)
        {

            EnterItemsModel Mod = new EnterItemsModel();

            string sql = "SELECT req.[ReqFormID],req.[RDivision],CONVERT(VARCHAR, req.[RDate], 23) AS RDate,req.[StatusID],req.[AssestRemarks], status.[AssestStatus],req.[Date1],req.[Remaks1],req.[Date2],req.[Remaks2],req.[Date3],req.[Remaks3],req.[Date4],req.[Remaks4],req.[Date5],req.[Remaks5],req.[Status] FROM [TechAssets].[dbo].[Hardware_Master_AssestReq] req left outer JOIN [TechAssets].[dbo].[Hardware_AssestStatus] status ON req.[StatusID] = status.[id] WHERE req.[Status] = '" + status + "'";
            Mod.AssestTbl = HardwareModule.Get_Any_DT(sql);
            Mod.ReqFormID = HardwareModule.GetFormID();
            Mod.ItemTypeList = HardwareModule.GetItemTypeListByDatabase();
            Mod.DivisionList = HardwareModule.GetDivisionListByDatabase();
            Mod.AssestsStatusList = HardwareModule.GetAssestsStatusListByDatabase();

            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            return View(Mod);
        }


        public ActionResult SubmitReqForm(EnterItemsModel mod, HttpPostedFileBase ReqFile)
        {
            try
            {
                if (ReqFile != null)
                {
                    HttpPostedFileBase document = ReqFile;
                    string fileExtension = Path.GetExtension(document.FileName).ToUpper();

                    if (fileExtension == ".PDF")
                    {
                        string uploadDirectory = Server.MapPath("~/Upload/AssetRequest");
                        if (!Directory.Exists(uploadDirectory))
                        {
                            _ = Directory.CreateDirectory(uploadDirectory);
                        }

                        string uniqueFileName = $"Request_{mod.ReqFormID}.pdf";
                        string path = Path.Combine(uploadDirectory, uniqueFileName);

                        document.SaveAs(path);

                        string sql = "INSERT INTO [TechAssets].[dbo].[Hardware_Master_AssestReq] " +
                                     "([ReqFormID], [RDivision], [RDate], [RsysDate], [RsysUser], [Status]) " +
                                     "VALUES (@ReqFormID, @RDivision, @RDate, @RsysDate, @RsysUser, 1)";

                        using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
                        {
                            SqlCommand cmd = new SqlCommand(sql, con);
                            _ = cmd.Parameters.AddWithValue("@ReqFormID", mod.ReqFormID);
                            _ = cmd.Parameters.AddWithValue("@RDivision", mod.RDivision);
                            _ = cmd.Parameters.AddWithValue("@RDate", mod.RDate);
                            _ = cmd.Parameters.AddWithValue("@RsysDate", DateTime.Now);
                            _ = cmd.Parameters.AddWithValue("@RsysUser", Constants.sysUser);

                            con.Open();
                            _ = cmd.ExecuteNonQuery();
                        }

                        return Json(new { success = true, message = "Request submitted successfully!" });
                    }
                    else
                    {
                        return Json(new { success = false, error = "Invalid file type. Only PDFs are allowed." });
                    }
                }
                else
                {
                    return Json(new { success = false, error = "No file uploaded." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }




        [HttpPost]
        public ActionResult SubmitAssets(EnterItemsModel mod)
        {
            Console.WriteLine($"ReqFormID: {mod.ReqFormID}, AssestCode: {mod.AssestCode}");
            try
            {
                string sql = "INSERT INTO [TechAssets].[dbo].[Hardware_Assest_Items] " +
                             "([ReqFormID], [ReqCode], [Item], [Quantity], [sysDate], [sysUser], [Status]) " +
                             "VALUES (@ReqFormID, @ReqCode, @Item, @Quantity, @sysDate, @sysUser,1)";
                using (SqlConnection conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
                {
                    conn_uat.Open();
                    using (SqlCommand sqlcmd = new SqlCommand(sql, conn_uat))
                    {
                        _ = sqlcmd.Parameters.AddWithValue("@ReqFormID", mod.ReqFormID);
                        _ = sqlcmd.Parameters.AddWithValue("@ReqCode", mod.AssestCode);
                        _ = sqlcmd.Parameters.AddWithValue("@Item", mod.Assest);
                        _ = sqlcmd.Parameters.AddWithValue("@Quantity", mod.quantity);
                        _ = sqlcmd.Parameters.AddWithValue("@sysDate", DateTime.Now);
                        _ = sqlcmd.Parameters.AddWithValue("@sysUser", Constants.sysUser);


                        _ = sqlcmd.ExecuteNonQuery();
                    }
                }
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }



        [HttpGet]
        public JsonResult GetNewAssetID(string reqFormID)
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["connx"].ConnectionString;
                string newAssetID;

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // SQL Query to Generate Asset ID
                    string sql = @"
                DECLARE @AssetCount INT;
                DECLARE @NewAssetID NVARCHAR(100);

                SELECT @AssetCount = COUNT(Id) + 1
                FROM [TechAssets].[dbo].[Hardware_Assest_Items]
                WHERE ReqFormID = @ReqFormID;

                SET @NewAssetID = CONCAT(@ReqFormID, '-', CAST(@AssetCount AS NVARCHAR(10)));

                SELECT @NewAssetID AS NewAssetID;";

                    using (SqlCommand command = new SqlCommand(sql, conn))
                    {
                        _ = command.Parameters.AddWithValue("@ReqFormID", reqFormID);

                        // Execute the query and get the result
                        newAssetID = command.ExecuteScalar()?.ToString();
                    }
                }

                return Json(new { success = true, assetID = newAssetID }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }



        public ActionResult DeleteRequestForm(string ReqFormID)
        {
            string sql1 = "UPDATE [TechAssets].[dbo].[Hardware_Master_AssestReq] SET [Status] = '2' WHERE ReqFormID = @ReqFormID";
            string sql2 = "UPDATE [TechAssets].[dbo].[Hardware_Assest_Items] SET [Status] = '2' WHERE ReqFormID = @ReqFormID";

            try
            {
                using (SqlConnection conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
                {
                    conn_uat.Open();

                    using (SqlCommand cmd1 = new SqlCommand(sql1, conn_uat))
                    {
                        _ = cmd1.Parameters.AddWithValue("@ReqFormID", ReqFormID);
                        _ = cmd1.ExecuteNonQuery();
                    }

                    using (SqlCommand cmd2 = new SqlCommand(sql2, conn_uat))
                    {
                        _ = cmd2.Parameters.AddWithValue("@ReqFormID", ReqFormID);
                        _ = cmd2.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the error and handle it appropriately
                Console.WriteLine($"Error: {ex.Message}");
                // Optionally, redirect to an error page
            }

            return RedirectToAction("CreateAssestReqForm", "Hardware");
        }



        public ActionResult RequestFormView(string ReqFormID)
        {
            EnterItemsModel mod = new EnterItemsModel
            {
                ReqFormID = ReqFormID
            };

            string sql = "SELECT HMA.[ReqFormID], HMA.[RDivision], CONVERT(VARCHAR, HMA.[RDate], 23) AS RDate, HAI.[ReqCode], HIT.[Item_Type], HAI.[Quantity] FROM [TechAssets].[dbo].[Hardware_Master_AssestReq] HMA JOIN [TechAssets].[dbo].[Hardware_Assest_Items] HAI ON HMA.[ReqFormID] = HAI.[ReqFormID] JOIN [TechAssets].[dbo].[Hardware_Item_Types] HIT ON HAI.[Item] = HIT.[Item_Type_Code] WHERE HMA.[ReqFormID] = '" + mod.ReqFormID + "'";

            mod.ARForm = HardwareModule.Get_Any_DT(sql);
            return View(mod);
        }



        public ActionResult DevicesInInvoice(string Invoice_Ref)
        {
            string sql = "SELECT it.[Item_Type], d.[IT_No], d.[Serial_No], d.[INV_No], d.[QR], CONVERT(VARCHAR, d.[Installation_Date], 23) AS Installation_Date, div_p.[division] AS Purchased_Division, div_c.[division] AS Current_Location, d.[Current_LocationID], d.[Purchased_DivisionID] FROM [TechAssets].[dbo].[Hardware_Devices] d JOIN [TechAssets].[dbo].[Hardware_Inovice_Items] i ON d.[DescriptionCode] = i.[DescriptionCode] INNER JOIN [TechAssets].[dbo].[Hardware_Item_Types] it ON i.[Item_Type] = it.[Item_Type_Code] INNER JOIN [TechAssets].[dbo].[Hardware_DeviceStatus] ds ON d.[Current_Status] = ds.[StatusID] INNER JOIN [EMMSDB].[dbo].[Staff_Division] div_p ON d.[Purchased_DivisionID] = div_p.[div_index] INNER JOIN [EMMSDB].[dbo].[Staff_Division] div_c ON d.[Current_LocationID] = div_c.[div_index] WHERE d.[Invoice_Reference] = '" + Invoice_Ref + "'";
            EnterItemsModel Inmod = new EnterItemsModel
            {
                DevicesinInvoices = HardwareModule.Get_Any_DT(sql)
            };

            return View(Inmod);


        }

        public ActionResult PartsView(string Invoice_Ref, string Invoice_No, string rItem)
        {
            string sql = "SELECT [ItemType],[IT_No],[Serial_No],[INV_No],[QR],[Installation_Device],CONVERT(VARCHAR,[Installation_Date], 23) FROM [TechAssets].[dbo].[Hardware_Stores] WHERE [Invoice_Reference] = '" + Invoice_Ref + "'";
            EnterItemsModel Inmod = new EnterItemsModel
            {
                Parts = HardwareModule.Get_Any_DT(sql)
            };

            ViewBag.InvoiceRef = Invoice_Ref;
            ViewBag.InvoiceNo = Invoice_No;
            ViewBag.RItem = rItem;

            return View(Inmod);


        }


        public ActionResult UpdateAStatus(string reqformid, string StatusID, string AssestRemarks,string AssestDate1, string AssestRemaks1, string AssestDate2, string AssestRemaks2, string AssestDate3, string AssestRemaks3, string AssestDate4, string AssestRemaks4, string AssestDate5, string AssestRemaks5)
        {
            string sql = "UPDATE [TechAssets].[dbo].[Hardware_Master_AssestReq] " +
                         "SET StatusID = @StatusID, AssestRemarks = @AssestRemarks, Date1=@Date1 ,Remaks1=@Remaks1, Date2=@Date2 ,Remaks2=@Remaks2, Date3=@Date3 ,Remaks3=@Remaks3, Date4=@Date4 ,Remaks4=@Remaks4, Date5=@Date5 ,Remaks5=@Remaks5 " +
                         "WHERE ReqFormID = @ReqFormID";

            using (SqlConnection conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
            {
                conn_uat.Open();
                SqlCommand sqlcmd = new SqlCommand(sql, conn_uat);

                _ = sqlcmd.Parameters.AddWithValue("@StatusID", StatusID);
                _ = sqlcmd.Parameters.AddWithValue("@AssestRemarks", AssestRemarks);
                _ = sqlcmd.Parameters.AddWithValue("@ReqFormID", reqformid);

                _ = sqlcmd.Parameters.AddWithValue("@Date1", AssestDate1 ?? (object)DBNull.Value);
                _ = sqlcmd.Parameters.AddWithValue("@Remaks1", AssestRemaks1 ?? (object)DBNull.Value);

                _ = sqlcmd.Parameters.AddWithValue("@Date2", AssestDate2 ?? (object)DBNull.Value);
                _ = sqlcmd.Parameters.AddWithValue("@Remaks2", AssestRemaks2 ?? (object)DBNull.Value);

                _ = sqlcmd.Parameters.AddWithValue("@Date3", AssestDate3 ?? (object)DBNull.Value);
                _ = sqlcmd.Parameters.AddWithValue("@Remaks3", AssestRemaks3 ?? (object)DBNull.Value);

                _ = sqlcmd.Parameters.AddWithValue("@Date4", AssestDate4 ?? (object)DBNull.Value);
                _ = sqlcmd.Parameters.AddWithValue("@Remaks4", AssestRemaks4 ?? (object)DBNull.Value);

                _ = sqlcmd.Parameters.AddWithValue("@Date5", AssestDate5 ?? (object)DBNull.Value);
                _ = sqlcmd.Parameters.AddWithValue("@Remaks5", AssestRemaks5 ?? (object)DBNull.Value);
                _ = sqlcmd.ExecuteNonQuery();
                conn_uat.Close();
            }

            return RedirectToAction("CreateAssestReqForm", "Hardware");
        }


        [HttpPost]
        public ActionResult OutsideRepair(string itno, string dutyID, string Rreason)
        {
            try
            {
                string sqlUpdateDutyAssign = "UPDATE [TechAssets].[dbo].[Hardware_DutyAssign] SET DutyStatus = @DutyStatus, OutsideRReqDate = @OutsideRReqDate WHERE DutyID = @DutyID";
                string sqlInsertRepair = "INSERT INTO [TechAssets].[dbo].[Hardware_RepairOut] (IT_No, DutyID, ROutsideStatus, OutsideRReqDate, ORReson) VALUES (@IT_No, @DutyID, @ROutsideStatus, @OutsideRReqDate, @ORReson)";
                string sqlUpdateDevices = "UPDATE [TechAssets].[dbo].[Hardware_Devices] SET Current_Status = @Current_Status WHERE IT_No = @IT_No";

                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
                {
                    conn.Open();

                    using (SqlCommand cmdDutyAssign = new SqlCommand(sqlUpdateDutyAssign, conn))
                    {
                        _ = cmdDutyAssign.Parameters.AddWithValue("@DutyID", dutyID);
                        _ = cmdDutyAssign.Parameters.AddWithValue("@DutyStatus", 37);
                        _ = cmdDutyAssign.Parameters.AddWithValue("@OutsideRReqDate", DateTime.Now);
                        _ = cmdDutyAssign.ExecuteNonQuery();
                    }

                    using (SqlCommand cmdRepair = new SqlCommand(sqlInsertRepair, conn))
                    {
                        _ = cmdRepair.Parameters.AddWithValue("@IT_No", itno);
                        _ = cmdRepair.Parameters.AddWithValue("@DutyID", dutyID);
                        _ = cmdRepair.Parameters.AddWithValue("@ROutsideStatus", 37);
                        _ = cmdRepair.Parameters.AddWithValue("@ORReson", Rreason);
                        _ = cmdRepair.Parameters.AddWithValue("OutsideRReqDate", DateTime.Now);
                        _ = cmdRepair.ExecuteNonQuery();
                    }

                    using (SqlCommand cmdDevices = new SqlCommand(sqlUpdateDevices, conn))
                    {
                        _ = cmdDevices.Parameters.AddWithValue("@IT_No", itno);
                        _ = cmdDevices.Parameters.AddWithValue("@Current_Status", 37);
                        _ = cmdDevices.ExecuteNonQuery();
                    }

                }

                return Json(new { success = true, message = "Successfully submitted!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.ToString() });
            }
        }


        [HttpPost]
        public ActionResult OutsideRepairAgain(string itno, string dutyID, string Rreason, string id)
        {
            try
            {
                string sqlUpdateDutyAssign = "UPDATE [TechAssets].[dbo].[Hardware_DutyAssign] SET DutyStatus = @DutyStatus, OutsideRReqDate = @OutsideRReqDate WHERE DutyID = @DutyID";
                string sqlInsertRepair = "INSERT INTO [TechAssets].[dbo].[Hardware_RepairOut] (IT_No, DutyID, ROutsideStatus, OutsideRReqDate, ORReson) VALUES (@IT_No, @DutyID, @ROutsideStatus, @OutsideRReqDate, @ORReson)";
                string sqlUpdateDevices = "UPDATE [TechAssets].[dbo].[Hardware_Devices] SET Current_Status = @Current_Status WHERE IT_No = @IT_No";

                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
                {
                    conn.Open();

                    using (SqlCommand cmdDutyAssign = new SqlCommand(sqlUpdateDutyAssign, conn))
                    {
                        _ = cmdDutyAssign.Parameters.AddWithValue("@DutyID", dutyID);
                        _ = cmdDutyAssign.Parameters.AddWithValue("@DutyStatus", 37);
                        _ = cmdDutyAssign.Parameters.AddWithValue("@OutsideRReqDate", DateTime.Now);
                        _ = cmdDutyAssign.ExecuteNonQuery();
                    }

                    using (SqlCommand cmdRepair = new SqlCommand(sqlInsertRepair, conn))
                    {
                        _ = cmdRepair.Parameters.AddWithValue("@IT_No", itno);
                        _ = cmdRepair.Parameters.AddWithValue("@DutyID", dutyID);
                        _ = cmdRepair.Parameters.AddWithValue("@ROutsideStatus", 37);
                        _ = cmdRepair.Parameters.AddWithValue("@ORReson", Rreason);
                        _ = cmdRepair.Parameters.AddWithValue("OutsideRReqDate", DateTime.Now);
                        _ = cmdRepair.ExecuteNonQuery();
                    }

                    using (SqlCommand cmdDevices = new SqlCommand(sqlUpdateDevices, conn))
                    {
                        _ = cmdDevices.Parameters.AddWithValue("@IT_No", itno);
                        _ = cmdDevices.Parameters.AddWithValue("@Current_Status", 37);
                        _ = cmdDevices.ExecuteNonQuery();
                    }

                }

                return Json(new { success = true, message = "Successfully submitted!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.ToString() });
            }
        }



        public ActionResult OutSideRepair(string DutyID)
        {
            string sql = "SELECT hr.DutyID, hr.IT_No, it.Item_Type, hd.INV_No, CONVERT(VARCHAR, hr.[OutsideRReqDate], 23) AS OutsideRReqDate, sd_loc.division AS Current_Location_Name, da.DutyAssignOfficer, da.DutyAssignOfficerSP, hr.id, hr.[ORReson] FROM [TechAssets].[dbo].[Hardware_RepairOut] hr LEFT JOIN [TechAssets].[dbo].[Hardware_Devices] hd ON hr.IT_No = hd.IT_No LEFT JOIN [TechAssets].[dbo].[Hardware_Inovice_Items] i ON hd.DescriptionCode = i.DescriptionCode LEFT JOIN [TechAssets].[dbo].[Hardware_Item_Types] it ON i.Item_Type = it.Item_Type_Code LEFT JOIN [EMMSDB].[dbo].[Staff_Division] sd_loc ON hd.Current_LocationID = sd_loc.div_index LEFT JOIN [TechAssets].[dbo].[Hardware_DutyAssign] da ON hr.DutyID = da.DutyID WHERE hr.ROutsideStatus = '37'";
            EnterItemsModel mod = new EnterItemsModel
            {
                OutsideRepair = HardwareModule.Get_Any_DT(sql),
                CompanySelectList = HardwareModule.GetRepairCompaniesSelectList(),
                ORBulkNo = HardwareModule.GetBulkNo()
            };

            return View(mod);
        }


        public ActionResult GatePass(EnterItemsModel mod, HttpPostedFileBase NICFile)
        {
            try
            {
                string URL = null;
                HttpPostedFileBase document = NICFile;

                if (NICFile != null)
                {
                    string fileExtension = Path.GetExtension(NICFile.FileName)?.ToUpper();

                    if (fileExtension == ".JPG")
                    {
                        string uniqueFileName = $"{mod.ORBulkNo}.jpg";
                        string path = Path.Combine(Server.MapPath("~/Upload/GatePass/NIC"), uniqueFileName);
                        document.SaveAs(path);
                        URL = $"~/Upload/GatePass/NIC/{mod.ORBulkNo}.jpg";

                    }
                    else
                    {
                        return Json(new { success = false, error = "Invalid file type. Only JPG files are allowed." });
                    }
                }


                if (mod.SentDate == null)
                {
                    mod.Time = DateTime.Now.ToString("HH:mm:ss");
                }

                if (string.IsNullOrEmpty(mod.DOfficerName))
                {
                    mod.DOfficerName = "";
                }

                if (string.IsNullOrEmpty(mod.BulkRemarks))
                {
                    mod.BulkRemarks = "";
                }

                string sql = @"INSERT INTO [TechAssets].[dbo].[Hardware_GatePass] 
                       ([ORBulkNo], [SentDate], [CompanyID], [Time], [OfficerName], [SysUser], [SysDate], [Remarks], [NIC_URL], [Status]) 
                       VALUES (@ORBulkNo, @SentDate, @CompanyID, @Time, @OfficerName, @SysUser, @SysDate, @Remarks, @NIC_URL, @Status)";

                using (SqlConnection conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
                {
                    conn_uat.Open();
                    using (SqlCommand sqlCmd3 = new SqlCommand(sql, conn_uat))
                    {
                        _ = sqlCmd3.Parameters.AddWithValue("@ORBulkNo", mod.ORBulkNo);
                        _ = sqlCmd3.Parameters.AddWithValue("@SentDate", mod.SentDate ?? (object)DBNull.Value);
                        _ = sqlCmd3.Parameters.AddWithValue("@CompanyID", mod.CompanyID);
                        _ = sqlCmd3.Parameters.AddWithValue("@Time", mod.Time);
                        _ = sqlCmd3.Parameters.AddWithValue("@OfficerName", mod.DOfficerName);
                        _ = sqlCmd3.Parameters.AddWithValue("@SysUser", Constants.sysUser);
                        _ = sqlCmd3.Parameters.AddWithValue("@SysDate", DateTime.Now);
                        _ = sqlCmd3.Parameters.AddWithValue("@Remarks", mod.BulkRemarks);
                        _ = sqlCmd3.Parameters.AddWithValue("@NIC_URL", URL ?? DBNull.Value.ToString());
                        _ = sqlCmd3.Parameters.AddWithValue("@Status", 1);

                        _ = sqlCmd3.ExecuteNonQuery();
                    }
                }

                return Json(new { success = true, message = "Gatepass generated successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }









        public JsonResult GetORBulkNos()
        {
            List<string> orBulkNos = new List<string>();

            try
            {
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
                {
                    string query = "SELECT [ORBulkNo] FROM [TechAssets].[dbo].[Hardware_GatePass] WHERE [Status] = 1";

                    using (SqlCommand command = new SqlCommand(query, conn))
                    {
                        conn.Open();

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                orBulkNos.Add(reader["ORBulkNo"].ToString());
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }

            return Json(orBulkNos, JsonRequestBehavior.AllowGet);
        }



        [HttpPost]
        public ActionResult Addtogatepass(string itno, string oid, string dutyid, string bulkNo)
        {
            try
            {
                string sqlUpdateDutyAssign = "UPDATE [TechAssets].[dbo].[Hardware_DutyAssign] SET DutyStatus = @DutyStatus WHERE DutyID = @DutyID";
                string sqlUpdateRepair = "UPDATE [TechAssets].[dbo].[Hardware_RepairOut] SET ROutsideStatus = @ROutsideStatus , GatepassNo = @GatepassNo WHERE id = @oid";
                string UpdateDevices = "UPDATE [TechAssets].[dbo].[Hardware_Devices] SET [Current_Status] = @CurrentStatus WHERE [IT_No] = @ITNo";

                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
                {
                    conn.Open();

                    using (SqlCommand cmdDutyAssign = new SqlCommand(sqlUpdateDutyAssign, conn))
                    {
                        _ = cmdDutyAssign.Parameters.AddWithValue("@DutyID", dutyid);
                        _ = cmdDutyAssign.Parameters.AddWithValue("@DutyStatus", 50);
                        _ = cmdDutyAssign.ExecuteNonQuery();
                    }

                    using (SqlCommand cmdRepair = new SqlCommand(sqlUpdateRepair, conn))
                    {
                        _ = cmdRepair.Parameters.AddWithValue("@oid", oid);
                        _ = cmdRepair.Parameters.AddWithValue("@ROutsideStatus", 50);
                        _ = cmdRepair.Parameters.AddWithValue("GatepassNo", bulkNo);
                        _ = cmdRepair.ExecuteNonQuery();
                    }

                    using (SqlCommand cmdDevices1 = new SqlCommand(UpdateDevices, conn))
                    {
                        _ = cmdDevices1.Parameters.AddWithValue("@ITNo", itno);
                        _ = cmdDevices1.Parameters.AddWithValue("@CurrentStatus", 50);
                        _ = cmdDevices1.ExecuteNonQuery();
                    }

                }

                return Json(new { success = true, message = "Device Added successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }


        public ActionResult GetORDeviceRemaks(int id)
        {
            string sql = "SELECT DeviceRemaks FROM [TechAssets].[dbo].[Hardware_RepairOut] WHERE id = @id";
            string remark = null;

            using (SqlConnection conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
            {
                conn_uat.Open();
                SqlCommand sqlcmd = new SqlCommand(sql, conn_uat);
                _ = sqlcmd.Parameters.AddWithValue("@id", id);

                object result = sqlcmd.ExecuteScalar();
                if (result != null)
                {
                    remark = result.ToString();
                }

                conn_uat.Close();
            }

            return Json(new { DeviceRemaks = remark }, JsonRequestBehavior.AllowGet);
        }



        [HttpPost]
        public ActionResult ORUpdateRemarks(int Rid, string remarks)
        {
            try
            {
                string sql = "UPDATE [TechAssets].[dbo].[Hardware_RepairOut] SET DeviceRemaks = @DeviceRemaks WHERE id = @Rid";

                using (SqlConnection conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
                {
                    conn_uat.Open();
                    SqlCommand sqlcmd = new SqlCommand(sql, conn_uat);

                    _ = sqlcmd.Parameters.AddWithValue("@DeviceRemaks", remarks);
                    _ = sqlcmd.Parameters.AddWithValue("@Rid", Rid);

                    int rowsAffected = sqlcmd.ExecuteNonQuery();
                    conn_uat.Close();

                    return rowsAffected > 0
                        ? Json(new { success = true, message = "Remarks added successfully!" })
                        : (ActionResult)Json(new { success = false, message = "No rows were updated." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }



        public ActionResult InOutSideRepair()
        {
            string sql = "SELECT it.[Item_Type], RO.[IT_No], hd.[Serial_No], hd.[INV_No], hd.[QR], CONVERT(VARCHAR,RO.[OutsideRReqDate], 23), RO.[GatepassNo], gp.[CompanyID], CONVERT(VARCHAR,gp.[SentDate], 23), RO.[DeviceRemaks], sd.[division], RO.[EstimateRecDate], RO.[Solution], RO.[RepairPrice], RO.[Warranty], RO.[EstimateRemaks], RO.[ApprovelLetterDate], RO.[ApproveStatus], RO.[RejectReson], RO.[ApproveInformDate], RO.[RepairCompleteDate], RO.[RepairRemaks], RO.[RepairOrNot], RO.[SysUser], RO.[SysDate], RO.[InvoiceNo], RO.[InvoiceDate], RO.[PayVoucherDate], RO.[VoucherNo], RO.[id], RO.[DutyID], RO.[ROutsideStatus] FROM [TechAssets].[dbo].[Hardware_RepairOut] AS RO LEFT JOIN [TechAssets].[dbo].[Hardware_GatePass] gp ON RO.GatepassNo = gp.ORBulkNo LEFT JOIN [TechAssets].[dbo].[Hardware_Devices] hd ON RO.IT_No = hd.IT_No LEFT JOIN [TechAssets].[dbo].[Hardware_Inovice_Items] i ON hd.DescriptionCode = i.DescriptionCode LEFT JOIN [EMMSDB].[dbo].[Staff_Division] sd ON hd.[Current_LocationID] = sd.[div_index] LEFT JOIN [TechAssets].[dbo].[Hardware_Item_Types] it ON i.Item_Type = it.Item_Type_Code WHERE ROutsideStatus = 50;";
            EnterItemsModel mod = new EnterItemsModel
            {
                Inoutside = HardwareModule.Get_Any_DT(sql)
            };

            return View(mod);
        }

        public ActionResult ViewGatePass(string status)
        {
            string sql = null;

            if (status == "All")
            {
                sql = "SELECT [id], [ORBulkNo], CONVERT(VARCHAR,[SentDate], 23), [CompanyID], [Time], [OfficerName], [SysUser], [SysDate], [Remarks], [NIC_URL], [Status] FROM [TechAssets].[dbo].[Hardware_GatePass]";
            }
            else if (status == status)
            {
                sql = "SELECT [id], [ORBulkNo], CONVERT(VARCHAR,[SentDate], 23), [CompanyID], [Time], [OfficerName], [SysUser], [SysDate], [Remarks], [NIC_URL], [Status] FROM [TechAssets].[dbo].[Hardware_GatePass] WHERE Status = '" + status + "'";
            }
            EnterItemsModel mod = new EnterItemsModel
            {
                gatepassTBL = HardwareModule.Get_Any_DT(sql),
                GPStatus = status
            };
            ViewBag.GPCurrentStatus = status;
            return View(mod);
        }



        [HttpPost]
        public ActionResult GatepassStatus(string id)
        {
            try
            {
                string UpdateGatepass = "UPDATE [TechAssets].[dbo].[Hardware_GatePass] SET Status = @Status WHERE id = @id";

                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
                {
                    conn.Open();

                    using (SqlCommand cmdgatepass = new SqlCommand(UpdateGatepass, conn))
                    {
                        _ = cmdgatepass.Parameters.AddWithValue("@id", id);
                        _ = cmdgatepass.Parameters.AddWithValue("@Status", 0);
                        _ = cmdgatepass.ExecuteNonQuery();
                    }

                }

                return Json(new { success = true, message = "Successfully submitted!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.ToString() });
            }
        }

        [HttpPost]
        public ActionResult GatepassCompleteProcess(string id)
        {
            try
            {
                string UpdateGatepass = "UPDATE [TechAssets].[dbo].[Hardware_GatePass] SET Status = @Status WHERE id = @id";

                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
                {
                    conn.Open();

                    using (SqlCommand cmdgatepass = new SqlCommand(UpdateGatepass, conn))
                    {
                        _ = cmdgatepass.Parameters.AddWithValue("@id", id);
                        _ = cmdgatepass.Parameters.AddWithValue("@Status", 3);
                        _ = cmdgatepass.ExecuteNonQuery();
                    }

                }

                return Json(new { success = true, message = "Successfully submitted!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.ToString() });
            }
        }


        [HttpPost]
        public ActionResult DoneProcess(string id)
        {
            try
            {
                string UpdateGatepass = "UPDATE [TechAssets].[dbo].[Hardware_GatePass] SET Status = @Status WHERE id = @id";

                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
                {
                    conn.Open();

                    using (SqlCommand cmdgatepass = new SqlCommand(UpdateGatepass, conn))
                    {
                        _ = cmdgatepass.Parameters.AddWithValue("@id", id);
                        _ = cmdgatepass.Parameters.AddWithValue("@Status", 4);
                        _ = cmdgatepass.ExecuteNonQuery();
                    }

                }

                return Json(new { success = true, message = "Successfully submitted!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.ToString() });
            }
        }





        [HttpPost]
        public JsonResult UpdateEstimate(EnterItemsModel model)
        {
            if (model == null)
            {
                return Json(new { success = false, message = "Invalid data received." });
            }

            try
            {
                string sql = "UPDATE [TechAssets].[dbo].[Hardware_RepairOut] SET EstimateRecDate = @Edate, Solution = @Esolution, RepairPrice = @Eprice, Warranty = @Ewarranty, EstimateRemaks = @ERemarks WHERE id = @Eid";

                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
                {
                    conn.Open();

                    using (SqlCommand cmdEstimate = new SqlCommand(sql, conn))
                    {
                        _ = cmdEstimate.Parameters.AddWithValue("@Eid", model.Eid);
                        _ = cmdEstimate.Parameters.AddWithValue("@Edate", model.Edate);
                        _ = cmdEstimate.Parameters.AddWithValue("@Esolution", model.Esolution);
                        _ = cmdEstimate.Parameters.AddWithValue("@Eprice", model.Eprice);
                        _ = cmdEstimate.Parameters.AddWithValue("@Ewarranty", model.Ewarranty);
                        _ = cmdEstimate.Parameters.AddWithValue("@ERemarks", model.ERemarks ?? (object)DBNull.Value);

                        _ = cmdEstimate.ExecuteNonQuery();
                    }
                }

                return Json(new { success = true, message = "Estimate updated successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }


        [HttpPost]
        public JsonResult UpdateApproval(EnterItemsModel model)
        {
            if (model == null)
            {
                return Json(new { success = false, message = "Invalid data received." });
            }

            try
            {
                string sql = "UPDATE [TechAssets].[dbo].[Hardware_RepairOut] SET ApprovelLetterDate = @ApprovelLetterDate, ApproveStatus = @ApproveStatus, RejectReson = @RejectReson, ApproveInformDate = @ApproveInformDate WHERE id = @Aid";

                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
                {
                    conn.Open();

                    using (SqlCommand cmdApproval = new SqlCommand(sql, conn))
                    {
                        _ = cmdApproval.Parameters.AddWithValue("@Aid", model.Aid);
                        _ = cmdApproval.Parameters.AddWithValue("@ApprovelLetterDate", model.Aldate);
                        _ = cmdApproval.Parameters.AddWithValue("@ApproveStatus", model.Astatus);
                        _ = cmdApproval.Parameters.AddWithValue("@RejectReson", model.Arreson ?? (object)DBNull.Value);
                        _ = cmdApproval.Parameters.AddWithValue("@ApproveInformDate", model.Ainformd ?? (object)DBNull.Value);  

                        _ = cmdApproval.ExecuteNonQuery();
                    }
                }

                return Json(new { success = true, message = "Approval updated successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }



        [HttpPost]
        public JsonResult UpdateReceive(EnterItemsModel model)
        {
            if (model == null)
            {
                return Json(new { success = false, message = "Invalid data received." });
            }

            try
            {
                string sqlRepaired = "UPDATE[TechAssets].[dbo].[Hardware_RepairOut] SET RepairCompleteDate = @RepairCompleteDate, RepairRemaks = @RepairRemaks, RepairOrNot = @RepairOrNot, SysUser = @SysUser, SysDate = @SysDate , ROutsideStatus = @ROutsideStatus WHERE id = @Rid";

                string sqlUpdateDutyAssign = "UPDATE [TechAssets].[dbo].[Hardware_DutyAssign] SET DutyStatus =@DutyStatus , SysRCompleteDate = @SysRCompleteDate WHERE DutyID = @DutyID";

                string UpdateDevices = "UPDATE [TechAssets].[dbo].[Hardware_Devices] SET [Current_Status] = @CurrentStatus WHERE [IT_No] = @Ritno";



                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
                {
                    conn.Open();

                    if (model.Rrornot == "Repaired")
                    {
                        using (SqlCommand cmd = new SqlCommand(sqlRepaired, conn))
                        {
                            _ = cmd.Parameters.AddWithValue("@Rid", model.Rid);
                            _ = cmd.Parameters.AddWithValue("@RepairCompleteDate", model.Rcdate);
                            _ = cmd.Parameters.AddWithValue("@RepairRemaks", model.Rremaks ?? (object)DBNull.Value);
                            _ = cmd.Parameters.AddWithValue("@RepairOrNot", model.Rrornot);
                            _ = cmd.Parameters.AddWithValue("@SysUser", Constants.sysUser);
                            _ = cmd.Parameters.AddWithValue("@SysDate", DateTime.Now);
                            _ = cmd.Parameters.AddWithValue("@ROutsideStatus", 36);
                            _ = cmd.ExecuteNonQuery();
                        }

                        using (SqlCommand cmdDutyAssign = new SqlCommand(sqlUpdateDutyAssign, conn))
                        {
                            _ = cmdDutyAssign.Parameters.AddWithValue("@DutyID", model.Rdutyid);
                            _ = cmdDutyAssign.Parameters.AddWithValue("@DutyStatus", 36);
                            _ = cmdDutyAssign.Parameters.AddWithValue("@SysRCompleteDate", model.Rcdate);
                            _ = cmdDutyAssign.ExecuteNonQuery();
                        }

                        using (SqlCommand cmd1 = new SqlCommand(UpdateDevices, conn))
                        {
                            _ = cmd1.Parameters.AddWithValue("@Ritno", model.Ritno);
                            _ = cmd1.Parameters.AddWithValue("@CurrentStatus", 36);
                            _ = cmd1.ExecuteNonQuery();
                        }

                    }
                    else
                    {
                        using (SqlCommand cmd = new SqlCommand(sqlRepaired, conn))
                        {
                            _ = cmd.Parameters.AddWithValue("@Rid", model.Rid);
                            _ = cmd.Parameters.AddWithValue("@RepairCompleteDate", model.Rcdate);
                            _ = cmd.Parameters.AddWithValue("@RepairRemaks", model.Rremaks ?? (object)DBNull.Value);
                            _ = cmd.Parameters.AddWithValue("@RepairOrNot", model.Rrornot);
                            _ = cmd.Parameters.AddWithValue("@SysUser", Constants.sysUser);
                            _ = cmd.Parameters.AddWithValue("@SysDate", DateTime.Now);
                            _ = cmd.Parameters.AddWithValue("@ROutsideStatus", 38);
                            _ = cmd.ExecuteNonQuery();
                        }

                        using (SqlCommand cmdDutyAssign = new SqlCommand(sqlUpdateDutyAssign, conn))
                        {
                            _ = cmdDutyAssign.Parameters.AddWithValue("@DutyID", model.Rdutyid);
                            _ = cmdDutyAssign.Parameters.AddWithValue("@DutyStatus", 35);
                            _ = cmdDutyAssign.Parameters.AddWithValue("@SysRCompleteDate", model.Rcdate);
                            _ = cmdDutyAssign.ExecuteNonQuery();
                        }

                        using (SqlCommand cmd1 = new SqlCommand(UpdateDevices, conn))
                        {
                            _ = cmd1.Parameters.AddWithValue("@Ritno", model.Ritno);
                            _ = cmd1.Parameters.AddWithValue("@CurrentStatus", 35);
                            _ = cmd1.ExecuteNonQuery();
                        }
                    }
                }

                return Json(new { success = true, message = "Received details updated successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }





        [HttpPost]
        public JsonResult UpdatePayment(EnterItemsModel model)
        {
            if (model == null)
            {
                return Json(new { success = false, message = "Invalid data received." });
            }

            try
            {
                string sql = "UPDATE [TechAssets].[dbo].[Hardware_RepairOut] SET InvoiceDate = @InvoiceDate, InvoiceNo = @InvoiceNo, PayVoucherDate = @PayVoucherDate, VoucherNo = @VoucherNo WHERE id = @Pid";

                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
                {
                    conn.Open();

                    using (SqlCommand cmdApproval = new SqlCommand(sql, conn))
                    {
                        _ = cmdApproval.Parameters.AddWithValue("@Pid", model.Pid);
                        _ = cmdApproval.Parameters.AddWithValue("@InvoiceDate", model.Pinvdate);
                        _ = cmdApproval.Parameters.AddWithValue("@InvoiceNo", model.Pinvno);
                        _ = cmdApproval.Parameters.AddWithValue("@PayVoucherDate", model.Pvoudate ?? (object)DBNull.Value);
                        _ = cmdApproval.Parameters.AddWithValue("@VoucherNo", model.Pvouno ?? (object)DBNull.Value);

                        _ = cmdApproval.ExecuteNonQuery();
                    }
                }

                return Json(new { success = true, message = "Updated successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }


        public ActionResult RequestDisposeFromOut(string itno, string dutyID, string ORID, string reason)
        {
            try
            {

                string sql1 = "INSERT INTO [TechAssets].[dbo].[Hardware_Dispose] ([IT_No],[DutyID],[sysReqUser],[sysReqDate],[DisposeStatus],[DisposeReason]) VALUES (@IT_No, @DutyID, @sysReqUser, @sysReqDate, @DisposeStatus, @DisposeReason)";
                string sql3 = "UPDATE [TechAssets].[dbo].[Hardware_DutyAssign] SET [DutyStatus] ='6' WHERE DutyID = @DutyID";
                string sql4 = "UPDATE [TechAssets].[dbo].[Hardware_RepairOut] SET [ROutsideStatus] ='6' WHERE id = @ORID";
                string sql2 = "UPDATE [TechAssets].[dbo].[Hardware_Devices] SET Current_Status='6' WHERE IT_No = @IT_No";

                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
                {
                    conn.Open();


                    using (SqlCommand cmdInsert = new SqlCommand(sql1, conn))
                    {
                        _ = cmdInsert.Parameters.AddWithValue("@IT_No", itno);
                        _ = cmdInsert.Parameters.AddWithValue("@DutyID", dutyID);
                        _ = cmdInsert.Parameters.AddWithValue("@DisposeReason", reason);
                        _ = cmdInsert.Parameters.AddWithValue("@sysReqUser", Constants.sysUser);
                        _ = cmdInsert.Parameters.AddWithValue("@sysReqDate", DateTime.Now);
                        _ = cmdInsert.Parameters.AddWithValue("@DisposeStatus", 6);
                        _ = cmdInsert.ExecuteNonQuery();
                    }

                    using (SqlCommand cmdUpdate1 = new SqlCommand(sql3, conn))
                    {
                        _ = cmdUpdate1.Parameters.AddWithValue("@DutyID", dutyID);
                        _ = cmdUpdate1.ExecuteNonQuery();
                    }

                    using (SqlCommand cmdUpdate4 = new SqlCommand(sql4, conn))
                    {
                        _ = cmdUpdate4.Parameters.AddWithValue("@ORID", ORID);
                        _ = cmdUpdate4.ExecuteNonQuery();
                    }

                    using (SqlCommand cmdUpdate = new SqlCommand(sql2, conn))
                    {
                        _ = cmdUpdate.Parameters.AddWithValue("@IT_No", itno);
                        _ = cmdUpdate.ExecuteNonQuery();
                    }
                }

                return Json(new { success = true, message = "Successfully submitted!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }



        [HttpPost]
        public ActionResult DisposeTakenAway(string iddis)
        {
            try
            {
                string sql1 = "UPDATE [TechAssets].[dbo].[Hardware_DisposeBulk] SET [DBulkStatus] = @DBulkStatus, [SysTakenAUser] = @SysTakenAUser, [SysTakenADate] = @SysTakenADate WHERE [DisposeBulkID] = @DisposeBulkID";
                string sql2 = "UPDATE [TechAssets].[dbo].[Hardware_Devices] SET [activeStatus] = @activeStatus , [Current_Status]=@Current_Status WHERE [DisposeBulkNo] = @DisposeBulkNo";

                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
                {
                    conn.Open();

                    using (SqlCommand cmdUpdateDis = new SqlCommand(sql1, conn))
                    {
                        _ = cmdUpdateDis.Parameters.AddWithValue("@DBulkStatus", 0);
                        _ = cmdUpdateDis.Parameters.AddWithValue("@DisposeBulkID", iddis);
                        _ = cmdUpdateDis.Parameters.AddWithValue("@SysTakenAUser", Constants.sysUser);
                        _ = cmdUpdateDis.Parameters.AddWithValue("@SysTakenADate", DateTime.Now);
                        _ = cmdUpdateDis.ExecuteNonQuery();
                    }

                    using (SqlCommand cmdUpdateDevice = new SqlCommand(sql2, conn))
                    {
                        _ = cmdUpdateDevice.Parameters.AddWithValue("@activeStatus", 0);
                        _ = cmdUpdateDevice.Parameters.AddWithValue("@Current_Status", 55);
                        _ = cmdUpdateDevice.Parameters.AddWithValue("@DisposeBulkNo", iddis);
                        _ = cmdUpdateDevice.ExecuteNonQuery();
                    }
                }

                return Json(new { success = true, message = "Successfully submitted!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }





        [HttpPost]
        public ActionResult UploadDocumentOutRepair(HttpPostedFileBase file, string id)
        {
            if (file != null && file.ContentLength > 0)
            {
                string fileName = id + Path.GetExtension(file.FileName);
                string filePath = Path.Combine(Server.MapPath("~/Upload/GatePass/Documents"), fileName);

                file.SaveAs(filePath);

                return Json(new { success = true, message = "File uploaded successfully!" });
            }
            else
            {
                return Json(new { success = false, message = "No file selected." });
            }
        }



        public ActionResult FullViewGatePass(string gatepassid, string company, string sdate)
        {
            string sql = "SELECT it.[Item_Type], RO.[IT_No], hd.[Serial_No], hd.[INV_No], hd.[QR], CONVERT(VARCHAR,RO.[OutsideRReqDate], 23), RO.[GatepassNo], gp.[CompanyID], CONVERT(VARCHAR,gp.[SentDate], 23), RO.[DeviceRemaks], sd.[division], RO.[EstimateRecDate], RO.[Solution], RO.[RepairPrice], RO.[Warranty], RO.[EstimateRemaks], RO.[ApprovelLetterDate], RO.[ApproveStatus], RO.[RejectReson], RO.[ApproveInformDate], CONVERT(VARCHAR,RO.[RepairCompleteDate], 23), RO.[RepairRemaks], RO.[RepairOrNot], RO.[SysUser], RO.[SysDate], RO.[InvoiceNo], RO.[InvoiceDate], RO.[PayVoucherDate], RO.[VoucherNo], RO.[id], RO.[DutyID], RO.[ROutsideStatus] FROM [TechAssets].[dbo].[Hardware_RepairOut] AS RO LEFT JOIN [TechAssets].[dbo].[Hardware_GatePass] gp ON RO.GatepassNo = gp.ORBulkNo LEFT JOIN [TechAssets].[dbo].[Hardware_Devices] hd ON RO.IT_No = hd.IT_No LEFT JOIN [TechAssets].[dbo].[Hardware_Inovice_Items] i ON hd.DescriptionCode = i.DescriptionCode LEFT JOIN [EMMSDB].[dbo].[Staff_Division] sd ON hd.[Current_LocationID] = sd.[div_index] LEFT JOIN [TechAssets].[dbo].[Hardware_Item_Types] it ON i.Item_Type = it.Item_Type_Code WHERE ro.GatepassNo = '" + gatepassid + "'";
            EnterItemsModel mod = new EnterItemsModel
            {
                DinGatePass = HardwareModule.Get_Any_DT(sql),
                GatepassNo = gatepassid,
                RCompanyName = company,
                RSentDate = sdate
            };

            return View(mod);
        }



        public ActionResult DevicesInIT()
        {
            string sql = "SELECT it.Item_Type, d.[Invoice_Reference], d.[IT_No], d.[Serial_No], d.[INV_No], d.[QR], d.Current_Status, ds.[Status] AS Current_Status_Text, d.[activeStatus], sd.[division] AS Purchased_Division_Name, sd_loc.[division] AS Current_Location_Name FROM [TechAssets].[dbo].[Hardware_Devices] d JOIN [TechAssets].[dbo].[Hardware_Inovice_Items] i ON d.[DescriptionCode] = i.[DescriptionCode] INNER JOIN [TechAssets].[dbo].[Hardware_Item_Types] it ON i.Item_Type = it.Item_Type_Code INNER JOIN [TechAssets].[dbo].[Hardware_DeviceStatus] ds ON d.[Current_Status] = ds.[StatusID] LEFT JOIN [EMMSDB].[dbo].[Staff_Division] sd ON d.[Purchased_DivisionID] = sd.[div_index] LEFT JOIN [EMMSDB].[dbo].[Staff_Division] sd_loc ON d.[Current_LocationID] = sd_loc.[div_index] WHERE d.[activeStatus] = 1 AND d.[Current_Status] = '2'";

            EnterItemsModel Inmod = new EnterItemsModel
            {
                InIT = HardwareModule.Get_Any_DT(sql)
            };
            return View(Inmod);
        }

        public ActionResult EditDevice()
        {
            string sql = "SELECT it.Item_Type, d.[Invoice_Reference], d.[IT_No], d.[Serial_No], d.[INV_No], d.[QR], d.Current_Status, ds.[Status] AS Current_Status_Text, d.[activeStatus], sd.[division] AS Purchased_Division_Name, sd_loc.[division] AS Current_Location_Name, d.[id] FROM [TechAssets].[dbo].[Hardware_Devices] d JOIN [TechAssets].[dbo].[Hardware_Inovice_Items] i ON d.[DescriptionCode] = i.[DescriptionCode] INNER JOIN [TechAssets].[dbo].[Hardware_Item_Types] it ON i.Item_Type = it.Item_Type_Code INNER JOIN [TechAssets].[dbo].[Hardware_DeviceStatus] ds ON d.[Current_Status] = ds.[StatusID] LEFT JOIN [EMMSDB].[dbo].[Staff_Division] sd ON d.[Purchased_DivisionID] = sd.[div_index] LEFT JOIN [EMMSDB].[dbo].[Staff_Division] sd_loc ON d.[Current_LocationID] = sd_loc.[div_index] WHERE d.[activeStatus] = 1";

            EnterItemsModel Inmod = new EnterItemsModel
            {
                EditDTBL = HardwareModule.Get_Any_DT(sql)
            };
            return View(Inmod);
        }



        [HttpPost]
        public JsonResult UpdateEditDevice(EnterItemsModel model)
        {
            if (model == null)
            {
                return Json(new { success = false, message = "Invalid data received." });
            }

            try
            {
                string sql = "UPDATE [TechAssets].[dbo].[Hardware_Devices] " +
                             "SET IT_No = @ItNoNew, Serial_No = @SerialNo, INV_No = @InvNo, QR = @QrNo " +
                             "WHERE id = @Id";

                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", model.Zitno);
                        cmd.Parameters.AddWithValue("@SerialNo", model.Zserialno ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@InvNo", model.Zinvno ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@QrNo", model.Zqrno ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@ItNoNew", model.ZitnoNew);
                        cmd.ExecuteNonQuery();
                    }
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }




        public ActionResult Parts(string status)
        {
            string sql = null;

            if (status == "All")
            {
                sql = "SELECT [id], [Current_Status], [Invoice_Reference], [DescriptionCode], [Invoice_No], [ItemType], [IT_No], [Serial_No], [INV_No], [QR], [SsysUser], [SsysDate], [Installation_Device], [RepairID], [Installation_User], CONVERT(VARCHAR,[Installation_Date], 23), [StoresRemarks], [PartUser], [PartUserEMP] FROM [TechAssets].[dbo].[Hardware_Stores]";
            }
            else if (status == status)
            {
                sql = "SELECT [id], [Current_Status], [Invoice_Reference], [DescriptionCode], [Invoice_No], [ItemType], [IT_No], [Serial_No], [INV_No], [QR], [SsysUser], [SsysDate], [Installation_Device], [RepairID], [Installation_User], CONVERT(VARCHAR,[Installation_Date], 23), [StoresRemarks], [PartUser], [PartUserEMP] FROM [TechAssets].[dbo].[Hardware_Stores] WHERE Current_Status = '" + status + "'";
            }
            EnterItemsModel mod = new EnterItemsModel
            {
                PartsTBL = HardwareModule.Get_Any_DT(sql)
            };
            return View(mod);
        }


        [HttpPost]
        public JsonResult ValidateItNo(string itno, string status)
        {
            string sql = "SELECT COUNT(1) FROM [TechAssets].[dbo].[Hardware_Devices] WHERE [activeStatus] = 1 AND IT_No = @ITNo";

            int count = 0;
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    _ = cmd.Parameters.AddWithValue("@ITNo", itno);
                    count = Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
            return Json(new { exists = count > 0 });
        }


        public JsonResult GetCompanyNames()
        {
            List<string> companies = new List<string>();

            using (var con = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT [Company_Name] FROM [TechAssets].[dbo].[Hardware_Companies]", con))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        companies.Add(reader["Company_Name"].ToString());
                    }
                }
            }

            return Json(companies, JsonRequestBehavior.AllowGet);
        }



        public ActionResult AddToDevice(string id, string PInsDevice, string PInsDate, string PInsRemaks)
        {
            try
            {
                string sql3 = "UPDATE [TechAssets].[dbo].[Hardware_Stores] SET [Installation_Device] =@Installation_Device, [Installation_Date] = @Installation_Date , [StoresRemarks] = @StoresRemarks , [Installation_User] = @Installation_User , [Installation_DateSys] = @Installation_DateSys , [Current_Status] = @Current_Status WHERE id = @id";
                EnterItemsModel mod = new EnterItemsModel();
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
                {
                    conn.Open();

                    using (SqlCommand cmdInsert = new SqlCommand(sql3, conn))
                    {
                        _ = cmdInsert.Parameters.AddWithValue("@Installation_Device", PInsDevice);
                        _ = cmdInsert.Parameters.AddWithValue("@Installation_Date", PInsDate);
                        _ = cmdInsert.Parameters.AddWithValue("@StoresRemarks", PInsRemaks);
                        _ = cmdInsert.Parameters.AddWithValue("@Installation_User", Constants.sysUser);
                        _ = cmdInsert.Parameters.AddWithValue("@Installation_DateSys", DateTime.Now);
                        _ = cmdInsert.Parameters.AddWithValue("@Current_Status", 0);
                        _ = cmdInsert.Parameters.AddWithValue("@id", id);
                        _ = cmdInsert.ExecuteNonQuery();
                    }

                }

                return Json(new { success = true, message = "Successfully submitted!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }



        [HttpPost]
        public ActionResult AddWarranty(string Witno, string Wdutyid, string WcallDate, string Wcompany, string WcontactP, string Wremaks)
        {
            try
            {
                string sqlInsertWarranty = @"INSERT INTO [TechAssets].[dbo].[HardwareWarranty]
                       ([DutyID],[WarrantyCallDate],[CompanyName],[ContactPersonName],[WarrantyRemaks],[sysUser],[sysDate],[WarrantyStatus]) 
                       VALUES (@DutyID, @WarrantyCallDate, @CompanyName, @ContactPersonName, @WarrantyRemaks, @sysUser, @sysDate, @WarrantyStatus)";
                string sqlUpdateDutyAssign = "UPDATE [TechAssets].[dbo].[Hardware_DutyAssign] SET DutyStatus =@DutyStatus WHERE DutyID = @DutyID";
                string sqlUpdateDevices = "UPDATE [TechAssets].[dbo].[Hardware_Devices] SET Current_Status = @Current_Status WHERE IT_No = @IT_No";

                EnterItemsModel mod = new EnterItemsModel();
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
                {
                    conn.Open();

                    using (SqlCommand cmdWarrantyAssign = new SqlCommand(sqlInsertWarranty, conn))
                    {
                        _ = cmdWarrantyAssign.Parameters.AddWithValue("@DutyID", Wdutyid);
                        _ = cmdWarrantyAssign.Parameters.AddWithValue("@WarrantyCallDate", WcallDate);
                        _ = cmdWarrantyAssign.Parameters.AddWithValue("@CompanyName", Wcompany ?? (object)DBNull.Value);
                        _ = cmdWarrantyAssign.Parameters.AddWithValue("@ContactPersonName", WcontactP ?? (object)DBNull.Value);
                        _ = cmdWarrantyAssign.Parameters.AddWithValue("@WarrantyRemaks", Wremaks ?? (object)DBNull.Value);
                        _ = cmdWarrantyAssign.Parameters.AddWithValue("@WarrantyStatus",51);
                        _ = cmdWarrantyAssign.Parameters.AddWithValue("@sysUser", Constants.sysUser);
                        _ = cmdWarrantyAssign.Parameters.AddWithValue("@sysDate", DateTime.Now);
                        _ = cmdWarrantyAssign.ExecuteNonQuery();
                    }

                    using (SqlCommand cmdDuty = new SqlCommand(sqlUpdateDutyAssign, conn))
                    {
                        _ = cmdDuty.Parameters.AddWithValue("@DutyID", Wdutyid);
                        _ = cmdDuty.Parameters.AddWithValue("@DutyStatus", 51);
                        _ = cmdDuty.ExecuteNonQuery();
                    }

                    using (SqlCommand cmdDevices = new SqlCommand(sqlUpdateDevices, conn))
                    {
                        _ = cmdDevices.Parameters.AddWithValue("@IT_No", Witno);
                        _ = cmdDevices.Parameters.AddWithValue("@Current_Status", 51);
                        _ = cmdDevices.ExecuteNonQuery();
                    }
                }

                return Json(new { success = true, message = "Successfully submitted!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }



        public ActionResult UpdateSpec()
        {
            string sql = "SELECT it.Item_Type, d.[Invoice_Reference], d.[IT_No], d.[Serial_No], d.[INV_No], d.[QR], d.Current_Status, ds.[Status] AS Current_Status_Text, d.[activeStatus], hds.[Processor], hds.[ProcessorSpeed], hds.[Memory], hds.[Storage], hds.[StorageType], hds.[OperatingSystem], hds.[ScreenSize], hds.[PanelType], hds.[SpeRemarks] FROM [TechAssets].[dbo].[Hardware_Devices] d JOIN [TechAssets].[dbo].[Hardware_Inovice_Items] i ON d.[DescriptionCode] = i.[DescriptionCode] INNER JOIN [TechAssets].[dbo].[Hardware_Item_Types] it ON i.Item_Type = it.Item_Type_Code INNER JOIN [TechAssets].[dbo].[Hardware_DeviceStatus] ds ON d.[Current_Status] = ds.[StatusID] LEFT JOIN [EMMSDB].[dbo].[Staff_Division] sd ON d.[Purchased_DivisionID] = sd.[div_index] LEFT JOIN [EMMSDB].[dbo].[Staff_Division] sd_loc ON d.[Current_LocationID] = sd_loc.[div_index] LEFT JOIN [TechAssets].[dbo].[Hardware_DeviceSpecification] hds ON d.[IT_No] = hds.[IT_No] WHERE d.[activeStatus] = 1";

            EnterItemsModel Inmod = new EnterItemsModel
            {
                TBLUpdateSpec = HardwareModule.Get_Any_DT(sql)
            };
            return View(Inmod);
        }


        public ActionResult UpdateHistory()
        {
            string sql = "SELECT it.Item_Type, d.[IT_No], d.[Serial_No], d.[INV_No], d.[QR], d.[activeStatus], hds.[PurchaseDate], hds.[PurchasingPrice] FROM [TechAssets].[dbo].[Hardware_Devices] d JOIN [TechAssets].[dbo].[Hardware_Inovice_Items] i ON d.[DescriptionCode] = i.[DescriptionCode] INNER JOIN [TechAssets].[dbo].[Hardware_Item_Types] it ON i.Item_Type = it.Item_Type_Code LEFT JOIN [TechAssets].[dbo].[Hardware_UpdateHistory] hds ON d.[IT_No] = hds.[IT_No] WHERE d.[activeStatus] = 1";

            EnterItemsModel Inmod = new EnterItemsModel
            {
                TBLUpdateHistory = HardwareModule.Get_Any_DT(sql)
            };
            return View(Inmod);
        }


        [HttpPost]

        public JsonResult SaveHistory(string itno, string purchasedate, string purchasingprice)
        {
            try

            {

                using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
                {
                    connection.Open();

                    string checkQuery = "SELECT COUNT(1) FROM [TechAssets].[dbo].[Hardware_UpdateHistory] WHERE IT_No = @IT_No";
                    SqlCommand checkCommand = new SqlCommand(checkQuery, connection);
                    checkCommand.Parameters.AddWithValue("@IT_No", itno);

                    int recordExists = Convert.ToInt32(checkCommand.ExecuteScalar());

                    if (recordExists > 0)
                    {
                        string updateQuery = @"UPDATE [TechAssets].[dbo].[Hardware_UpdateHistory]
                                           SET [IT_No] = @IT_No,
                                               [PurchaseDate] = @PurchaseDate,
                                               [PurchasingPrice] = @PurchasingPrice,
                                               SysUpUser = @SysUpUser, 
                                               SysUpDate = @SysUpDate
                                           WHERE IT_No = @IT_No";

                        SqlCommand updateCommand = new SqlCommand(updateQuery, connection);
                        updateCommand.Parameters.AddWithValue("@PurchaseDate", purchasedate ?? (object)DBNull.Value);
                        updateCommand.Parameters.AddWithValue("@PurchasingPrice", purchasingprice ?? (object)DBNull.Value);
                        updateCommand.Parameters.AddWithValue("@SysUpUser", Constants.sysUser);
                        updateCommand.Parameters.AddWithValue("@SysUpDate", DateTime.Now);
                        updateCommand.Parameters.AddWithValue("@IT_No", itno);

                        updateCommand.ExecuteNonQuery();
                    }
                    else
                    {
                        string insertQuery = @"INSERT INTO [TechAssets].[dbo].[Hardware_UpdateHistory]
                                           (IT_No, PurchaseDate, PurchasingPrice, SysUpUser, SysUpDate)
                                           VALUES
                                           (@IT_No, @PurchaseDate, @PurchasingPrice, @SysUpUser, @SysUpDate)";

                        SqlCommand insertCommand = new SqlCommand(insertQuery, connection);
                        insertCommand.Parameters.AddWithValue("@IT_No", itno);
                        insertCommand.Parameters.AddWithValue("@PurchaseDate", purchasedate ?? (object)DBNull.Value);
                        insertCommand.Parameters.AddWithValue("@PurchasingPrice", purchasingprice ?? (object)DBNull.Value);
                        insertCommand.Parameters.AddWithValue("@SysUpUser", Constants.sysUser);
                        insertCommand.Parameters.AddWithValue("@SysUpDate", DateTime.Now);

                        insertCommand.ExecuteNonQuery();
                    }

                    return Json(new { success = true });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }



        [HttpPost]
        public JsonResult SaveSpecification(string itno, string Processor, string ProcessorSpeed, string Memory, string Storage, string StorageType, string OperatingSystem, string ScreenSize, string PanelType, string SpeRemarks)
        {
            try

            {
                
                using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
                {
                    connection.Open();

                    string checkQuery = "SELECT COUNT(1) FROM [TechAssets].[dbo].[Hardware_DeviceSpecification] WHERE IT_No = @IT_No";
                    SqlCommand checkCommand = new SqlCommand(checkQuery, connection);
                    checkCommand.Parameters.AddWithValue("@IT_No", itno);

                    int recordExists = Convert.ToInt32(checkCommand.ExecuteScalar());

                    if (recordExists > 0)
                    {
                        string updateQuery = @"UPDATE [TechAssets].[dbo].[Hardware_DeviceSpecification]
                                           SET Processor = @Processor,
                                               ProcessorSpeed = @ProcessorSpeed,
                                               Memory = @Memory,
                                               Storage = @Storage,
                                               StorageType = @StorageType,
                                               OperatingSystem = @OperatingSystem,
                                               ScreenSize = @ScreenSize,
                                               PanelType = @PanelType,
                                               SpeRemarks = @SpeRemarks,
                                               SysUpUser = @SysUpUser, 
                                               SysUpDate = @SysUpDate
                                           WHERE IT_No = @IT_No";

                        SqlCommand updateCommand = new SqlCommand(updateQuery, connection);
                        updateCommand.Parameters.AddWithValue("@Processor", Processor ?? (object)DBNull.Value);
                        updateCommand.Parameters.AddWithValue("@ProcessorSpeed", ProcessorSpeed ?? (object)DBNull.Value);
                        updateCommand.Parameters.AddWithValue("@Memory", Memory ?? (object)DBNull.Value);
                        updateCommand.Parameters.AddWithValue("@Storage", Storage ?? (object)DBNull.Value);
                        updateCommand.Parameters.AddWithValue("@StorageType", StorageType ?? (object)DBNull.Value);
                        updateCommand.Parameters.AddWithValue("@OperatingSystem", OperatingSystem ?? (object)DBNull.Value);
                        updateCommand.Parameters.AddWithValue("@ScreenSize", ScreenSize ?? (object)DBNull.Value);
                        updateCommand.Parameters.AddWithValue("@PanelType", PanelType ?? (object)DBNull.Value);
                        updateCommand.Parameters.AddWithValue("@SpeRemarks", SpeRemarks ?? (object)DBNull.Value);
                        updateCommand.Parameters.AddWithValue("@SysUpUser", Constants.sysUser); 
                        updateCommand.Parameters.AddWithValue("@SysUpDate", DateTime.Now);
                        updateCommand.Parameters.AddWithValue("@IT_No", itno);

                        updateCommand.ExecuteNonQuery();
                    }
                    else
                    {
                        string insertQuery = @"INSERT INTO [TechAssets].[dbo].[Hardware_DeviceSpecification]
                                           (IT_No, Processor, ProcessorSpeed, Memory, Storage, StorageType, OperatingSystem, ScreenSize, PanelType, SpeRemarks, SysUpUser, SysUpDate)
                                           VALUES
                                           (@IT_No, @Processor, @ProcessorSpeed, @Memory, @Storage, @StorageType, @OperatingSystem, @ScreenSize, @PanelType, @SpeRemarks, @SysUpUser, @SysUpDate)";

                        SqlCommand insertCommand = new SqlCommand(insertQuery, connection);
                        insertCommand.Parameters.AddWithValue("@IT_No", itno);
                        insertCommand.Parameters.AddWithValue("@Processor", Processor ?? (object)DBNull.Value);
                        insertCommand.Parameters.AddWithValue("@ProcessorSpeed", ProcessorSpeed ?? (object)DBNull.Value);
                        insertCommand.Parameters.AddWithValue("@Memory", Memory ?? (object)DBNull.Value);
                        insertCommand.Parameters.AddWithValue("@Storage", Storage ?? (object)DBNull.Value);
                        insertCommand.Parameters.AddWithValue("@StorageType", StorageType ?? (object)DBNull.Value);
                        insertCommand.Parameters.AddWithValue("@OperatingSystem", OperatingSystem ?? (object)DBNull.Value);
                        insertCommand.Parameters.AddWithValue("@ScreenSize", ScreenSize ?? (object)DBNull.Value);
                        insertCommand.Parameters.AddWithValue("@PanelType", PanelType ?? (object)DBNull.Value);
                        insertCommand.Parameters.AddWithValue("@SpeRemarks", SpeRemarks ?? (object)DBNull.Value);
                        insertCommand.Parameters.AddWithValue("@SysUpUser", Constants.sysUser); 
                        insertCommand.Parameters.AddWithValue("@SysUpDate", DateTime.Now);

                        insertCommand.ExecuteNonQuery();
                    }

                    return Json(new { success = true });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }



     


        [HttpPost]
        public JsonResult GetTechnicalAssessment(string technicalassesmentids)
        {
            if (string.IsNullOrEmpty(technicalassesmentids))
            {
                return Json(new { success = false, message = "Invalid input" });
            }

            List<object> faultSolutions = new List<object>();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
            {
                conn.Open();

                string query = @"
                                DECLARE @Input NVARCHAR(MAX) = @TechnicalAssessmentIDs;
                                WITH Split AS (
                                    SELECT CAST('<x>' + REPLACE(@Input, ',', '</x><x>') + '</x>' AS XML) AS Data
                                )
                                SELECT [Fault_Solution]
                                FROM [TechAssets].[dbo].[Hardware_Fault&Solution]
                                WHERE [FSid] IN (
                                    SELECT T.x.value('.', 'INT')
                                    FROM Split
                                    CROSS APPLY Data.nodes('/x') AS T(x)
                                );";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@TechnicalAssessmentIDs", technicalassesmentids);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            faultSolutions.Add(new { Fault_Solution = reader["Fault_Solution"].ToString() });
                        }
                    }
                }
            }

            return Json(faultSolutions);
        }




        [HttpPost]
        public JsonResult GetTechnicalSolution(string technicalsolutionids)
        {
            if (string.IsNullOrEmpty(technicalsolutionids))
            {
                return Json(new { success = false, message = "Invalid input" });
            }

            List<object> faultSolutions = new List<object>();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
            {
                conn.Open();

                string query = @"
                        DECLARE @Input NVARCHAR(MAX) = @TechnicalSolutionIDs;
                        WITH Split AS (
                            SELECT CAST('<x>' + REPLACE(@Input, ',', '</x><x>') + '</x>' AS XML) AS Data
                        )
                        SELECT [Fault_Solution]
                        FROM [TechAssets].[dbo].[Hardware_Fault&Solution]
                        WHERE [FSid] IN (
                            SELECT T.x.value('.', 'INT')
                            FROM Split
                            CROSS APPLY Data.nodes('/x') AS T(x)
                        );";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@TechnicalSolutionIDs", technicalsolutionids);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            faultSolutions.Add(new { Fault_Solution = reader["Fault_Solution"].ToString() });
                        }
                    }
                }
            }

            return Json(faultSolutions);
        }








    }


}






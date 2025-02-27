using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.Mvc;
using slbfeHardware.Models;


namespace slbfeHardware.DAL
{
    public class HardwareModule
    {
        public static int insert_SQL(string sql)
        {

            using (var conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
            {
                conn_uat.Open();
                SqlCommand sqlcmd = new SqlCommand(sql, conn_uat);
                sqlcmd.ExecuteNonQuery();
                conn_uat.Close();
            }
            return 1;
        }

        public static string GetReference()
        {
            string Reference;
            DateTime sys_date = DateTime.Now;

            string year = sys_date.Year.ToString("0000");

            string InvRef = HardwareModule.GetCount();

            string Inv_Ref = string.Format("{0:000000}", (int.Parse(InvRef)) + 1);

            Reference = year + "Inv" + Inv_Ref;


            return Reference;
        }

        public static string GetCount()
        {
            string count = null;
            string year = DateTime.Now.Year.ToString();
            string sql = "SELECT count(*) FROM [TechAssets].[dbo].[Hardware_Master_Invoice]";

            using (var conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
            {
                conn_uat.Open();
                SqlCommand sqlcmd = new SqlCommand(sql, conn_uat);
                SqlDataReader dr = null;
                dr = sqlcmd.ExecuteReader();
                while (dr.Read())
                {
                    count = dr[0].ToString();
                }
            }
            return count;
        }






        public static string GetCountreqForm()
        {
            string count = null;
            string year = DateTime.Now.Year.ToString();
            string sql = "SELECT count(id) FROM [TechAssets].[dbo].[Hardware_Master_AssestReq] where Status='1' and year([RsysDate])='" + year + "'";

            using (var conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
            {
                conn_uat.Open();
                SqlCommand sqlcmd = new SqlCommand(sql, conn_uat);
                SqlDataReader dr = null;
                dr = sqlcmd.ExecuteReader();
                while (dr.Read())
                {
                    count = dr[0].ToString();
                }
            }
            return count;
        }

        public static string GetFormID()
        {
            string formID;
            DateTime RsysDate = DateTime.Now;

            string year = RsysDate.Year.ToString("0000");

            string reqid = HardwareModule.GetCountreqForm();

            string req_id = string.Format("{0:000000}", (int.Parse(reqid)) + 1);

            formID = year + "Req" + req_id;


            return formID;
        }




        public static DataTable Get_Any_DT(string sql)
        {
            DataTable dt1 = new DataTable();
            try
            {
                using (var conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
                {
                    conn_uat.Open();
                    using (SqlCommand sqlcmd = new SqlCommand(sql, conn_uat))
                    {
                        SqlDataAdapter ad = new SqlDataAdapter(sqlcmd);
                        ad.Fill(dt1);
                    }
                    conn_uat.Close();
                }
            }
            catch (SqlException ex)
            {
                // Log the SQL exception or handle it as necessary
                Console.WriteLine("SQL Error: " + ex.Message);
            }
            catch (Exception ex)
            {
                // Log the general exception or handle it as necessary
                Console.WriteLine("Error: " + ex.Message);
            }
            return dt1;
        }


        public static InvoicenGRNModel Get_Invoice_Documents(InvoicenGRNModel jsm)
        {
            using (var con = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("select [Invoice_Image] FROM [TechAssets].[dbo].[Hardware_Master_Invoice] where [Invoice_Reference]='" + jsm.Invoice_Reference + "' and status='1'", con);
                SqlDataReader dr = null;
                dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    jsm.Doc = (byte[])dr[0];
                }
            }
            return jsm;
        }

        public static suppliersmodel GetSupplierInfo(string sql)
        {
            suppliersmodel supllier = new suppliersmodel();

            using (var conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
            {
                conn_uat.Open();
                SqlCommand sqlcmd = new SqlCommand(sql, conn_uat);
                IDataReader dr = null;
                dr = sqlcmd.ExecuteReader();
                dr.Read();

                supllier.Name_of_Supplier = dr[0].ToString();
                supllier.Registration_Status = dr[1].ToString();
                supllier.Registration_Year = dr[2].ToString();
                supllier.Address_of_Dealer = dr[3].ToString();
                supllier.email = dr[4].ToString();
                supllier.fax = dr[5].ToString();
                supllier.Telephone = dr[6].ToString();
                supllier.Contact_Person1 = dr[7].ToString();
                supllier.Contact_Person1_Phone = dr[8].ToString();
                supllier.Contact_Person2 = dr[9].ToString();
                supllier.Contact_Person2_Phone = dr[10].ToString();
                supllier.status = dr[11].ToString();
                conn_uat.Close();

            }

            return supllier;
        }

        public static EnterItemsModel GetInvoiceInfo(string invno)
        {
            //string sql1 = "SELECT a.[Invoice_Reference],a.[Invoice_No],CONVERT(VARCHAR,a.[Invoice_Date], 23),a.[Name_of_Supplier] ,b.[DescriptionCode]FROM [TechAssets].[dbo].[Hardware_Master_Invoice] a JOIN [TechAssets].[dbo].[Hardware_Devices] b on a.Invoice_Reference = b.[Invoice_Reference] where a.Invoice_Reference='" + invno + "'";

            string sql1 = "SELECT [id],[Invoice_Reference],[Invoice_No],CONVERT(VARCHAR, [Invoice_Date], 23) AS Invoice_Date,[Name_of_Supplier] FROM [TechAssets].[dbo].[Hardware_Master_Invoice] where [Invoice_Reference] = '" + invno + "'";

            EnterItemsModel invoice = new EnterItemsModel();

            using (var conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
            {
                conn_uat.Open();
                SqlCommand sqlcmd = new SqlCommand(sql1, conn_uat);
                IDataReader dr = null;
                dr = sqlcmd.ExecuteReader();
                if (dr.Read())
                {

                    invoice.Invoice_Ref = dr["Invoice_Reference"].ToString();
                    invoice.Invoice_No = dr["Invoice_No"].ToString();
                    invoice.Invoice_Date = dr["Invoice_Date"].ToString();
                    invoice.Name_of_Supplier = dr["Name_of_Supplier"].ToString();

                }

                conn_uat.Close();
            }

            return invoice;
        }


        public static EnterItemsModel GetInvoiceInfo2(string desCode)
        {
            string sql1 = "SELECT a.[Invoice_Reference],a.[Invoice_No],CONVERT(VARCHAR,a.[Invoice_Date], 23) ,b.[DescriptionCode]" +
            "FROM [TechAssets].[dbo].[Hardware_Master_Invoice] a " +
                "JOIN [TechAssets].[dbo].[Hardware_Devices] b on a.Invoice_Reference = b.[Invoice_Reference] " +
                "WHERE b.[DescriptionCode] = '" + desCode + "'";
            EnterItemsModel invoice = new EnterItemsModel();

            using (var conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
            {
                conn_uat.Open();
                SqlCommand sqlcmd = new SqlCommand(sql1, conn_uat);
                IDataReader dr = null;
                dr = sqlcmd.ExecuteReader();
                if (dr.Read())
                {
                    invoice.Invoice_Ref = dr[0].ToString();
                    invoice.Invoice_No = dr[1].ToString();
                    invoice.Invoice_Date = dr[2].ToString();
                    invoice.Name_of_Supplier = dr[3].ToString();

                }

                conn_uat.Close();
            }

            return invoice;
        }


        public static InvoicenGRNModel GetFullInvoiceInfo(string sql)
        {
            InvoicenGRNModel invoiceFull = new InvoicenGRNModel();
            using (var conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
            {
                conn_uat.Open();
                SqlCommand sqlcmd = new SqlCommand(sql, conn_uat);
                IDataReader dr = null;
                dr = sqlcmd.ExecuteReader();
                dr.Read();


                invoiceFull.Invoice_Reference = dr[0].ToString();
                invoiceFull.Invoice_No = dr[1].ToString();
                invoiceFull.Invoice_Date = dr[2].ToString();
                invoiceFull.PO_No = dr[3].ToString();
                invoiceFull.Name_of_Supplier = dr[4].ToString();
                invoiceFull.Vat = Convert.ToDecimal(dr[5]);
                invoiceFull.Dicount_Amount = Convert.ToDecimal(dr[6]);
                invoiceFull.Total_Amount = Convert.ToDecimal(dr[7]);
                invoiceFull.Invoice_Image = dr[8].ToString();
                invoiceFull.GRN_No = dr[9].ToString();
                invoiceFull.GRN_Date = dr[10].ToString();
                invoiceFull.Remarks = dr[11].ToString();
                conn_uat.Close();
            }

            return invoiceFull;

        }


        public static List<SelectListItem> GetCompaniesSelectList()
        {
            List<SelectListItem> list = new List<SelectListItem>();

            string sql = "SELECT [id],[Company_Name] FROM [TechAssets].[dbo].[Hardware_Companies] Where [DStatus]=1 AND [status]='Active'";
            using (var conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
            {
                conn_uat.Open();
                SqlCommand sqlcmd = new SqlCommand(sql, conn_uat);

                SqlDataReader dr = null;

                dr = sqlcmd.ExecuteReader();

                while (dr.Read())
                {
                    list.Add(new SelectListItem { Text = dr[1].ToString(), Value = dr[1].ToString() });
                }
                conn_uat.Close();
            }
            return list;
        }

        public static List<SelectListItem> GetRepairCompaniesSelectList()
        {
            List<SelectListItem> list = new List<SelectListItem>();

            string sql = "SELECT [id],[Company_Name] FROM [TechAssets].[dbo].[Hardware_Companies] WHERE [DStatus]=1 AND [status]='Active' AND [CRepair] = 1";
            using (var conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
            {
                conn_uat.Open();
                SqlCommand sqlcmd = new SqlCommand(sql, conn_uat);

                SqlDataReader dr = null;

                dr = sqlcmd.ExecuteReader();

                while (dr.Read())
                {
                    list.Add(new SelectListItem { Text = dr[1].ToString(), Value = dr[1].ToString() });
                }
                conn_uat.Close();
            }
            return list;
        }

        public static string GetCount1()
        {
            string count = null;
            string sql = "SELECT count(id) FROM [TechAssets].[dbo].[Hardware_Item_Types]";

            using (var conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
            {
                conn_uat.Open();
                SqlCommand sqlcmd = new SqlCommand(sql, conn_uat);
                SqlDataReader dr = null;
                dr = sqlcmd.ExecuteReader();
                while (dr.Read())
                {
                    count = dr[0].ToString();
                }
            }
            return count;
        }


        public static string GetItemTypeCode()
        {
            string ItmTypeCode;
            string ICode = "Item";

            string ItemCode = HardwareModule.GetCount1();
            string ItemCode_New = string.Format("{0:0000}", int.Parse(ItemCode));
            ItmTypeCode = ICode + ItemCode_New;

            return ItmTypeCode;
        }


        public static string GetCount2()
        {
            string count = null;
            string sql = "SELECT count(id) FROM [TechAssets].[dbo].[Hardware_Brand]";

            using (var conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
            {
                conn_uat.Open();
                SqlCommand sqlcmd = new SqlCommand(sql, conn_uat);
                SqlDataReader dr = null;
                dr = sqlcmd.ExecuteReader();
                while (dr.Read())
                {
                    count = dr[0].ToString();
                }
            }
            return count;
        }


        public static string GetBrandCode()
        {
            string Brand_Code;
            string BCode = "B";

            string brndCode = HardwareModule.GetCount2();
            string BrandCode_New = string.Format("{0:000}", int.Parse(brndCode));
            Brand_Code = BCode + BrandCode_New;

            return Brand_Code;
        }


        public static string GetCount3()
        {
            string count = null;
            string sql = "  select count(id) FROM [TechAssets].[dbo].[Hardware_ModelNew]";

            using (var conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
            {
                conn_uat.Open();
                SqlCommand sqlcmd = new SqlCommand(sql, conn_uat);
                SqlDataReader dr = null;
                dr = sqlcmd.ExecuteReader();
                while (dr.Read())
                {
                    count = dr[0].ToString();
                }
            }
            return count;

        }




        public static string GetPendingHandoverCount()
        {
            string count = null;
            string sql = "SELECT COUNT([id]) FROM [TechAssets].[dbo].[Hardware_Devices] WHERE [Current_Status] = 2";

            using (var conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
            {
                conn_uat.Open();

                using (SqlCommand command = new SqlCommand(sql, conn_uat))
                {

                    int result = (int)command.ExecuteScalar();
                    count = result.ToString();
                }
            }
            return count;
        }


        public static string GetInITCount()
        {
            string count = null;
            string sql = "SELECT COUNT(id) FROM [TechAssets].[dbo].[Hardware_Devices] WHERE Current_Status=2";

            using (var conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
            {
                conn_uat.Open();
                SqlCommand sqlcmd = new SqlCommand(sql, conn_uat);
                SqlDataReader dr = null;
                dr = sqlcmd.ExecuteReader();
                while (dr.Read())
                {
                    count = dr[0].ToString();
                }
            }
            return count;
        }

        public static string GetProcessInITCount()
        {
            string count = null;
            string sql = "SELECT COUNT(DutyID) FROM [TechAssets].[dbo].[Hardware_DutyAssign] WHERE DutyStatus=35";

            using (var conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
            {
                conn_uat.Open();
                SqlCommand sqlcmd = new SqlCommand(sql, conn_uat);
                SqlDataReader dr = null;
                dr = sqlcmd.ExecuteReader();
                while (dr.Read())
                {
                    count = dr[0].ToString();
                }
            }
            return count;
        }

        public static string GetProcessOutsideCount()
        {
            string count = null;
            string sql = "SELECT COUNT(DutyID) FROM [TechAssets].[dbo].[Hardware_DutyAssign] WHERE DutyStatus=50";

            using (var conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
            {
                conn_uat.Open();
                SqlCommand sqlcmd = new SqlCommand(sql, conn_uat);
                SqlDataReader dr = null;
                dr = sqlcmd.ExecuteReader();
                while (dr.Read())
                {
                    count = dr[0].ToString();
                }
            }
            return count;
        }


        public static string GetDisposeCount()
        {
            string count = null;
            string sql = "SELECT COUNT(DutyID) FROM [TechAssets].[dbo].[Hardware_Dispose] WHERE DisposeStatus=9";

            using (var conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
            {
                conn_uat.Open();
                SqlCommand sqlcmd = new SqlCommand(sql, conn_uat);
                SqlDataReader dr = null;
                dr = sqlcmd.ExecuteReader();
                while (dr.Read())
                {
                    count = dr[0].ToString();
                }
            }
            return count;
        }

        public static string GetModelCode()
        {
            string Model_Code;
            string MCode = "M";

            string mdlCode = HardwareModule.GetCount3();
            string ModelCode_New = string.Format("{0:000}", int.Parse(mdlCode) + 1);
            Model_Code = MCode + ModelCode_New;

            return Model_Code;
        }


        //add new brands
        public static string AddNewBrand(string brandName, string itemtypecode)
        {
            string brandCode = GetBrandCode();

            string sql1 = "INSERT INTO [TechAssets].[dbo].[Hardware_Brand](BrandCode,BrandName,sysDate,sysUser) VALUES ('" + brandCode + "','" + brandName + "','','')";

            string sql2 = "INSERT INTO [TechAssets].[dbo].[Hardware_Item_Brand](Item_Type_Code, BrandCode) VALUES ('" + itemtypecode + "','" + brandCode + "')";

            using (var con = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(sql1, con);
                SqlCommand cmd2 = new SqlCommand(sql2, con);
                cmd.ExecuteNonQuery();
                cmd2.ExecuteNonQuery();
                con.Close();
            }
            return brandCode;
        }


        //add new model
        public static void AddNewModel(string modelName, string brandCode)
        {
            string modelCode = GetModelCode();

            string sql1 = "INSERT INTO [TechAssets].[dbo].[Hardware_Brand_Model](BrandCode,ModelCode) VALUES ('" + brandCode + "','" + modelCode + "')";

            string sql2 = "INSERT INTO [TechAssets].[dbo].[Hardware_Model](ModelCode, ModelName) VALUES ('" + modelCode + "','" + modelName + "')";


            using (var con = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(sql1, con);
                SqlCommand cmd2 = new SqlCommand(sql2, con);
                cmd.ExecuteNonQuery();
                cmd2.ExecuteNonQuery();
                con.Close();
            }

        }


        public static List<SelectListItem> GetItemTypeListByDatabase()
        {
            List<SelectListItem> list = new List<SelectListItem>();

            string sql = "SELECT [Item_Type_Code],[Item_Type]FROM [TechAssets].[dbo].[Hardware_Item_Types] order by [Item_Type]";
            using (var conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
            {
                conn_uat.Open();
                SqlCommand sqlcmd = new SqlCommand(sql, conn_uat);

                SqlDataReader dr = null;

                dr = sqlcmd.ExecuteReader();

                while (dr.Read())
                {
                    list.Add(new SelectListItem { Text = dr[1].ToString(), Value = dr[0].ToString() });
                }
                conn_uat.Close();
            }
            return list;
        }



        public static List<SelectListItem> GetBrandListByDatabase()
        {
            List<SelectListItem> list = new List<SelectListItem>();

            string sql = "SELECT [BrandCode],[BrandName] FROM [TechAssets].[dbo].[Hardware_Brand] ORDER BY BrandName desc";
            using (var conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
            {
                conn_uat.Open();
                SqlCommand sqlcmd = new SqlCommand(sql, conn_uat);

                SqlDataReader dr = null;

                dr = sqlcmd.ExecuteReader();

                while (dr.Read())
                {
                    list.Add(new SelectListItem { Text = dr[1].ToString().Trim(), Value = dr[0].ToString().Trim() });
                }


                conn_uat.Close();
            }
            return list;
        }


        public static List<SelectListItem> GetBrandListByDatabase2(string ItemCode)
        {
            List<SelectListItem> list = new List<SelectListItem>();

            string sql = "SELECT distinct a.[Brand_Code],b.[BrandName]  FROM [TechAssets].[dbo].[Hardware_ModelNew] as a  inner join [TechAssets].[dbo].[Hardware_Brand] as b on a.[Brand_Code]=b.[BrandCode]  where [Item_Code]='" + ItemCode + "' order by b.[BrandName] ";
            using (var conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
            {
                conn_uat.Open();
                SqlCommand sqlcmd = new SqlCommand(sql, conn_uat);

                SqlDataReader dr = null;

                dr = sqlcmd.ExecuteReader();

                while (dr.Read())
                {
                    list.Add(new SelectListItem { Text = dr[1].ToString().Trim(), Value = dr[0].ToString().Trim() });
                }


                conn_uat.Close();
            }
            return list;
        }

        public static List<SelectListItem> GetModelListByDatabase2(string BrandCode, string ItemCode)
        {
            List<SelectListItem> list = new List<SelectListItem>();

            string sql = "SELECT [Model_Code],[Model_Name] FROM [TechAssets].[dbo].[Hardware_ModelNew] where [Item_Code]='" + ItemCode + "' and [Brand_Code]='" + BrandCode + "'";
            using (var conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
            {
                conn_uat.Open();
                SqlCommand sqlcmd = new SqlCommand(sql, conn_uat);

                SqlDataReader dr = null;

                dr = sqlcmd.ExecuteReader();

                while (dr.Read())
                {
                    list.Add(new SelectListItem { Text = dr[1].ToString().Trim(), Value = dr[0].ToString().Trim() });
                }


                conn_uat.Close();
            }
            return list;
        }

        public static List<SelectListItem> GetModelListByDatabase()
        {
            List<SelectListItem> list = new List<SelectListItem>();

            string sql = "SELECT [ModelCode],[ModelName]   FROM [TechAssets].[dbo].[Hardware_Model]";
            using (var conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
            {
                conn_uat.Open();
                SqlCommand sqlcmd = new SqlCommand(sql, conn_uat);

                SqlDataReader dr = null;

                dr = sqlcmd.ExecuteReader();

                while (dr.Read())
                {
                    list.Add(new SelectListItem { Text = dr[1].ToString(), Value = dr[0].ToString() });
                }
                conn_uat.Close();
            }
            return list;

        }


        public static List<SelectListItem> GetDivisionListByDatabase()
        {
            List<SelectListItem> list = new List<SelectListItem>();

            string sql = "SELECT [id],[division],[div_index] FROM [EMMSDB].[dbo].[Staff_Division] Where status = '1' and location_id !='EMB'";
            using (var conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
            {
                conn_uat.Open();
                SqlCommand sqlcmd = new SqlCommand(sql, conn_uat);

                SqlDataReader dr = null;

                dr = sqlcmd.ExecuteReader();

                while (dr.Read())
                {
                    list.Add(new SelectListItem { Text = dr[1].ToString().Trim(), Value = dr[2].ToString() });
                }
                conn_uat.Close();
            }
            return list;

        }


        public static List<System.Web.Mvc.SelectListItem> get_any_list(string sql)
        {

            List<System.Web.Mvc.SelectListItem> return_list = new List<System.Web.Mvc.SelectListItem>();

            using (var conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
            {
                conn_uat.Open();
                SqlCommand sqlcmd = new SqlCommand(sql, conn_uat);
                SqlDataReader dr = null;
                dr = sqlcmd.ExecuteReader();

                while (dr.Read())
                {
                    return_list.Add(new System.Web.Mvc.SelectListItem { Text = (dr[1]).ToString().Trim(), Value = (dr[0]).ToString().Trim() });
                }
                conn_uat.Close();

                return return_list;

            }
        }


        public static List<SelectListItem> GetItemData(string itemCode)
        {
            List<SelectListItem> bNames = new List<SelectListItem>();

            string sql = " SELECT DISTINCT b.BrandName as bName, b.BrandCode as bCode FROM [TechAssets].[dbo].[Hardware_NewItemTypes] i " +
                "INNER JOIN [TechAssets].[dbo].[Hardware_Brand] b ON i.brand = b.BrandCode " +
                "Where i.ItemType_Code = '" + itemCode + "'";


            using (var conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
            {
                conn_uat.Open();
                SqlCommand sqlcmd = new SqlCommand(sql, conn_uat);

                SqlDataReader dr = null;

                dr = sqlcmd.ExecuteReader();

                while (dr.Read())
                {
                    bNames.Add(new SelectListItem { Text = dr["bName"].ToString(), Value = dr["bCode"].ToString() });

                }

                conn_uat.Close();
            }

            return bNames;
        }


        public static List<SelectListItem> GetModelData(string brandCode, string itemCode)
        {
            List<SelectListItem> mNames = new List<SelectListItem>();


            string sql = "SELECT DISTINCT m.ModelName, m.ModelCode  FROM [TechAssets].[dbo].[Hardware_NewItemTypes] i " +
                "INNER JOIN [TechAssets].[dbo].[Hardware_Brand] b ON i.brand = b.BrandCode " +
                "INNER JOIN [TechAssets].[dbo].[Hardware_Model] m  ON i.model = m.ModelCode " +
                "Where i.ItemType_Code = '" + itemCode + "' and i.Brand = '" + brandCode + "'";

            using (var conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
            {
                conn_uat.Open();
                SqlCommand sqlcmd = new SqlCommand(sql, conn_uat);

                SqlDataReader dr = null;

                dr = sqlcmd.ExecuteReader();

                while (dr.Read())
                {
                    mNames.Add(new SelectListItem { Text = dr["ModelName"].ToString(), Value = dr["ModelCode"].ToString() });

                }

                conn_uat.Close();
            }

            return mNames;

        }


        public static string GetEmpName(string empNo)
        {
            string name = "";

            string sql1 = " SELECT Name_with_ini FROM [EMMSDB].[dbo].[Staff_employee_Details] WHERE Emp_no = '" + empNo + "'";
            //string sql1 = "SELECT name FROM [EMMSDB].[dbo].[staff] WHERE EmpNo = '" + empNo + "'";

            using (var conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
            {
                conn_uat.Open();
                SqlCommand sqlcmd = new SqlCommand(sql1, conn_uat);
                sqlcmd.Parameters.AddWithValue("@empNo", empNo);

                // Use ExecuteScalar to retrieve a single value
                object result = sqlcmd.ExecuteScalar();

                if (result != null)
                {
                    name = result.ToString();
                }
            }

            return name;
        }


        public static int GetItemCountForInvoice(string invoiceRef)
        {
            int count = 0;

            string sql = "SELECT COUNT(id) FROM [TechAssets].[dbo].[Hardware_Inovice_Items] WHERE Invoice_Ref = @InvoiceRef";

            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
            {
                conn.Open();
                SqlCommand sqlCommand = new SqlCommand(sql, conn);
                sqlCommand.Parameters.AddWithValue("@InvoiceRef", invoiceRef);

                // ExecuteScalar is used to retrieve a single value (in this case, the count)
                count = (int)sqlCommand.ExecuteScalar();

                count = count + 1;
            }

            return count;
        }


        public static string CreateInvoiceItemNumber(string invoiceRef)
        {

            int count = GetItemCountForInvoice(invoiceRef);
            string countString = count.ToString();

            string invoiceItemNumber = invoiceRef + "-" + countString;

            return invoiceItemNumber;
        }


        public static EnterItemsModel GetDeviceInfo(string sql)
        {
            EnterItemsModel device = new EnterItemsModel();

            using (var conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
            {
                conn_uat.Open();
                SqlCommand sqlcmd = new SqlCommand(sql, conn_uat);
                IDataReader dr = null;
                dr = sqlcmd.ExecuteReader();
                dr.Read();
                device.Invoice_Ref = dr[0].ToString();
                device.Invoice_No = dr[1].ToString();
                device.DescriptionCode = dr[2].ToString();
                device.Name_of_Supplier = dr[3].ToString();

                conn_uat.Close();
            }

            return device;
        }


        //public static suppliersmodel EditSuppliers(string Name_of_Supplier)
        //{
        //    suppliersmodel data = new suppliersmodel();


        //    String sql = "SELECT [Company_Name], [Registration_Status], [Registration_Year], [Address_of_Dealer], [email], [fax], [Telephone], [Contact_Person1], [Contact_Person1_Phone], [Contact_Person2], [Contact_Person2_Phone], [status] FROM [TechAssets].[dbo].[Hardware_Companies] WHERE [Company_Name] ='" + Name_of_Supplier + "'";

        //    using (var conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
        //    {
        //        conn_uat.Open();
        //        SqlCommand sqlcmd2 = new SqlCommand(sql, conn_uat);
        //        sqlcmd2.Parameters.AddWithValue("@Name_of_Supplier", Name_of_Supplier);

        //        SqlDataReader dr = sqlcmd2.ExecuteReader();
        //        if (dr.Read())
        //        {
        //            data.Name_of_Supplier = dr["Company_Name"].ToString();
        //            data.Registration_Status = dr["Registration_Status"].ToString();
        //            data.Registration_Year = dr["Registration_Year"].ToString();
        //            data.Address_of_Dealer = dr["Address_of_Dealer"].ToString();
        //            data.email = dr["email"].ToString();
        //            data.fax = dr["fax"].ToString();
        //            data.Telephone = dr["Telephone"].ToString();
        //            data.Contact_Person1 = dr["Contact_Person1"].ToString();
        //            data.Contact_Person1_Phone = dr["Contact_Person1_Phone"].ToString();
        //            data.Contact_Person2 = dr["Contact_Person2"].ToString();
        //            data.Contact_Person2_Phone = dr["Contact_Person2_Phone"].ToString();
        //            data.status = dr["status"].ToString();
        //        }
        //    }
        //    return data;
        //}

        public static suppliersmodel EditSuppliers(string Name_of_Supplier)
        {
            suppliersmodel data = new suppliersmodel();            
            string errorMessage = string.Empty;

            try
            {
                string sql = "SELECT [Company_Name], [Registration_Status], [Registration_Year], [Address_of_Dealer], [email], [fax], [Telephone], [Contact_Person1], [Contact_Person1_Phone], [Contact_Person2], [Contact_Person2_Phone], [status],[CVendor],[CCourier],[CRepair],[id] FROM [TechAssets].[dbo].[Hardware_Companies] WHERE [Company_Name] = @Name_of_Supplier";

                using (var conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
                {
                    conn_uat.Open();
                    SqlCommand sqlcmd2 = new SqlCommand(sql, conn_uat);
                    sqlcmd2.Parameters.AddWithValue("@Name_of_Supplier", Name_of_Supplier);

                    SqlDataReader dr = sqlcmd2.ExecuteReader();
                    if (dr.Read())
                    {
                        data.Name_of_Supplier = dr["Company_Name"].ToString();
                        data.Registration_Status = dr["Registration_Status"].ToString();
                        data.Registration_Year = dr["Registration_Year"].ToString();
                        data.Address_of_Dealer = dr["Address_of_Dealer"].ToString();
                        data.email = dr["email"].ToString();
                        data.fax = dr["fax"].ToString();
                        data.Telephone = dr["Telephone"].ToString();
                        data.Contact_Person1 = dr["Contact_Person1"].ToString();
                        data.Contact_Person1_Phone = dr["Contact_Person1_Phone"].ToString();
                        data.Contact_Person2 = dr["Contact_Person2"].ToString();
                        data.Contact_Person2_Phone = dr["Contact_Person2_Phone"].ToString();
                        data.status = dr["status"].ToString();
                        data.SupplierID = dr["id"].ToString();
                        data.CCategory1 = dr["CVendor"].ToString();
                        data.CCategory2 = dr["CCourier"].ToString();
                        data.CCategory3 = dr["CRepair"].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }

            if (!string.IsNullOrEmpty(errorMessage))
            {
                data.ErrorMessage = errorMessage;
            }

            return data;
        }






        public static InvoicenGRNModel EditInvoice(string Invoice_Reference)
        {
            InvoicenGRNModel data = new InvoicenGRNModel();

            string sql = "SELECT [Invoice_Reference],[Invoice_No],[Invoice_Date],[PO_No],[Name_of_Supplier],[Total_Amount],[Dicount_Amount],[Vat],[GRN_No],[GRN_Date],[Remarks],[sysDate],[sysUser],[Status] FROM [TechAssets].[dbo].[Hardware_Master_Invoice] WHERE [Invoice_Reference] = @Invoice_Reference";

            using (var conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
            {
                conn_uat.Open();
                SqlCommand sqlcmd2 = new SqlCommand(sql, conn_uat);
                sqlcmd2.Parameters.AddWithValue("@Invoice_Reference", Invoice_Reference);

                SqlDataReader dr = sqlcmd2.ExecuteReader();
                if (dr.Read())
                {
                    data.Invoice_Reference = dr["Invoice_Reference"].ToString();
                    data.Invoice_No = dr["Invoice_No"].ToString();
                    data.Invoice_Date = Convert.ToDateTime(dr["Invoice_Date"]).ToString("yyyy-MM-dd");
                    data.PO_No = dr["PO_No"].ToString();
                    data.Name_of_Supplier = dr["Name_of_Supplier"].ToString();
                    data.Vat = Convert.ToDecimal(dr["vat"]);
                    data.Dicount_Amount = Convert.ToDecimal(dr["Dicount_Amount"]);
                    data.Total_Amount = Convert.ToDecimal(dr["Total_Amount"]);
                    //data.InvoiceImagePath = dr["Invoice_Image"].ToString();
                    data.GRN_No = dr["GRN_No"].ToString();
                    data.GRN_Date = Convert.ToDateTime(dr["GRN_Date"]).ToString("yyyy-MM-dd");
                    data.Remarks = dr["Remarks"].ToString();
                    data.sysDate = Convert.ToDateTime(dr["sysDate"]);
                    data.sysUser = dr["sysUser"].ToString();
                    data.status = dr["status"].ToString();
                }
            }
            return data;
        }


        public static EnterItemsModel GetFUllDevicesInfo(string itNO)
        {
            string sql = "SELECT d.[Invoice_Reference], d.[DescriptionCode], d.[Invoice_No], it.[Item_Type], d.[IT_No], d.[Serial_No], d.[INV_No], d.[QR], d.[Current_Status], d.[Authorized_Officer], CONVERT(VARCHAR, d.[Installation_Date], 23) AS Installation_Date, d.[Remarks1], d.[Remarks2], it.[ShortCode], ds.[Status] AS Current_Status_Description, div_p.[division] AS Purchased_Division, div_c.[division] AS Current_Location, d.[Current_LocationID], d.[Purchased_DivisionID] FROM [TechAssets].[dbo].[Hardware_Devices] d JOIN [TechAssets].[dbo].[Hardware_Inovice_Items] i ON d.[DescriptionCode] = i.[DescriptionCode] INNER JOIN [TechAssets].[dbo].[Hardware_Item_Types] it ON i.[Item_Type] = it.[Item_Type_Code] INNER JOIN [TechAssets].[dbo].[Hardware_DeviceStatus] ds ON d.[Current_Status] = ds.[StatusID] INNER JOIN [EMMSDB].[dbo].[Staff_Division] div_p ON d.[Purchased_DivisionID] = div_p.[div_index] INNER JOIN [EMMSDB].[dbo].[Staff_Division] div_c ON d.[Current_LocationID] = div_c.[div_index] WHERE d.[IT_No] = @ITNo";

            EnterItemsModel DevicesFull = new EnterItemsModel();
            try
            {
                using (var conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
                {
                    conn_uat.Open();
                    using (SqlCommand sqlcmd = new SqlCommand(sql, conn_uat))
                    {
                        sqlcmd.Parameters.AddWithValue("@ITNo", itNO);
                        using (IDataReader dr = sqlcmd.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                DevicesFull.Invoice_Ref = dr["Invoice_Reference"].ToString();
                                DevicesFull.DescriptionCode = dr["DescriptionCode"].ToString();
                                DevicesFull.Invoice_No = dr["Invoice_No"].ToString();
                                DevicesFull.Item_Type = dr["Item_Type"].ToString();
                                DevicesFull.IT_No = dr["IT_No"].ToString();
                                DevicesFull.Serial_No = dr["Serial_No"].ToString();
                                DevicesFull.INV_No = dr["INV_No"].ToString();
                                DevicesFull.QR = dr["QR"].ToString();
                                DevicesFull.Authorized_Officer = dr["Authorized_Officer"].ToString();
                                DevicesFull.Installation_Date = dr["Installation_Date"].ToString();
                                DevicesFull.Remarks1 = dr["Remarks1"].ToString();
                                DevicesFull.Remarks2 = dr["Remarks2"].ToString();
                                DevicesFull.ShortCode = dr["ShortCode"].ToString();
                                DevicesFull.Current_Status = dr["Current_Status_Description"].ToString();
                                DevicesFull.Purchased_Division = dr["Purchased_Division"].ToString().Trim();
                                DevicesFull.Current_Location = dr["Current_Location"].ToString().Trim();
                                DevicesFull.Purchased_DivisionID = dr["Purchased_DivisionID"].ToString().Trim();
                                DevicesFull.Current_LocationID = dr["Current_LocationID"].ToString().Trim();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
            return DevicesFull;
        }



        public static EnterItemsModel EditDevicesData(string IT_No)
        {
            EnterItemsModel data = new EnterItemsModel();


            String sql = "SELECT [Invoice_Reference],[DescriptionCode],[Invoice_No],[IT_No],[Serial_No],[INV_No],[QR],[Purchased_Division],[Current_Location],[Current_Status],[Authorized_Officer], CONVERT(VARCHAR, [Installation_Date], 23) AS Installation_Date,[Remarks1],[Remarks2],[activeStatus],[sysDate],[sysUser] FROM [TechAssets].[dbo].[Hardware_Devices] WHERE [IT_No] ='" + IT_No + "'";

            using (var conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
            {
                conn_uat.Open();
                SqlCommand sqlcmd2 = new SqlCommand(sql, conn_uat);
                sqlcmd2.Parameters.AddWithValue("@IT_No", IT_No);

                SqlDataReader dr = sqlcmd2.ExecuteReader();
                if (dr.Read())
                {
                    data.invno = dr["Invoice_Reference"].ToString();
                    data.invID = dr["DescriptionCode"].ToString();
                    data.invoiceNo = dr["Invoice_No"].ToString();
                    data.IT_No = dr["IT_No"].ToString();
                    data.Serial_No = dr["Serial_No"].ToString();
                    data.INV_No = dr["INV_No"].ToString();
                    data.QR = dr["QR"].ToString();
                    data.Purchased_Division = dr["Purchased_Division"].ToString();
                    data.Current_Location = dr["Current_Location"].ToString();
                    data.Current_Status = dr["Current_Status"].ToString();
                    data.Authorized_Officer = dr["Authorized_Officer"].ToString();
                    data.Installation_Date = dr["Installation_Date"].ToString();
                    data.Remarks1 = dr["Remarks1"].ToString();
                    data.Remarks2 = dr["Remarks2"].ToString();
                    data.activeStatus = dr["activeStatus"].ToString();
                    data.sys_Date = Convert.ToDateTime(dr["sysDate"]);
                    data.sys_User = dr["sysUser"].ToString();
                }
            }
            return data;
        }


        //public static EnterItemsModel GetSingleRecords(string sql)
        //{
        //    EnterItemsModel data = new EnterItemsModel();

        //    using (var conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
        //    {
        //        conn_uat.Open();
        //        SqlCommand sqlcmd2 = new SqlCommand(sql, conn_uat);


        //        SqlDataReader dr = sqlcmd2.ExecuteReader();
        //        if (dr.Read())
        //        {
        //            data.ShortCode = dr["DisposeCode"].ToString();
        //            data.IT_No = dr["IT_No"].ToString();
        //            data.INV_No = dr["INV_No"].ToString();
        //            data.Serial_No = dr["Serial_No"].ToString();
        //            data.Item = dr["Item_Type"].ToString();
        //            data.QR = dr["QR"].ToString();
        //        }
        //    }
        //    return data;
        //}







        public static bool CheckNewDisposeCode(string newDisCode)
        {
            string sql = "SELECT COUNT(id) AS cnt FROM TechAssets.dbo.hardware_dispose WHERE [DisposeCode] = @DisposeCode";

            using (var conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
            {
                conn_uat.Open();
                using (SqlCommand sqlcmd = new SqlCommand(sql, conn_uat))
                {
                    // Use parameterized query to prevent SQL injection
                    sqlcmd.Parameters.AddWithValue("@DisposeCode", newDisCode);

                    // ExecuteScalar returns the first column of the first row
                    int count = (int)sqlcmd.ExecuteScalar();

                    return count > 0; // Return true if count is greater than 0
                }
            }
        }


        public static void UpdateRecords(string sql)
        {
            using (var conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
            {
                conn_uat.Open();
                using (SqlCommand sqlcmd = new SqlCommand(sql, conn_uat))
                {
                    sqlcmd.ExecuteScalar();
                }
            }
        }



        public static List<SelectListItem> GetCourierCompaniesSelectList()
        {
            List<SelectListItem> list = new List<SelectListItem>();

            string sql = "SELECT [Company_Name] FROM [TechAssets].[dbo].[Hardware_Companies] WHERE [DStatus]=1 AND [status]='Active' AND [CCourier] = 1";

            try
            {
                using (var conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
                {
                    conn_uat.Open();
                    using (SqlCommand sqlcmd = new SqlCommand(sql, conn_uat))
                    {
                        SqlDataReader dr = sqlcmd.ExecuteReader();

                        if (dr.HasRows)  // Check if there are any rows returned
                        {
                            while (dr.Read())
                            {
                                list.Add(new SelectListItem { Text = dr["Company_Name"].ToString(), Value = dr["Company_Name"].ToString() });
                            }
                        }
                        else
                        {
                            // Optional: Log or handle empty result set
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle or log the exception
                Console.WriteLine("Error: " + ex.Message);
            }

            return list;
        }

        public static EnterItemsModel EditInvoiceDescriptionData(string DescriptionCode)
        {
            EnterItemsModel data = new EnterItemsModel();


            String sql = "SELECT [Invoice_Ref],[DescriptionCode],[Item_Type],[BrandName],[ModelName],[Warranty_Period],[description],[quantity],[unitprice],[sys_Date],[sys_User],[status] FROM [TechAssets].[dbo].[Hardware_Inovice_Items] where [DescriptionCode] = '" + DescriptionCode + "'";

            using (var conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
            {
                conn_uat.Open();
                SqlCommand sqlcmd2 = new SqlCommand(sql, conn_uat);
                sqlcmd2.Parameters.AddWithValue("@DescriptionCode", DescriptionCode);

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
                    data.sys_Date = Convert.ToDateTime(dr["sys_Date"]);
                    data.sys_User = dr["sys_User"].ToString();
                    data.status = dr["status"].ToString();

                }
            }
            return data;
        }



        public static int GetAssetCount(string ReqFormID)
        {
            int count = 0;

            string sql = "SELECT COUNT(id) FROM [TechAssets].[dbo].[Hardware_Assest_Items] WHERE ReqFormID = @ReqFormID";

            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
            {
                conn.Open();
                SqlCommand sqlCommand = new SqlCommand(sql, conn);
                sqlCommand.Parameters.AddWithValue("@ReqFormID", ReqFormID);

                // ExecuteScalar is used to retrieve a single value (in this case, the count)
                count = (int)sqlCommand.ExecuteScalar();

                count = count + 1;
            }

            return count;
        }


        public static string CreateAssestID(string ReqFormID)
        {
            int count = GetAssetCount(ReqFormID);
            string countString = count.ToString();

            string assestCodeID = ReqFormID + "-" + countString;

            return assestCodeID;
        }



        //public static EnterItemsModel GetRequestFormInFo(string reqFormID)
        //{
        //    EnterItemsModel data = new EnterItemsModel();

        //    string sql = "SELECT HAI.[ReqFormID] AS HAI_ReqFormID, " +
        //                 "HAI.[ReqCode], HAI.[Item], HAI.[Quantity], " +
        //                 "HAI.[sysDate] AS ItemSysDate, HAI.[sysUser] AS ItemSysUser, " +
        //                 "HAI.[Status] AS ItemStatus, HMAR.[RDivision], " +
        //                 "HMAR.[RDate], HMAR.[RsysDate], " +
        //                 "HMAR.[RsysUser] AS MasterSysUser, HMAR.[Status] AS MasterStatus " +
        //                 "FROM [TechAssets].[dbo].[Hardware_Assest_Items] AS HAI " +
        //                 "JOIN [TechAssets].[dbo].[Hardware_Master_AssestReq] AS HMAR " +
        //                 "ON HAI.[ReqFormID] = HMAR.[ReqFormID] " +
        //                 "WHERE HAI.[ReqFormID] = @ReqFormID";

        //    using (var conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
        //    {
        //        conn_uat.Open();
        //        SqlCommand sqlcmd = new SqlCommand(sql, conn_uat);
        //        sqlcmd.Parameters.AddWithValue("@ReqFormID", reqFormID);

        //        using (IDataReader dr = sqlcmd.ExecuteReader())
        //        {
        //            if (dr.Read())
        //            {
        //                data.ReqFormID = dr["HAI_ReqFormID"].ToString();
        //                data.AssestCode = dr["ReqCode"].ToString();
        //                data.Assest = dr["Item"].ToString();
        //                data.quantity = dr["Quantity"].ToString();
        //                data.RDivision = dr["RDivision"].ToString();
        //                data.RDate = dr["RDate"].ToString();
        //            }
        //        }
        //    }

        //    return data;
        //}



        public static DataTable Get_Any_DT1(string Invoice_Ref)
        {
            DataTable DevicesinInvoices = new DataTable();
            try
            {
                using (var conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
                {
                    conn_uat.Open();
                    using (SqlCommand sqlcmd = new SqlCommand(Invoice_Ref, conn_uat))
                    {
                        SqlDataAdapter ad = new SqlDataAdapter(sqlcmd);
                        ad.Fill(DevicesinInvoices);
                    }
                    conn_uat.Close();
                }
            }
            catch (SqlException ex)
            {
                // Log the SQL exception or handle it as necessary
                Console.WriteLine("SQL Error: " + ex.Message);
            }
            catch (Exception ex)
            {
                // Log the general exception or handle it as necessary
                Console.WriteLine("Error: " + ex.Message);
            }
            return DevicesinInvoices;
        }



        public static EnterItemsModel GetDevicesInInvoice(string invoiceRef)
        {
            EnterItemsModel devices = new EnterItemsModel
            {
                DevicesinInvoices = new DataTable()
            };

            string sql = @"SELECT d.[Invoice_Reference], d.[Invoice_No], it.[Item_Type], d.[IT_No], d.[Serial_No], d.[INV_No], 
                          d.[QR], CONVERT(VARCHAR, d.[Installation_Date], 23) AS Installation_Date, 
                          div_p.[division] AS Purchased_Division, div_c.[division] AS Current_Location, 
                          d.[Current_LocationID], d.[Purchased_DivisionID] 
                   FROM [TechAssets].[dbo].[Hardware_Devices] d 
                   JOIN [TechAssets].[dbo].[Hardware_Inovice_Items] i ON d.[DescriptionCode] = i.[DescriptionCode] 
                   INNER JOIN [TechAssets].[dbo].[Hardware_Item_Types] it ON i.[Item_Type] = it.[Item_Type_Code] 
                   INNER JOIN [TechAssets].[dbo].[Hardware_DeviceStatus] ds ON d.[Current_Status] = ds.[StatusID] 
                   INNER JOIN [EMMSDB].[dbo].[Staff_Division] div_p ON d.[Purchased_DivisionID] = div_p.[div_index] 
                   INNER JOIN [EMMSDB].[dbo].[Staff_Division] div_c ON d.[Current_LocationID] = div_c.[div_index] 
                   WHERE d.[Invoice_Reference] = @InvoiceRef";

            using (var conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
            {
                conn_uat.Open();
                using (SqlCommand sqlcmd = new SqlCommand(sql, conn_uat))
                {
                    sqlcmd.Parameters.AddWithValue("@InvoiceRef", invoiceRef);

                    using (SqlDataAdapter adapter = new SqlDataAdapter(sqlcmd))
                    {
                        // Fill the DataTable
                        adapter.Fill(devices.DevicesinInvoices);
                    }

                    // Populate individual properties from the first row, if available
                    if (devices.DevicesinInvoices.Rows.Count > 0)
                    {
                        DataRow dr = devices.DevicesinInvoices.Rows[0];
                        devices.Invoice_Ref = dr["Invoice_Reference"].ToString();
                        devices.Invoice_No = dr["Invoice_No"].ToString();
                        devices.Item_Type = dr["Item_Type"].ToString();
                        devices.IT_No = dr["IT_No"].ToString();
                        devices.Serial_No = dr["Serial_No"].ToString();
                        devices.INV_No = dr["INV_No"].ToString();
                        devices.QR = dr["QR"].ToString();
                        devices.Installation_Date = dr["Installation_Date"].ToString();
                        devices.Purchased_Division = dr["Purchased_Division"].ToString().Trim();
                        devices.Current_Location = dr["Current_Location"].ToString().Trim();
                        devices.Purchased_DivisionID = dr["Purchased_DivisionID"].ToString().Trim();
                        devices.Current_LocationID = dr["Current_LocationID"].ToString().Trim();
                    }
                }
            }

            return devices;
        }

        public static List<SelectListItem> GetAssestsStatusListByDatabase()
        {
            List<SelectListItem> list = new List<SelectListItem>();

            string sql = "SELECT [id],[AssestStatus] FROM [TechAssets].[dbo].[Hardware_AssestStatus]";
            using (var conn_uat = new SqlConnection(ConfigurationManager.ConnectionStrings["connx"].ConnectionString))
            {
                conn_uat.Open();
                SqlCommand sqlcmd = new SqlCommand(sql, conn_uat);

                SqlDataReader dr = null;

                dr = sqlcmd.ExecuteReader();

                while (dr.Read())
                {
                    list.Add(new SelectListItem { Text = dr[1].ToString(), Value = dr[0].ToString() });
                }
                conn_uat.Close();
            }
            return list;
        }



        public static string GetBulkNo()
        {

            string BulkNo;
            DateTime sys_date = DateTime.Now;

            string year = sys_date.Year.ToString("0000");
            string month = sys_date.Month.ToString("00");
            string day = sys_date.Day.ToString("00");

            BulkNo = year + month + day;


            return BulkNo;
        }


        public static string GetDisposeBulkNo()
        {

            string DBulkNo;
            DateTime sys_date = DateTime.Now;
            string year = sys_date.Year.ToString("0000");
            DBulkNo = "Dispose-" + year.Substring(2, 2);
            return DBulkNo;

        }


    }

}
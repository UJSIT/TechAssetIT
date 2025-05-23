using System;
using System.Collections.Generic;
using System.Data;
using System.Web.Mvc;

namespace slbfeHardware.Models
{
    public class EnterItemsModel
    {
        public string Item_Type { get; set; }
        public string DevicesFull { get; set; }
        public string Invoice_No { get; set; }
        public string Invoice_Ref { get; set; }
        public string Invoice_Date { get; set; }
        public string Name_of_Supplier { get; set; }
        public string Item { get; set; }
        public string Warranty_Period { get; set; }
        public string description { get; set; }
        public string quantity { get; set; }
        public string unitprice { get; set; }
        public DateTime sys_Date { get; set; }
        public string sys_User { get; set; }
        public string status { get; set; }

        public string purchasedate { get; set; }
        public string purchasingprice { get; set; }


        public string Item_Type_Code { get; set; }

        public string Asset { get; set; }
        
        public string SelectedCompany { get; set; }

        public string BrandCode { get; set; }
        public string BrandName { get; set; }

        public string ItmTypeCode { get; set; }
        public string Brand_Code { get; set; }
        public string Model_Code { get; set; }

        public string ModelCode { get; set; }
        public string ModelName { get; set; }
        public string ShortCode { get; set; }

        public DataTable dt2 { get; set; }

        public DataTable dt4 { get; set; }

        public DataTable dt5 { get; set; }

        public DataTable dt6 { get; set; }

        public DataTable dt1 { get; set; }

        public DataTable dt8 { get; set; }

        public DataTable dt9 { get; set; }

        public DataTable dt10 { get; set; }    
        public DataTable UPHTbl { get; set; }
        public DataTable dt11 { get; set; }

        public DataTable dt12 { get; set; }

        public DataTable dt13 { get; set; }

        public DataTable HOHistory { get; set; }

        public DataTable Dinfor { get; set; }

        public DataTable Cinfor { get; set; }

        public DataTable COHistory { get; set; }
        public DataTable TBLUpdateHistory { get; set; }

        public DataTable DisTbl { get; set; }
        public DataTable AssestTbl { get; set; }

        public DataTable InvTbl { get; set; }

        public DataTable SpecTbl { get; set; }

        public DataTable LastHOTbl { get; set; }

        public DataTable Letterdt { get; set; }

        public DataTable DLetterdt { get; set; }

        public DataTable CRecommend { get; set; }

        public DataTable CFilter { get; set; }
        public DataTable OutReTBL { get; set; }


        public DataTable faultSolutions { get; set; }

        public string invno { get; set; }
        public string invID { get; set; }
        public string invoiceNo { get; set; }

        public string RedirectType { get; set; }

        public List<SelectListItem> ItemTypeList { get; set; }
        public List<SelectListItem> BrandList { get; set; }
        public List<SelectListItem> BrandCodeList { get; set; }
        public List<SelectListItem> ModelList { get; set; }


        public string IT_No { get; set; }
        public string Serial_No { get; set; }
        public string INV_No { get; set; }
        public string QR { get; set; }
        public string Purchased_Division { get; set; }
        public string Current_Location { get; set; }

        public string Purchased_DivisionID { get; set; }
        public string Current_LocationID { get; set; }


        public string Current_Status { get; set; }
        public string Authorized_Officer { get; set; }
        public string Installation_Date { get; set; }
        public string Remarks1 { get; set; }
        public string Remarks2 { get; set; }
        public string activeStatus { get; set; }
        public string DescriptionCode { get; set; }

        public List<SelectListItem> DivisionList { get; set; }

        public int invoice_item_count { get; set; }

        public string ItemInputVal { get; set; }
        public string BrandInputVal { get; set; }

        public string HOLocation { get; set; }
        public string NewHOLocation { get; set; }

        public string HOstatus { get; set; }
        public string sysRe_User { get; set; }
        public DateTime sysRe_Date { get; set; }
        public string sysApp_User { get; set; }
        public DateTime sysApp_Date { get; set; }
        public string Remarks3 { get; set; }

        public string PendingHandoverCount { get; set; }
        public string InITCount { get; set; }
        public string ProcessInITCount { get; set; }
        public string ProcessOutsideCount { get; set; }

        public string DisposeCount { get; set; }

        public string ReqDisposeDate { get; set; }
        public string ReqDisposeReason { get; set; }
        public string sysReqDate { get; set; }
        public string sysReqUser { get; set; }
        public string sysRecDate { get; set; }
        public string RecRemarks { get; set; }
        public string sysAppUser { get; set; }
        public string sysAppDate { get; set; }
        public string AppRemarks { get; set; }
        public string ITDisposeNo { get; set; }
        public string BulkNo { get; set; }
        public string FinalRemarks { get; set; }
        public string DisposeDate { get; set; }
        public string DisposeRemarks { get; set; }
        public string DisposeStatus { get; set; }
        public string RLetterRemarks { get; set; }
        public string RLetterDate { get; set; }
        public string DRejectRemarks { get; set; }

        public string DisYear { get; set; }
        public List<SelectListItem> Bulks { get; set; }
        public string Bulk { get; set; }
        public string DisNo { get; set; }

        public string LetterRDate { get; set; }

        public string CRemarks1 { get; set; }
        public string CourierLocation { get; set; }
        public string CourierLID { get; set; }
        public string CourierReqDate { get; set; }
        public string CourierStatus { get; set; }
        public string CsysReqUser { get; set; }
        public string CsysReqDate { get; set; }
        public string CompanyName { get; set; }
        public string StickerDate { get; set; }
        public string StickerNo { get; set; }
        public string CRemarks2 { get; set; }
        public string AdditionalInfo1 { get; set; }
        public string PickupDate { get; set; }
        public string CompleteDate { get; set; }
        public string CCompleteDate { get; set; }
        public string AdditionalInfo2 { get; set; }

        public string HOComDate { get; set; }
        public string HOComRemarks { get; set; }
        public string DAuthorized { get; set; }
        public string DCurrentLocation { get; set; }
        public string HOLid { get; set; }

        public string HStatus { get; set; }
        public string CStatus { get; set; }

        public string DStatus { get; set; }
        public string COLid { get; set; }


        public string hoid { get; set; }
        public int count { get; set; }

        public int remain { get; set; }

        public int remainP { get; set; }

        public string EmployeeName { get; set; }

        public string ConfirmUser { get; set; }

        public List<SelectListItem> CCategory { get; set; }
           

        public DataTable ReceptionPTable { get; set; }

        public string ItemReciveDate { get; set; }

        public string ItemReciveRemaks { get; set; }

        public string employeeNoTextbox { get; set; }

        public string employeeNameTextbox { get; set; }

        public string ErrorMessage { get; set; }

        public DataTable DutyAssingTBL { get; set; }

        public string dutyID { get; set; }

        public string DisposeID { get; set; }
        public string DutyAssignEmpNoSP { get; set; }
        public string DutyAssignOfficerSP { get; set; }
        public string DutyAssignRemaks { get; set; }

        public string Solutions { get; set; }

        public DataTable CardTBL { get; set; }

        public DataTable CardTBL1 { get; set; }

        public DataTable CardTBL2 { get; set; }

        public DataTable TaskTbl { get; set; }

        public DataTable CompleteTaskTBL { get; set; }

        public string thandoverIDRe { get; set; }

        public string FaultType { get; set; }
        public string FaultItem { get; set; }
        public string Fault_Solution { get; set; }
        public string FSStatus { get; set; }
        public DataTable faultList { get; set; }
        public string FSid { get; set; }



        public string SsysUser { get; set; }
        public string SsysDate { get; set; }
        public string Installation_Device { get; set; }
        public string RepairID { get; set; }
        public string Installation_User { get; set; }
        public string StoresRemarks { get; set; }



        public DataTable VThandoverTBL { get; set; }

        public DataTable VDutyATBL { get; set; }

        public DataTable RHFilter { get; set; }

        public DataTable CNFilter { get; set; }

        public DataTable ReHFilter { get; set; }


        public DataTable QReHFilter { get; set; }

        public string ReqFormID { get; set; }
        public string RDivision { get; set; }
        public string RDate { get; set; }
        public string AssestCode { get; set; }
        public byte[] File { get; set; }

        public string Assest { get; set; }
        public string AssestNO { get; set; }
        public string formIdRe { get; set; }
        public string assetIdInput { get; set; }

        public DataTable ARForm { get; set; }
        public DataTable DevicesinInvoices { get; set; }

        public DataTable Parts { get; set; }
        public List<SelectListItem> AssestsStatusList { get; set; }
        public string StatusID { get; set; }
        public string AssestRemarks { get; set; }
        public DataTable OutsideRepair { get; set; }

        public string ORBulkNo { get; set; }
        public string SentDate { get; set; }
        public string CompanyID { get; set; }
        public string Time { get; set; }
        public string DOfficerName { get; set; }
        public string SubjectOfficer { get; set; }
        public string BulkSysDate { get; set; }
        public string BulkRemarks { get; set; }
        public string ROStatus { get; set; }
        public byte[] NIC { get; set; }


        public string Zitno { get; set; }
        public string ZitnoNew { get; set; }

        public string Zserialno { get; set; }
        public string Zinvno { get; set; }
        public string Zqrno { get; set; }

        public string Zerem { get; set; }


        public List<SelectListItem> CompanySelectList { get; set; }

        public List<SelectListItem> RepairCompanySelectList { get; set; }

        public DataTable InOutsideRepair { get; set; }
        public string GPStatus { get; set; }
        public DataTable gatepassTBL { get; set; }

        public string dpNumber { get; set; }


        public string DisposeBulkNo { get; set; }
        public string DisposeBulkL { get; set; }

        public DataTable DApproval { get; set; }

        public string BulkNo1 { get; set; }

        public DataTable DisposeBulkTBL { get; set; }

        public string DisStatus { get; set; }

        public DataTable Inoutside { get; set; }

        public DataTable InIT { get; set; }

        public DataTable EditDTBL { get; set; }

        public string Eid { get; set; }
        public string Eitno { get; set; }
        public string Edate { get; set; }
        public string Esolution { get; set; }
        public string Eprice { get; set; }
        public string Ewarranty { get; set; }
        public string ERemarks { get; set; }




        public string Aid { get; set; }
        public string Aitno { get; set; }
        public string Aldate { get; set; }
        public string Astatus { get; set; }
        public string Arreson { get; set; }
        public string Ainformd { get; set; }

        public string Rid { get; set; }
        public string Ritno { get; set; }
        public string Rcdate { get; set; }
        public string Rremaks { get; set; }
        public string Rrornot { get; set; }
        public string Rdutyid { get; set; }

        public string Pid { get; set; }
        public string Pitno { get; set; }
        public string Pinvdate { get; set; }
        public string Pinvno { get; set; }
        public string Pvoudate { get; set; }
        public string Pvouno { get; set; }



        public string Ridate { get; set; }
        public string Rinvno { get; set; }


        public DataTable DinGatePass { get; set; }

        public DataTable PartsTBL { get; set; }
        public string GatepassNo { get; set; }
        public string RCompanyName { get; set; }
        public string RSentDate { get; set; }


        public string StickerLastNo { get; set; }

        public string PInsDevice { get; set; }

        public string PInsDate { get; set; }

        public string PInsRemaks { get; set; }

        public DataTable DisposeMoreD { get; set; }
        public string DispodeBulk { get; set; }



        public string Dbid { get; set; }
        public string Dinfdate { get; set; }
        public string Dinfremaks { get; set; }
        public string Dtakendate { get; set; }
        public string Dtakencom { get; set; }
        public string Dtakeremaks { get; set; }
        public string Dcomremaks { get; set; }

        public string Witno { get; set; }
        public string Wdutyid { get; set; }
        public string WcallDate { get; set; }
        public string Wcompany { get; set; }
        public string WcontactP { get; set; }
        public string Wremaks { get; set; }


        public string AssestRemaks1 { get; set; }
        public string AssestRemaks2 { get; set; }
        public string AssestRemaks3 { get; set; }
        public string AssestRemaks4 { get; set; }
        public string AssestRemaks5 { get; set; }

        public string AssestDate1 { get; set; }
        public string AssestDate2 { get; set; }
        public string AssestDate3 { get; set; }
        public string AssestDate4 { get; set; }
        public string AssestDate5 { get; set; }

        public DataTable TBLUpdateSpec { get; set; }



        public string Processor { get; set; }
        public string ProcessorSpeed { get; set; }
        public string Memory { get; set; }
        public string Storage { get; set; }
        public string StorageType { get; set; }
        public string OperatingSystem { get; set; }
        public string ScreenSize { get; set; }
        public string PanelType { get; set; }
        public string SpeRemarks { get; set; }

    }

}
using Data.Infrastructure;
using Service;
using System;
using System.Configuration;
using System.IO;
using System.Web.Mvc;
using Microsoft.Reporting.WebForms;
using System.Data.SqlClient;
using System.Data;
using System.Drawing;
using Reports.Common;
using Reports.Controllers;
using ThoughtWorks.QRCode.Codec;
using System.Collections.Generic;
using System.Linq;

namespace Jobs.Areas.Rug.Controllers
{
    [Authorize]
    public class Report_PackingPrintController : ReportController
    {

        public Report_PackingPrintController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        
        [HttpGet]
        public ActionResult PrintBarCode(int PackingHeaderId, string ListofRollNo)
        {
            string mQry, bConStr = "";
            DataTable DtTemp = new DataTable();
            DataTable Dt = new DataTable();
            
            bConStr = " AND H.PackingHeaderId In ( " + PackingHeaderId + " )";
            if (ListofRollNo == null)
            {
                ListofRollNo = "''";
            }

            { bConStr = bConStr + " AND L.BaleNo In ( " + ListofRollNo + " )"; }


            mQry = "SELECT B.Name AS BuyerName, P.ProductName AS CarpetSKU, SOH.DocNo AS SaleOrder , SOH.SaleOrderHeaderId, " +
                    " PB.BuyerUpcCode AS UpcCode,isnumeric(PB.BuyerUpcCode) AS BuyerUpcCodeNumeric, L.ProductId, L.Qty, L.PartyProductUid, isnull(PG.ProductTypeId,0) AS ProductTypeId, " +
                    " PU.ProductUidName, L.BaleNo, SOH.SaleToBuyerId, PB.BuyerSku " +
                    " FROM Web.PackingHeaders H " +
                    " LEFT JOIN Web.PackingLines L ON L.PackingHeaderId = H.PackingHeaderId  " +
                    " LEFT JOIN Web.Products P ON P.ProductId = L.ProductId  " +
                    " LEFT JOIN Web.ProductUids PU ON PU.ProductUIDId = L.ProductUidId " +
                    " LEFT JOIN web.SaleOrderLines SOL ON SOL.SaleOrderLineId = L.SaleOrderLineId  " +
                    " LEFT JOIN Web.SaleOrderHeaders SOH ON SOH.SaleOrderHeaderId = SOL.SaleOrderHeaderId  " +
                    " LEFT JOIN Web.ProductBuyers PB ON PB.ProductId = L.ProductId AND PB.BuyerId = H.BuyerId  " +
                    " LEFT JOIN Web.People B ON B.PersonID = SOH.SaleToBuyerId " +
                    " LEFT JOIN WEb.ProductGroups PG ON PG.ProductGroupId = P.ProductGroupId " +
                    " Where 1=1  " + bConStr +
                    " ORDER BY H.DocDate, H.PackingHeaderId, L.PackingLineId ";

            SqlConnection con = new SqlConnection(connectionString);
            if (con.State != ConnectionState.Open)
            {
                con.Open();
            }


            SqlDataAdapter sqlDataAapter = new SqlDataAdapter(mQry, con);
            dsRep.EnforceConstraints = false;
            sqlDataAapter.Fill(DtTemp);

            if (DtTemp.Rows.Count > 0)
            {
                if (DtTemp.Rows[0]["BuyerName"].ToString() == "Home Decorators Collection")
                {                
                    RepName = "Packing_HDCLabelPrint";                    
                    Dt = FGetDataForHDCLabelPrint(DtTemp, con);
                }
                else if (DtTemp.Rows[0]["BuyerName"].ToString() == "Artistic Weavers")
                {
                    //RepName = "Packing_AWLabelPrint";
                    //RepName = "Packing_AWLabelPrintNew";
                    RepName = "Packing_AWLabelPrintNew11317";
                    Dt = FGetDataForSCILabelPrint(DtTemp, con);
                }
                else
                {
                    //RepName = "Packing_LabelPrint";
                    if (DtTemp.Rows[0]["ProductTypeId"].ToString() != "1")
                    {
                        RepName = "Packing_LabelPrint_ForPouf";
                        Dt = FGetDataForSCILabelPrint(DtTemp, con);
                    }
                    else
                    {
                        //RepName = "Packing_LabelPrintNew";                       
                        //RepName = "Packing_LabelPrintNew_1422017";
                        RepName = ReportName(DtTemp);
                        Dt = FGetDataForSCILabelPrint(DtTemp, con);
                    }
                }

                reportdatasource = new ReportDataSource("DsMain", Dt);
                reportViewer.LocalReport.ReportPath = Request.MapPath(ConfigurationManager.AppSettings["ReportsPath"] + RepName + ".rdlc");
                ReportService reportservice = new ReportService();
                reportservice.SetReportAttibutes(reportViewer, reportdatasource, "Packing Label Print", "");
                return PrintReport(reportViewer, "PDF");
            }

            else
            { return View("Close"); }


        }

        [HttpGet]
        public ActionResult PrintOnlyQRCode(int PackingHeaderId, string ListofRollNo)
        {
            string mQry, bConStr = "";
            DataTable DtTemp = new DataTable();
            DataTable Dt = new DataTable();

            bConStr = " AND H.PackingHeaderId In ( " + PackingHeaderId + " )";
            if (ListofRollNo == null)
            {
                ListofRollNo = "''";
            }

            { bConStr = bConStr + " AND L.BaleNo In ( " + ListofRollNo + " )"; }


            mQry = "SELECT B.Name AS BuyerName, P.ProductName AS CarpetSKU, SOH.DocNo AS SaleOrder , SOH.SaleOrderHeaderId, " +
                    " ISNULL(PB.BuyerUpcCode,'') AS UpcCode, isnumeric(PB.BuyerUpcCode) AS BuyerUpcCodeNumeric, L.ProductId, L.Qty, L.PartyProductUid,  " +
                    " PU.ProductUidName, L.BaleNo, SOH.SaleToBuyerId, PB.BuyerSku " +
                    " FROM Web.PackingHeaders H " +
                    " LEFT JOIN Web.PackingLines L ON L.PackingHeaderId = H.PackingHeaderId  " +
                    " LEFT JOIN Web.Products P ON P.ProductId = L.ProductId  " +
                    " LEFT JOIN Web.ProductUids PU ON PU.ProductUIDId = L.ProductUidId " +
                    " LEFT JOIN web.SaleOrderLines SOL ON SOL.SaleOrderLineId = L.SaleOrderLineId  " +
                    " LEFT JOIN Web.SaleOrderHeaders SOH ON SOH.SaleOrderHeaderId = SOL.SaleOrderHeaderId  " +
                    " LEFT JOIN Web.ProductBuyers PB ON PB.ProductId = L.ProductId AND PB.BuyerId = H.BuyerId  " +
                    " LEFT JOIN Web.People B ON B.PersonID = SOH.SaleToBuyerId " +
                    " Where 1=1  " + bConStr +
                    " ORDER BY H.DocDate, H.PackingHeaderId, L.PackingLineId ";

            SqlConnection con = new SqlConnection(connectionString);
            if (con.State != ConnectionState.Open)
            {
                con.Open();
            }


            SqlDataAdapter sqlDataAapter = new SqlDataAdapter(mQry, con);
            dsRep.EnforceConstraints = false;
            sqlDataAapter.Fill(DtTemp);

            if (DtTemp.Rows.Count > 0)
            {

                RepName = "Packing_LabelPrint_OnlyQRCode";
                Dt = FGetDataForSCILabelPrint(DtTemp, con);


                reportdatasource = new ReportDataSource("DsMain", Dt);
                //reportViewer.LocalReport.ReportPath = Request.MapPath(ConfigurationManager.AppSettings["ReportsPath"] + RepName + ".rdlc");
                reportViewer.LocalReport.ReportPath = ConfigurationManager.AppSettings["PhysicalRDLCPath"] + ConfigurationManager.AppSettings["ReportsPathFromService"] + RepName + ".rdlc";
                ReportService reportservice = new ReportService();
                reportservice.SetReportAttibutes(reportViewer, reportdatasource, "Packing Label Print", "");
                return PrintReport(reportViewer, "PDF");
            }

            else
            {
                return View("Close");
            }
        }

        public string ReportName(DataTable dt)
        {
            string Report = string.Empty;
            string SaleOrderName = string.Empty;
            int res = 0;
            List<string> lst = new List<string> { };
            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i <= dt.Rows.Count - 1; i++)
                {
                    lst.Add(dt.Rows[i]["SaleOrder"].ToString().Substring(0, 3));
                }

                res = (from x in lst select x).Distinct().Count();
                SaleOrderName = lst.FirstOrDefault();
            }

            if (res == 1)
            {
                if (SaleOrderName == "HOS")
                {
                    Report = "Packing_LabelPrintHOS_02112017";
                }
                else if (SaleOrderName == "CUS")

                {
                    Report = "Packing_LabelPrintCUS_02112017";
                }
                else
                {
                    Report = "Packing_LabelPrintNew_1422017";
                }
            }
            else
            {
                Report = "Packing_LabelPrintMultiplePrint";
            }

            return Report;
        }
        public DataTable FGetDataForSCILabelPrint(DataTable DtTemp, SqlConnection con)
        {
            QRCodeEncoder QRCodeEncoder;
            QRCodeEncoder = new QRCodeEncoder();
            DataTable Dt = new DataTable();

            Int16 I = 0;
            Int16 J = 0;
           // Int16 K = 0;
            string bTempTable = "", bSerial = "", bPrintSign = "";
            string mQry = "";
            bTempTable = "TempUIdTable";

            string BaleCarpetSKU = "";

            mQry = "CREATE TABLE [#" + bTempTable + "] " +
                    " (BarCode Image, CarpetSku nVarChar(100), SaleOrder nVarChar(100), SaleOrderHeaderId nVarChar(100), Serial nVarChar(100), UPCCode Image, UPCCodeValue nVarChar(100), ProductId nVarChar(100), PrintSign VarChar(2), BaleNo nVarChar(10), CarpetNo nVarChar(10), CarpetNoImg Image ,CarpetSKUImg Image, UPCCodeImg Image,  BaleNoCarpetImg Image )";
            SqlCommand Cmd = new SqlCommand(mQry.ToString(), con);
            Cmd.ExecuteNonQuery();


            if (DtTemp.Rows.Count > 0)
            {
                for (I = 0; I <= DtTemp.Rows.Count - 1; I++)
                {

                    //        '================= Setting QrCode Encode Mode ===================
                    //        '====Possible Values Are 
                    //        '====1.QRCodeEncoder.ENCODE_MODE.Byte()
                    //        '====2.QRCodeEncoder.ENCODE_MODE.ALPHA_NUMERIC()
                    //        '====3.QRCodeEncoder.ENCODE_MODE.NUMERIC()
                    //        '================================================================
                    QRCodeEncoder.QRCodeEncodeMode = QRCodeEncoder.ENCODE_MODE.BYTE;
                    //        '================================================================


                    //        '======================= Setting QrCode Scale ===================
                    QRCodeEncoder.QRCodeScale = 3;
                    //        '================================================================


                    //        '================= Setting QrCode Encode Mode ===================
                    //        '====Possible Values Are Between 1 to 40
                    //        '================================================================
                    QRCodeEncoder.QRCodeVersion = 3;
                    //        '================================================================

                    //        '================= Setting QrCode Encode Mode ===================
                    //        '====Possible Values Are L,M,Q,H
                    //        '================================================================
                    QRCodeEncoder.QRCodeErrorCorrect = QRCodeEncoder.ERROR_CORRECTION.M;
                    //        '================================================================


                    for (J = 1; J <= Convert.ToDouble(DtTemp.Rows[I]["Qty"].ToString()); J++)
                    {
                        //For Bar Code Mapping Work
                        if (DtTemp.Rows[I]["PartyProductUid"].ToString() != "")
                        {
                            //bSerial = DtTemp.Rows[I]["PartyProductUid"].ToString();
                            bSerial = DtTemp.Rows[I]["PartyProductUid"].ToString().Substring(DtTemp.Rows[I]["PartyProductUid"].ToString().Length - 5);
                            bPrintSign = "";
                        }
                        else
                            bPrintSign = "*";

                        //End For Bar Code Mapping

                        

                        System.Drawing.Bitmap Bmp = QRCodeEncoder.Encode(DtTemp.Rows[I]["SaleOrder"].ToString() + "|" + DtTemp.Rows[I]["CarpetSku"].ToString() + "_" + bSerial);
                        System.IO.MemoryStream Stream = new System.IO.MemoryStream();
                        Byte[] Data;

                        Bmp.Save(Stream, System.Drawing.Imaging.ImageFormat.Bmp);
                        Stream.Position = 0;
                        Data = Stream.GetBuffer();

                        Image ImgUPCCode = FRetUPCImage(DtTemp.Rows[I]["UPCCode"].ToString());
                        System.IO.MemoryStream StreamUPCCode = new System.IO.MemoryStream();
                        Byte[] ByteUPCCode;

                        if (ImgUPCCode != null)
                            ImgUPCCode.Save(StreamUPCCode, System.Drawing.Imaging.ImageFormat.Bmp);

                        StreamUPCCode.Position = 0;
                        ByteUPCCode = StreamUPCCode.GetBuffer();



                        String sSQL = "Insert Into [#" + bTempTable + "] (BarCode, SaleOrder, SaleOrderHeaderId, CarpetSku, Serial, UPCCode, UPCCodeValue, ProductId, PrintSign, BaleNo, CarpetNo, CarpetNoImg, CarpetSKUImg, UPCCodeImg, BaleNoCarpetImg) Values(@BarCode, @SaleOrder, @SaleOrderHeaderId, @CarpetSku, @Serial, @UPCCode, @UPCCodeValue, @ProductId, @PrintSign, @BaleNo, @CarpetNo, @CarpetNoImg, @CarpetSKUImg, @UPCCodeImg, @BaleNoCarpetImg )";

                        SqlCommand cmd = new SqlCommand(sSQL, con);

                        SqlParameter BarCode = new SqlParameter("@BarCode", SqlDbType.Image);
                        SqlParameter SaleOrder = new SqlParameter("@SaleOrder", SqlDbType.VarChar);
                        SqlParameter SaleOrderHeaderId = new SqlParameter("@SaleOrderHeaderId", SqlDbType.Int);
                        SqlParameter CarpetSku = new SqlParameter("@CarpetSku", SqlDbType.VarChar);
                        SqlParameter Serial = new SqlParameter("@Serial", SqlDbType.VarChar);
                        SqlParameter UPCCodeValue = new SqlParameter("@UPCCodeValue", SqlDbType.VarChar);
                        SqlParameter UPCCode = new SqlParameter("@UPCCode", SqlDbType.Image);
                        SqlParameter ProductId = new SqlParameter("@ProductId", SqlDbType.Int);
                        SqlParameter PrintSign = new SqlParameter("@PrintSign", SqlDbType.VarChar);
                        SqlParameter BaleNo = new SqlParameter("@BaleNo", SqlDbType.VarChar);
                        SqlParameter CarpetNo = new SqlParameter("@CarpetNo", SqlDbType.VarChar);
                        SqlParameter CarpetNoImg = new SqlParameter("@CarpetNoImg", SqlDbType.Image);
                        SqlParameter CarpetSKUImg = new SqlParameter("@CarpetSKUImg", SqlDbType.Image);
                        SqlParameter UPCCodeImg = new SqlParameter("@UPCCodeImg", SqlDbType.Image);
                        SqlParameter BaleNoCarpetImg = new SqlParameter("@BaleNoCarpetImg", SqlDbType.Image);

                        BarCode.Value = Data;
                        SaleOrder.Value = DtTemp.Rows[I]["SaleOrder"];
                        SaleOrderHeaderId.Value = DtTemp.Rows[I]["SaleOrderHeaderId"];
                        CarpetSku.Value = DtTemp.Rows[I]["CarpetSku"];
                        Serial.Value = bSerial;
                        ProductId.Value = DtTemp.Rows[I]["ProductId"];
                        PrintSign.Value = bPrintSign;
                        UPCCode.Value = ByteUPCCode;

                      

                        UPCCodeValue.Value = DtTemp.Rows[I]["UPCCode"];
                        BaleNo.Value = DtTemp.Rows[I]["BaleNo"];
                        CarpetNo.Value = DtTemp.Rows[I]["ProductUidName"];

                        if (DtTemp.Rows[I]["ProductUidName"].ToString() != "")
                            CarpetNoImg.Value = PrintToBarCode(DtTemp.Rows[I]["ProductUidName"].ToString(), 500, 150);
                        else
                            CarpetNoImg.Value = PrintToBarCode("0", 300, 100);


                        if (DtTemp.Rows[I]["CarpetSku"].ToString() != "")
                            CarpetSKUImg.Value = PrintToBarCode(DtTemp.Rows[I]["CarpetSku"].ToString(), 500, 150);
                        else
                            CarpetSKUImg.Value = PrintToBarCode("0", 300, 100);


                        if (DtTemp.Rows[I]["UPCCode"].ToString() != "" && DtTemp.Rows[I]["BuyerUpcCodeNumeric"].ToString() == "1")
                            UPCCodeImg.Value  = PrintToUPCCode(DtTemp.Rows[I]["UPCCode"].ToString(), 500, 150);
                        else
                            UPCCodeImg.Value = PrintToUPCCode("0", 300, 100);

                        if (DtTemp.Rows[I]["BaleNo"].ToString() != "" && DtTemp.Rows[I]["CarpetSku"].ToString() != "")
                            {
                                BaleCarpetSKU = DtTemp.Rows[I]["BaleNo"].ToString() + '-' + DtTemp.Rows[I]["CarpetSku"].ToString();
                                BaleNoCarpetImg.Value = PrintToBarCode(BaleCarpetSKU, 600, 150);
                            }
                        else
                            BaleNoCarpetImg.Value = PrintToBarCode("0", 300, 100);


                        cmd.Parameters.Add(BarCode);
                        cmd.Parameters.Add(SaleOrder);
                        cmd.Parameters.Add(SaleOrderHeaderId);
                        cmd.Parameters.Add(CarpetSku);
                        cmd.Parameters.Add(Serial);
                        cmd.Parameters.Add(ProductId);
                        cmd.Parameters.Add(PrintSign);
                        cmd.Parameters.Add(UPCCode);
                        cmd.Parameters.Add(UPCCodeValue);
                        cmd.Parameters.Add(BaleNo);
                        cmd.Parameters.Add(CarpetNo);
                        cmd.Parameters.Add(CarpetNoImg);
                        cmd.Parameters.Add(CarpetSKUImg);
                        cmd.Parameters.Add(UPCCodeImg);
                        cmd.Parameters.Add(BaleNoCarpetImg);

                        cmd.ExecuteNonQuery();

                    }

                }


                String strQry = "";
                //if (PrintReportType == "Print For Artistic Weavers")
                //    RepName = "Packing_AWLabelPrint";
                //else
                //    RepName = "Packing_LabelPrint";

                strQry = " Select H.BarCode, H.UPCCode, H.UPCCodeValue, ISNULL(IB.BuyerSKU,H.CarpetSku) AS CarpetSku, ISNULL(H.SaleOrder,'') AS SaleOrder, H.SaleOrderHeaderId, ISNULL(H.Serial,'') AS Serial, H.ProductId, So.SaleToBuyerId, " +
                        " ISNULL(IB.BuyerSpecification2, PC.ProductCollectionName) As Collection, ISNULL(IB.BuyerSpecification1,PG.ProductGroupName) As Design, " +
                        " S.SizeName + IsNull(Prs.ProductShapeShortName,'') As Size, PCH.ProductContentName AS FaceContents,PCH1.ProductContentName AS Contents, H.PrintSign, NUll AS Construction, " +
                        " H.BaleNo, H.CarpetNo, ISNULL(H.CarpetNoImg,'') AS CarpetNoImg , ISNULL(H.CarpetSKUImg,'') AS CarpetSKUImg, ISNULL(H.UPCCodeImg,'') AS UPCCodeImg, ISNULL(H.BaleNoCarpetImg,'') AS BaleNoCarpetImg,isnull(IB.BuyerSpecification,' ') as BuyerSpecification,isnull(PG.ProductGroupName,' ') as GroupName,isnull(IB.BuyerSpecification5,' ') as BuyerSpecification5" + 
                        " From [#" + bTempTable + "] H " +
                        " LEFT JOIN Web.SaleOrderHeaders So On H.SaleOrderHeaderId = So.SaleOrderHeaderId  " +
                        " LEFT JOIN Web.ProductBuyers IB On H.ProductId = IB.ProductId AND So.SaleToBuyerId = IB.BuyerId " +
                        " LEFT JOIN Web.Products P On  P.ProductId = H.ProductId " +
                        " LEFT JOIN Web.ProductGroups PG ON PG.ProductGroupId = P.ProductGroupId " +
                        " LEFT JOIN Web.FinishedProduct FP ON FP.ProductId = P.ProductId  " +
                        " LEFT JOIN Web.ProductShapes Prs On Fp.ProductShapeId = Prs.ProductShapeId " + 
                        " LEFT JOIN Web.ProductCollections PC ON PC.ProductCollectionId = FP.ProductCollectionId  " +
                        " LEFT JOIN Web.ProductSizes PS ON PS.ProductId = P.ProductId AND PS.ProductSizeTypeId = 1 " +
                        " LEFT JOIN WEb.Sizes S ON S.SizeId = PS.SizeId   " +
                        " LEFT JOIN web.ProductContentHeaders PCH ON PCH.ProductContentHeaderId = FP.FaceContentId  " +
                        " LEFT JOIN web.ProductContentHeaders PCH1 ON PCH1.ProductContentHeaderId = FP.ContentId  " +
                        " Order By (Case When IsNumeric(H.BaleNo)>0 Then Convert(Numeric,H.BaleNo) Else 0 End) ";

               
                SqlDataAdapter sqlDataAapter = new SqlDataAdapter(strQry.ToString(), con);
                dsRep.EnforceConstraints = false;
                sqlDataAapter.Fill(Dt);
            }
        return Dt;
     }

        public DataTable FGetDataForHDCLabelPrint(DataTable DtTemp, SqlConnection con)
        {
            int I = 0;
            string mQry = "";
            DataSet DsRep = new DataSet();
            String bTempTable, strQry = "";
            DataTable Dt = new DataTable();

            bTempTable = "TempUIdTable";


            mQry = "CREATE TABLE [#" + bTempTable + "] " +
                    " ( CarpetSku nVarChar(100), SaleOrder nVarChar(100), ProductId nVarChar(100), " +
                    " SaleToBuyerId nVarChar(10), BuyerSKUImg Image, BuyerSKU nVarChar(100), SaleOrderImg Image , BaleNo nVarChar(10) )";


            SqlCommand Cmd = new SqlCommand(mQry.ToString(), con);
            Cmd.ExecuteNonQuery();



            if (DtTemp.Rows.Count > 0)
            {
                for (I = 0; I <= DtTemp.Rows.Count - 1; I++)
                {
                    String sSQL = "Insert Into [#" + bTempTable + "] ( SaleOrder, CarpetSku, ProductId, SaleToBuyerId, BuyerSKUImg, BuyerSKU, SaleOrderImg, BaleNo ) Values(@SaleOrder, @CarpetSku, @ProductId, @SaleToBuyerId, @BuyerSKUImg, @BuyerSKU, @SaleOrderImg, @BaleNo )";

                    SqlCommand cmd = new SqlCommand(sSQL, con);

                    SqlParameter SaleOrder = new SqlParameter("@SaleOrder", SqlDbType.VarChar);
                    SqlParameter CarpetSku = new SqlParameter("@CarpetSku", SqlDbType.VarChar);
                    SqlParameter ProductId = new SqlParameter("@ProductId", SqlDbType.Int);
                    SqlParameter SaleToBuyerId = new SqlParameter("@SaleToBuyerId", SqlDbType.Int);
                    SqlParameter BuyerSKUImg = new SqlParameter("@BuyerSKUImg", SqlDbType.Image);
                    SqlParameter BuyerSKU = new SqlParameter("@BuyerSKU", SqlDbType.VarChar);
                    SqlParameter SaleOrderImg = new SqlParameter("@SaleOrderImg", SqlDbType.Image);
                    SqlParameter BaleNo = new SqlParameter("@BaleNo", SqlDbType.VarChar);




                    SaleOrder.Value = DtTemp.Rows[I]["SaleOrder"].ToString();
                    CarpetSku.Value = DtTemp.Rows[I]["CarpetSku"].ToString();
                    ProductId.Value = DtTemp.Rows[I]["ProductId"].ToString();
                    SaleToBuyerId.Value = DtTemp.Rows[I]["SaleToBuyerId"].ToString();

                    if (DtTemp.Rows[I]["BuyerSKU"].ToString() != "")
                        BuyerSKUImg.Value = PrintToBarCode(DtTemp.Rows[I]["BuyerSKU"].ToString(), 450, 150);
                    else
                        BuyerSKUImg.Value = PrintToBarCode("0", 400, 150);


                    string TempStr = DtTemp.Rows[I]["BuyerSKU"].ToString();
                    string str = "";
                    for (int LS = 0; LS <= TempStr.Length - 1; LS++)
                    {
                        if (str.Length == 0 && str == "")
                        {
                            str = TempStr.Substring(LS, 1);
                        }
                        else
                        {
                            str = str + " " + TempStr.Substring(LS, 1);
                        }

                    }

                    BuyerSKU.Value = str;

                    if (DtTemp.Rows[I]["SaleOrder"].ToString() != "")
                        SaleOrderImg.Value = PrintToBarCode(DtTemp.Rows[I]["SaleOrder"].ToString(), 500, 300);
                    else
                        SaleOrderImg.Value = PrintToBarCode("0", 400, 150);

                    BaleNo.Value = DtTemp.Rows[I]["BaleNo"].ToString();

                    cmd.Parameters.Add(SaleOrder);
                    cmd.Parameters.Add(CarpetSku);
                    cmd.Parameters.Add(ProductId);
                    cmd.Parameters.Add(SaleToBuyerId);
                    cmd.Parameters.Add(BuyerSKUImg);
                    cmd.Parameters.Add(SaleOrderImg);
                    cmd.Parameters.Add(BaleNo);
                    cmd.Parameters.Add(BuyerSKU);
                    cmd.ExecuteNonQuery();
                }

                strQry = "Select H.CarpetSku, H.SaleOrder, H.SaleToBuyerId, H.BuyerSKUImg, H.SaleOrderImg, H.BaleNo, " +
                        " H.BuyerSku AS BuyerSkuToPrint, IB.BuyerSku, IB.BuyerSpecification, IB.BuyerSpecification1, IB.BuyerSpecification2, IB.BuyerSpecification3, " +
                        " ISNULL(IB.BuyerSpecification4,'100% Wool Pile') AS BuyerSpecification4, IB.BuyerSpecification5 AS BuyerSpecification5, IB.BuyerSpecification6 " +
                        " From [#" + bTempTable + "] H " +
                        " LEFT JOIN Web.ProductBuyers IB On H.ProductId = IB.ProductId AND H.SaleToBuyerId = IB.BuyerId ";
               

                
                SqlDataAdapter DA = new SqlDataAdapter(strQry.ToString(), con);
                dsRep.EnforceConstraints = false;
                DA.Fill(Dt);                

            }
            return Dt;
        }
        
        public Image FRetUPCImage(String UPCCode)
        {
            Image FRetUPCImage;
            CUPCA upca = new CUPCA();

            if (UPCCode.Length == 12)
            {
                UPCCode = UPCCode.Substring(0, 11) + upca.GetCheckSum(UPCCode).ToString();
                FRetUPCImage = upca.CreateBarCode(UPCCode, 3);
            }
            else
                FRetUPCImage = null;

            return FRetUPCImage;
        }


        public Byte[] PrintToBarCode(String TextValue, int Width, int Hight)
        {
            Byte[] PrintToBarCode;
            BarcodeLib.Barcode b;
            b = new BarcodeLib.Barcode();

            Image Img;
            b.Alignment = BarcodeLib.AlignmentPositions.CENTER;
            b.IncludeLabel = false;
            b.RotateFlipType = RotateFlipType.RotateNoneFlipNone;
            b.LabelPosition = BarcodeLib.LabelPositions.BOTTOMCENTER;

            if (TextValue == "0")
            Img = b.Encode(BarcodeLib.TYPE.CODE39Extended, TextValue, Color.White, Color.White, Width, Hight);
            else
            Img = b.Encode(BarcodeLib.TYPE.CODE39Extended, TextValue, Color.Black, Color.White, Width, Hight);

            PrintToBarCode = b.Encoded_Image_Bytes;
            return PrintToBarCode;
        }

        public Byte[] PrintToUPCCode(String TextValue, int Width, int Hight)
        {
            Byte[] PrintToBarCode;
            BarcodeLib.Barcode b;
            b = new BarcodeLib.Barcode();

            Image Img;
            b.Alignment = BarcodeLib.AlignmentPositions.CENTER;
            b.IncludeLabel = false;
            b.RotateFlipType = RotateFlipType.RotateNoneFlipNone;
            b.LabelPosition = BarcodeLib.LabelPositions.BOTTOMCENTER;

            if (TextValue == "0")
                Img = b.Encode(BarcodeLib.TYPE.UPCA , "00000000000", Color.White, Color.White, Width, Hight);
            else
                Img = b.Encode(BarcodeLib.TYPE.UPCA, TextValue, Color.Black, Color.White, Width, Hight);

            PrintToBarCode = b.Encoded_Image_Bytes;
            return PrintToBarCode;
        }
    }
}
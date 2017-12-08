using Microsoft.Owin.Security;
using Data.Infrastructure;
using Data.Models;
using Model.ViewModels;
using Service;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Model.Models;
using System.Data.SqlClient;
using System.Data;
using System.Text;
using Core.Common;
using Jobs.Helpers;
using Core;
using Model.ViewModel;
using System.Data.OleDb;
using System.Globalization;


namespace Customize
{
    [Authorize]
    public class ImportController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        protected IUnitOfWork _unitOfWork;
        IImportLineService _ImportLineService;
        IImportHeaderService _ImportHeaderservice;
        public ImportController(IUnitOfWork unitOfWork, IImportLineService line, IImportHeaderService ImportHeaderServ)
        {
            _unitOfWork = unitOfWork;
            _ImportLineService = line;
            _ImportHeaderservice = ImportHeaderServ;
        }

        [HttpGet]
        public ActionResult Import(int MenuId, int id)//id == DocTypeId
        {
            Menu menu = new MenuService(_unitOfWork).Find(MenuId);
            ImportHeader header = _ImportHeaderservice.GetImportHeaderByName(menu.MenuName);
            List<ImportLine> lines = _ImportLineService.GetImportLineList(header.ImportHeaderId).ToList();

            Dictionary<int, string> DefaultValues = TempData["ImportLayoutDefaultValues"] as Dictionary<int, string>;
            TempData["ImportLayoutDefaultValues"] = DefaultValues;
            foreach (var item in lines)
            {
                if (DefaultValues != null && DefaultValues.ContainsKey(item.ImportLineId))
                {
                    item.DefaultValue = DefaultValues[item.ImportLineId];
                }
            }


            ImportMasterViewModel vm = new ImportMasterViewModel();

            if (TempData["closeOnSelectOption"] != null)
                vm.closeOnSelect = (bool)TempData["closeOnSelectOption"];

            vm.ImportHeader = header;
            vm.ImportLine = lines;
            vm.ImportHeaderId = header.ImportHeaderId;
            vm.DocTypeId = id;

            int ImportMessageCnt = (from H in db.ImportMessage where H.ImportHeaderId == header.ImportHeaderId && H.IsActive == true && H.ValueType == "Error" select H).Count();
            ViewBag.ImportMessageCnt = ImportMessageCnt;

            return View(vm);
        }

        [HttpPost]
        public ActionResult Import(FormCollection form, ImportMasterViewModel vm)
        {
            string[] StrArr = new string[] { };

            //int MenuId = Convert.ToInt32(Request.QueryString["MenuId"]);

            int ImportHeaderId = vm.ImportHeaderId;
            int DocTypeId = vm.DocTypeId;



            string ErrorText = "";
            //string WarningText = "";

            if (Request.Files.Count == 0 || Request.Files[0].FileName == "")
            {
                ViewBag.id = form["DocTypeId"];
                ModelState.AddModelError("", "Please select file.");
                return View("Import", vm);
            }

            string ConnectionString = (string)System.Web.HttpContext.Current.Session["DefaultConnectionString"];
            SqlConnection sqlConnection = new SqlConnection(ConnectionString);
            sqlConnection.Open();
            
            var ImportHeader = new ImportHeaderService(_unitOfWork).Find(ImportHeaderId);

            for (int filecnt = 0; filecnt <= Request.Files.Count - 1; filecnt++)
            {
                var file = Request.Files[filecnt];
                string filePath = Request.MapPath(ConfigurationManager.AppSettings["ExcelFilePath"] +  file.FileName);
                file.SaveAs(filePath);

                DataTable RecordList = new DataTable();

                if (System.IO.Path.GetExtension(file.FileName).ToUpper() == ".XLS" || System.IO.Path.GetExtension(file.FileName).ToUpper() == ".XLSX")
                {
                    var excel = new ExcelQueryFactory();
                    excel.FileName = filePath;

                    RecordList = exceldata(filePath);
                }
                else if (System.IO.Path.GetExtension(file.FileName).ToUpper() == ".TXT")
                {
                    RecordList = GetDataSourceFromFile(filePath);
                }
                else
                {
                    ModelState.AddModelError("", "File is not in correct format.");
                    return View("Import", vm);
                }


                if (RecordList.Rows.Count == 0)
                {
                    ModelState.AddModelError("", "There is no records to import.");
                    return View("Import", vm);
                }


                var ImportLineList = _ImportLineService.GetImportLineList(ImportHeaderId).Where(m => m.FileNo == (filecnt + 1));


                foreach (var ImportLine in ImportLineList)
                {
                    if (ImportLine.DataType != "Input-Text" && ImportLine.DataType != "Input-Number" && ImportLine.DataType != "Input-Date" && ImportLine.DataType != "Single Select" && ImportLine.DataType != "Multi Select")
                    {
                        if (ImportLine.IsVisible == true && RecordList.Columns.Contains(ImportLine.FieldName) == false && ImportLine.FileNo == filecnt + 1)
                        {
                            ModelState.AddModelError("", ImportLine.FieldName + " is manadatary for file " + ImportLine.Type + ", but file does not contain this column.");
                            return View("Import", vm);
                        }

                        if (RecordList.Columns.Contains(ImportLine.FieldName))
                        {
                            if (ImportLine.DataType == "Number" && (RecordList.Columns[ImportLine.FieldName].DataType.Name.ToString() != "Decimal"
                                         && RecordList.Columns[ImportLine.FieldName].DataType.Name.ToString() != "Double"
                                         && RecordList.Columns[ImportLine.FieldName].DataType.Name.ToString() != "Float"
                                         && RecordList.Columns[ImportLine.FieldName].DataType.Name.ToString() != "Int"))
                            {
                                ModelState.AddModelError("", ImportLine.FieldName + " should be number for file " + ImportLine.Type + ".");
                                return View("Import", vm);
                            }
                        }
                    }



                    if (ImportLine.IsMandatory == true && ImportLine.FileNo == filecnt + 1)
                    {
                        for (int i = 0; i <= RecordList.Rows.Count - 1; i++)
                        {
                            if (RecordList.Rows[i][ImportLine.FieldName] == null)
                            {
                                ModelState.AddModelError("", ImportLine.FieldName + " is manadatary, it is blank at row no." + (i + 2).ToString());
                                return View("Import", vm);
                            }
                            else if (RecordList.Rows[i][ImportLine.FieldName].ToString() == "")
                            {
                                ModelState.AddModelError("", ImportLine.FieldName + " is manadatary, it is blank at row no." + (i + 2).ToString());
                                return View("Import", vm);
                            }
                        }
                    }

                    if (ImportLine.MaxLength > 0)
                    {
                        for (int i = 0; i <= RecordList.Rows.Count - 1; i++)
                        {
                            if (RecordList.Rows[i][ImportLine.FieldName].ToString().Length > ImportLine.MaxLength)
                            {
                                ModelState.AddModelError("", ImportLine.FieldName + " should be maximum length of " + ImportLine.MaxLength + ", it is exceeding at row no." + (i + 2).ToString());
                                return View("Import", vm);
                            }
                        }
                    }
                }


                if (ImportLineList.Count() > 0)
                {
                    RecordList.Columns.Add("ImportHeaderId");
                    RecordList.Columns.Add("DocTypeId");
                    RecordList.Columns.Add("UserName");
                    RecordList.Columns.Add("SiteId");
                    RecordList.Columns.Add("DivisionId");

                    for (int i = 0; i <= RecordList.Rows.Count - 1; i++)
                    {
                        //RecordList.Rows[i]["DocTypeId"] = form["DocTypeId"];
                        RecordList.Rows[i]["ImportHeaderId"] = vm.ImportHeaderId;
                        RecordList.Rows[i]["DocTypeId"] = DocTypeId;
                        RecordList.Rows[i]["UserName"] = User.Identity.Name;
                        RecordList.Rows[i]["SiteId"] = (int)System.Web.HttpContext.Current.Session["SiteId"];
                        RecordList.Rows[i]["DivisionId"] = (int)System.Web.HttpContext.Current.Session["DivisionId"];
                    }

                    foreach (var ImportLine in ImportLineList)
                    {
                        if (RecordList.Columns.Contains(ImportLine.FieldName) == false && ImportLine.FileNo == filecnt + 1)
                        {
                            RecordList.Columns.Add(ImportLine.FieldName);

                            if (ImportLine.DataType == "Input-Text" || ImportLine.DataType == "Input-Number" || ImportLine.DataType == "Input-Date" || ImportLine.DataType == "Single Select" || ImportLine.DataType == "Multi Select")
                            {
                                for (int i = 0; i <= RecordList.Rows.Count - 1; i++)
                                {
                                    RecordList.Rows[i][ImportLine.FieldName] = form[ImportLine.FieldName];
                                }
                            }
                        }
                    }
                }




                string TempTableName = "#TempTable_" + (filecnt + 1).ToString();
                CreateSqlTableFromDataTable(RecordList, TempTableName, sqlConnection);
            }


            
            DataSet ds = new DataSet();
            using (sqlConnection)
            {
                //sqlConnection.Open();
                using (SqlCommand cmd = new SqlCommand("" + ConfigurationManager.AppSettings["DataBaseSchema"] + "." + ImportHeader.SqlProc))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = sqlConnection;

                    for (int filecnt = 0; filecnt <= Request.Files.Count - 1; filecnt++)
                    {
                        cmd.Parameters.AddWithValue("@TableName_" + (filecnt + 1).ToString(), "#TempTable_" + (filecnt + 1).ToString());
                    }


                    cmd.CommandTimeout = 1000;
                    using (SqlDataAdapter adp = new SqlDataAdapter(cmd))
                    {
                        adp.Fill(ds);
                    }
                    //cmd.Connection.Close();
                }
            }


            List<ImportMessage> MsgList = new List<ImportMessage>();
            if (ds.Tables.Count > 0)
            {
                for (int i = 0; i <= ds.Tables[0].Rows.Count - 1; i++)
                {
                    ImportMessage Msg = new ImportMessage();
                    Msg.Head = ds.Tables[0].Rows[i]["Head"].ToString();
                    Msg.Value = ds.Tables[0].Rows[i]["Value"].ToString();
                    Msg.ValueType = ds.Tables[0].Rows[i]["ValueType"].ToString();
                    MsgList.Add(Msg);
                }
            }

            return View("Result", MsgList);


            //if (ds.Tables.Count == 0)
            //{
            //    ViewBag.id = form["DocTypeId"];
            //    return View("Sucess");
            //}

            //List<ImportErrors> ImportErrorList = new List<ImportErrors>();

            //if (ds.Tables[0].Rows.Count == 0)
            //{
            //    ViewBag.id = form["DocTypeId"];
            //    return View("Sucess");
            //}
            //else
            //{
            //    for (int j = 0; j <= ds.Tables[0].Rows.Count - 1; j++)
            //    {
            //        if (ds.Tables[0].Rows[j]["ErrorText"].ToString() != "")
            //        {
            //            ErrorText = ErrorText + ds.Tables[0].Rows[j]["ErrorText"].ToString() + "." + Environment.NewLine;
            //        }



            //        ImportErrors ImportError = new ImportErrors();
            //        ImportError.ErrorText = ds.Tables[0].Rows[j]["ErrorText"].ToString();
            //        ImportError.BarCodes = ds.Tables[0].Rows[j]["Head"].ToString();
            //        ImportErrorList.Add(ImportError);
            //    }

            //    if (ErrorText != "")
            //    {
            //        ViewBag.Error = ErrorText; // +WarningText;
            //        ViewBag.id = form["DocTypeId"];
            //        //string DataTableSessionVarName = "";
            //        //DataTableSessionVarName = User.Identity.Name.ToString() + "ImportData" + form["DocTypeId"].ToString();
            //        //Session[DataTableSessionVarName] = dataTable;
            //        return View("Error", ImportErrorList);
            //    }


            //    return View("Sucess");

            //}
        }


        //public ActionResult ReturnToRoute(int id)//Document Type Id
        //{
        //    return Redirect(System.Configuration.ConfigurationManager.AppSettings["JobsDomain"] + "/" + "JobOrderHeader" + "/" + "Index" + "/" + id);
        //}


        //public ImportMessage RetImportMessageModel(string Message)
        //{

        //}

        

        public void CreateSqlTableFromDataTable(DataTable dt, string tablename, SqlConnection _connection)
        {
            //string strconnection = connection;
            string table = "";
            table += "IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[" + tablename + "]') AND type in (N'U'))";
            table += "BEGIN ";
            table += "create table [" + tablename + "]";
            table += "(";
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                if (i != dt.Columns.Count - 1)
                    table += "[" + dt.Columns[i].ColumnName + "] " + "varchar(max)" + ",";
                else
                    table += "[" + dt.Columns[i].ColumnName + "] " + "varchar(max)";
            }
            table += ") ";
            table += "END";


            //SqlConnection _connection = new SqlConnection(connection);
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = table;
            cmd.Connection = _connection;
            //_connection.Open();
            cmd.ExecuteNonQuery();
            //_connection.Close();

            SqlBulkCopy oSqlBulkCopy = new SqlBulkCopy(_connection);
            oSqlBulkCopy.DestinationTableName = tablename;
            oSqlBulkCopy.WriteToServer(dt);

            //InsertQuery(table, strconnection);
            //CopyData(strconnection, dt, tablename);
        }
        //public void InsertQuery(string qry, string connection)
        //{
        //    SqlConnection _connection = new SqlConnection(connection);
        //    SqlCommand cmd = new SqlCommand();
        //    cmd.CommandType = CommandType.Text;
        //    cmd.CommandText = qry;
        //    cmd.Connection = _connection;
        //    _connection.Open();
        //    cmd.ExecuteNonQuery();
        //    _connection.Close();
        //}


        //public static void CopyData(string connStr, DataTable dt, string tablename)
        //{
        //    using (SqlBulkCopy bulkCopy =
        //    new SqlBulkCopy(connStr, SqlBulkCopyOptions.TableLock))
        //    {
        //        bulkCopy.DestinationTableName = "[" + tablename + "]";
        //        bulkCopy.WriteToServer(dt);
        //    }
        //}

        public DataTable GetDataSourceFromFile(string fileName)
        {
            DataTable dt = new DataTable("CreditCards");
            string[] columns = null;

            var lines = System.IO.File.ReadAllLines(fileName);

            // assuming the first row contains the columns information
            if (lines.Count() > 0)
            {
                columns = lines[0].Split(new char[] { ',' });

                int i = 0;
                foreach (var column in columns)
                {
                    if (i % 2 == 0)
                    {
                        dt.Columns.Add(column);
                    }
                    i++;
                }
            }

            // reading rest of the data
            for (int i = 1; i < lines.Count(); i++)
            {
                DataRow dr = dt.NewRow();
                string[] values = lines[i].Split(new char[] { ',' });

                //for (int j = 0; j < values.Count() && j < columns.Count(); j++)
                //    dr[j] = values[j];


                for (int j = 0; j < dt.Columns.Count ; j++)
                {
                    dr[j] = values[(j *2) + 1];
                }


                //for (int j = 0; j < values.Count() ; j++)
                //    dr[j] = values[j];

                dt.Rows.Add(dr);
            }
            return dt;
        }

        public DataTable ConvertAnonymousToDataTable(string filePath)
        {
            var excel = new ExcelQueryFactory();
            excel.FileName = filePath;

            var temp = (from c in excel.Worksheet() select c).ToList();

            var firstRecord = temp.First();

            var infos = firstRecord.GetType().GetProperties();
            DataTable table = new DataTable();
            foreach (var info in infos)
            {
                DataColumn column = new DataColumn(info.Name, info.PropertyType);
                table.Columns.Add(column);
            }

            foreach (var record in temp)
            {
                DataRow row = table.NewRow();
                for (int i = 0; i < table.Columns.Count; i++)
                    row[i] = infos[i].GetValue(record);
                table.Rows.Add(row);
            }

            return table;
        }


        public static DataTable exceldata(string filePath)
        {
            DataTable dtexcel = new DataTable();
            bool hasHeaders = false;
            string HDR = hasHeaders ? "Yes" : "No";
            string strConn;
            if (filePath.Substring(filePath.LastIndexOf('.')).ToLower() == ".xlsx")
                strConn = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + filePath + ";Extended Properties=\"Excel 12.0;HDR=" + HDR + ";IMEX=0\"";
            else
                strConn = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + filePath + ";Extended Properties=\"Excel 8.0;HDR=" + HDR + ";IMEX=0\"";
            OleDbConnection conn = new OleDbConnection(strConn);
            conn.Open();
            DataTable schemaTable = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });
            //Looping Total Sheet of Xl File
            /*foreach (DataRow schemaRow in schemaTable.Rows)
            {
            }*/
            //Looping a first Sheet of Xl File
            DataRow schemaRow = schemaTable.Rows[0];
            string sheet = schemaRow["TABLE_NAME"].ToString();
            if (!sheet.EndsWith("_"))
            {
                string query = "SELECT  * FROM [" + sheet + "]";
                OleDbDataAdapter daexcel = new OleDbDataAdapter(query, conn);
                dtexcel.Locale = CultureInfo.CurrentCulture;
                daexcel.Fill(dtexcel);
            }

            conn.Close();
            return dtexcel;

        }

        public JsonResult SetSelectOption(bool Checked)
        {
            TempData["closeOnSelectOption"] = Checked;
            return Json(new { success = true });
        }


        [HttpGet]
        public ActionResult _ImportMessages(int id)
        {
            ImportMessageViewModel temp = new ImportMessageViewModel();
            List<ImportMessage> ImportMessage = (from H in db.ImportMessage where H.ImportHeaderId == id && H.IsActive == true && H.ValueType == "Error" select H).ToList();
            
            ImportHeader Header = _ImportHeaderservice.Find(id);

            temp.ImportHeaderId = id;
            temp.ImportMessage = ImportMessage;
            ViewBag.Name = Header.ImportName;

            if (temp == null)
            {
                return HttpNotFound();
            }
            return PartialView("_ImportMessages", temp);
        }

        [HttpGet]
        public ActionResult __ImportMessages_DeletePost(int id)//LedgerAdjId
        {
            ImportMessage ImportMessage = db.ImportMessage.Find(id);
            ImportMessage.IsActive = false;
            ImportMessage.ObjectState = Model.ObjectState.Modified;
            db.ImportMessage.Add(ImportMessage);
            db.SaveChanges();

            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }
    }
}
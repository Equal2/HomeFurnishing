using System.Collections.Generic;
using System.Linq;
using System;
using Model;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Configuration;
using Models.Company.ViewModels;
using Infrastructure.IO;
using ProjLib;
using AutoMapper;
using Models.BasicSetup.ViewModels;

namespace Services.BasicSetup
{
    public interface IChargesCalculationService : IDisposable
    {
        IEnumerable<HeaderChargeViewModel> GetCalculationFooterListSProc(int HeaderTableId, string HeaderTableName, string LineTableName);
        IEnumerable<LineChargeViewModel> GetCalculationProductListSProc(int LineTableId, string LineTableName);
        void CalculateCharges(List<LineDetailListViewModel> LineList, int HeaderTableId, int CalculationId, int? MaxLineId, out List<LineChargeViewModel> RLineCharges, out bool RHeaderChargeEdit, out List<HeaderChargeViewModel> RHeaderCharges, string HeaderChargeTable, string LineChargeTable, out int PersonCount, int DocTypeId, int SiteId, int DivisionId);
    }

    public class ChargesCalculationService : IChargesCalculationService
    {
        private readonly IUnitOfWork _unitOfWork;
        public ChargesCalculationService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        #region ↨▬ChargesCalculation▬↨




        //Seperatable Content
        List<LineChargeViewModel> UpdateLineCharges(List<LineChargeViewModel> LineCharges, Dictionary<string, decimal?> Line, decimal Amount, decimal? DealQty)
        {
            decimal SubTotalProduct = 0;

            for (int i = 0; i < LineCharges.Count(); i++)
            {
                string selector = LineCharges[i].ChargeCode;
                string selectorRate = LineCharges[i].ChargeCode + "RATE";


                if (LineCharges[i].ChargeTypeId == (byte)ChargeTypesEnum.SubTotal)
                {
                    Line[selector] = SubTotalProduct;
                }

                else if (LineCharges[i].ChargeTypeId == (byte)ChargeTypesEnum.Amount)
                {
                    Line[selector] = decimal.Round(Amount, 2);
                }
                else if (LineCharges[i].RateType == (byte)RateTypeEnum.Na && LineCharges[i].CalculateOnId != null)
                {
                    string calculateon = LineCharges[i].CalculateOnCode;
                    Line[selector] = Line[calculateon];
                }
                else if (LineCharges[i].RateType == (byte)RateTypeEnum.Percentage)
                {
                    var calculateOn = LineCharges[i].CalculateOnCode;
                    Line[selector] = Line[calculateOn] * (Line[selectorRate] / 100);
                }
                else if (LineCharges[i].RateType == (byte)RateTypeEnum.Rate)
                {
                    var calculateOn = LineCharges[i].CalculateOnCode;
                    if (Line[selectorRate] != 0)
                        Line[selector] = DealQty * Line[selectorRate];
                }


                if (LineCharges[i].AddDeduct == true)
                {
                    SubTotalProduct = (SubTotalProduct + Line[selector] ?? 0);
                }
                else if (LineCharges[i].AddDeduct == false)
                {
                    SubTotalProduct = (SubTotalProduct - Line[selector] ?? 0);
                }
            }





            for (int i = 0; i < LineCharges.Count(); i++)
            {
                string selector = LineCharges[i].ChargeCode;
                string selectorRate = LineCharges[i].ChargeCode + "RATE";

                if (Line.ContainsKey(selectorRate))
                    LineCharges[i].Rate = Line[selectorRate];
                if (Line.ContainsKey(selector))
                    LineCharges[i].Amount = decimal.Round( (Line[selector] ?? 0),2);
            }

            return LineCharges;
            /////////////////////////////////////////////////////////////////////////////////////////////////////SaveLineCharges(id);
        }

        List<HeaderChargeViewModel> UpdateFooterCharges(List<HeaderChargeViewModel> HeaderCharges, Dictionary<string, decimal?> Footers, Dictionary<string, decimal?> Line, decimal Amount)
        {
            decimal SubTotalFooter = 0;

            for (int i = 0; i < HeaderCharges.Count(); i++)
            {
                string selector = HeaderCharges[i].ChargeCode;
                string selectorRate = HeaderCharges[i].ChargeCode + "RATE";
                string xselector = "X" + HeaderCharges[i].ChargeCode;
                string xselectorRate = "X" + HeaderCharges[i].ChargeCode + "RATE";


                if (HeaderCharges[i].ChargeTypeId == (byte)ChargeTypesEnum.SubTotal)
                {
                    Footers[selector] = SubTotalFooter;
                }
                else if (HeaderCharges[i].ChargeTypeId == (byte)ChargeTypesEnum.Amount)
                {
                    Footers[selector] = Footers[xselector] + Amount;

                }

                else if (HeaderCharges[i].ProductChargeId != null)
                {
                    string ProductChargeElement = HeaderCharges[i].ProductChargeCode;
                    var xProductChargeElement = "X" + HeaderCharges[i].ProductChargeCode;

                    Footers[selector] = Footers[xselector] - Line[xProductChargeElement] + Line[ProductChargeElement];
                }

                else if (HeaderCharges[i].ChargeTypeId == (byte)ChargeTypesEnum.RoundOFF)
                {

                    string calculateon = HeaderCharges[i].CalculateOnCode;
                    decimal val = Math.Round(Footers[calculateon] ?? 0, 0, MidpointRounding.ToEven) - Footers[calculateon] ?? 0;
                    Footers[selector] = val;
                }

                else if (HeaderCharges[i].ChargeTypeId == (byte)ChargeTypesEnum.Amount)
                {
                    Footers[selector] = Amount;
                }
                else if (HeaderCharges[i].RateType == (byte)RateTypeEnum.Na && HeaderCharges[i].CalculateOnId != null)
                {
                    var calculateon = HeaderCharges[i].CalculateOnCode;
                    Footers[selector] = Footers[calculateon];
                }
                else if (HeaderCharges[i].RateType == (byte)RateTypeEnum.Percentage)
                {
                    var calculateOn = HeaderCharges[i].CalculateOnCode;
                    Footers[selector] = Footers[calculateOn] * (Footers[selectorRate] / 100);
                }
                else if (HeaderCharges[i].RateType == (byte)RateTypeEnum.Rate)
                {
                    var calculateOn = HeaderCharges[i].CalculateOnCode;
                    Footers[selector] = Footers[calculateOn] * Footers[selectorRate];
                }


                if (HeaderCharges[i].AddDeduct == true)
                {
                    SubTotalFooter = (SubTotalFooter + Footers[selector] ?? 0);
                }
                else if (HeaderCharges[i].AddDeduct == false)
                {
                    SubTotalFooter = (SubTotalFooter - Footers[selector] ?? 0);
                }

            }


            for (int i = 0; i < HeaderCharges.Count(); i++)
            {
                var selector = HeaderCharges[i].ChargeCode;
                var selectorRate = HeaderCharges[i].ChargeCode + "RATE";

                if (Footers.ContainsKey(selectorRate))
                    HeaderCharges[i].Rate = Footers[selectorRate];
                if (Footers.ContainsKey(selector))
                    HeaderCharges[i].Amount = decimal.Round((Footers[selector] ?? 0), 2);

            }

            return HeaderCharges;

        }




        Dictionary<string, decimal?> CreateLineDictionary(List<LineChargeViewModel> LineCharges, Dictionary<string, decimal?> Line, Dictionary<string, decimal> IOP)
        {
            foreach (var item2 in LineCharges)
            {
                if (item2.RateType == (byte)RateTypeEnum.Na)
                {
                    Line.Add(item2.ChargeCode, item2.Amount);
                }
                else
                {
                    Line.Add(item2.ChargeCode + "RATE", item2.Rate);
                    Line.Add(item2.ChargeCode, (IOP.ContainsKey(item2.ChargeCode) && IOP[item2.ChargeCode] != 0 ? IOP[item2.ChargeCode] : item2.Amount));
                }
                Line.Add("X" + item2.ChargeCode, item2.Amount);
                Line.Add(item2.ChargeCode + "ACCR", item2.LedgerAccountCrId);
                Line.Add(item2.ChargeCode + "ACDR", item2.LedgerAccountDrId);
                Line.Add(item2.ChargeCode + "CLAC", item2.ContraLedgerAccountId);

            }
            return Line;
        }

        Dictionary<string, decimal?> CreateFooterDictionary(List<HeaderChargeViewModel> HeaderCharges, Dictionary<string, decimal?> Footers)
        {

            foreach (var item2 in HeaderCharges)
            {
                if (item2.RateType == (byte)RateTypeEnum.Na)
                {
                    Footers.Add(item2.ChargeCode, item2.Amount);
                    if (item2.ProductChargeId != null || item2.ChargeTypeId == (byte)ChargeTypesEnum.Amount)
                    {
                        Footers.Add("X" + item2.ChargeCode, item2.Amount);
                    }
                }
                else
                {
                    Footers.Add(item2.ChargeCode + "RATE", item2.Rate);
                    Footers.Add(item2.ChargeCode, item2.Amount);
                    if (item2.ProductChargeId != null || item2.ChargeTypeId == (byte)ChargeTypesEnum.Amount)
                    {
                        Footers.Add("X" + item2.ChargeCode + "RATE", item2.Rate);
                        Footers.Add("X" + item2.ChargeCode, item2.Amount);
                    }
                }
                Footers.Add(item2.ChargeCode + "ACCR", item2.LedgerAccountCrId);
                Footers.Add(item2.ChargeCode + "ACDR", item2.LedgerAccountDrId);
                Footers.Add(item2.ChargeCode + "CLAC", item2.ContraLedgerAccountId);

            }

            return Footers;

        }

        Dictionary<string, decimal?> UpdateFooterDictionary(List<HeaderChargeViewModel> HeaderCharges, Dictionary<string, decimal?> Footers)
        {
            foreach (var item2 in HeaderCharges)
            {
                if (item2.RateType == (byte)RateTypeEnum.Na)
                {
                    if (item2.ProductChargeId != null || item2.ChargeTypeId == (byte)ChargeTypesEnum.Amount)
                    {
                        Footers["X" + item2.ChargeCode] = Footers[item2.ChargeCode];
                    }
                }
                else
                {
                    if (item2.ProductChargeId != null || item2.ChargeTypeId == (byte)ChargeTypesEnum.Amount)
                    {
                        Footers["X" + item2.ChargeCode + "RATE"] = Footers[item2.ChargeCode + "RATE"];
                        Footers["X" + item2.ChargeCode] = Footers[item2.ChargeCode];
                    }
                }
            }
            return Footers;
        }



        List<HeaderChargeViewModel> GetHeaderCharges(int HeaderTableId, string HeaderChargesTable, string LineChargesTable, int CalculationId, List<HeaderChargeViewModel> HeaderCharges, out bool HeaderChargeEdit, int DocTypeId, int SiteId, int DivisionId)
        {

            List<HeaderChargeViewModel> TempHeaderCharges = new List<HeaderChargeViewModel>();
            HeaderChargeEdit = false;
            var count = GetCalculationFooterListSProc(HeaderTableId, HeaderChargesTable, LineChargesTable).Count();

            if (count > 0)
            {

                TempHeaderCharges = GetCalculationFooterListSProc(HeaderTableId, HeaderChargesTable, LineChargesTable).ToList();
                HeaderChargeEdit = true;

            }
            else
            {
                TempHeaderCharges = Mapper.Map<List<CalculationFooterViewModel>, List<HeaderChargeViewModel>>(new CalculationFooterService(_unitOfWork).GetCalculationFooterList(CalculationId, DocTypeId, SiteId, DivisionId).ToList());
            }

            return TempHeaderCharges;


        }

        List<LineChargeViewModel> GetLineCharges(int? MaxLineId, List<LineChargeViewModel> LineCharges, int CalculationId, string LineChargeTable, int DocTypeId, int SiteId, int DivisionId)
        {
            List<LineChargeViewModel> TempLineCharges = new List<LineChargeViewModel>();

            if (MaxLineId.HasValue && MaxLineId.Value > 0)
            {
                TempLineCharges = GetCalculationProductListSProc(MaxLineId ?? 0, LineChargeTable).ToList();

                foreach (var Titem in TempLineCharges)
                {
                    Titem.Amount = 0;
                }

            }
            else
            {
                TempLineCharges = Mapper.Map<List<CalculationProductViewModel>, List<LineChargeViewModel>>(new CalculationProductService(_unitOfWork).GetCalculationProductList(CalculationId, DocTypeId, SiteId, DivisionId).ToList());
            }

            return TempLineCharges;

        }



        /// <summary>
        /// Function Calculating Charges in C#
        /// </summary>
        /// <param name="LineList"> (List(LineDetailListViewModel))List containing Amount,Rate and LineTableId  </param>
        /// <param name="HeaderTableId">(int) Header table key value i.e.HeaderId </param>
        /// <param name="CalculationId">(int) Default Calculation Id</param>
        /// <param name="MaxLineId"> (int ?) Max lineId of the corresponding Header record </param>
        /// <param name="RLineCharges"> (List(LinechargeViewModel)) Output parameter-- List of line charges</param>
        /// <param name="RHeaderChargeEdit">(bool) Output parameter--Whether HeaderCharges should be created or updated </param>
        /// <param name="RHeaderCharges"> (List(HeaderchargeViewModel)) Output parameter-- List of Header charges</param>
        /// <param name="HeaderChargeTable">(string) -Header Charges table name</param>
        /// <param name="LineChargeTable">(string) -Line Charges table name</param>
        /// <returns></returns>


        public void CalculateCharges(List<LineDetailListViewModel> LineList, int HeaderTableId, int CalculationId, int? MaxLineId, out List<LineChargeViewModel> RLineCharges, out bool RHeaderChargeEdit, out List<HeaderChargeViewModel> RHeaderCharges, string HeaderChargeTable, string LineChargeTable, out int PersonCount, int DocTypeId, int SiteId, int DivisionId)
        {

            decimal Amount;
            decimal Rate;
            PersonCount = 0;
            bool HeaderChargeEdit = false;
            Dictionary<string, decimal?> Footers = new Dictionary<string, decimal?>();
            Dictionary<string, decimal?> Line = new Dictionary<string, decimal?>();
            List<LineChargeViewModel> LineChargeList = new List<LineChargeViewModel>();
            List<HeaderChargeViewModel> HeaderCharges = new List<HeaderChargeViewModel>();
            List<LineChargeViewModel> LineCharges = new List<LineChargeViewModel>();

            List<HeaderChargeViewModel> TEmpHeaderCharges = GetHeaderCharges(HeaderTableId, HeaderChargeTable, LineChargeTable, CalculationId, HeaderCharges, out HeaderChargeEdit, DocTypeId, SiteId, DivisionId);
            List<LineChargeViewModel> TempLineCharges = GetLineCharges(MaxLineId, LineCharges, CalculationId, LineChargeTable, DocTypeId, SiteId, DivisionId);

            foreach (var line in LineList)
            {

                //Saving Calculation
                Amount = line.Amount ?? 0;
                Rate = line.Rate ?? 0;


                Dictionary<string, decimal> IOP = new Dictionary<string, decimal>();

                if (line.Penalty != null && line.Penalty != 0)
                    IOP.Add(ChargeCodeConstants.Penalty, line.Penalty);

                //Getting Header and Line Charges

                HeaderCharges = Mapper.Map<List<HeaderChargeViewModel>, List<HeaderChargeViewModel>>(TEmpHeaderCharges);
                LineCharges = Mapper.Map<List<LineChargeViewModel>, List<LineChargeViewModel>>(TempLineCharges);

                if (line.Incentive > 0 && LineCharges.Where(m => m.ChargeCode == ChargeCodeConstants.Incentive).FirstOrDefault() != null)
                    LineCharges.Where(m => m.ChargeCode == ChargeCodeConstants.Incentive).FirstOrDefault().Rate = line.Incentive;

                if (line.RLineCharges != null)
                {
                    foreach (var LineCharg in line.RLineCharges)
                    {
                        if (LineCharges.Where(m => m.ChargeId == LineCharg.ChargeId).Any())
                        {
                            LineCharges.Where(m => m.ChargeId == LineCharg.ChargeId).FirstOrDefault().Rate = LineCharg.Rate ?? 0;
                        }
                    }
                }

                //Creating Dictionary
                if (line.LineTableId == 0)
                {
                    Line = CreateLineDictionary(LineCharges, Line, IOP);
                    Footers = CreateFooterDictionary(HeaderCharges, Footers);
                }

                else
                {
                    Line = CreateLineDictionary(LineCharges, Line, IOP);
                    Footers = UpdateFooterDictionary(HeaderCharges, Footers);
                }

                //Updating Dictionary
                LineCharges = UpdateLineCharges(LineCharges, Line, Amount, line.DealQty);
                HeaderCharges = UpdateFooterCharges(HeaderCharges, Footers, Line, Amount);





                for (int i = 0; i < LineCharges.Count(); i++)
                {

                    LineCharges[i].LineTableId = line.LineTableId;
                    LineCharges[i].PersonID = line.PersonID;
                    LineCharges[i].DealQty = line.DealQty;
                    LineCharges[i].CostCenterId = line.CostCenterId;
                    LineCharges[i].HeaderTableId = line.HeaderTableId;
                    LineChargeList.Add(LineCharges[i]);
                }

                Line = new Dictionary<string, decimal?>();


            }

            RLineCharges = LineChargeList;
            RHeaderChargeEdit = HeaderChargeEdit;
            RHeaderCharges = HeaderCharges;
            PersonCount = (from p in LineChargeList
                           group p by p.PersonID into g
                           select g).Count();

            TEmpHeaderCharges = new List<HeaderChargeViewModel>();
            TempLineCharges = new List<LineChargeViewModel>();

        }



        #endregion





        public IEnumerable<HeaderChargeViewModel> GetCalculationFooterListSProc(int HeaderTableId, string HeaderTableName, string LineTableName)
        {
            SqlParameter SqlParameterHeaderTableId = new SqlParameter("@HeaderTableId", HeaderTableId);
            SqlParameter SqlParameterHeaderTableName = new SqlParameter("@HeaderTableName", HeaderTableName);
            SqlParameter SqlParameterLineTableName = new SqlParameter("@LineTableName", LineTableName);

            IEnumerable<HeaderChargeViewModel> CalculationHeaderList = _unitOfWork.SqlQuery<HeaderChargeViewModel>("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".CalculationHeaderCharge @HeaderTableId, @HeaderTableName, @LineTableName", SqlParameterHeaderTableId, SqlParameterHeaderTableName, SqlParameterLineTableName).ToList();

            return CalculationHeaderList;
        }

        public IEnumerable<LineChargeViewModel> GetCalculationProductListSProc(int LineTableId, string LineTableName)
        {
            SqlParameter SqlParameterLineTableId = new SqlParameter("@LineTableld", LineTableId);
            SqlParameter SqlParameterLineTableName = new SqlParameter("@LineTableName", LineTableName);

            IEnumerable<LineChargeViewModel> CalculationLineList = _unitOfWork.SqlQuery<LineChargeViewModel>("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".CalculationLineCharge @LineTableld, @LineTableName", SqlParameterLineTableId, SqlParameterLineTableName).ToList();

            return CalculationLineList;
        }






        public void Dispose()
        {
            _unitOfWork.Dispose();
        }


    }
}

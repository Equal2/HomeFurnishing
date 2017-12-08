using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Models;
using System.ComponentModel.DataAnnotations;

namespace CustomEventArgs
{
    public class StockEventArgs : EventArgs
    {

        int P_DocId;
        int P_LineId;
        string P_Mode;
        public StockEventArgs(int Id)
        {
            P_DocId = Id;
        }
        public StockEventArgs(int Id,string Mode)
        {
            P_DocId = Id;
            P_Mode = Mode;
        }
        public StockEventArgs(int Id, int LineId)
        {
            P_DocId = Id;
            P_LineId = LineId;
        }
        public StockEventArgs(int Id,int LineId, string Mode)
        {
            P_DocId = Id;
            P_LineId = LineId;
            P_Mode = Mode;
        }
        public int DocId
        {
            get { return P_DocId; }
        }
        public string Mode
        {
            get { return P_Mode; }
        }
        public int DocLineId
        {
            get { return P_LineId; }
        }

    }
}

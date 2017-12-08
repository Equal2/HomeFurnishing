using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Models;
using System.ComponentModel.DataAnnotations;

namespace CustomEventArgs
{
    public class JobEventArgs : EventArgs
    {

        int P_DocId;
        int P_LineId;
        string P_Mode;
        public JobEventArgs(int Id)
        {
            P_DocId = Id;
        }
        public JobEventArgs(int Id,string Mode)
        {
            P_DocId = Id;
            P_Mode = Mode;
        }
        public JobEventArgs(int Id, int LineId)
        {
            P_DocId = Id;
            P_LineId = LineId;
        }
        public JobEventArgs(int Id,int LineId, string Mode)
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

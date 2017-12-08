using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Models;
using System.ComponentModel.DataAnnotations;

namespace Model.ViewModel
{
    public class SmsViewModel
    {
        public string MobileNoList { get; set; }
        public string Message { get; set; }
    }
}

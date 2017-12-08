using Model.Models;
using Model.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.ViewModels
{
    public class ImportMessageViewModel
    {
        public int ImportHeaderId { get; set; }
        public List<ImportMessage> ImportMessage { get; set; }

    }
}

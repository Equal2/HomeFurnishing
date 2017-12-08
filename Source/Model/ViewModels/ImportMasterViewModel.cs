using Model.Models;
using Model.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.ViewModels
{
    public class ImportMasterViewModel
    {
        public int ImportHeaderId { get; set; }
        public int DocTypeId { get; set; }
        public bool closeOnSelect { get; set; }
        public ImportHeader ImportHeader { get; set; }        
        public List<ImportLine> ImportLine{ get; set; }

    }
}

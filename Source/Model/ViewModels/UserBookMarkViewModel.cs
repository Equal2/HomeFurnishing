using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.ViewModel
{
    [Serializable]
    public class UserBookMarkViewModel
    {
        public int MenuId { get; set; }
        public string MenuName { get; set; }
        public string IconName { get; set; }

    }
}

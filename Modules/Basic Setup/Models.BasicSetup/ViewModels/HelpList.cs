using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Models.BasicSetup.ViewModels
{


    public class ComboBoxList
    {
        public int Id { get; set; }
        public string PropFirst { get; set; }
        public string PropSecond { get; set; }

        public string PropThird { get; set; }

        //Other data
    }

    //Extra classes to format the results the way the select2 dropdown wants them
    public class ComboBoxPagedResult
    {
        public int Total { get; set; }
        public List<ComboBoxResult> Results { get; set; }
    }

    public class ComboBoxResult
    {
        public string id { get; set; }
        public string text { get; set; }
        public string AProp1 { get; set; }
        public string AProp2 { get; set; }
        public string TextProp1 { get; set; }
        public string TextProp2 { get; set; }
    }

    public class CustomComboBoxResult
    {
        public string id { get; set; }
        public string text { get; set; }
        public int RecCount { get; set; }
    }

    public class CustomComboBoxPagedResult
    {
        public int Total { get; set; }
        public List<CustomComboBoxResult> Results { get; set; }
    }
}

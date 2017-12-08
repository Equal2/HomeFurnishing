using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Components.Logging
{
    public interface IModificationCheck
    {
        XElement CheckChanges(List<LogTypeViewModel> LogList);
    }
}

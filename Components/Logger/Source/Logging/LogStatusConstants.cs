using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Components.Logging
{
    public enum ActivityTypeContants
    {
        UnKnown = 0,
        Submitted = 1,
        Approved = 2,
        Modified = 3,
        ModificationSumbitted = 4,
        Deleted = 5,
        Added = 6,
        Closed = 7,
        Report = 8,
        Reviewed = 9,
        Import = 10,
        WizardCreate = 11,
        MultipleCreate = 12,
        Print = 13,
        Detail = 14,
        FileAdded = 15,
        FileRemoved = 16,
        SettingsAdded = 17,
        SettingsModified = 18,
    }
}

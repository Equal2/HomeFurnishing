using System.Collections.Generic;
using System.Linq;
using Data;
using Data.Infrastructure;
using Model.Models;

using Core.Common;
using System;
using Model;
using System.Threading.Tasks;
using Data.Models;
using Model.ViewModel;
using System.Data.SqlClient;
using System.Configuration;

namespace Service
{
    public interface ICommonService : IDisposable
    {
        void ExecuteCustomiseEvents(string EventName, object[] Parameters);
    }

    public class CommonService : ICommonService
    {
        public void ExecuteCustomiseEvents(string EventName, object[] Parameters)
        {
            if (EventName != null)
            {
                string[] FunctionPartArr;
                FunctionPartArr = EventName.Split(new Char[] { '.' });

                string NameSpace = FunctionPartArr[0];
                string ClassName = FunctionPartArr[1];
                string FunctionName = FunctionPartArr[2];

                object obj = (object)Activator.CreateInstance("CustomiseEvents", NameSpace + "." + ClassName).Unwrap();
                Type T = obj.GetType();
                T.GetMethod(FunctionName).Invoke(obj, Parameters);
            }
        }

        public void Dispose()
        {
        }
    }
}

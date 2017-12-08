using System;
using System.Collections.Generic;
using System.Diagnostics;
using Service;
using Data.Models;
using System.Configuration;
using Mailer;
using Core.Common;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using Mailer.Model;

namespace EmailContents
{
    public class DataExecution
    {
        private static ApplicationDbContext db = new ApplicationDbContext();
        public static void DataExecutionProcess()
        {

            try
            {
                using (StreamWriter writer =
                        new StreamWriter(@"c:\temp\Cachecallback.txt", true))
                        {
                            writer.WriteLine("Daily Data Execution Event", DateTime.Now);
                            writer.Close();
                        }
            }
            catch (Exception x)
            {
                Debug.WriteLine(x);
            }


            try
            {
                db.Database.ExecuteSqlCommand("Web.sp_ScheduleDataExecution");
            }

            catch (Exception ex)
            {
                return;
            }

        }
    }
}

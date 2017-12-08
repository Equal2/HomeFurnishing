using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace Notifier.Hubs
{
    public class DailyNotificationScheduler
    {
       
        public static void DailyNotifications()
        {

            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ToString()))
            {
                try
                {

                    connection.Open();
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = connection;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = "Web.DailyNotifications";
                            
                        cmd.ExecuteNonQuery();


                    }                    
                    connection.Close();
                }
                catch (SqlException ex)
                {
                    
                }
                catch (Exception ex)
                {
                    
                }
               
            }


        }


    }
}
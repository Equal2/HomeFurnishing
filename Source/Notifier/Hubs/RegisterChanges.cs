using Microsoft.AspNet.SignalR;
using Data.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
//using Models.Login.ViewModels;
using Model.Models;

namespace Notifier.Hubs
{
    public class RegisterChanges
    {

        public static void RegisterDependency()
        {

            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["LoginDB"].ToString()))
            {
                try
                {
                    SqlCommand command = new SqlCommand("Select NotificationUserId from dbo.NotificationUsers", connection);

                    //Monitor the Service Broker, and get notified if there is change in the query result
                    SqlDependency dependency = new SqlDependency(command, null, int.MaxValue);

                    //Fire event when message is arrived
                    dependency.OnChange += new OnChangeEventHandler(dependency_OnChange);

                    connection.Open();
                    // NOTE: You have to execute the command, or the notification will never fire.
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                    }
                    connection.Close();
                }
                catch (SqlException ex)
                {
                    Console.WriteLine(string.Format("Error: {0}", ex.Message));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(string.Format("Error: {0}", ex.Message));
                }
            }
        }

        protected static void dependency_OnChange(object sender, SqlNotificationEventArgs e)
        {
            var info = e.Info;
            var source = e.Source;
            var type = e.Type;

            try
            {

                if (info == SqlNotificationInfo.Insert)
                {
                    var noti = GetUpdates();

                    foreach (var item in noti)
                        GlobalHost.ConnectionManager.GetHubContext<Notifications>().Clients.User(item.UserName).NarrationUpdate(item.NotificationCount);
                }

            }
            catch (Exception ex)
            {

            }
            SqlDependency dependency = sender as SqlDependency;

            dependency.OnChange -= new OnChangeEventHandler(dependency_OnChange);
            RegisterDependency(); //Re-register dependency is required after a notification is received everytime            
            //Do whatever you like here after message arrive
            //Can be calling WCF service or anything supported in C#
        }

        public static List<NotificationViewModel> GetUpdates()
        {
            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["LoginDB"].ToString()))
            {
                try
                {
                    //                   SqlCommand command = new SqlCommand("SELECT TOP 10 NotificationText, dbo.Notifications.NotificationId, dbo.NotificationUsers.UserName, dbo.Notifications.CreatedDate, SeenDate, NotificationUrl,dbo.NotificationSubjects.NotificationSubjectName as NotificationSubject,dbo.NotificationSubjects.IconName as IconName FROM dbo.Notifications"
                    //+ "                                                     Left Join dbo.NotificationSubjects on dbo.Notifications.NotificationSubjectId = dbo.NotificationSubjects.NotificationSubjectId"
                    //+ "                                                     Left Join dbo.NotificationUsers on dbo.Notifications.NotificationId = dbo.NotificationUsers.NotificationId "
                    //+ "                                                     WHERE ReadDate IS NULL  AND IsNull(ExpiryDate,Getdate()) >= current_timestamp ORDER BY dbo.Notifications.NotificationId DESC", connection);

                    SqlCommand command = new SqlCommand("SELECT dbo.NotificationUsers.UserName, count(*) AS Count "
                    + "FROM dbo.Notifications "
                    + "Left Join dbo.NotificationUsers on dbo.Notifications.NotificationId = dbo.NotificationUsers.NotificationId "
                    + "WHERE ReadDate IS NULL AND SeenDate IS NULL  AND IsNull(ExpiryDate,Getdate()) >= current_timestamp "
                    + "GROUP BY dbo.NotificationUsers.UserName", connection);

                    connection.Open();

                    SqlDataReader reader = command.ExecuteReader();

                    List<NotificationViewModel> narr = new List<NotificationViewModel>();

                    while (reader.Read())
                    {
                        NotificationViewModel noti = new NotificationViewModel();
                        noti.UserName = reader["UserName"].ToString();
                        noti.NotificationCount = (int)reader["Count"];
                        narr.Add(noti);
                    }

                    connection.Close();


                    return narr;
                }

                catch (SqlException ex)
                {
                    return null;
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
        }



        public static NotificationListViewModel GetUserUpdates(string UserName)
        {
            string TotalCount = "";
            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["LoginDB"].ToString()))
            {
                try
                {
                    SqlCommand command = new SqlCommand("SELECT TOP 10 dbo.Notifications.NotificationId, NotificationText, dbo.Notifications.CreatedDate AS CreatedDate, dbo.Notifications.SeenDate AS SeenDate ,NotificationUrl,dbo.NotificationSubjects.NotificationSubjectName as NotificationSubject,dbo.NotificationSubjects.IconName as IconName " +
                                                        " FROM dbo.Notifications "
 + "                                                     Left Join dbo.NotificationSubjects on dbo.Notifications.NotificationSubjectId = dbo.NotificationSubjects.NotificationSubjectId "
 + "                                                     Left Join dbo.NotificationUsers on dbo.Notifications.NotificationId = dbo.NotificationUsers.NotificationId "
 + "                                                     WHERE ReadDate IS NULL AND dbo.NotificationUsers.UserName = '" + UserName + "'  " +
                                                        " AND IsNull(ExpiryDate,Getdate()) >= current_timestamp ORDER BY NotificationId DESC", connection);

                    connection.Open();

                    SqlDataReader reader = command.ExecuteReader();

                    List<NotificationViewModel> narr = new List<NotificationViewModel>();

                    while (reader.Read())
                    {
                        NotificationViewModel noti = new NotificationViewModel();
                        noti.NotificationId = Convert.ToInt32(reader["NotificationId"]);
                        noti.CreatedDate = Convert.ToDateTime(reader["CreatedDate"]);
                        noti.NotificationText = reader["NotificationText"].ToString();
                        noti.IconName = reader["IconName"].ToString();
                        noti.NotificationSubjectName = reader["NotificationSubject"].ToString();
                        noti.NotificationUrl = reader["NotificationUrl"].ToString();
                        //noti.UserName = reader["UserName"].ToString();
                        if (!(reader["SeenDate"] is DBNull))
                            noti.SeenDate = Convert.ToDateTime(reader["SeenDate"]);

                        narr.Add(noti);
                    }
                    connection.Close();

                    SqlCommand SeenDatecmd = new SqlCommand("UPDATE dbo.Notifications SET SeenDate = CURRENT_TIMESTAMP FROM dbo.Notifications "
        + "                                                     Left Join dbo.NotificationUsers on dbo.Notifications.NotificationId = dbo.NotificationUsers.NotificationId "
        + "                                                     WHERE dbo.NotificationUsers.UserName=@name  AND SeenDate IS NULL ", connection);

                    connection.Open();

                    SeenDatecmd.Parameters.AddWithValue("@name", UserName);
                    SeenDatecmd.ExecuteNonQuery();

                    connection.Close();

                    SqlCommand Countcommand = new SqlCommand("SELECT count(*) AS Count " +
                                                       " FROM dbo.Notifications "
+ "                                                     Left Join dbo.NotificationUsers on dbo.Notifications.NotificationId = dbo.NotificationUsers.NotificationId "
+ "                                                     WHERE ReadDate IS NULL AND dbo.NotificationUsers.UserName = '" + UserName + "'  " +
                                                       " AND IsNull(ExpiryDate,Getdate()) >= current_timestamp ", connection);

                    connection.Open();

                    SqlDataReader Countreader = Countcommand.ExecuteReader();


                    while (Countreader.Read())
                    {
                        TotalCount = Countreader["Count"].ToString();
                    }
                    connection.Close();

                    return new NotificationListViewModel { NotificationViewModel = narr, Count = TotalCount };
                }

                catch (SqlException ex)
                {
                    return null;
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
        }

        public static void SetNotificationSeen(string UserName)
        {

            if (!string.IsNullOrEmpty(UserName))
            {
                using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["LoginDB"].ToString()))
                {
                    try
                    {

                        using (SqlCommand cmd = new SqlCommand("UPDATE dbo.Notifications SET SeenDate = CURRENT_TIMESTAMP FROM dbo.Notifications "
        + "                                                     Left Join dbo.NotificationUsers on dbo.Notifications.NotificationId = dbo.NotificationUsers.NotificationId "
        + "                                                     WHERE dbo.NotificationUsers.UserName=@name  AND SeenDate IS NULL ", connection))
                        {

                            connection.Open();

                            cmd.Parameters.AddWithValue("@name", UserName);
                            cmd.ExecuteNonQuery();

                            connection.Close();
                        }


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




        public static List<NotificationViewModel> GetAllNotifications(string UserName)
        {
            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["LoginDB"].ToString()))
            {
                try
                {
                    SqlCommand command = new SqlCommand("SELECT dbo.Notifications.NotificationId, ExpiryDate, SeenDate, NotificationText, dbo.NotificationUsers.UserName ,dbo.Notifications.CreatedDate, NotificationUrl, dbo.NotificationSubjects.NotificationSubjectName as NotificationSubject, dbo.NotificationSubjects.IconName as IconName FROM dbo.Notifications"
 + "                                                     Left Join dbo.NotificationSubjects on dbo.Notifications.NotificationSubjectId = dbo.NotificationSubjects.NotificationSubjectId"
 + "                                                     Left Join dbo.NotificationUsers on dbo.Notifications.NotificationId = dbo.NotificationUsers.NotificationId "
 + "                                                     WHERE dbo.NotificationUsers.UserName = @name AND ExpiryDate > CURRENT_TIMESTAMP ORDER BY NotificationId DESC", connection);

                    command.Parameters.AddWithValue("@name", UserName);

                    connection.Open();

                    SqlDataReader reader = command.ExecuteReader();

                    List<NotificationViewModel> narr = new List<NotificationViewModel>();

                    while (reader.Read())
                    {
                        NotificationViewModel noti = new NotificationViewModel();
                        noti.CreatedDate = Convert.ToDateTime(reader["CreatedDate"]);
                        if (!(reader["ExpiryDate"] is DBNull))
                            noti.ExpiryDate = Convert.ToDateTime(reader["ExpiryDate"]);
                        if (!(reader["SeenDate"] is DBNull))
                            noti.SeenDate = Convert.ToDateTime(reader["SeenDate"]);
                        noti.NotificationText = reader["NotificationText"].ToString();
                        noti.IconName = reader["IconName"].ToString();
                        noti.NotificationId = Convert.ToInt32(reader["NotificationId"]);
                        noti.NotificationSubjectName = reader["NotificationSubject"].ToString();
                        noti.NotificationUrl = reader["NotificationUrl"].ToString();
                        noti.UserName = reader["UserName"].ToString();
                        narr.Add(noti);
                    }

                    connection.Close();


                    return narr;
                }

                catch (SqlException ex)
                {
                    return null;
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
        }

        public static string SetReadDate(int id)
        {
            string Url = "";
            string UrlKey = "";
            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["LoginDB"].ToString()))
            {
                try
                {

                    connection.Open();
                    using (SqlCommand cmd = new SqlCommand("UPDATE dbo.Notifications SET ReadDate = CURRENT_TIMESTAMP FROM dbo.Notifications "
       + "                                                  WHERE NotificationId =@id ", connection))
                    {



                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.ExecuteNonQuery();


                    }

                    using (SqlCommand cmd = new SqlCommand("SELECT  NotificationUrl,UrlKey  FROM dbo.Notifications "
      + "                                                  WHERE NotificationId=@id ", connection))
                    {

                        cmd.Parameters.AddWithValue("@id", id);
                        SqlDataReader reader = cmd.ExecuteReader();



                        while (reader.Read())
                        {
                            Url = reader["NotificationUrl"].ToString();
                            UrlKey = reader["UrlKey"].ToString();
                        }

                    }
                    connection.Close();
                }
                catch (SqlException ex)
                {
                    return null;
                }
                catch (Exception ex)
                {
                    return null;
                }

                if (string.IsNullOrEmpty(Url) || string.IsNullOrEmpty(UrlKey))
                    return "";
                else
                    return System.Configuration.ConfigurationManager.AppSettings[UrlKey] + Url;
            }


        }


    }
}
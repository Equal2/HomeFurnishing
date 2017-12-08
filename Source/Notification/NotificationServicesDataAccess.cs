using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Data.Sql;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;

namespace Surya.Notification
{
    public class NotificationServicesDataAccess
    {

        public List<PurchaseOrderHeader> GetData()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString.ToString();

           var purchaseOrder = new List<PurchaseOrderHeader>();

            using (SqlConnection connection =
            new SqlConnection(connectionString))
            {
                // Create the Command and Parameter objects.
                SqlCommand command = new SqlCommand();
                command.CommandText = "GETPURCHASEORDER_ForNotification";
                command.CommandType = CommandType.StoredProcedure;
                command.Connection = connection;
               
                try
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        
                        while (reader.Read())
                        {
                            var purchaseOrderHeader = new PurchaseOrderHeader
                            {

                                OrderNumber = reader["OrderNumber"] is DBNull ? null : reader["OrderNumber"].ToString(),
                                OrderDate = reader.GetFieldValue<DateTime>(1),
                                ShipDate = reader.GetFieldValue<DateTime>(2),
                                Status = reader["STATUS"] is DBNull ? null : reader["STATUS"].ToString(),
                                ProgressPer =Convert.ToInt32(reader["ProgressPer"]),
                                SupplierName = reader["Name"] is DBNull ? null : reader["Name"].ToString(),
                                Email = reader["Email"] is DBNull ? null : reader["Email"].ToString()
                            };

                            purchaseOrder.Add(purchaseOrderHeader);
                            
                        }
                        reader.Close();
                    }
                   // return purchaseOrder;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                return purchaseOrder;
            }

        }
    }

    public class PurchaseOrderHeader
    {
        public string OrderNumber { get; set; }

        public DateTime OrderDate { get; set; }

       public string SupplierName { get; set; }
        
        public DateTime ShipDate { get; set; }
        
        public string Status { get; set; }

        public int? ProgressPer
        {
            get;
            set;

        }
        public string Email { get; set; }
    }
}

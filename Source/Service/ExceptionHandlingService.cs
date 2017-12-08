using System;
using System.Data.Entity.Validation;
using System.Data.Entity.Infrastructure;
using System.IO;

namespace Service
{
    public interface IExceptionHandlingService : IDisposable
    {
        string HandleException(Exception ex);
    }

    public class ExceptionHandlingService : IExceptionHandlingService
    {

        public string HandleException(Exception ex)
        {
            LogException(ex);

            if (ex is DbEntityValidationException)
            {
                var tempex = (DbEntityValidationException)ex;
                string message = "";
                foreach (var validationErrors in tempex.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        message += validationError.PropertyName + "-" + validationError.ErrorMessage;
                    }
                }

                return message;
            }
            else if (ex is DbUpdateException)
            {
                string message = "";

                if (ex.InnerException != null && ex.InnerException.InnerException.Message.Contains("The DELETE statement conflicted with the REFERENCE constraint "))
                {
                    message = ex.InnerException.InnerException.Message;// "Cannot delete this as this record is in use by other documents.";
                }
                else if (ex.InnerException != null)
                {
                    message = ex.InnerException.InnerException.Message;
                }
                else
                {
                    message = ex.Message;
                }

                return message;
            }

            else
            {
                string message;
                if (ex.InnerException != null)
                {
                    message = ex.InnerException.InnerException.Message;
                }
                else
                {
                    message = ex.Message;
                }

                return message;
            }
        }

        void LogException(Exception ex)
        {
            try
            {
                using (StreamWriter writer =
                 new StreamWriter(@"c:\temp\Exception.txt", true))
                {
                    writer.WriteLine(" {0} : " + ex.ToString(), DateTime.Now);
                    writer.Close();
                }
            }
            catch (Exception x)
            {

            }
        }

        public void Dispose()
        {
        }

    }
}

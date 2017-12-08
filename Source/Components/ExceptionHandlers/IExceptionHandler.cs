using System;

namespace Components.ExceptionHandlers
{
    public interface IExceptionHandler
    {
        string HandleException(Exception ex);
    }
}

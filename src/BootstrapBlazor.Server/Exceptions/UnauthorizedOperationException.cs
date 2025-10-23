using System;

namespace  BootstrapBlazor.Server.Exceptions
{
    public class UnauthorizedOperationException : Exception
    {
        public UnauthorizedOperationException(string message) : base(message)
        {

        }
    }
}

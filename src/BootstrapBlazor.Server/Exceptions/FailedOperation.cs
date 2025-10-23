using System;

namespace  BootstrapBlazor.Server.Exceptions
{
    public class FailedOperation : Exception
    {
        public FailedOperation(string message) : base(message)
        {

        }
    }
}

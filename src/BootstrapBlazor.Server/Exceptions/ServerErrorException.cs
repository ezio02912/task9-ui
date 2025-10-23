using System;

namespace  BootstrapBlazor.Server.Exceptions
{
    public class ServerErrorException : Exception
    {
        public ServerErrorException(string message) : base(message)
        {

        }
    }
}

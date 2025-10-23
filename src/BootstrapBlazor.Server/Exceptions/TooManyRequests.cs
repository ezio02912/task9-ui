using System;

namespace  BootstrapBlazor.Server.Exceptions
{
    public class TooManyRequests : Exception
    {
        public TooManyRequests(string message) : base(message)
        {

        }
    }
}

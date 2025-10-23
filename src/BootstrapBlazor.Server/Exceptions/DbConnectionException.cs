using System;

namespace  BootstrapBlazor.Server.Exceptions
{
    public class DbConnectionException : Exception
    {
        public DbConnectionException(string message) : base(message)
        {

        }
    }
}

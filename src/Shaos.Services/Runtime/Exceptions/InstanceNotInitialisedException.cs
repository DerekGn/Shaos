using System.Diagnostics.CodeAnalysis;

namespace Shaos.Services.Runtime.Exceptions
{
    [ExcludeFromCodeCoverage]
    public class InstanceNotInitialisedException : Exception
    {
        public InstanceNotInitialisedException()
        { }

        public InstanceNotInitialisedException(string message) : base(message)
        {
        }

        public InstanceNotInitialisedException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
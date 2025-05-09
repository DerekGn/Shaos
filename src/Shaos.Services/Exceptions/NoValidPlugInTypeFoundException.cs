using System.Diagnostics.CodeAnalysis;

namespace Shaos.Services.Exceptions
{
    [ExcludeFromCodeCoverage]
    public class NoValidPlugInAssemblyFoundException : Exception
    {
        public NoValidPlugInAssemblyFoundException()
        { }

        public NoValidPlugInAssemblyFoundException(string message) : base(message)
        {
        }

        public NoValidPlugInAssemblyFoundException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
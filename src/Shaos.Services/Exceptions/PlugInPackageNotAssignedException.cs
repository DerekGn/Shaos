using System.Diagnostics.CodeAnalysis;

namespace Shaos.Services.Exceptions
{
    [ExcludeFromCodeCoverage]
    public class PlugInPackageNotAssignedException : Exception
    {
        public PlugInPackageNotAssignedException(int id)
        {
            Id = id;
        }

        public PlugInPackageNotAssignedException(int id, string message) : base(message)
        {
            Id = id;
        }

        public PlugInPackageNotAssignedException(int id, string message, Exception inner) : base(message, inner)
        {
            Id = id;
        }

        public int Id { get; }
    }
}
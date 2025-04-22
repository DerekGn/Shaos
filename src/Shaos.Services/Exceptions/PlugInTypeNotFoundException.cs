namespace Shaos.Services.Exceptions
{
    public class PlugInTypeNotFoundException : Exception
    {
        public PlugInTypeNotFoundException()
        { }

        public PlugInTypeNotFoundException(string message) : base(message)
        {
        }

        public PlugInTypeNotFoundException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
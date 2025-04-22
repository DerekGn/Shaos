namespace Shaos.Services.Exceptions
{
    public class PlugInTypesFoundException : Exception
    {
        public PlugInTypesFoundException(int count)
        {
            Count = count;
        }

        public PlugInTypesFoundException(int count, string message) : base(message)
        {
            Count = count;
        }

        public PlugInTypesFoundException(int count, string message, Exception inner) : base(message, inner)
        {
            Count = count;
        }

        public int Count { get; }
    }
}
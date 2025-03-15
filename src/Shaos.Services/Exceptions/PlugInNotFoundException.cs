
namespace Shaos.Services.Exceptions
{
	public class PlugInNotFoundException : Exception
	{
		public PlugInNotFoundException() { }
		public PlugInNotFoundException(string message) : base(message) { }
		public PlugInNotFoundException(string message, Exception inner) : base(message, inner) { }
	}
}

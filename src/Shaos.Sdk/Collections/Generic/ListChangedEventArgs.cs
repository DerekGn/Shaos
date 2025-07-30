
namespace Shaos.Sdk.Collections.Generic
{
    public class ListChangedEventArgs<T> : EventArgs
    {
        public ListChangedEventArgs(ListChangedAction action, IList<T> items)
        {
            Action = action;
            Items = items;
        }

        public ListChangedAction Action { get; }
        public IList<T> Items { get; }
    }
}

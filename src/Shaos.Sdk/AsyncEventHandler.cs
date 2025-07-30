namespace Shaos.Sdk
{
    /// <summary>
    /// An async event handler
    /// </summary>
    /// <typeparam name="TEventArgs"></typeparam>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate Task AsyncEventHandler<TEventArgs>(object sender, TEventArgs e);
}

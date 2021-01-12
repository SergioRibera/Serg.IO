namespace Serg.IO.Helper {
    /// <summary>
    /// Serg.IO Callback.
    /// </summary>
    public delegate void SergIOCallback(string data);

    public class SergIOCallbackData
    {
        /// <summary>
        /// Occurs when callback.
        /// </summary>
        public event SergIOCallback Callback;
        /// <summary>
        /// Invokes the callback.
        /// </summary>
        /// <param name="data">Data.</param>
        public void InvokeCallback(string data)
        {
            Callback?.Invoke(data);
        }
    }
}

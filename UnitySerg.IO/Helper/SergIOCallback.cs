namespace UnitySerg.IO.Helper {
    public delegate void SergIOCallback(string data);

    public class SergIOCallbackData {
        public event SergIOCallback callback;
        public void InvokeCallback(string data){
            callback?.Invoke(data);
        }
    }
}

namespace Serg.IO {
    /// <summary>
    /// Type event.
    /// </summary>
    public enum TypeEvent
    {
        UNKNOW = -1,
        CONNECT = 0,
        DISCONNECT = 1,
        PING = 2,
        PONG = 3,
        MESSAGE = 4,
        NEWCONNECTION = 7
    }
}

using System;
namespace Serg.IO.Data
{
    [Serializable]
    public class SergServerSettings
    {
        /// <summary>
        /// Indicates if the logs should be activated in the console.
        /// </summary>
        public bool debug = false;
        /// <summary>
        /// It is the port to which the client and server will connect
        /// </summary>
        public int port = 8465;
        /// <summary>
        /// The max clients.
        /// </summary>
        public int maxClients = 10;
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Serg.IO.SergIOSettings"/> class.
        /// </summary>
        /// <param name="_debug">If set to <c>true</c> debug.</param>
        /// <param name="_port">Port.</param>
        /// <param name="_maxClients">Max clients.</param>
        public SergServerSettings(bool _debug = false, int _port = 8465, int _maxClients = 10)
        {
            this.debug = _debug;
            this.port = _port;
            this.maxClients = _maxClients;
        }
    }
}

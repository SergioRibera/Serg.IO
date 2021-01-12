using System.Net;
using System;
namespace Serg.IO.Data
{
    [Serializable]
    public class SergClientSettings
    {
        /// <summary>
        /// Indicates if the logs should be activated in the console.
        /// </summary>
        public bool debug = false;
        /// <summary>
        /// Indicates if the server is local network (Intranet) or external server
        /// </summary>
        public bool isLocal = true;
        /// <summary>
        /// The ip connect.
        /// </summary>
        public string ipConnect;
        /// <summary>
        /// It is the port to which the client and server will connect
        /// </summary>
        public int port = 8465;

        public SergClientSettings(bool _debug = false, string _ipConnect = "", int _port = 8465) {
            this.debug = _debug;
            this.ipConnect = _ipConnect;
            this.port = _port;
        }
        public IPAddress GetIP {
            get => ipConnect == "" ? Dns.GetHostEntry("localhost").AddressList[0] : IPAddress.Parse(ipConnect);
        }        
    }
}

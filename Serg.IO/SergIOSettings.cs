using System;

namespace Serg.IO{
    [Serializable]
    public class SergIOSettings{
        public bool debug = false;
        public int port = 8465;
        public int maxClients = 10;
        
        public SergIOSettings(bool _debug=false, int _port=8465, int _maxClients=10){
            this.debug = _debug;
            this.port = _port;
            this.maxClients = _maxClients;
        }
    }
}

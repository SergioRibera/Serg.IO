using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

using UnitySerg.IO.Data;
using UnitySerg.IO.Packet;
using UnitySerg.IO.Helper;

using UnityEngine;
using Newtonsoft.Json;

namespace UnitySerg.IO{
    public class SergServerBehaviour : MonoBehaviour {

        public SergIOSettings settings; 
        
        Dictionary<string, SergIOCallbackData> callbacks;

        TcpListener listener;
        bool stop = false;

        public bool isServerRunning{
            private set; get;
        }
        TcpClient client = null;

        private List<TcpClient> clients = new List<TcpClient>();
        const int SEND_RECEIVE_COUNT = 1024;

        protected void StartServerAsync()
        {
            //Application.runInBackground = true;
            if(stop) stop = false;
            callbacks = new Dictionary<string, SergIOCallbackData>();
            if(settings == null) settings = new SergIOSettings();

            listener = new TcpListener(IPAddress.Parse("192.168.0.1"), settings.port);
            listener.Start();
            isServerRunning = true;
            NewConnectionsCheck();
        }
        void NewConnectionsCheck(){
            while (isServerRunning)
            {
                client = listener.AcceptTcpClient();
                Task.Run(() => HandleConnectionsAsync(client));
            }
        }
        async Task HandleConnectionsAsync(object obj)
        {
            var thisClient = (TcpClient) obj;
            var writer = new StreamWriter(thisClient.GetStream(), Encoding.ASCII);
            var reader = new StreamReader(thisClient.GetStream(), Encoding.ASCII);
            var isClientConnected = true;
            LOG($"New connection from {thisClient.GetHashCode()}");
            var rData = string.Empty;
            while (isClientConnected)
            {
                rData = await reader.ReadToEndAsync();
                if (string.IsNullOrWhiteSpace(rData)) continue;
                var package = SergIOPacket.Deserialize(rData);
                var data = SergIOData.Deserialize(package.json);
                LOG($"SERVER received {data.name} from {thisClient.GetHashCode()}");
                switch(package.packetType){
                    case TypeEvent.MESSAGE:
                        CallCallback(data);
                        if(!clients.Contains(thisClient)) clients.Add(thisClient);
                        EmitPackage(package);    
                        break;
                    case TypeEvent.DISCONNECT:
                        EmitPackage(package);
                        clients.Remove(thisClient);
                        isClientConnected = DisconnectClient(thisClient);
                        break;
                }
                rData = string.Empty;
            }
        }
        //send data for all clients
        protected void Emit(string name){
            stop = false;
            EmitPackage(new SergIOPacket(TypeEvent.MESSAGE, new SergIOData(name)));
        }
        ///
        ///<sumary name="This is a name to function to search">This function
        ///Emit to clients</sumary>
        ///
        protected void Emit(string name, string data){
            stop = false;
            EmitPackage(new SergIOPacket(TypeEvent.MESSAGE, new SergIOData(name, data)));
        }
        protected void Emit(string name, object data){
            stop = false;
            EmitPackage(new SergIOPacket(TypeEvent.MESSAGE, new SergIOData(name, JsonConvert.SerializeObject(data))));
        }
        
        protected void On(string name, SergIOCallback callback){ // guardamos el callback
            if (callbacks.ContainsKey(name)){
                callbacks[name].callback += callback;
            }else{
                SergIOCallbackData d = new SergIOCallbackData();
                d.callback += callback;
                callbacks.Add(name, d);
            }
        }
        void CallCallback(SergIOData data){
            if(callbacks.ContainsKey(data.name))
                callbacks[data.name].InvokeCallback(data.data);
        }

        void EmitPackage(SergIOPacket packet){
            if(stop) return;
            byte[] b = packet.Seralize().GetBytes();
            foreach (var c in clients){
                NetworkStream ns = c.GetStream();
                ns.Write(b, 0, b.Length);
            }
        }

        void LOG(string messsage)
        {
            if (settings.debug)
                Console.WriteLine(messsage);
        }

        // stop everything
        protected void Stop()
        {
            if (listener != null)
                listener.Stop();
            foreach (TcpClient c in clients)
                DisconnectClient(c);
            stop = true;
        }
        private bool DisconnectClient(TcpClient client)
        {
            LOG($"Disconnected {client.GetHashCode()}");
            client.GetStream().Close();
            client.Close();
            client = null;
            return false;
        }
    }
}

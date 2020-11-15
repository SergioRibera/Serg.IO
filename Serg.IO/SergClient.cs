using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

using Serg.IO.Data;
using Serg.IO.Packet;
using Serg.IO.Helper;

using Newtonsoft.Json;

namespace Serg.IO{
    public class SergClient {

        public bool Connected { private set; get; }
        public SergIOSettings settings; 
        Dictionary<string, SergIOCallbackData> callbacks;

        private TcpClient client { get; set; } = new TcpClient();
        private StreamWriter Writer { get; set; }
        private StreamReader Reader { get; set; }

        protected void ConnectToServer(){
            if(TryConnect()) return;
            Connected = true;
            Task.Run(HandleCommunication);
        }
        protected bool TryConnect(){
            try {
                client.Connect("192.168.0.1", settings.port);
                LOG("Connected to server 192.168.0.1:" + settings.port);
                return true;
            }catch (SocketException e){
                LOG(e.Message);
                return false;
            }
        }
        
        private Task HandleCommunication()
        {
            Reader = new StreamReader(client.GetStream(), Encoding.ASCII);
            Writer = new StreamWriter(client.GetStream(), Encoding.ASCII);
            Task.Run(HandleCommandsAsync);
            return Task.Delay(-1);
        }
        
        private async Task HandleCommandsAsync()
        {
            string rData = string.Empty;
            while (Connected)
            {
                rData = await Reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(rData)) continue;
                var package = SergIOPacket.Deserialize(rData);
                var data = SergIOData.Deserialize(package.json);
                LOG($"SERVER received {data.name} from {client.GetHashCode()}");
                switch(package.packetType){
                    case TypeEvent.MESSAGE:
                        CallCallback(data);
                        break;
                    case TypeEvent.DISCONNECT:
                        Connected = Disconnect();
                        break;
                }
                rData = string.Empty;
            }
        }

        //send data for all clients
        protected void Emit(string name) =>
            EmitPackage(new SergIOPacket(TypeEvent.MESSAGE, new SergIOData(name)));
        ///
        ///<sumary name="This is a name to function to search">This function
        ///Emit to clients</sumary>
        ///
        protected void Emit(string name, string data) =>
            EmitPackage(new SergIOPacket(TypeEvent.MESSAGE, new SergIOData(name, data)));
        protected void Emit(string name, object data) =>
            EmitPackage(new SergIOPacket(TypeEvent.MESSAGE, new SergIOData(name, JsonConvert.SerializeObject(data))));
        
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

        async void EmitPackage(SergIOPacket packet){
            if(!Connected) return;
            await Writer.WriteLineAsync(packet.Seralize());
            await Writer.FlushAsync();
        }

        void LOG(string messsage)
        {
            if (settings.debug)
                Console.WriteLine(messsage);
        }

        protected bool Disconnect(bool isSenderLocal = false)
        {
            if(isSenderLocal) EmitPackage(new SergIOPacket(TypeEvent.DISCONNECT));
            LOG($"Disconnected {client.GetHashCode()}");
            client.GetStream().Close();
            client.Close();
            client = null;
            return false;
        }
    }
}

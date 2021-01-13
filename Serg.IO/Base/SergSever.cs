using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

using Serg.IO.Data;
using Serg.IO.Packet;
using Serg.IO.Helper;

using Newtonsoft.Json;

namespace Serg.IO{
    public class SergServer {
        static readonly int maxBufferSize = 4096;
        /// <summary>
        /// The settings.
        /// </summary>
        public SergServerSettings settings = new SergServerSettings();
        /// <summary>
        /// Gets a value indicating whether this <see cref="T:Serg.IO.SergServer"/> is server running.
        /// </summary>
        /// <value><c>true</c> if is server running; otherwise, <c>false</c>.</value>
        public bool IsServerRunning { private set; get; }

        Dictionary<string, SergIOCallbackData> callbacks = null;
        IPEndPoint IpEndPoint;
        Socket Connection;
        List<Socket> clients;
        internal Action<string> LOG;

        byte[] rcvBuffer;
        byte[] sndBuffer;

        public SergServer(){
            rcvBuffer = new byte[maxBufferSize];
            sndBuffer = new byte[maxBufferSize];
            clients = new List<Socket>();
            if (callbacks == null) callbacks = new Dictionary<string, SergIOCallbackData>();
            LOG = LOG ?? new Action<string>((data) => {
                if (settings.debug) Console.WriteLine(data);
            });
        }

        /// <summary>
        /// Starts the server async.
        /// </summary>
        public void StartServer()
        {
            if (!IsServerRunning) IsServerRunning = true;
            IpEndPoint = new IPEndPoint(IPAddress.Parse(Helper.Helper.MiIp()), settings.port);
            Connection = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Connection.Bind(IpEndPoint);
            Connection.Listen(settings.maxClients);

            IsServerRunning = true;
            CallCallback(new SergIOData("server-start", Helper.Helper.MiIp()));
            Connection.BeginAccept(AcceptCallback, null);
        }
        void AcceptCallback(IAsyncResult ar)
        {
            try
            {
                var accepted = Connection.EndAccept(ar);
                clients.Add(accepted);
                Connection.BeginAccept(AcceptCallback, null);
                LOG("New Client Connected");
                accepted.BeginReceive(rcvBuffer, 0, maxBufferSize, SocketFlags.None, (ReceiveCallback), accepted);
                EmitPackage(new SergIOPacket(TypeEvent.NEWCONNECTION));
            }
            catch { return; }
        }
        void ReceiveCallback(IAsyncResult ar)
        {
            Socket sock = (Socket) ar.AsyncState;
            int receivedBytes = 0;
            try {
                receivedBytes = sock.EndReceive(ar);
            } catch {
                sock.Close();
                clients.Remove(sock);
                return;
            }
            sock.BeginReceive(rcvBuffer, 0, receivedBytes, SocketFlags.None, (ReceiveCallback), sock);
            ProcessData(sock, rcvBuffer);
        }
        void ProcessData(Socket thisClient, byte[] _data)
        {
            string rData = Encoding.ASCII.GetString(_data);
            if (!string.IsNullOrWhiteSpace(rData) || !string.IsNullOrEmpty(rData))
            {
                var package = SergIOPacket.Deserialize(rData);
                LOG(package.json);
                var data = SergIOData.Deserialize(package.json);
                LOG($"SERVER received {data.name}");
                switch (package.packetType)
                {
                    case TypeEvent.CONNECT:
                        break;
                    case TypeEvent.NEWCONNECTION:
                        break;
                    case TypeEvent.PONG:
                        break;
                    case TypeEvent.MESSAGE:
                        CallCallback(data);
                        if (!clients.Contains(thisClient)) clients.Add(thisClient);
                        EmitPackage(package);
                        break;
                    case TypeEvent.DISCONNECT:
                        EmitPackage(package);
                        clients.Remove(thisClient);
                        DisconnectClient(thisClient);
                        break;
                }
            }else 
                LOG("The data received is empty");
        }
        /// <summary>
        /// Emit the specified name.
        /// </summary>
        /// <param name="name">Name.</param>
        public void Emit(string name)
        {
            IsServerRunning = true;
            EmitPackage(new SergIOPacket(TypeEvent.MESSAGE, new SergIOData(name)));
        }
        /// <summary>
        /// Emit the specified name and data.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="data">Data.</param>
        public void Emit(string name, string data)
        {
            IsServerRunning = true;
            EmitPackage(new SergIOPacket(TypeEvent.MESSAGE, new SergIOData(name, data)));
        }
        /// <summary>
        /// Emit the specified name and data.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="data">Data.</param>
        public void Emit(string name, object data)
        {
            string d = (string)data;
            if (d == null)
            {
                IsServerRunning = true;
                EmitPackage(new SergIOPacket(TypeEvent.MESSAGE, new SergIOData(name, d)));
                return;
            }
            IsServerRunning = true;
            EmitPackage(new SergIOPacket(TypeEvent.MESSAGE, new SergIOData(name, JsonConvert.SerializeObject(data))));
        }
        /// <summary>
        /// On the specified name and callback.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="callback">Callback.</param>
        public void On(string name, SergIOCallback callback)
        {
            if (callbacks == null) callbacks = new Dictionary<string, SergIOCallbackData>();
            if (callbacks.ContainsKey(name))
                callbacks[name].Callback += callback;
            else
            {
                SergIOCallbackData d = new SergIOCallbackData();
                d.Callback += callback;
                callbacks.Add(name, d);
            }
        }
        void CallCallback(SergIOData data)
        {
            if (callbacks.ContainsKey(data.name))
                callbacks[data.name].InvokeCallback(data.data);
        }

        void EmitPackage(SergIOPacket packet)
        {
            if (!IsServerRunning) return;
            LOG("Prepare to send data");
            try
            {
                byte[] b = packet.Seralize().GetBytes();
                foreach (var c in clients)
                    if(c.Connected)
                        c.Send(b);
            }catch(Exception e){
                LOG(e.ToString());
            }
            LOG("End sending Data");
        }
        /// <summary>
        /// Stop this instance.
        /// </summary>
        public void Stop()
        {
            if (IsServerRunning)
            {
                EmitPackage(new SergIOPacket(TypeEvent.DISCONNECT, new SergIOData("disconnect-server")));
                if (Connection != null)
                    Connection.Close();
                foreach (Socket c in clients)
                    DisconnectClient(c);
                IsServerRunning = false;
            }
        }
        bool DisconnectClient(Socket client)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));

            LOG($"Disconnected {client.GetHashCode()}");
            client.Close();
            client = null;
            return false;
        }
    }
}

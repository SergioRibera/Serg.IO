using System;
using System.Text;
using System.Collections.Generic;
using System.Net.Sockets;

using Serg.IO.Data;
using Serg.IO.Packet;
using Serg.IO.Helper;

using Newtonsoft.Json;
using System.Net;
using WebSocketSharp;
using System.Threading;

namespace Serg.IO{
    public class SergClient {
        #region SergIO
        public static int maxBufferSize = 4096;
        /// <summary>
        /// Gets a value indicating whether this <see cref="T:Serg.IO.SergClient"/> is connected.
        /// </summary>
        /// <value><c>true</c> if connected; otherwise, <c>false</c>.</value>
        public bool ConnectedLocal { private set; get; }
        /// <summary>
        /// The settings.
        /// </summary>
        public SergClientSettings settings = new SergClientSettings();

        Dictionary<string, SergIOCallbackData> callbacks;
        Socket Connection { get; set; }

        internal Action<string> LOG;
        IPEndPoint IpEndPoint;
        byte[] rcvBuffer;
        #endregion

        #region SocketIO
        public bool ConnectedOnline{ private set; get; }
        public WebSocket OnlineSocket { get => ws; }
        WebSocket ws;
        Thread socketThread;
        #endregion

        #region Public Functions
        public SergClient(SergClientSettings _settings = null){
            LOG = LOG ?? new Action<string>((data) => { if (settings.debug) Console.WriteLine(data); });
            settings = _settings == null ? new SergClientSettings() : _settings;
            if (callbacks == null) callbacks = new Dictionary<string, SergIOCallbackData>();
			ws = new WebSocket(settings.url);
            ws.OnOpen += (sender, e) => CallCallback(new SergIOData("server-connect"));
			ws.OnMessage += (sender, e)  => ProcessData(e.Data.GetBytes());
			ws.OnError += (sender, e) => {};
			ws.OnClose += (sender, e) => Disconnect();
        }

        /// <summary>
        /// Connects to server.
        /// </summary>
        public void ConnectToLocalServer()
        {
            LOG = LOG ?? new Action<string>((data) => { if (settings.debug) Console.WriteLine(data); });
            ConnectedOnline = false;
            ConnectedLocal = true;
            try
            {
                IpEndPoint = new IPEndPoint(settings.GetIP, settings.port);
                Connection = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                Connection.BeginConnect(IpEndPoint, (ConnectCallback), Connection);
                rcvBuffer = new byte[maxBufferSize];
                Connection.BeginReceive(rcvBuffer, 0, maxBufferSize, SocketFlags.None, ReceiveCallback, Connection);
            }
            catch (SocketException e) {
                LOG(e.Message);
            }
            if(socketThread == null){
                socketThread = new Thread(RunSocketThread);
                socketThread.Start(ws);
            }
        }
        public void ConnectToOnlineServer(){
            ConnectedOnline = true;
            ConnectedLocal = false;
            if(socketThread == null){
                socketThread = new Thread(RunSocketThread);
                socketThread.Start(ws);
            }
        }
        public void ChangeConnectionServer(bool isLocal){
            Disconnect();
            if(isLocal)
                ConnectToLocalServer();
            else
                ConnectToOnlineServer();
        }
        /// <summary>
        /// Emit the specified event.
        /// </summary>
        /// <param name="name">Name of event</param>
        public void Emit(string name) =>
            EmitPackage(new SergIOPacket(TypeEvent.MESSAGE, new SergIOData(name)));
        /// <summary>
        /// Emit the specified name and data.
        /// </summary>
        /// <param name="name">Nombre del evento</param>
        /// <param name="data">Contenido de datos</param>
        public void Emit(string name, string data) =>
            EmitPackage(new SergIOPacket(TypeEvent.MESSAGE, new SergIOData(name, data)));
        /// <summary>
        /// Emit the specified name and data.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="data">Data.</param>
        public void Emit(string name, object data) =>
            EmitPackage(new SergIOPacket(TypeEvent.MESSAGE, new SergIOData(name, JsonConvert.SerializeObject(data))));
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
            else {
                SergIOCallbackData d = new SergIOCallbackData();
                d.Callback += callback;
                callbacks.Add(name, d);
            }
        }
         /// <summary>
        /// Disconnect the specified isSenderLocal.
        /// </summary>
        public void Disconnect()
        {
            EmitPackage(new SergIOPacket(TypeEvent.DISCONNECT));
            if (ConnectedLocal) {
                ConnectedLocal = false;
                LOG($"Disconnected {Connection.GetHashCode()}");
                Connection.BeginDisconnect(false, (DisconnectCallback), Connection);
            }
            if(ConnectedOnline){
                ws.Close();
                ConnectedOnline = false;
            }
        }
        #endregion

        #region Private Functions
        void ConnectCallback(IAsyncResult ar) {
            if(ConnectedLocal){
                Connection.EndConnect(ar);
                LOG($"Connected to server {settings.ipConnect}:{settings.port}");
                CallCallback(new SergIOData("server-connect"));
            }
        }
        void ReceiveCallback(IAsyncResult ar)
        {
            if(ConnectedLocal){
                int receivedBytes = Connection.EndReceive(ar);
                if (receivedBytes > 0)
                {
                    Connection.BeginReceive(rcvBuffer, 0, receivedBytes, SocketFlags.None, (ReceiveCallback), Connection);
                    ProcessData(rcvBuffer);
                }
            }
        }
        void ProcessData(byte[] _data)
        {
            if(_data.Length > 0){
                string _d = Encoding.ASCII.GetString(_data);
                if(_d.Length > 0){
                    if (ConnectedLocal)
                    {
                        LOG("Process Package");
                        LOG($"Received Package: {_d}");
                        var package = SergIOPacket.Deserialize(_d);
                        var data = SergIOData.Deserialize(package.json);
                        LOG($"SERVER received {data.name} from {Connection.GetHashCode()}");
                        switch (package.packetType)
                        {
                            case TypeEvent.CONNECT:
                                break;
                            case TypeEvent.NEWCONNECTION:
                                break;
                            case TypeEvent.PING:
                                break;
                            case TypeEvent.MESSAGE:
                                CallCallback(data);
                                break;
                            case TypeEvent.DISCONNECT:
                                Disconnect();
                                break;
                        }
                    }
                }else
                    LOG("Data Processed is Empty");
            }else
                LOG("The data received is empty");
        }
        void CallCallback(SergIOData data)
        {
            if (callbacks.ContainsKey(data.name))
                callbacks[data.name].InvokeCallback(data.data);
        }

        void EmitPackage(SergIOPacket packet)
        {
            if (ConnectedLocal) {
                LOG("Prepare to Send Data");
                byte[] data = packet.Seralize().GetBytes();
                if(Connection.Connected)
                    Connection.Send(data);
                LOG("Data Sended");
            }
            if(ConnectedOnline){
                LOG("Prepare to Send Data");
                ws.Send(packet.Seralize());
                LOG("Data Sended");
            }
        }
        void DisconnectCallback(IAsyncResult ar)
        {
            Connection.EndDisconnect(ar);
            CallCallback(new SergIOData("disconnected"));
        }
        private void RunSocketThread(object obj)
		{
			WebSocket webSocket = (WebSocket)obj;
			while(ConnectedLocal || ConnectedOnline){
				if(ConnectedLocal || ConnectedLocal){
					Thread.Sleep(settings.reconnectDelay);
				} else {
                    if(ConnectedOnline)
                        webSocket.Connect();
                    else{
                        Connection.BeginConnect(IpEndPoint, (ConnectCallback), Connection);
                        Connection.BeginReceive(rcvBuffer, 0, maxBufferSize, SocketFlags.None, ReceiveCallback, Connection);
                    }
				}
			}
			Disconnect();
		}
        #endregion
    }
}

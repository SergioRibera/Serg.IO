using System;
using UnitySerg.IO.Data;
using UnitySerg.IO.Helper;
using UnityEngine;

namespace UnitySerg.IO{
    public class SergServerBehaviour : MonoBehaviour {

        public SergServerSettings settings = new SergServerSettings();

        //public bool IsServerRunning { get { return server.IsServerRunning; } }

        internal SergServer server;
        public void Start()
        {
            server = new SergServer();
            server.settings = settings;
            Application.quitting += Stop;
        }
        /// <summary>
        /// Starts the server async.
        /// </summary>
        public void StartServer(int port, int maxBufferSize = 1024) => server.StartServer(port, maxBufferSize);
        /// <summary>
        /// Emit the specified name.
        /// </summary>
        /// <param name="name">Name.</param>
        public void Emit(string name) => server.Emit(name);
        /// <summary>
        /// Emit the specified name and data.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="data">Data.</param>
        public void Emit(string name, string data) => server.Emit(name, data);
        /// <summary>
        /// Emit the specified name and data.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="data">Data.</param>
        public void Emit(string name, object data) => server.Emit(name, data);
        /// <summary>
        /// On the specified name and callback.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="callback">Callback.</param>
        public void On(string name, SergIOCallback callback) => server.On(name, callback);
        /// <summary>
        /// Stop this instance.
        /// </summary>
        public void Stop() => server.Stop();
    }
}

using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Serg.IO.Helper{
    public static class Helper{
        /// <summary>
        /// Gets the bytes.
        /// </summary>
        /// <returns>The bytes.</returns>
        /// <param name="text">Text.</param>
        public static byte[] GetBytes(this string text) => Encoding.UTF8.GetBytes(text);
        /// <summary>
        /// 
        /// </summary>
        /// <returns>The ip.</returns>
        public static string MiIp()
        {
            string ips = "";
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    ips = ip.ToString();
                    break;
                }
            return ips;
        }
    }
}

using System.Text;

namespace UnitySerg.IO.Helper{
    public static class Helper{
        public static byte[] GetBytes(this string text) => Encoding.UTF8.GetBytes(text);
    }
}

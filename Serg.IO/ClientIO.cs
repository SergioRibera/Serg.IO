using System;
using System.IO;
using System.Net.Sockets;

namespace Serg.IO{
    public class ClientIO {
        public const int BUFFER_SIZE = 255;
        public string ID{
            private set;
            get;
        }
        public Socket Socket;
        public byte[] Buffer; 
        public MemoryStream Data;

        public ClientIO(Socket _socket){
            Socket = _socket;
            ID = RandomId();
        }
        public void Disconnect() {
            Socket.Shutdown(SocketShutdown.Both);
            Socket.Close();
        }
        string RandomId(int Length = 15, bool Mayusculas = true, bool minusculas = true, bool simbolos = true)
        {
            if (!Mayusculas && !minusculas && !simbolos || Length <= 0)
                return "Null";
            string newId = "";
            string abecedarioMayus = "A-B-C-D-E-F-G-H-I-J-K-L-M-N-Ñ-O-P-Q-R-S-T-U-V-W-X-Y-Z";
            string abecedarioMinus = "a-b-c-d-e-f-g-h-i-j-k-l-m-n-ñ-o-p-q-r-s-t-u-v-w-x-y-z";
            string caracteres = "- _ / & * !";
            Random r = new Random();
            int actualLength = 0;
            while (actualLength < Length)
            {
                int value = r.Next(0, 1);
                string letra = "";
                switch (value)
                {
                    case 0:
                        int proba_Char = r.Next(0, 5);
                        if (Mayusculas)
                        {
                            if (proba_Char == 2)
                            {
                                if (simbolos)
                                {
                                    letra = caracteres.Split(' ')[r.Next(0, 4)] + abecedarioMayus.Split('-')[r.Next(0, 26)];
                                    actualLength++;
                                }
                            }
                            else
                            {
                                letra = abecedarioMayus.Split('-')[r.Next(0, 26)];
                                actualLength++;
                            }
                        }
                        break;
                    case 1:
                        int proba_Char2 = r.Next(0, 5);
                        if (minusculas)
                        {
                            if (proba_Char2 == 3)
                            {
                                if (simbolos)
                                {
                                    letra = caracteres.Split(' ')[r.Next(0, 4)] + abecedarioMinus.Split('-')[r.Next(0, 26)];
                                    actualLength++;
                                }
                            }
                            else
                            {
                                letra = abecedarioMinus.Split('-')[r.Next(0, 26)];
                                actualLength++;
                            }
                        }
                        break;
                }
                newId = newId + letra;
            }
            return newId;
        }
    }
}

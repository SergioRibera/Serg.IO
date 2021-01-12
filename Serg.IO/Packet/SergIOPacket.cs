using Serg.IO.Data;
using Newtonsoft.Json;

namespace Serg.IO.Packet {
    public class SergIOPacket {
        /// <summary>
        /// The type of the packet.
        /// </summary>
        public TypeEvent packetType;
        /// <summary>
        /// The json data.
        /// </summary>
        public string json;
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Serg.IO.Packet.SergIOPacket"/> class.
        /// </summary>
        public SergIOPacket() : this(TypeEvent.UNKNOW) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Serg.IO.Packet.SergIOPacket"/> class.
        /// </summary>
        /// <param name="packetType">Packet type.</param>
        public SergIOPacket(TypeEvent packetType) : this(packetType, "") { }
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Serg.IO.Packet.SergIOPacket"/> class.
        /// </summary>
        /// <param name="packetType">Packet type.</param>
        /// <param name="data">Data.</param>
        public SergIOPacket(TypeEvent packetType, SergIOData data) : this(packetType, JsonConvert.SerializeObject(data)) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Serg.IO.Packet.SergIOPacket"/> class.
        /// </summary>
        /// <param name="packetType">Packet type.</param>
        /// <param name="json">Json.</param>
        public SergIOPacket(TypeEvent packetType, string json)
        {
            this.packetType = packetType;
            this.json = json;
        }
        /// <summary>
        /// Deserialize the specified json.
        /// </summary>
        /// <returns>The deserialize.</returns>
        /// <param name="json">Json.</param>
        public static SergIOPacket Deserialize(string json) => JsonConvert.DeserializeObject<SergIOPacket>(json);
        /// <summary>
        /// Seralize this instance.
        /// </summary>
        /// <returns>The seralize.</returns>
        public string Seralize() => JsonConvert.SerializeObject(this);
    }
}

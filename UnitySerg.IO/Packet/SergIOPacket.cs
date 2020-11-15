using UnitySerg.IO.Data;
using Newtonsoft.Json;

namespace UnitySerg.IO.Packet {
    public class SergIOPacket {
        public TypeEvent packetType;
        public string json;

        public SergIOPacket() : this(TypeEvent.UNKNOW) { }
        public SergIOPacket(TypeEvent packetType) : this(packetType, "") { }
        public SergIOPacket(TypeEvent packetType, SergIOData data) : this(packetType, JsonConvert.SerializeObject(data)){}
        public SergIOPacket(TypeEvent packetType, string json) {
            this.packetType = packetType;
            this.json = json;
        }
        public static SergIOPacket Deserialize(string json) => JsonConvert.DeserializeObject<SergIOPacket>(json);
        public string Seralize() => JsonConvert.SerializeObject(this);
    }
}

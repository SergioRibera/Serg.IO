using Newtonsoft.Json;

namespace Serg.IO.Data {
    public class SergIOData {

        public string name;

        public string data;

        public SergIOData(string name) {
            this.name = name;
            this.data = "";
        }

        public SergIOData(string name, string data) {
            this.name = name;
            this.data = data;
        }
        public static SergIOData Deserialize(string json) => JsonConvert.DeserializeObject<SergIOData>(json);
    }
}

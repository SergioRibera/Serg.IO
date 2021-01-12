using Newtonsoft.Json;

namespace Serg.IO.Data {
    public class SergIOData {
        /// <summary>
        /// The name.
        /// </summary>
        public string name;
        /// <summary>
        /// The data.
        /// </summary>
        public string data;
        public SergIOData(){
            this.name = "";
            this.data = "";
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Serg.IO.Data.SergIOData"/> class.
        /// </summary>
        /// <param name="name">Name.</param>
        public SergIOData(string name)
        {
            this.name = name;
            this.data = "";
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Serg.IO.Data.SergIOData"/> class.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="data">Data.</param>
        public SergIOData(string name, string data)
        {
            this.name = name;
            this.data = data;
        }
        /// <summary>
        /// Deserialize the specified json.
        /// </summary>
        /// <returns>The deserialize.</returns>
        /// <param name="json">Json.</param>
        public static SergIOData Deserialize(string json) => JsonConvert.DeserializeObject<SergIOData>(json);
    }
}

using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace Message_Board.Network
{
    public class NetworkPacket
    {
        [JsonProperty] public int index;
        [JsonProperty] public int Max;
        [JsonProperty] public readonly byte[] Content;


        [JsonConstructor]
        public NetworkPacket(int index, int max, byte[] content)
        {
            this.index = index;
            Max = max;
            Content = content;
        }
    }
}
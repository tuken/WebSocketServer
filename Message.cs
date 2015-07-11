using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;

namespace WebSocketServer
{
    public class Message
    {
        public string eventName { get; set; }

        //public abstract object data { get; set; }

        public string ToJson()
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(GetType());
            MemoryStream stream = new MemoryStream();
            serializer.WriteObject(stream, this);
            stream.Position = 0;
            StreamReader reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }

    public class JoinRoom
    {
        public string eventName { get; set; }

        public object data { get; set; }
    }

    public class NewPeerConnected : Message
    {
        public NewPeerConnected()
        {
            eventName = "new_peer_connected";
        }

        public Dictionary<string, List<string>> data { get; set; }
    }

    public class GetPeers : Message
    {
        public class InnerData
        {
            public List<string> connections { get; set; }

            public string you { get; set; }
        }

        public GetPeers()
        {
            eventName = "get_peers";
        }

        public InnerData data { get; set; }
    }
}

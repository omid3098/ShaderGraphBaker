using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ShaderGraphBaker
{
    public class Edge
    {
        [JsonProperty("m_OutputSlot")]
        public PutSlot OutputSlot { get; set; }

        [JsonProperty("m_InputSlot")]
        public PutSlot InputSlot { get; set; }

        public override bool Equals(object obj)
        {
            return obj is Edge edge &&
                InputSlot.Node.Id == edge.InputSlot.Node.Id &&
                OutputSlot.Node.Id == edge.OutputSlot.Node.Id;
        }
    }
    public enum PutType
    {
        Input,
        Output,
    }

    public class PutSlot
    {
        [JsonProperty("m_Node")]
        public Node Node { get; set; }

        [JsonProperty("m_SlotId")]
        public long SlotId { get; set; }
    }

    public class Node
    {
        [JsonProperty("m_Id")]
        public string Id { get; set; }
    }
}
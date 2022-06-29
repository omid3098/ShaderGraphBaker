namespace ShaderGraphBaker
{
    public interface ISubgraphNode
    {
        public Node Node { get; }
        public string[] JsonData { get; }
        public int[] SlotIDs { get; }
    }
    public class RawNormalMapSerialized : ISubgraphNode
    {
        readonly string[] _JsonData = {
                        "{    \"m_SGVersion\": 0,    \"m_Type\": \"UnityEditor.ShaderGraph.Vector3MaterialSlot\",    \"m_ObjectId\": \"ba4e521c51824a6aa652dba4f57a1cbb\",    \"m_Id\": 0,    \"m_DisplayName\": \"Out\",    \"m_SlotType\": 1,    \"m_Hidden\": false,    \"m_ShaderOutputName\": \"Out\",    \"m_StageCapability\": 3,    \"m_Value\": {        \"x\": 0.0,        \"y\": 0.0,        \"z\": 0.0    },    \"m_DefaultValue\": {        \"x\": 0.0,        \"y\": 0.0,        \"z\": 0.0    },    \"m_Labels\": []}",
                        "{    \"m_SGVersion\": 0,    \"m_Type\": \"UnityEditor.ShaderGraph.SubGraphNode\",    \"m_ObjectId\": \"b3ea08d4b1f946b09996e306c629962d\",    \"m_Group\": {        \"m_Id\": \"\"    },    \"m_Name\": \"RawNormalMap\",    \"m_DrawState\": {        \"m_Expanded\": true,        \"m_Position\": {            \"serializedVersion\": \"2\",            \"x\": -247.2000274658203,            \"y\": 268.0,            \"width\": 208.00006103515626,            \"height\": 277.5999755859375        }    },    \"m_Slots\": [        {            \"m_Id\": \"6deb35c64afe4519887613322dcbdb89\"        },        {            \"m_Id\": \"ba4e521c51824a6aa652dba4f57a1cbb\"        }    ],    \"synonyms\": [],    \"m_Precision\": 0,    \"m_PreviewExpanded\": true,    \"m_PreviewMode\": 0,    \"m_CustomColors\": {        \"m_SerializableColors\": []    },    \"m_SerializedSubGraph\": \"{\\n    \\\"subGraph\\\": {\\n        \\\"fileID\\\": -5475051401550479605,\\n        \\\"guid\\\": \\\"106081550ef712e4299a6b308d605ea1\\\",\\n        \\\"type\\\": 3\\n    }\\n}\",    \"m_PropertyGuids\": [        \"4b3617e8-72be-40d5-93b6-1a27d32b4d42\"    ],    \"m_PropertyIds\": [        1857353750    ],    \"m_Dropdowns\": [],    \"m_DropdownSelectedEntries\": []}",
                        "{    \"m_SGVersion\": 0,    \"m_Type\": \"UnityEditor.ShaderGraph.Vector4MaterialSlot\",    \"m_ObjectId\": \"6deb35c64afe4519887613322dcbdb89\",    \"m_Id\": 1857353750,    \"m_DisplayName\": \"Vector4\",    \"m_SlotType\": 0,    \"m_Hidden\": false,    \"m_ShaderOutputName\": \"_Vector4\",    \"m_StageCapability\": 3,    \"m_Value\": {        \"x\": 0.0,        \"y\": 0.0,        \"z\": 0.0,        \"w\": 0.0    },    \"m_DefaultValue\": {        \"x\": 0.0,        \"y\": 0.0,        \"z\": 0.0,        \"w\": 0.0    },    \"m_Labels\": []}"
                        };
        readonly int[] _SlotIDs = { 0, 1857353750 };
        readonly Node _Node = new()
        {
            Id = "b3ea08d4b1f946b09996e306c629962d"
        };

        public string[] JsonData => _JsonData;
        public int[] SlotIDs => _SlotIDs;
        public Node Node => _Node;
    }
}
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace ShaderGraphBaker
{

    public class ShaderGraphParser
    {
        private const string TempShaderName = "shader_bake_temp.shadergraph";
        public ShaderGraphParser(Shader shader)
        {
            m_Shader = shader;
            m_ShaderString = File.ReadAllText(ShaderPath);
            m_TempShaderString = m_ShaderString;
        }

        private static string m_ShaderString;
        private string m_TempShaderString;
        private static Shader m_Shader;
        private static string ShaderPath => AssetDatabase.GetAssetPath(m_Shader);
        private static string ShaderDirectory => Path.GetDirectoryName(ShaderPath);
        private string ShaderFullName => m_Shader.name;
        private string ShaderLocalPath => m_Shader.name.Replace(ShaderName, "");
        private string ShaderName => m_Shader.name.Split('/').Last();
        private Material CreateMaterial(bool useTempShader = false)
        {
            string _shaderName = useTempShader == false ? m_Shader.name : ShaderLocalPath + TempShaderName.Split(".")[0];
            return new(Shader.Find(_shaderName));
        }

        public override string ToString() => m_ShaderString;
        private void ResetTempShaderString() => m_TempShaderString = m_ShaderString;

        public string GetBlockFieldID(BlockFieldDescriptor blockField)
        {
            return GetSlotID(JKeys.NodeNameKey, blockField.ToString());
        }

        internal void AddNode(ISubgraphNode subgraphNode)
        {
            List<string> m_splittedJson = SplitedJson(m_TempShaderString).ToList();
            string m_NewShaderData = "";
            for (int i = 0; i < subgraphNode.JsonData.Length; i++)
            {
                m_splittedJson.Add(subgraphNode.JsonData[i]);

            }
            for (int i = 0; i < m_splittedJson.Count; i++)
            {
                m_NewShaderData += m_splittedJson[i] + "\n\n";
            }
            m_TempShaderString = m_NewShaderData;
            AddNodeIDToNodes(subgraphNode);
        }

        private void AddNodeIDToNodes(ISubgraphNode subgraphNode)
        {
            List<string> m_splittedJson = SplitedJson(m_TempShaderString).ToList();
            string m_NewShaderData = "";
            for (int i = 0; i < m_splittedJson.Count; i++)
            {
                JObject data = (JObject)JsonConvert.DeserializeObject(m_splittedJson[i]);
                if (data != null)
                {
                    if (data.TryGetValue(JKeys.NodesKey, out JToken m_NodeToken))
                    {
                        var lastNode = m_NodeToken[((JArray)m_NodeToken).Count - 1];
                        lastNode.AddAfterSelf(JToken.Parse(JsonConvert.SerializeObject(subgraphNode.Node)));
                    }
                    m_NewShaderData += data.ToString() + "\n\n";
                }
            }
            m_TempShaderString = m_NewShaderData;
        }

        private string GetSlotID(string key, string value)
        {
            string[] splittedJson = SplitedJson(m_ShaderString);
            for (int i = 0; i < splittedJson.Length; i++)
            {
                JObject data = (JObject)JsonConvert.DeserializeObject(splittedJson[i]);
                // find the json section with Base Color data
                if (data != null)
                {
                    data.TryGetValue(key, out JToken jsonKey);
                    if (jsonKey != null && jsonKey.ToString() == value)
                    {
                        data.TryGetValue(JKeys.ObjectId, out JToken _id);
                        return _id.ToString();
                    }
                }
            }
            return null;
        }

        internal void RemoveTempShader()
        {
            string tempShaderFullPath = Path.Combine(ShaderDirectory, TempShaderName);
            if (File.Exists(tempShaderFullPath))
            {
                AssetDatabase.DeleteAsset(tempShaderFullPath);
            }
            AssetDatabase.Refresh();
        }

        internal void SaveTempShader()
        {
            if (m_TempShaderString != string.Empty)
            {
                // Create a new shaderGraph file with new data
                string tempShaderPath = Path.Combine(ShaderDirectory, TempShaderName);
                File.WriteAllText(tempShaderPath, m_TempShaderString);
            }
            ResetTempShaderString();
            AssetDatabase.Refresh();
        }


        public void RenderToFile(Vector2Int resolution, string suffix, int pass = -1, bool useTempShader = false)
        {
            RenderTexture m_renderTexture = RenderTexture.GetTemporary(resolution.x, resolution.y);
            Texture2D texture = new Texture2D(resolution.x, resolution.y, TextureFormat.RGBA32, false);
            Material mat = CreateMaterial(useTempShader);
            Graphics.Blit(null, m_renderTexture, mat, pass);
            //transfer image from rendertexture to texture
            RenderTexture.active = m_renderTexture;
            texture.ReadPixels(new Rect(Vector2.zero, resolution), 0, 0);
            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(m_renderTexture);

            WriteToFile(suffix, texture);

            Object.DestroyImmediate(mat);
            Object.DestroyImmediate(texture);
        }


        internal void CreateEdge(Edge edge)
        {
            string[] m_splittedJson = SplitedJson(m_TempShaderString);
            string m_NewShaderData = "";
            for (int i = 0; i < m_splittedJson.Length; i++)
            {
                JObject shaderData = (JObject)JsonConvert.DeserializeObject(m_splittedJson[i]);
                if (shaderData != null)
                {
                    shaderData.TryGetValue(JKeys.EdgesKey, out JToken allEdgeData);
                    if (allEdgeData != null)
                    {
                        JToken edgeToken = JToken.Parse(JsonConvert.SerializeObject(edge));
                        ((JArray)allEdgeData).Add(edgeToken);
                    }
                    m_NewShaderData += shaderData.ToString() + "\n\n";
                }
            }
            m_TempShaderString = m_NewShaderData;
        }

        internal void RemoveEdge(Edge edge)
        {
            string[] m_splittedJson = SplitedJson(m_TempShaderString);
            string m_NewShaderData = "";
            for (int i = 0; i < m_splittedJson.Length; i++)
            {
                JObject shaderData = (JObject)JsonConvert.DeserializeObject(m_splittedJson[i]);
                if (shaderData != null)
                {
                    shaderData.TryGetValue(JKeys.EdgesKey, out JToken allEdgeData);
                    if (allEdgeData != null)
                    {
                        for (int j = 0; j < ((JArray)allEdgeData).Count; j++)
                        {
                            Edge _edge = JsonConvert.DeserializeObject<Edge>(allEdgeData[j].ToString());
                            if (_edge.Equals(edge))
                            {
                                allEdgeData[j].Remove();
                            }
                        }
                    }
                    m_NewShaderData += shaderData.ToString() + "\n\n";
                }
            }
            m_TempShaderString = m_NewShaderData;
        }

        public Edge GetEdge(BlockFieldDescriptor blockFieldDescriptor, PutType putType)
        {
            string blockFieldID = GetBlockFieldID(blockFieldDescriptor);
            string[] m_splittedJson = SplitedJson();
            for (int i = 0; i < m_splittedJson.Length; i++)
            {
                JObject data = (JObject)JsonConvert.DeserializeObject(m_splittedJson[i]);
                if (data != null)
                {
                    data.TryGetValue(JKeys.EdgesKey, out JToken allEdgeData);
                    if (allEdgeData != null)
                    {
                        // We find all edge data that stores target blockFieldDescriptor
                        // And because all BlockFieldDescriptors only have inputs, there is no need to check for 
                        // separate input or output.
                        for (int j = 0; j < ((JArray)allEdgeData).Count; j++)
                        {
                            Edge edge = JsonConvert.DeserializeObject<Edge>(allEdgeData[j].ToString());
                            // Lets find the edge that has base color ID and normalTS
                            switch (putType)
                            {
                                case PutType.Input:
                                    if (edge.InputSlot.Node.Id == blockFieldID)
                                    {
                                        return edge;
                                    }
                                    break;
                                case PutType.Output:
                                    if (edge.OutputSlot.Node.Id == blockFieldID)
                                    {
                                        return edge;
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
            }
            return null;
        }
        internal int GetEdgeIndex(BlockFieldDescriptor blockFieldDescriptor, PutType putType)
        {
            string blockFieldID = GetBlockFieldID(blockFieldDescriptor);
            string[] m_splittedJson = SplitedJson();
            for (int i = 0; i < m_splittedJson.Length; i++)
            {
                JObject data = (JObject)JsonConvert.DeserializeObject(m_splittedJson[i]);
                if (data != null)
                {
                    data.TryGetValue(JKeys.EdgesKey, out JToken allEdgeData);
                    if (allEdgeData != null)
                    {
                        // We find all edge data that stores target blockFieldDescriptor
                        // And because all BlockFieldDescriptors only have inputs, there is no need to check for 
                        // separate input or output.
                        for (int j = 0; j < ((JArray)allEdgeData).Count; j++)
                        {
                            Edge edge = JsonConvert.DeserializeObject<Edge>(allEdgeData[j].ToString());
                            // Lets find the edge that has base color ID and normalTS
                            switch (putType)
                            {
                                case PutType.Input:
                                    if (edge.InputSlot.Node.Id == blockFieldID)
                                    {
                                        Debug.Log(blockFieldDescriptor.ToString() + " id is: " + edge.InputSlot.Node.Id);
                                        return j;
                                    }
                                    break;
                                case PutType.Output:
                                    if (edge.OutputSlot.Node.Id == blockFieldID)
                                    {
                                        Debug.Log(blockFieldDescriptor.ToString() + " id is: " + edge.InputSlot.Node.Id);
                                        return j;
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
            }
            return -1;
        }

        private void WriteToFile(string suffix, Texture2D texture)
        {
            string fileName = ShaderName + "_" + suffix + ".png";
            string path = Path.GetDirectoryName(ShaderPath);
            string fullPath = Path.Combine(path, fileName);
            byte[] png = texture.EncodeToPNG();
            File.WriteAllBytes(fullPath, png);
        }

        private static string[] SplitedJson(string shaderGraphText = null)
        {
            if (shaderGraphText == null) shaderGraphText = m_ShaderString;
            return shaderGraphText.Split("\n\n");
        }
    }
}
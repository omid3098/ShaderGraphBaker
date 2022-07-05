using UnityEngine;
using NaughtyAttributes;
using System.IO;
using UnityEditor;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
namespace ShaderGraphBaker
{
    [CreateAssetMenu()]
    public class ShaderRenderer : ScriptableObject
    {
        private const string TempShaderName = "shader_bake_temp";
        [SerializeField] Shader _shader;
        [SerializeField] Vector2Int Resolution = new Vector2Int(256, 256);
        [SerializeField] bool BaseColor;
        [SerializeField] bool Normal;
        [SerializeField] bool Metalic;
        [SerializeField] bool Smoothness;
        [SerializeField] bool Emission;
        [SerializeField] bool AmbientOcclusion;
        [SerializeField] bool Displacement;

        [Button]
        void BakeTexture()
        {
            var shaderGraphParser = new ShaderGraphParser(_shader);
            Edge baseColorInputEdge = shaderGraphParser.GetEdgeByBlockField(BlockFields.SurfaceDescription.BaseColor, PutType.Input);
            if (baseColorInputEdge != null)
            {
                if (BaseColor)
                {
                    shaderGraphParser.RenderToFile(Resolution, BlockFields.SurfaceDescription.BaseColor.name, colorSpace: RenderTextureReadWrite.sRGB);
                }
                if (Normal)
                {
                    Edge normalTSInputEdge = shaderGraphParser.GetEdgeByBlockField(BlockFields.SurfaceDescription.NormalTS, PutType.Input);
                    if (normalTSInputEdge != null)
                    {
                        shaderGraphParser.RemoveEdge(baseColorInputEdge);
                        RawNormalMapSerialized rawNormalNode = new RawNormalMapSerialized();
                        shaderGraphParser.AddNode(rawNormalNode);
                        Edge normalResultOutputToRawNormal = new Edge()
                        {
                            InputSlot = new PutSlot()
                            {
                                Node = rawNormalNode.Node,
                                SlotId = rawNormalNode.SlotIDs[1]
                            },
                            OutputSlot = normalTSInputEdge.OutputSlot
                        };
                        Edge rawOutputToBaseColorInput = new Edge()
                        {
                            InputSlot = baseColorInputEdge.InputSlot,
                            OutputSlot = new PutSlot()
                            {
                                Node = rawNormalNode.Node,
                                SlotId = rawNormalNode.SlotIDs[0]
                            }
                        };
                        shaderGraphParser.RemoveEdge(normalTSInputEdge);
                        shaderGraphParser.CreateEdge(normalResultOutputToRawNormal);
                        shaderGraphParser.CreateEdge(rawOutputToBaseColorInput);
                        shaderGraphParser.SaveTempShader();
                        var fullPath = shaderGraphParser.RenderToFile(Resolution, BlockFields.SurfaceDescription.NormalTS.name, useTempShader: true);
                        AssetDatabase.Refresh();
                        FixNormalImportSettings(fullPath);
                        shaderGraphParser.RemoveTempShader();
                    }
                    else
                    {
                        Debug.LogWarning($"There is no {BlockFields.SurfaceDescription.NormalTS.name} assigned to this shader.");
                    }
                }
                if (Metalic)
                {
                    BlockFieldDescriptor targetBlockField = BlockFields.SurfaceDescription.Metallic;
                    BakeTargetSlot(shaderGraphParser, baseColorInputEdge, targetBlockField);
                }
                if (Smoothness)
                {
                    BlockFieldDescriptor targetBlockField = BlockFields.SurfaceDescription.Smoothness;
                    BakeTargetSlot(shaderGraphParser, baseColorInputEdge, targetBlockField);
                }
                if (Emission)
                {
                    BlockFieldDescriptor targetBlockField = BlockFields.SurfaceDescription.Emission;
                    BakeTargetSlot(shaderGraphParser, baseColorInputEdge, targetBlockField);
                }
                if (AmbientOcclusion)
                {
                    BlockFieldDescriptor targetBlockField = BlockFields.SurfaceDescription.Occlusion;
                    BakeTargetSlot(shaderGraphParser, baseColorInputEdge, targetBlockField);
                }
                if (Displacement)
                {
                    Edge displacementEdgeInput = shaderGraphParser.GetEdgeByNodeName(NodeKeys.Displacement, PutType.Input, 0);
                    if (displacementEdgeInput != null)
                    {
                        shaderGraphParser.RemoveEdge(baseColorInputEdge);
                        Edge targetOutputToBaseColorInput = new Edge()
                        {
                            OutputSlot = displacementEdgeInput.OutputSlot,
                            InputSlot = baseColorInputEdge.InputSlot
                        };
                        shaderGraphParser.CreateEdge(targetOutputToBaseColorInput);
                        shaderGraphParser.SaveTempShader();
                        shaderGraphParser.RenderToFile(Resolution, NodeKeys.DisplacementFileName, useTempShader: true);
                        shaderGraphParser.RemoveTempShader();
                    }
                    else
                    {
                        Debug.LogWarning($"There is no {NodeKeys.Displacement} assigned to this shader. Make sure you have a Displacement Node in your graph");
                    }
                    Debug.Log($"Node id for displacement is {displacementEdgeInput.OutputSlot.Node.Id}");
                }
                AssetDatabase.Refresh();
            }
            else
            {
                Debug.LogWarning($"There is no {BlockFields.SurfaceDescription.BaseColor.name} assigned to this shader. Please make sure to add at lease a solid color to {BlockFields.SurfaceDescription.BaseColor.name}");
            }
            return;
        }

        private void BakeTargetSlot(ShaderGraphParser shaderGraphParser, Edge baseColorInputEdge, BlockFieldDescriptor targetBlockField)
        {
            Edge targetInputEdge = shaderGraphParser.GetEdgeByBlockField(targetBlockField, PutType.Input);
            if (targetInputEdge != null)
            {
                shaderGraphParser.RemoveEdge(baseColorInputEdge);
                Edge targetOutputToBaseColorInput = new Edge()
                {
                    OutputSlot = targetInputEdge.OutputSlot,
                    InputSlot = baseColorInputEdge.InputSlot
                };
                shaderGraphParser.CreateEdge(targetOutputToBaseColorInput);
                shaderGraphParser.SaveTempShader();
                shaderGraphParser.RenderToFile(Resolution, targetBlockField.name, useTempShader: true);
                shaderGraphParser.RemoveTempShader();
            }
            else
            {
                Debug.LogWarning($"There is no {targetBlockField.name} assigned to this shader.");
            }
        }

        private void SwapAndBake(string targetKey, string textureSuffix, int pass = -1)
        {
            var tempShaderPath = CreateSwappedShaderFile(BlockFields.SurfaceDescription.BaseColor.ToString(), targetKey);
            if (tempShaderPath != string.Empty)
            {
                AssetDatabase.Refresh();
                Material m_material = CreateMaterialFromShader(GetShaderLocalPath() + TempShaderName);
                string m_targetTextureName = GetShaderName() + textureSuffix + ".png";
                RenderTextureFromMaterial(m_targetTextureName, m_material, pass);
                CleanUpTempShader(tempShaderPath);
            }
            else
            {
                Debug.LogWarning($"There is no {textureSuffix} node connected to this shader or BaseColor is empty.\n Make sure both these slots have a connected node.");
            }
        }

        private static void CleanUpTempShader(string tempShaderPath)
        {
            if (File.Exists(tempShaderPath))
                AssetDatabase.DeleteAsset(tempShaderPath);
        }

        private string CreateSwappedShaderFile(string baseColorKey, string targetSlotKey)
        {
            var m_shaderGraphText = File.ReadAllText(GetShaderFullPath());
            string m_baseColorID = GetSlotID(m_shaderGraphText, JKeys.NodeNameKey, baseColorKey);
            string m_targetSlotID = GetSlotID(m_shaderGraphText, JKeys.NodeNameKey, targetSlotKey);

            var shaderCode = GenerateSwappedShader(m_shaderGraphText, m_baseColorID, m_targetSlotID);
            if (shaderCode != string.Empty)
            {
                // Create a new shaderGraph file with new data
                var m_filePath = Path.GetDirectoryName(GetShaderFullPath());
                string tempShaderPath = m_filePath + "/" + TempShaderName + ".shadergraph";
                File.WriteAllText(tempShaderPath, shaderCode);
                return tempShaderPath;
            }
            return string.Empty;
        }

        private static string GenerateSwappedShader(string m_shaderGraphText, string m_baseColorID, string m_normalSlotID)
        {
            // Swap target edge with color edge
            var targetSlotID = m_normalSlotID;
            string[] m_splittedJson = SplitedJson(m_shaderGraphText);
            string m_swappedShaderData = "";
            bool skipSwap = false;
            for (int i = 0; i < m_splittedJson.Length; i++)
            {
                JObject data = (JObject)JsonConvert.DeserializeObject(m_splittedJson[i]);
                if (data != null)
                {
                    data.TryGetValue(JKeys.EdgesKey, out JToken allEdgeData);
                    if (allEdgeData != null)
                    {
                        int targetEdgeIndex = -1;
                        int colorEdgeIndex = -1;
                        // We find all edge data and store target and base color edges
                        for (int j = 0; j < ((JArray)allEdgeData).Count; j++)
                        {
                            string edgeData = allEdgeData[j].ToString();
                            // Lets find the edge that has base color ID and normalTS
                            if (edgeData.Contains(m_baseColorID))
                            {
                                colorEdgeIndex = j;
                            }
                            if (edgeData.Contains(targetSlotID))
                            {
                                targetEdgeIndex = j;
                            }
                        }
                        // Find output id and its slot index
                        // JToken colorOutPutID = allEdgeData[colorEdgeIndex][SGKey.OutputEdgeKey][SGKey.NodeKey][SGKey.IDKey];
                        // JToken colorOutPutSlotIndex = allEdgeData[colorEdgeIndex][SGKey.OutputEdgeKey][SGKey.SlotIdKey];
                        // JToken targetOutPutID = allEdgeData[targetEdgeIndex][SGKey.OutputEdgeKey][SGKey.NodeKey][SGKey.IDKey];
                        // JToken targetOutPutSlotIndex = allEdgeData[targetEdgeIndex][SGKey.OutputEdgeKey][SGKey.SlotIdKey];


                        if (targetEdgeIndex == -1)
                        {
                            skipSwap = true;
                        }
                        else if (colorEdgeIndex == -1)
                        {
                            Debug.LogWarning($"No nodes is connected to BaseColor in this shader");
                            skipSwap = true;
                        }
                        else
                        {
                            // Swap target ID and Slot with Color ID and Slot
                            JToken tmp = allEdgeData[colorEdgeIndex][JKeys.OutputEdgeKey];
                            allEdgeData[colorEdgeIndex][JKeys.OutputEdgeKey] = allEdgeData[targetEdgeIndex][JKeys.OutputEdgeKey];
                            allEdgeData[targetEdgeIndex][JKeys.OutputEdgeKey] = tmp;
                        }
                    }
                    m_swappedShaderData += data.ToString() + "\n\n";
                }
            }
            return skipSwap ? string.Empty : m_swappedShaderData;
        }

        private void RenderTextureFromMaterial(string baseColorTextureName, Material m_material, int pass = -1)
        {
            RenderTexture m_renderTexture = RenderTexture.GetTemporary(Resolution.x, Resolution.y);
            Texture2D m_texture = new Texture2D(Resolution.x, Resolution.y, TextureFormat.RGBA32, false);
            Graphics.Blit(null, m_renderTexture, m_material, pass);
            //transfer image from rendertexture to texture
            RenderTexture.active = m_renderTexture;
            m_texture.ReadPixels(new Rect(Vector2.zero, Resolution), 0, 0);
            SaveTextureToFile(baseColorTextureName, m_texture);

            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(m_renderTexture);
            DestroyImmediate(m_texture);
            DestroyImmediate(m_material);
        }

        private Material CreateMaterialFromShader(string shaderName) => new(Shader.Find(shaderName));
        private string GetShaderFullPath() => AssetDatabase.GetAssetPath(_shader);
        private string GetShaderName() => _shader.name.Split('/').Last();
        private string GetShaderLocalPath() => _shader.name.Replace(_shader.name.Split('/').Last(), "");

        private string GetShaderFullName() => _shader.name;
        private static string[] SplitedJson(string shaderGraphText) => shaderGraphText.Split("\n\n");

        private static string GetSlotID(string shaderGraphText, string key, string value)
        {
            string[] splittedJson = SplitedJson(shaderGraphText);
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


        private void SaveTextureToFile(string fileName, Texture2D texture)
        {
            // save texture to file
            byte[] png = texture.EncodeToPNG();
            // Get current Asset path

            var FilePath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(_shader));

            File.WriteAllBytes(FilePath + "/" + fileName, png);
        }
        private void FixNormalImportSettings(string fullPath)
        {
            Texture2D newTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(fullPath);
            if (newTexture != null)
            {
                EditorUtility.FocusProjectWindow();
                // Selection.activeObject = newTexture;
                // EditorGUIUtility.PingObject(newTexture);
                TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(fullPath);
                importer.textureType = TextureImporterType.NormalMap;
                importer.textureFormat = TextureImporterFormat.DXT5;
                importer.SaveAndReimport();
            }
        }
    }
}
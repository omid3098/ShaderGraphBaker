using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.Rendering;

class ConvertObjectToTangentNormalMap : EditorWindow
{
    Texture2D objectSpaceNormalMap;
    Mesh mesh;

    public enum AxisSwizzleEnum
    {
        positiveX = 0,
        positiveY = 1,
        positiveZ = 2,
        negativeX = 3,
        negativeY = 4,
        negativeZ = 5
    }

    public AxisSwizzleEnum[] swizzle = new AxisSwizzleEnum[3] { AxisSwizzleEnum.positiveX, AxisSwizzleEnum.positiveY, AxisSwizzleEnum.positiveZ };

    Material bakeMaterial;
    RenderTexture rt, rt2;
    int colorMaskInt = 3;

    bool perPixelBitangent = false;

    public enum DilateEnum
    {
        none = 0,
        eight = 8,
        sixteen = 16,
        thirtytwo = 32,
        sixtyfour = 64
    }
    DilateEnum dilate = DilateEnum.thirtytwo;

    [MenuItem("Window/Convert Object Normal Map To Tangent Normal Map...")]
    static void Init()
    {
        var window = GetWindow<ConvertObjectToTangentNormalMap>("Convert Object Normal Map To Tangent Normal Map...");
        window.position = new Rect(0, 0, 400, 630);
        window.Show();

        // swizzle[0] = AxisSwizzleEnum.positiveX;
        // swizzle[1] = AxisSwizzleEnum.positiveY;
        // swizzle[2] = AxisSwizzleEnum.positiveZ;
    }

    Vector3 GetAxisVector(AxisSwizzleEnum axis)
    {
        int axisInt = (int)axis;
        int axisAxis = axisInt % 3;
        float axisSign = axisInt < 3 ? 1f : -1f;
        Vector3 dir;
        switch (axisAxis)
        {
            case 0:
                dir = new Vector3(1f, 0f, 0f);
                break;
            case 1:
                dir = new Vector3(0f, 1f, 0f);
                break;
            case 2:
            default:
                dir = new Vector3(0f, 0f, 1f);
                break;
        }

        return dir * axisSign;
    }

    void OnGUI()
    {
        EditorGUI.BeginChangeCheck();
        objectSpaceNormalMap = (Texture2D)EditorGUILayout.ObjectField("Object Space Normal Map:", objectSpaceNormalMap, typeof(Texture2D));
        mesh = (Mesh)EditorGUILayout.ObjectField("Target Mesh:", mesh, typeof(Mesh));

        AxisSwizzleEnum[] oldSwizzle = new AxisSwizzleEnum[3];
        for (int i = 0; i < 3; i++)
            oldSwizzle[i] = swizzle[i];

        swizzle[0] = (AxisSwizzleEnum)EditorGUILayout.EnumPopup("Object Space X", swizzle[0]);
        swizzle[1] = (AxisSwizzleEnum)EditorGUILayout.EnumPopup("Object Space Y", swizzle[1]);
        swizzle[2] = (AxisSwizzleEnum)EditorGUILayout.EnumPopup("Object Space Z", swizzle[2]);

        for (int i = 0; i < 3; i++)
        {
            if (swizzle[i] != oldSwizzle[i])
            {
                int axisAxis = (int)swizzle[i] % 3;

                for (int j = 0; j < 3; j++)
                {
                    if (i == j) continue;

                    if (axisAxis == ((int)swizzle[j] % 3))
                    {
                        int axisSign = (int)swizzle[j] < 3 ? 0 : 3;
                        swizzle[j] = (AxisSwizzleEnum)(((int)oldSwizzle[i] % 3) + axisSign);
                        break;
                    }
                }
            }
        }

        perPixelBitangent = EditorGUILayout.Toggle("Per Pixel Bitangent (SRP)", perPixelBitangent);

        dilate = (DilateEnum)EditorGUILayout.EnumPopup("Dilation", dilate);

        if (EditorGUI.EndChangeCheck())
        {
            if (bakeMaterial == null)
            {
                bakeMaterial = new Material(Shader.Find("Hidden/ObjectToTangentNormal"));
            }

            if ((mesh == null || objectSpaceNormalMap == null) && rt != null)
            {
                DestroyImmediate(rt);
                DestroyImmediate(rt2);
            }

            if (objectSpaceNormalMap != null && mesh != null)
            {
                if (rt != null && (rt.width != objectSpaceNormalMap.width || rt.height != objectSpaceNormalMap.height))
                {
                    DestroyImmediate(rt);
                    DestroyImmediate(rt2);
                }

                if (rt == null)
                {
                    rt = new RenderTexture(objectSpaceNormalMap.width, objectSpaceNormalMap.height, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
                    rt2 = new RenderTexture(objectSpaceNormalMap.width, objectSpaceNormalMap.height, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
                }

                Matrix4x4 objectSpaceCorrection = Matrix4x4.identity;
                objectSpaceCorrection.SetRow(0, GetAxisVector(swizzle[0]));
                objectSpaceCorrection.SetRow(1, GetAxisVector(swizzle[1]));
                objectSpaceCorrection.SetRow(2, GetAxisVector(swizzle[2]));

                RenderTexture.active = rt;
                GL.Clear(false, true, new Color(0.5f, 0.5f, 1f, 0f), 0f);

                bakeMaterial.SetTexture("_ObjectNormal", objectSpaceNormalMap);
                bakeMaterial.SetMatrix("_ObjectSpaceCorrection", objectSpaceCorrection);
                bakeMaterial.SetFloat("_PerPixelBitangent", perPixelBitangent ? 1f : 0f);
                bakeMaterial.SetPass(0);
                Graphics.DrawMeshNow(mesh, Matrix4x4.identity);
                RenderTexture.active = null;

                // dilate
                if ((int)dilate > 0)
                {
                    for (int i = 0; i < (int)dilate; i += 2)
                    {
                        Graphics.Blit(rt, rt2, bakeMaterial, 1);
                        Graphics.Blit(rt2, rt, bakeMaterial, 1);
                    }
                }
            }
        }

        EditorGUI.BeginDisabledGroup(objectSpaceNormalMap == null || mesh == null);
        bool doIt = GUILayout.Button("Save Tangent Space Normal Map");
        EditorGUI.EndDisabledGroup();

        if (rt != null)
        {
            if (doIt)
            {
                RenderTexture.active = rt;
                Texture2D tangentMap = new Texture2D(objectSpaceNormalMap.width, objectSpaceNormalMap.height, TextureFormat.RGBA32, false);
                tangentMap.ReadPixels(new Rect(0, 0, objectSpaceNormalMap.width, objectSpaceNormalMap.height), 0, 0, false);
                RenderTexture.active = null;

                string textureFullPath = AssetDatabase.GetAssetPath(objectSpaceNormalMap);
                string texturePath = Path.GetDirectoryName(textureFullPath);
                string textureFileName = Path.GetFileNameWithoutExtension(textureFullPath);

                string savePath = texturePath + "/" + textureFileName + "_TangentSpace.png";

                byte[] bytes = tangentMap.EncodeToPNG();
                File.WriteAllBytes(savePath, bytes);

                DestroyImmediate(tangentMap);

                Debug.Log("Saved Tangent Space texture to " + savePath);

                AssetDatabase.ImportAsset(savePath);
                Texture2D newTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(savePath);
                if (newTexture != null)
                {
                    EditorUtility.FocusProjectWindow();
                    Selection.activeObject = newTexture;
                    EditorGUIUtility.PingObject(newTexture);
                    TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(savePath);
                    importer.textureType = TextureImporterType.NormalMap;
                    importer.SaveAndReimport();
                    Debug.Log(newTexture);
                }
            }

            ColorWriteMask colorMask = ColorWriteMask.All;

            int previewSize = Mathf.Max(100, Mathf.Min((int)position.height - 230, (int)position.width));
            int left = (int)(position.width - (float)previewSize) / 2;

            string[] maskStrings = { "R", "G", "B", "All" };
            colorMaskInt = GUI.SelectionGrid(new Rect(left, 210, previewSize, 10), colorMaskInt, maskStrings, 4, EditorStyles.toolbarButton);

            switch (colorMaskInt)
            {
                case 0:
                    colorMask = ColorWriteMask.Red;
                    break;
                case 1:
                    colorMask = ColorWriteMask.Green;
                    break;
                case 2:
                    colorMask = ColorWriteMask.Blue;
                    break;
                case 3:
                    colorMask = ColorWriteMask.All;
                    break;
            }
            EditorGUI.DrawPreviewTexture(
                new Rect(left, 230, previewSize, previewSize),
                rt, null,
                ScaleMode.StretchToFill, 0, -1,
                colorMask
            );
        }
        else
        {
            string errorString = "";
            if (objectSpaceNormalMap == null)
                errorString = "Must assign a vald Object Space Normal Map";
            if (mesh == null)
                errorString += (objectSpaceNormalMap == null ? "\n" : "") + "Must assign a valid Target Mesh";

            int previewSize = Mathf.Max(100, Mathf.Min((int)position.height - 230, (int)position.width));
            int left = (int)(position.width - (float)previewSize) / 2;

            EditorGUI.HelpBox(new Rect(left, 220, previewSize, previewSize + 10), errorString, MessageType.Error);
        }
    }

    void OnDestory()
    {
        if (rt != null)
        {
            DestroyImmediate(rt);
            DestroyImmediate(rt2);
        }
        if (bakeMaterial != null)
            DestroyImmediate(bakeMaterial);
    }
}
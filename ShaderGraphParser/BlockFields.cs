namespace ShaderGraphBaker
{
    internal static class BlockFields
    {
        public struct VertexDescription
        {
            public static string name = "VertexDescription";
            public static BlockFieldDescriptor Position = new BlockFieldDescriptor(VertexDescription.name, "Position", "VERTEXDESCRIPTION_POSITION");
            public static BlockFieldDescriptor Normal = new BlockFieldDescriptor(VertexDescription.name, "Normal", "VERTEXDESCRIPTION_NORMAL");
            public static BlockFieldDescriptor Tangent = new BlockFieldDescriptor(VertexDescription.name, "Tangent", "VERTEXDESCRIPTION_TANGENT");
        }

        public struct SurfaceDescription
        {
            public static string name = "SurfaceDescription";
            public static BlockFieldDescriptor BaseColor = new BlockFieldDescriptor(SurfaceDescription.name, "BaseColor", "Base Color", "SURFACEDESCRIPTION_BASECOLOR");
            public static BlockFieldDescriptor NormalTS = new BlockFieldDescriptor(SurfaceDescription.name, "NormalTS", "Normal (Tangent Space)", "SURFACEDESCRIPTION_NORMALTS");
            public static BlockFieldDescriptor NormalOS = new BlockFieldDescriptor(SurfaceDescription.name, "NormalOS", "Normal (Object Space)", "SURFACEDESCRIPTION_NORMALOS");
            public static BlockFieldDescriptor NormalWS = new BlockFieldDescriptor(SurfaceDescription.name, "NormalWS", "Normal (World Space)", "SURFACEDESCRIPTION_NORMALWS");
            public static BlockFieldDescriptor Metallic = new BlockFieldDescriptor(SurfaceDescription.name, "Metallic", "SURFACEDESCRIPTION_METALLIC");
            public static BlockFieldDescriptor Specular = new BlockFieldDescriptor(SurfaceDescription.name, "Specular", "Specular Color", "SURFACEDESCRIPTION_SPECULAR");
            public static BlockFieldDescriptor Smoothness = new BlockFieldDescriptor(SurfaceDescription.name, "Smoothness", "SURFACEDESCRIPTION_SMOOTHNESS");
            public static BlockFieldDescriptor Occlusion = new BlockFieldDescriptor(SurfaceDescription.name, "Occlusion", "Ambient Occlusion", "SURFACEDESCRIPTION_OCCLUSION");
            public static BlockFieldDescriptor Emission = new BlockFieldDescriptor(SurfaceDescription.name, "Emission", "SURFACEDESCRIPTION_EMISSION");
            public static BlockFieldDescriptor Alpha = new BlockFieldDescriptor(SurfaceDescription.name, "Alpha", "SURFACEDESCRIPTION_ALPHA");
            public static BlockFieldDescriptor AlphaClipThreshold = new BlockFieldDescriptor(SurfaceDescription.name, "AlphaClipThreshold", "Alpha Clip Threshold", "SURFACEDESCRIPTION_ALPHACLIPTHRESHOLD");
            public static BlockFieldDescriptor CoatMask = new BlockFieldDescriptor(SurfaceDescription.name, "CoatMask", "Coat Mask", "SURFACEDESCRIPTION_COATMASK");
            public static BlockFieldDescriptor CoatSmoothness = new BlockFieldDescriptor(SurfaceDescription.name, "CoatSmoothness", "Coat Smoothness", "SURFACEDESCRIPTION_COATSMOOTHNESS");
        }
    }
}
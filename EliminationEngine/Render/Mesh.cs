using OpenTK.Mathematics;

namespace EliminationEngine.Render
{
    public class Mesh
    {
        public string Name = "Unknown";
        public float[]? Vertices { get; set; }
        public float[]? VerticesFull { get; set; }
        public float[]? VerticesFullFlipped { get; set; }
        public uint[]? Indices { get; set; }
        public uint[]? FlippedIndices { get; set; }
        public float[]? TexCoords { get; set; }
        public float[]? Normals { get; set; }
        public int _buffer = 0;
        public int _fullBuffer = 0;
        public int _vertexArr = 0;
        public int _vertexFullArr = 0;
        public int _indicesBuffer = 0;
        public int _texCoordBuffer = 0;
        public int _tex = 0;
        public int _normalTex = 0;
        public int _displacementTex = 0;
        public int _normalsBuffer = 0;
        public Shader? _shader;
        public byte[]? Image;
        public Dictionary<string, List<ImageData>> ImageSequence = new();
        public int Width = 0;
        public int Height = 0;
        public bool OverrideShader = false;
        public string ShaderVertPath = "Shaders/textured.vert";
        public string ShaderFragPath = "Shaders/textured.frag";
        public Vector3 Position = Vector3.Zero;
        public Vector3 OffsetPosition = Vector3.Zero;
        public string MaterialName = "placeholder";
        public float DisplaceValue = 0.03f;
    }
}

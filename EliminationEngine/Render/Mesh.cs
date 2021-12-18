using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliminationEngine.Render
{
    public class Mesh
    {
        public float[]? Vertices { get; set; }
        public uint[]? Indices { get; set; }
        public float[]? TexCoords { get; set; }
        protected float[]? Normals { get; set; }
        public int _buffer = 0;
        public int _vertexArr = 0;
        public int _indicesBuffer = 0;
        public int _texCoordBuffer = 0;
        public int _tex = 0;
        public Shader? _shader;
        public byte[] Image;
        public int Width = 0;
        public int Height = 0;
    }
}

using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliminationEngine.GameObjects
{
    public class Mesh : EntityComponent
    {
        public List<float> Vertices { get; set; } = new();
        public List<int> Indices { get; set; } = new();
        public List<float> TexCoords { get; set; } = new();
        protected List<float> Normals { get; set; } = new();
        private int _buffer = 0;
        private int _vertexArr = 0;
        private int _indicesBuffer = 0;
        private int _texCoordBuffer = 0;
        private int _tex = 0;
        private Shader _shader;

        public Mesh()
        {
            _buffer = GL.GenBuffer();
        }

        public void LoadMesh(string texPath)
        {
            var image = ImageLoader.LoadTexture(texPath);
            _vertexArr = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArr);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _buffer);
            GL.BufferData(BufferTarget.ArrayBuffer, Vertices.Count * sizeof(float), Vertices.ToArray(), BufferUsageHint.StaticDraw);

            _indicesBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _indicesBuffer);
            GL.BufferData(BufferTarget.ElementArrayBuffer, Indices.Count * sizeof(uint), Indices.ToArray(), BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            _tex = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, _tex);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, image.Pixels.ToArray());

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Nearest);
            _texCoordBuffer = GL.GenBuffer();
            
            GL.BindBuffer(BufferTarget.TextureBuffer, _texCoordBuffer);
            GL.BufferData(BufferTarget.TextureBuffer, TexCoords.Count * sizeof(float), TexCoords.ToArray(), BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(1);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _buffer);


            _shader = new Shader("Shaders/textured.vert", "Shaders/textured.frag");

        }

        public void DrawMesh()
        {
            _shader.Use();

            //GL.BindVertexArray(_vertexArr);
            //GL.DrawArrays(PrimitiveType.Triangles, 0, 3);

            GL.BindVertexArray(_vertexArr);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _indicesBuffer);
            GL.DrawElements(PrimitiveType.Triangles, Indices.Count, DrawElementsType.UnsignedInt, 0);
        }
    }
}

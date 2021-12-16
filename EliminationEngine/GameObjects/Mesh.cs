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
        protected List<float> Normals { get; set; } = new();
        private int _buffer = 0;
        private int _vertexArr = 0;
        private int _indicesBuffer = 0;
        private Shader _shader;

        public Mesh()
        {
            _buffer = GL.GenBuffer();
        }

        public void LoadMesh()
        {
            _vertexArr = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArr);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _buffer);
            GL.BufferData(BufferTarget.ArrayBuffer, Vertices.Count * sizeof(float), Vertices.ToArray(), BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _buffer);

            _indicesBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _indicesBuffer);
            GL.BufferData(BufferTarget.ElementArrayBuffer, Indices.Count * 3 * sizeof(uint), Indices.ToArray(), BufferUsageHint.StaticDraw);

            _shader = new Shader("Shaders/textured.vert", "Shaders/textured.frag");
        }

        public void DrawMesh()
        {
            _shader.Use();

            //GL.BindVertexArray(_vertexArr);
            //GL.DrawArrays(PrimitiveType.Triangles, 0, 3);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _indicesBuffer);
            GL.DrawArrays(PrimitiveType.Triangles, 0, Indices.Count);
        }
    }
}

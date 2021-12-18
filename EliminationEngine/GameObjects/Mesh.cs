using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliminationEngine.GameObjects
{
    public class Mesh : EntityComponent
    {
        public float[] Vertices { get; set; }
        public uint[] Indices { get; set; }
        public float[] TexCoords { get; set; }
        protected float[] Normals { get; set; }
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
            GL.BufferData(BufferTarget.ArrayBuffer, Vertices.Length * sizeof(float), Vertices, BufferUsageHint.StaticDraw);

            _indicesBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _indicesBuffer);
            GL.BufferData(BufferTarget.ElementArrayBuffer, Indices.Length * sizeof(uint), Indices, BufferUsageHint.StaticDraw);

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
            
            GL.BindBuffer(BufferTarget.ArrayBuffer, _texCoordBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, TexCoords.Length * sizeof(float), TexCoords.ToArray(), BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
            GL.EnableVertexAttribArray(1);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _buffer);


            _shader = new Shader("Shaders/textured.vert", "Shaders/textured.frag");

        }

        public void UpdatePos()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, _buffer);

            var vertsPos = Vertices.ToArray();
            for (var i = 0; i < vertsPos.Length; i += 3)
            {
                var vec = new Vector4(vertsPos[i], vertsPos[i + 1], vertsPos[i + 2], 1.0f);
                var trans = Matrix4.CreateTranslation(Owner.Position);
                var matrix = Matrix4.CreateFromQuaternion(Owner.Rotation);
                var scale = Matrix4.CreateScale(Owner.Scale);
                //var fovMatrix = Matrix4.CreateOrthographic(1, 1, 0.1f, 100f);
                var fovMatrix = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(80), 800.0f / 600.0f, 0.01f, 1000f);
                var lookAt = Matrix4.LookAt(new Vector3(0, 0, 4), new Vector3(0, 0, -1), new Vector3(0, 1, 0)); // TODO: Replace with camera position and rotation
                //var res = fovMatrix * lookAt * trans * matrix * vec;
                var res = vec * trans * matrix * scale * lookAt * (fovMatrix * 0.1f);
                //vec *= lookAt;
                //Console.WriteLine(vec.X + ":" + vec.Y +":" + vec.Z);
                vertsPos[i] = res.X;
                vertsPos[i + 1] = res.Y;
                vertsPos[i + 2] = res.Z;
                //Console.WriteLine(res.X + ":" + res.Y + ":" + res.Z);
            }

            GL.BufferData(BufferTarget.ArrayBuffer, vertsPos.Length * sizeof(float), vertsPos, BufferUsageHint.StaticDraw);
        }

        public void DrawMesh()
        {
            _shader.Use();

            GL.BindTexture(TextureTarget.Texture2D, _tex);
            GL.BindVertexArray(_vertexArr);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _indicesBuffer);
            GL.DrawElements(PrimitiveType.Triangles, Indices.Length, DrawElementsType.UnsignedInt, 0);
        }
    }
}

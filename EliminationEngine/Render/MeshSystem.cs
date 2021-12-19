using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EliminationEngine.GameObjects;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace EliminationEngine.Render
{
    public class MeshSystem : EntitySystem
    {
        public override void PostLoad()
        {
            base.OnLoad();

            var meshGroups = Engine.GetObjectsOfType<MeshGroupComponent>();
            foreach (var meshGroup in meshGroups)
            {
                foreach (var mesh in meshGroup.Meshes)
                {
                    mesh._buffer = GL.GenBuffer();
                    mesh._vertexArr = GL.GenVertexArray();
                    GL.BindVertexArray(mesh._vertexArr);

                    GL.BindBuffer(BufferTarget.ArrayBuffer, mesh._buffer);
                    GL.BufferData(BufferTarget.ArrayBuffer, mesh.Vertices.Length * sizeof(float), mesh.Vertices, BufferUsageHint.StaticDraw);

                    mesh._indicesBuffer = GL.GenBuffer();
                    GL.BindBuffer(BufferTarget.ElementArrayBuffer, mesh._indicesBuffer);
                    GL.BufferData(BufferTarget.ElementArrayBuffer, mesh.Indices.Length * sizeof(uint), mesh.Indices, BufferUsageHint.StaticDraw);

                    GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
                    GL.EnableVertexAttribArray(0);

                    mesh._tex = GL.GenTexture();
                    GL.BindTexture(TextureTarget.Texture2D, mesh._tex);
                    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, mesh.Width, mesh.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, mesh.Image);

                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Nearest);
                    mesh._texCoordBuffer = GL.GenBuffer();

                    GL.BindBuffer(BufferTarget.ArrayBuffer, mesh._texCoordBuffer);
                    GL.BufferData(BufferTarget.ArrayBuffer, mesh.TexCoords.Length * sizeof(float), mesh.TexCoords.ToArray(), BufferUsageHint.StaticDraw);

                    GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
                    GL.EnableVertexAttribArray(1);

                    GL.BindBuffer(BufferTarget.ArrayBuffer, mesh._buffer);

                    mesh._shader = new Shader("Shaders/textured.vert", "Shaders/textured.frag");
                }
            }
        }

        public override void OnDraw()
        {
            base.OnUpdate();

            var cameras = Engine.GetObjectsOfType<CameraComponent>().Select(e => { if (e.Active) return e; else return null; });
            if (cameras == null) return;
            var camera = cameras.ElementAt(0);
            if (camera == null) return;
            var cameraRot = camera.Owner.Rotation;
            var forward = camera.Owner.Forward();
            var up = Vector3.UnitY;

            var meshGroups = Engine.GetObjectsOfType<MeshGroupComponent>();
            foreach (var meshGroup in meshGroups)
            {
                foreach (var mesh in meshGroup.Meshes)
                {
                    GL.BindBuffer(BufferTarget.ArrayBuffer, mesh._buffer);
                    GL.BufferData(BufferTarget.ArrayBuffer, mesh.Vertices.Length * sizeof(float), mesh.Vertices, BufferUsageHint.StaticDraw);
                    
                    mesh._shader.Use();
                    var trans = Matrix4.CreateTranslation(meshGroup.Owner.Position);
                    var matrix = Matrix4.CreateFromQuaternion(meshGroup.Owner.Rotation);
                    var scale = Matrix4.CreateScale(meshGroup.Owner.Scale);
                    var fovMatrix = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(80), 800.0f / 600.0f, 0.01f, 1000f);
                    var lookAt = Matrix4.LookAt(camera.Owner.Position, forward, up);
                    mesh._shader.SetMatrix4("mvpMatrix", trans * matrix * scale * lookAt * (fovMatrix * 0.1f));

                    GL.BindTexture(TextureTarget.Texture2D, mesh._tex);
                    GL.BindVertexArray(mesh._vertexArr);
                    GL.BindBuffer(BufferTarget.ElementArrayBuffer, mesh._indicesBuffer);
                    GL.DrawElements(PrimitiveType.Triangles, mesh.Indices.Length, DrawElementsType.UnsignedInt, 0);
                }
            }
        }
    }
}

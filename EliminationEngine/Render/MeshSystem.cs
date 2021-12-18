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

                    var image = ImageLoader.LoadTexture("res/cube_test_texture.png");
                    mesh._tex = GL.GenTexture();
                    GL.BindTexture(TextureTarget.Texture2D, mesh._tex);
                    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, image.Pixels.ToArray());

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

            var meshGroups = Engine.GetObjectsOfType<MeshGroupComponent>();
            foreach (var meshGroup in meshGroups)
            {
                foreach (var mesh in meshGroup.Meshes)
                {
                    GL.BindBuffer(BufferTarget.ArrayBuffer, mesh._buffer);

                    var vertsPos = mesh.Vertices;
                    for (var i = 0; i < vertsPos.Length; i += 3)
                    {
                        var vec = new Vector4(vertsPos[i], vertsPos[i + 1], vertsPos[i + 2], 1.0f);
                        var trans = Matrix4.CreateTranslation(meshGroup.Owner.Position);
                        var matrix = Matrix4.CreateFromQuaternion(meshGroup.Owner.Rotation);
                        var scale = Matrix4.CreateScale(meshGroup.Owner.Scale);
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


                    mesh._shader.Use();

                    GL.BindTexture(TextureTarget.Texture2D, mesh._tex);
                    GL.BindVertexArray(mesh._vertexArr);
                    GL.BindBuffer(BufferTarget.ElementArrayBuffer, mesh._indicesBuffer);
                    GL.DrawElements(PrimitiveType.Triangles, mesh.Indices.Length, DrawElementsType.UnsignedInt, 0);
                }
            }
        }
    }
}

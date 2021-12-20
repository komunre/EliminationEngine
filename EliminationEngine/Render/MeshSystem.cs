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
        protected int lightsBuffer = 0;
        public float MaxLightDistance = 30;
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

                    mesh._normalsBuffer = GL.GenBuffer();
                    GL.BindBuffer(BufferTarget.ArrayBuffer, mesh._normalsBuffer);
                    GL.BufferData(BufferTarget.ArrayBuffer, mesh.Normals.Length * sizeof(float), mesh.Normals, BufferUsageHint.StaticDraw);

                    GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
                    GL.EnableVertexAttribArray(2);

                    mesh._shader = new Shader("Shaders/textured.vert", "Shaders/textured.frag");
                }
            }

            lightsBuffer = GL.GenBuffer();
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
            var up = camera.Owner.Up();

            var lightPos = new List<float>();
            var lightColors = new List<float>();

            var lights = Engine.GetObjectsOfType<LightComponent>();
            foreach (var light in lights)
            {
                var trans = light.Owner.Position;
                lightPos.AddRange(new float[] { trans.X, trans.Y, trans.Z });
                lightColors.AddRange(new float[] { light.Color.R, light.Color.G, light.Color.B, light.Color.A });
            }

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
                    var fovMatrix = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(80), (float)camera.Width / (float)camera.Height, camera.ClipNear, camera.ClipFar);
                    var lookAt = Matrix4.LookAt(camera.Owner.Position, forward, up);
                    mesh._shader.SetMatrix4("mvpMatrix", (matrix * trans * scale) * lookAt * (fovMatrix * 0.1f));
                    mesh._shader.SetVector3("viewPos", camera.Owner.Position);

                    var counter = 0;
                    for (var i = 0; i < lights.Length; i++)
                    {
                        var light = lights[i];
                        if ((light.Owner.Position - meshGroup.Owner.Position).Length > MaxLightDistance)
                        {
                            continue;
                        }
                        if (counter >= 20) break;
                        mesh._shader.SetVector3("pointLights[" + counter + "].pos", light.Owner.Position);
                        mesh._shader.SetFloat("pointLights[" + counter + "].constant", light.Constant);
                        mesh._shader.SetFloat("pointLights[" + counter + "].linear", light.Diffuse);
                        mesh._shader.SetVector3("pointLights[" + counter + "].diffuse", new Vector3(light.Color.R, light.Color.G, light.Color.B));
                        counter++;
                    }
                    mesh._shader.SetInt("lightsNum", counter);

                    GL.BindTexture(TextureTarget.Texture2D, mesh._tex);
                    GL.BindVertexArray(mesh._vertexArr);
                    GL.BindBuffer(BufferTarget.ElementArrayBuffer, mesh._indicesBuffer);
                    GL.DrawElements(PrimitiveType.Triangles, mesh.Indices.Length, DrawElementsType.UnsignedInt, 0);
                }
            }
        }
    }
}

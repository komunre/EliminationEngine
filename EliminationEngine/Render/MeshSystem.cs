using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EliminationEngine.GameObjects;
using EliminationEngine.Tools;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace EliminationEngine.Render
{
    public class MeshSystem : EntitySystem
    {
        protected int lightsBuffer = 0;

        public MeshSystem(Elimination e) : base(e)
        {

        }
        public override void OnLoad()
        {
            base.OnLoad();

            DefaultCameraBuffers.InitValues();
        }
        public override void PostLoad()
        {
            base.OnLoad();

            var meshGroups = Engine.GetObjectsOfType<MeshGroupComponent>();
            if (meshGroups == null) return;
            //foreach (var meshGroup in meshGroups)
            //{
            //    LoadMeshGroup(meshGroup);
            //}

            lightsBuffer = GL.GenBuffer();
        }

        public void LoadMeshGroup(MeshGroupComponent meshGroup)
        {
            foreach (var mesh in meshGroup.Meshes)
            {
                mesh._buffer = GL.GenBuffer();
                mesh._vertexArr = GL.GenVertexArray();
                mesh._indicesBuffer = GL.GenBuffer();
                mesh._texCoordBuffer = GL.GenBuffer();
                mesh._normalsBuffer = GL.GenBuffer();

                if (mesh._shader == null)
                {
                    if (mesh.OverrideShader)
                    {
                        mesh._shader = new Shader(mesh.ShaderVertPath, mesh.ShaderFragPath);
                    }
                    else
                    {
                        mesh._shader = new Shader("Shaders/textured.vert", "Shaders/textured.frag");
                    }
                }

                GL.BindVertexArray(mesh._vertexArr);

                if (mesh.Vertices == null) continue;
                GL.BindBuffer(BufferTarget.ArrayBuffer, mesh._buffer);
                GL.BufferData(BufferTarget.ArrayBuffer, mesh.Vertices.Length * sizeof(float), mesh.Vertices, BufferUsageHint.StaticDraw);

                
                if (mesh.Indices == null) continue;
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

                if (mesh.TexCoords == null) continue;
                GL.BindBuffer(BufferTarget.ArrayBuffer, mesh._texCoordBuffer);
                GL.BufferData(BufferTarget.ArrayBuffer, mesh.TexCoords.Length * sizeof(float), mesh.TexCoords.ToArray(), BufferUsageHint.StaticDraw);

                GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
                GL.EnableVertexAttribArray(1);

                GL.BindBuffer(BufferTarget.ArrayBuffer, mesh._buffer);

                if (mesh.Normals == null) continue;
                GL.BindBuffer(BufferTarget.ArrayBuffer, mesh._normalsBuffer);
                GL.BufferData(BufferTarget.ArrayBuffer, mesh.Normals.Length * sizeof(float), mesh.Normals, BufferUsageHint.StaticDraw);

                GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
                GL.EnableVertexAttribArray(2);
            }
        }

        public override void OnDraw()
        {
            base.OnUpdate();

            var cameras = Engine.GetObjectsOfType<CameraComponent>().Select(x => { if (x.Active) { return x; } return null; });
            var activeCamera = cameras.First();

            foreach (var camera in Engine.GetObjectsOfType<CameraComponent>())
            {
                if (!camera.Active && !camera.RenderToTexture) continue;

                var cameraRot = camera.Owner.GlobalRotation;
                var forward = camera.Owner.Forward();
                var up = camera.Owner.Up();

                var lights = Engine.GetObjectsOfType<LightComponent>();

                var meshGroups = Engine.GetObjectsOfType<MeshGroupComponent>();
                if (meshGroups == null) return;
                foreach (var meshGroup in meshGroups)
                {
                    foreach (var mesh in meshGroup.Meshes)
                    {
                        if (mesh.Vertices == null) continue;
                        if (mesh.Indices == null) continue;
                        if (mesh._shader == null) continue;
                        GL.BindBuffer(BufferTarget.ArrayBuffer, mesh._buffer);
                        GL.BufferData(BufferTarget.ArrayBuffer, mesh.Vertices.Length * sizeof(float), mesh.Vertices, BufferUsageHint.StaticDraw);

                        var cameraPos = camera.Owner.GlobalPosition;

                        mesh._shader.Use();
                        var trans = Matrix4.CreateTranslation(meshGroup.Owner.GlobalPosition);
                        var matrix = Matrix4.CreateFromQuaternion(meshGroup.Owner.GlobalRotation);
                        var scale = Matrix4.CreateScale(meshGroup.Owner.GlobalScale);
                        var fovMatrix = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(camera.FoV), (float)camera.Width / (float)camera.Height, camera.ClipNear, camera.ClipFar);
                        var lookAt = Matrix4.LookAt(cameraPos, forward, up);
                        mesh._shader.SetMatrix4("mvpMatrix", (matrix * trans * scale) * lookAt * (fovMatrix));
                        mesh._shader.SetMatrix4("modelMatrix", matrix * trans * scale);
                        mesh._shader.SetVector3("viewPos", cameraPos);
                        mesh._shader.SetVector3("worldPos", meshGroup.Owner.GlobalPosition);

                        var counter = 0;
                        if (lights != null && lights.Length > 0)
                        {
                            for (var i = 0; i < lights.Length; i++)
                            {
                                var light = lights[i];
                                if ((light.Owner.GlobalPosition - meshGroup.Owner.GlobalPosition).Length > light.MaxAffectDstance)
                                {
                                    continue;
                                }
                                if (counter >= 20) break;
                                mesh._shader.SetVector3("pointLights[" + counter + "].pos", light.Owner.GlobalPosition);
                                mesh._shader.SetFloat("pointLights[" + counter + "].constant", light.Constant);
                                mesh._shader.SetFloat("pointLights[" + counter + "].linear", light.Diffuse);
                                mesh._shader.SetVector3("pointLights[" + counter + "].diffuse", new Vector3(light.Color.R, light.Color.G, light.Color.B));
                                counter++;
                            }
                        }
                        mesh._shader.SetInt("lightsNum", counter);

                        var col = meshGroup.Owner.BaseColor;
                        mesh._shader.SetVector3("addColor", new Vector3(col.R, col.G, col.B));

                        GL.Enable(EnableCap.DepthTest);
                        GL.BindTexture(TextureTarget.Texture2D, mesh._tex);
                        GL.BindVertexArray(mesh._vertexArr);
                        GL.BindFramebuffer(FramebufferTarget.Framebuffer, camera.GetFrameBuffer());
                        GL.BindBuffer(BufferTarget.ElementArrayBuffer, mesh._indicesBuffer);
                        GL.DrawElements(PrimitiveType.Triangles, mesh.Indices.Length, DrawElementsType.UnsignedInt, 0);
                    }
                }

                if (camera.Active)
                {
                    // Direct Copy
                    //GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, camera.GetFrameBuffer());
                    //GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);
                    //GL.BlitFramebuffer(0, 0, camera.Width, camera.Height, 0, 0, activeCamera.Width, activeCamera.Height, ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Nearest);

                    // Object draw
                    GL.Disable(EnableCap.DepthTest);
                    GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
                    GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, camera.GetRBO());
                    camera.CameraShader.Use();
                    camera.CameraShader.SetFloat("time", (float)Engine.Elapsed.TotalMilliseconds);
                    GL.BindBuffer(BufferTarget.ArrayBuffer, DefaultCameraBuffers.VertexBuff);
                    GL.BindTexture(TextureTarget.Texture2D, camera.GetTexture());
                    GL.BindVertexArray(DefaultCameraBuffers.VertexArray);
                    GL.BindBuffer(BufferTarget.ElementArrayBuffer, DefaultCameraBuffers.IndicesBuff);
                    GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);
                    GL.Enable(EnableCap.DepthTest);
                }
            }
        }
    }
}

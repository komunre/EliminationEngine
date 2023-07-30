using BepuPhysics.Collidables;
using EliminationEngine.GameObjects;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using SixLabors.ImageSharp.PixelFormats;
using System.Runtime.CompilerServices;

namespace EliminationEngine.Render
{
    public class MeshSystem : EntitySystem
    {
        public static CameraComponent? ActiveCamera;
        public static Shader TexturedShader = new Shader("Shaders/textured.vert", "Shaders/textured.frag", "Shaders/textured.geom");
        public static Shader? CurrentShader;

        protected int lightsBuffer = 0;

        public static bool ForceWiremode = false;

        public static int DiffusePlaceholder = 0;
        public static int NormalPlaceholder = 0;
        public static int DisplacementPlaceholer = 0;

        public static Shader DefaultTexturedShader = new Shader("Shaders/textured.vert", "Shaders/textured.frag", "Shaders/textured.geom");

        public static bool LightsAdded = true;

        public MeshSystem(Elimination e) : base(e)
        {
            RunsWhilePaused = true;
        }

        public override void OnLoad()
        {
            base.OnLoad();

            DiffusePlaceholder = ImageLoader.CreateTextureFromImage(SixLabors.ImageSharp.Image.Load<Rgba32>("res/UV-placeholder.png"), ImageFilter.Nearest, false, false).TextureID;
            NormalPlaceholder = ImageLoader.CreateTextureFromImage(SixLabors.ImageSharp.Image.Load<Rgba32>("res/normaldef.png"), ImageFilter.Linear, false, false).TextureID;
            DisplacementPlaceholer = ImageLoader.CreateTextureFromImage(SixLabors.ImageSharp.Image.Load<Rgba32>("res/displacedef.png"), ImageFilter.Linear, false, false).TextureID;
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

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static int GenerateTexture(Mesh mesh, int textureReference = -1)
        {
            if (mesh._tex != 0) return mesh._tex;
            if (textureReference != -1)
            {
                mesh._tex = textureReference;
            }
            else if (mesh.Image == null)
            {
                mesh._tex = DiffusePlaceholder;
            }
            else
            {
                mesh._tex = GL.GenTexture();
                GL.BindTexture(TextureTarget.Texture2D, mesh._tex);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, mesh.Width, mesh.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, mesh.Image);

                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Nearest);
            }
            return mesh._tex;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
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
                    mesh._shader = TexturedShader;
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

                GenerateTexture(mesh);

                if (mesh._normalTex == 0)
                {
                    mesh._normalTex = NormalPlaceholder;
                }
                if (mesh._displacementTex == 0)
                {
                    mesh._displacementTex = DisplacementPlaceholer;
                }

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

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void CreateShaderData(Shader shader, Vector3 position, Vector3 positionOffset, Quaternion rotation, Vector3 Scale, CameraComponent camera, UpdateInfo updateInfo)
        {
            if (CurrentShader != shader)
            {
                shader.Use();
            }
            Matrix4 trans;
            Matrix4 matrix;
            Matrix4 scale;
            if (updateInfo.RequiresUpdate)
            {
                trans = Matrix4.CreateTranslation(position + positionOffset);
                matrix = Matrix4.CreateFromQuaternion(rotation);
                scale = Matrix4.CreateScale(Scale);
            }
            else
            {
                trans = updateInfo.TransformationMatrix;
                matrix = updateInfo.RotationMatrix;
                scale = updateInfo.ScaleMatrix;
            }
            Matrix4 fovMatrix;
            if (camera.Perspective)
            {
                fovMatrix = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(camera.FoV), (float)camera.Width / (float)camera.Height, camera.ClipNear, camera.ClipFar);
            }
            else
            {
                fovMatrix = Matrix4.CreateOrthographic(camera.OrthoWidth, camera.OrthoHeight, camera.ClipNear, camera.ClipFar);
            }
            var cameraPos = camera.Owner.GlobalPosition;
            var directions = camera.Owner.GetDirections();
            var forward = cameraPos + directions[0];
            var up = directions[2];
            var lookAt = Matrix4.LookAt(cameraPos, forward, up);
            shader.SetMatrix4("viewMatrix", lookAt);
            shader.SetMatrix4("projectionMatrix", fovMatrix);
            shader.SetMatrix4("mvpMatrix", (scale * matrix * trans) * lookAt * fovMatrix);
            if (updateInfo.RequiresUpdate) shader.SetMatrix4("modelMatrix", matrix * trans * scale);
            shader.SetVector3("viewPos", cameraPos);
            //shader.SetVector3("cameraForward", camera.Owner.GetDirections()[0]);
            shader.SetVector3("worldPos", position);
            //shader.SetFloat("time", 1.0f / ((float)(Elimination.GlobalEngine.Elapsed.Ticks % 150)));
            updateInfo.RequiresUpdate = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void SendLightData(Mesh mesh)
        {
            Logger.Info(Loc.Get("ADDING_LIGHT_INFORMATION"));

            var lights = Elimination.GlobalEngine.GetObjectsOfType<LightComponent>();

            if (lights == null) return;

            var counter = 0;
            for (var i = 0; i < lights.Length; i++)
            {
                var light = lights[i];
                /*if (light.IgnoredLayers.Any(x => meshGroup.Owner.Layers.Contains(x)))
                {
                    continue;
                }*/
                if (counter >= 20) break;
                mesh._shader.SetVector3("pointLights[" + counter + "].pos", light.Owner.GlobalPosition);
                mesh._shader.SetFloat("pointLights[" + counter + "].constant", light.Constant);
                mesh._shader.SetFloat("pointLights[" + counter + "].linear", light.Diffuse);
                mesh._shader.SetFloat("pointLights[" + counter + "].quadratic", light.Qudratic);
                mesh._shader.SetVector3("pointLights[" + counter + "].diffuse", new Vector3(light.Color.R, light.Color.G, light.Color.B));
                mesh._shader.SetInt("pointLights[" + counter + "].directional", light.Directional ? 1 : 0);
                mesh._shader.SetVector3("pointLights[" + counter + "].direction", light.Owner.GetDirections()[0]);
                mesh._shader.SetFloat("pointLights[" + counter + "].cutoff", (float)MathHelper.Cos(MathHelper.DegreesToRadians(light.DirectionalCutoffAngle)));
                mesh._shader.SetFloat("pointLights[" + counter + "].distanceFactor", light.DistanceFactor);
                mesh._shader.SetFloat("pointLights[" + counter + "].maxBrightness", light.MaxBrightness);
                mesh._shader.SetFloat("pointLights[" + counter + "].constantDiffuse", light.ConstantDiffuse);
                counter++;
            }
            mesh._shader.SetInt("lightsNum", counter);

            Logger.Info(Loc.Get("ADDED_LIGHTS_COUNT") + counter);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void RenderEverything(CameraComponent camera)
        {
            GL.Viewport(0, 0, camera.Width, camera.Height);

            var cameraPos = camera.Owner.Position;
            var directions = camera.Owner.GetDirections();
            var forward = directions[0] + camera.Owner.Position;
            var up = directions[2];

            var meshGroups = Elimination.GlobalEngine.GetObjectsOfType<MeshGroupComponent>();
            if (meshGroups == null) return;
            foreach (var meshGroup in meshGroups)
            {
                var ownerObject = meshGroup.Owner;
                if (ownerObject.ObjectUpdateInfo.LastPosition != ownerObject.Position || ownerObject.ObjectUpdateInfo.LastRotation != ownerObject.Rotation || ownerObject.ObjectUpdateInfo.LastScale != ownerObject.Scale)
                {
                    ownerObject.ObjectUpdateInfo.RequiresUpdate = true;
                }
                foreach (var mesh in meshGroup.Meshes)
                {
                    if (!mesh.IsVisible) continue;
                    if (mesh.Vertices == null) continue;
                    if (mesh.Indices == null) continue;
                    if (mesh._shader == null) continue;

                    CreateShaderData(mesh._shader, meshGroup.Owner.GlobalPosition, mesh.OffsetPosition, meshGroup.Owner.GlobalRotation, meshGroup.Owner.GlobalScale, camera, meshGroup.Owner.ObjectUpdateInfo);

                    if (LightsAdded) SendLightData(mesh);

                    mesh._shader.SetFloat("heightScale", mesh.DisplaceValue);
                    mesh._shader.SetFloat("zeroHeight", mesh.DisplaceZeroHeight);

                    var col = meshGroup.Owner.BaseColor;
                    mesh._shader.SetVector3("addColor", new Vector3(col.R, col.G, col.B));

                    if (ForceWiremode)
                    {
                        GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
                    }

                    // camera framebuffer!
                    GL.BindFramebuffer(FramebufferTarget.Framebuffer, camera.GetFrameBuffer());
                    GL.Enable(EnableCap.DepthTest);

                    // geometry
                    GL.BindVertexArray(mesh._vertexArr);
                    GL.BindBuffer(BufferTarget.ElementArrayBuffer, mesh._indicesBuffer);

                    // textures
                    GL.ActiveTexture(TextureUnit.Texture0);
                    GL.BindTexture(TextureTarget.Texture2D, mesh._tex);
                    GL.ActiveTexture(TextureUnit.Texture1);
                    GL.BindTexture(TextureTarget.Texture2D, mesh._normalTex);
                    GL.ActiveTexture(TextureUnit.Texture2);
                    GL.BindTexture(TextureTarget.Texture2D, mesh._displacementTex);

                    // render
                    GL.DrawElements(PrimitiveType.Triangles, mesh.Indices.Length, DrawElementsType.UnsignedInt, 0);
                   
                    // reset
                    GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
                    GL.ActiveTexture(TextureUnit.Texture0);
                }
            }

            if (camera.Active)
            {
                ActiveCamera = camera;

                // Direct Copy
                //GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, camera.GetFrameBuffer());
                //GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);
                //GL.BlitFramebuffer(0, 0, camera.Width, camera.Height, 0, 0, activeCamera.Width, activeCamera.Height, ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Nearest);

                // Object draw
                RenderToScreen(camera);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void RenderToScreen(CameraComponent camera)
        {
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.Disable(EnableCap.DepthTest);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, camera.GetRBO());
            camera.CameraShader.Use();
            camera.CameraShader.SetFloat("time", 1.0f / ((float)(Elimination.GlobalEngine.Elapsed.Ticks % 150)));
            GL.BindBuffer(BufferTarget.ArrayBuffer, EngineStatics.CameraStatics.VertexBuffer);
            GL.BindTexture(TextureTarget.Texture2D, camera.GetTexture());
            GL.BindVertexArray(EngineStatics.CameraStatics.VertexArray);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, EngineStatics.CameraStatics.IndicesBuffer);
            GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);
            GL.Enable(EnableCap.DepthTest);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public override void OnDraw()
        {
            base.OnUpdate();

            foreach (var camera in Engine.GetObjectsOfType<CameraComponent>())
            {
                if (!camera.RenderToTexture) continue;

                //new Thread(new ThreadStart(() =>
                //{
                    RenderEverything(camera);
                //}));
            }

            foreach (var camera in Engine.GetObjectsOfType<CameraComponent>())
            {
                if (!camera.Active) continue;

                RenderEverything(camera);
            }

            LightsAdded = false;
        }


        /// <summary>
        /// Modifies texture by rendering itself using override shaders.
        /// </summary>
        /// <param name="texture">Texture data itself.</param>
        /// <param name="over">Override shaders. Recommended to use camera.vert as vertex shader.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public void ModifyTexture(TextureData texture, Shader over)
        {
            if (texture.ImageData == null) return; // due to need in knowing width and height.
            if (texture.FBO == 0)
            {
                texture.GenerateFBO();
            }
            over.Use();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, texture.FBO);
            GL.Viewport(0, 0, texture.ImageData.Width, texture.ImageData.Height);

            // render
            GL.Disable(EnableCap.DepthTest);
            GL.BindVertexArray(EngineStatics.CameraStatics.VertexArray);
            GL.BindBuffer(BufferTarget.ArrayBuffer, EngineStatics.CameraStatics.VertexBuffer);
            GL.BindTexture(TextureTarget.Texture2D, texture.TextureID);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, EngineStatics.CameraStatics.IndicesBuffer);
            GL.DrawElements(BeginMode.Triangles, 6, DrawElementsType.UnsignedInt, 0);

            // cleanup
            GL.Enable(EnableCap.DepthTest);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public override void OnObjectAdded(GameObject obj)
        {
            if (obj.HasComponent<LightComponent>()) LightsAdded = true;
        }
    }
}

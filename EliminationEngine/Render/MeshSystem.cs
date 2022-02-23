using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using EliminationEngine.GameObjects;
using EliminationEngine.Tools;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using SharpGLTF.Schema2;
using SharpGLTF.Transforms;
using PrimitiveType = OpenTK.Graphics.OpenGL4.PrimitiveType;
using TextureWrapMode = OpenTK.Graphics.OpenGL4.TextureWrapMode;

namespace EliminationEngine.Render
{
    public class MeshSystem : EntitySystem
    {
        protected int lightsBuffer = 0;

        public MeshSystem(Elimination e) : base(e)
        {

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
                    mesh._shader = new Shader("Shaders/textured.vert", "Shaders/textured.frag");
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

                if (mesh.Weights == null) continue;
                mesh._weightsBuffer = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.ArrayBuffer, mesh._weightsBuffer);
                GL.BufferData(BufferTarget.ArrayBuffer, mesh.Weights.Length * sizeof(float), mesh.Weights, BufferUsageHint.StaticDraw);

                GL.VertexAttribPointer(3, 4, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
                GL.EnableVertexAttribArray(3);

                if (mesh.Joints == null) continue;
                mesh._jointsBuffer = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.ArrayBuffer, mesh._jointsBuffer);
                GL.BufferData(BufferTarget.ArrayBuffer, mesh.Joints.Length * sizeof(float), mesh.Joints, BufferUsageHint.StaticDraw);

                GL.VertexAttribPointer(4, 4, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
                GL.EnableVertexAttribArray(4);
            }
        }

        private void ChangeMeshChildrenAnim(ref Mesh mesh, AnimationChannel[] channels, float time)
        {
            var decomposed = mesh.AssignedNode?.LocalTransform.GetDecomposed();
            var totalMat = Matrix4.Identity;
            foreach (var channel in channels)
            {
                var translationAnim = channel.GetTranslationSampler()?.CreateCurveSampler()?.GetPoint(time) ?? decomposed?.Translation;
                var scaleAnim = channel.GetScaleSampler()?.CreateCurveSampler()?.GetPoint(time) ?? decomposed?.Scale;
                var rotationAnim = channel.GetRotationSampler()?.CreateCurveSampler()?.GetPoint(time) ?? decomposed?.Rotation;
                var affine = new AffineTransform(scaleAnim, rotationAnim, translationAnim);
                var mat = EliminationMathHelper.NumericsMatrixToMatrix(affine.Matrix);

                totalMat += mat;
            }

            mesh.AnimationMatrix = totalMat;

            /*foreach (var child in mesh.TechnicalChildren)
            {
                if (child.IsJoint)
                {
                    child.AnimationMatrix = totalMat;
                }
            }*/
        }

        private void SetAllAnimMatricesToIdent(Mesh mesh)
        {
            mesh.AnimationMatrix = Matrix4.Identity;
            foreach (var child in mesh.TechnicalChildren)
            {
                SetAllAnimMatricesToIdent(child);
            }
        }

        private Matrix4? GetAnimMatrixByIndex(int index, Mesh current)
        {
            if (current.AssignedNode.LogicalIndex.Equals(index))
            {
                return current.AnimationMatrix;
            }
            foreach (var child in current.TechnicalChildren)
            {
                var result = GetAnimMatrixByIndex(index, child);
                if (result != null)
                {
                    return result;
                }
            }
            return null;
        }

        private void SetShaderJoints(ref Mesh root, ref int counter, Matrix4x4[] accessor, Mesh current, OpenTK.Mathematics.Matrix4 globalPos)
        {
            for (var i = 0; i < root.AssignedNode.LogicalParent.LogicalSkins[0].JointsCount; i++) {
                var (bone, inverseBindMatrix) = root.AssignedNode.LogicalParent.LogicalSkins[0].GetJoint(i);
                var animMatrix = GetAnimMatrixByIndex(bone.LogicalIndex, current);
                if (animMatrix == null) continue;
                root._shader?.SetMatrix4("in_jointsTransform[" + i + "]", Matrix4.Invert(EliminationMathHelper.NumericsMatrixToMatrix(root.AssignedNode.WorldMatrix)) * (EliminationMathHelper.NumericsMatrixToMatrix(bone.WorldMatrix) * (Matrix4)animMatrix) * EliminationMathHelper.NumericsMatrixToMatrix(inverseBindMatrix));
            }
        }

        public override void OnDraw()
        {
            base.OnUpdate();

            var cameras = Engine.GetObjectsOfType<CameraComponent>()?.Select(e => { if (e.Active) return e; else return null; });
            if (cameras == null) return;
            var camera = cameras.ElementAt(0);
            if (camera == null) return;
            var cameraRot = camera.Owner.GlobalRotation;
            var forward = camera.Owner.Forward();
            var up = camera.Owner.Up();

            var lights = Engine.GetObjectsOfType<LightComponent>();

            var meshGroups = Engine.GetObjectsOfType<MeshGroupComponent>();
            if (meshGroups == null) return;
            foreach (var meshGroup in meshGroups)
            {
                meshGroup.Animator.Time += Engine.DeltaTime;
                var time = meshGroup.Animator.Time;
                var loop = meshGroup.Animator.Loop;
                foreach (var mesh in meshGroup.Meshes)
                {
                    if (mesh.Vertices == null) continue;
                    if (mesh.Indices == null) continue;
                    if (mesh._shader == null) continue;
                    GL.BindBuffer(BufferTarget.ArrayBuffer, mesh._buffer);
                    GL.BufferData(BufferTarget.ArrayBuffer, mesh.Vertices.Length * sizeof(float), mesh.Vertices, BufferUsageHint.StaticDraw);

                    var cameraPos = camera.Owner.GlobalPosition;

                    mesh._shader.Use();
                    // Basic
                    var trans = Matrix4.CreateTranslation(meshGroup.Owner.GlobalPosition);
                    var matrix = Matrix4.CreateFromQuaternion(meshGroup.Owner.GlobalRotation);
                    var scale = Matrix4.CreateScale(meshGroup.Owner.GlobalScale);
                    var fovMatrix = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(camera.FoV), (float)camera.Width / (float)camera.Height, camera.ClipNear, camera.ClipFar);
                    var lookAt = Matrix4.LookAt(cameraPos, forward, up);

                    // Animation
                    var meshVar = mesh;
                    if (mesh.IsJoint || mesh.IsSkeletonChild)
                    {
                        if (mesh.IsJoint && mesh.Animations != null)
                        {
                            foreach (var anim in mesh.Animations)
                            {
                                if (anim.Name != meshGroup.Animator.ActiveAnimation) continue;
                                if (anim.Duration < time)
                                {
                                    meshGroup.Animator.Time = 0;
                                }

                                var relatedChannels = anim.Channels.Where(item => item.TargetNode.LogicalIndex == mesh.AssignedNode.LogicalIndex).ToArray();
                                ChangeMeshChildrenAnim(ref meshVar, relatedChannels, time);
                            }
                        }

                        if (mesh.IsSkeletonChild)
                        {
                            var accessor = mesh.AssignedNode.LogicalParent.LogicalSkins[0].GetInverseBindMatricesAccessor().AsMatrix4x4Array();
                            var jcounter = 0;
                            if (mesh.Parent != null)
                            {
                                SetShaderJoints(ref meshVar, ref jcounter, accessor.ToArray(), mesh.Parent, scale * matrix * trans);
                            }
                        }
                    }

                    // Apply
                    mesh._shader.SetMatrix4("mvpMatrix", (scale * matrix * trans) * lookAt * (fovMatrix));
                    mesh._shader.SetMatrix4("modelMatrix", scale * matrix * trans);
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
                            mesh._shader.SetVector3("pointLights[" + counter + "].diffuse", new OpenTK.Mathematics.Vector3(light.Color.R, light.Color.G, light.Color.B));
                            counter++;
                        }
                    }
                    mesh._shader.SetInt("lightsNum", counter);

                    var col = meshGroup.Owner.BaseColor;
                    mesh._shader.SetVector4("addColor", new OpenTK.Mathematics.Vector4(col.R, col.G, col.B, col.A));

                    GL.BindTexture(TextureTarget.Texture2D, mesh._tex);
                    GL.BindVertexArray(mesh._vertexArr);
                    GL.BindBuffer(BufferTarget.ElementArrayBuffer, mesh._indicesBuffer);
                    GL.DrawElements(PrimitiveType.Triangles, mesh.Indices.Length, DrawElementsType.UnsignedInt, 0);
                }
            }
        }
    }
}


using SharpGLTF.Schema2;
using SharpGLTF.Animations;
using SharpGLTF.Memory;
using EliminationEngine.GameObjects;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using OpenTK.Mathematics;

namespace EliminationEngine
{
    public static class ModelHelper
    {
        public class MeshData
        {
            public List<float> Vertices = new();
            public List<uint> Indices = new();
            public List<float> UVs = new();
        }

        //TODO: something other than a ref uint?
        public static MeshData PostParseMesh(List<ModelParser.GLTFData.MeshData> meshes, ref uint indexOffset, MeshData? data = null)
        {
            data ??= new MeshData();
            foreach (var meshData in meshes)
            {
                foreach (var prim in meshData.Primitives)
                {
                    data.Vertices.AddRange(prim.Vertices.SelectMany(e => new []{e.X, e.Y, e.Z}));
                    data.UVs.AddRange(prim.UVs.SelectMany(e => new []{e.X, e.Y}));

                    uint indexCopy = indexOffset;
                    data.Indices.AddRange(prim.Indices.Select(e => e + indexCopy));
                    indexOffset += (uint)prim.Vertices.Count;
                }
                
                PostParseMesh(meshData.Children, ref indexOffset, data);
            }

            return data;
        }

        public static void PostParseMeshes(ref MeshGroupComponent meshGroup, List<ModelParser.GLTFData.MeshData> meshes, EliminationEngine.Render.Mesh parent = null)
        {
            foreach (var mesh in meshes)
            {
                var renderMesh = new Render.Mesh();
                var vertsData = new List<float>();
                var uvData = new List<float>();
                var indices = new List<uint>();
                var normals = new List<float>();
                var weights = new List<float>();
                var joints = new List<float>();
                foreach (var primitive in mesh.Primitives) {
                    vertsData.AddRange(primitive.Vertices.SelectMany(e => new[] { e.X, e.Y, e.Z }));
                    uvData.AddRange(primitive.UVs.SelectMany(e => new[] { e.X, e.Y }));
                    indices.AddRange(primitive.Indices.Select(e => e));
                    normals.AddRange(primitive.Normals.SelectMany(e => new[] { e.X, e.Y, e.Z }));
                    if (primitive.Joints != null && primitive.Weights != null)
                    {
                        joints.AddRange(primitive.Joints?.SelectMany(e => new[] { e.X, e.Y, e.Z, e.W }));
                        weights.AddRange(primitive.Weights?.SelectMany(e => new[] { e.X, e.Y, e.Z, e.W }));
                    }
                }

                if (mesh.Mat != null && mesh.Mat.Channels.ElementAt(0).Texture != null)
                {
                    var image = (Image<Rgba32>)SixLabors.ImageSharp.Image.Load(mesh.Mat.Channels.ElementAt(0).Texture.PrimaryImage.Content.Open());
                    renderMesh.Width = image.Width;
                    renderMesh.Height = image.Height;
                    renderMesh.Image = ImageLoader.LoadTextureFromImage(image).Pixels.ToArray();
                }
                else
                {
                    var image = new Image<Rgba32>(32, 32);
                    for (var i = 0; i < image.Height; i++)
                    {
                        for (var j = 0; j < image.Width; j++)
                        {
                            image[j, i] = new Rgba32(251, 72, 196);
                        }
                    }
                    renderMesh.Width = image.Width;
                    renderMesh.Height = image.Height;
                    renderMesh.Image = ImageLoader.LoadTextureFromImage(image).Pixels.ToArray();
                }
                renderMesh.Vertices = vertsData.ToArray();
                renderMesh.TexCoords = uvData.ToArray();
                renderMesh.Indices = indices.ToArray();
                renderMesh.Normals = normals.ToArray();
                renderMesh.AssignedNode = mesh.AssignedNode;
                renderMesh.Weights = weights.ToArray();
                renderMesh.Joints = joints.ToArray();
                renderMesh.IsSkeletonChild = mesh.IsSkeletonChild;
                renderMesh.Name = mesh.Name;
                renderMesh.IsSkeleton = mesh.IsSkeleton;
                renderMesh.JointsTransforms = mesh.BakedJointsTransform;
                renderMesh.IsJoint = mesh.IsJoint;
                if (parent != null)
                {
                    parent.TechnicalChildren.Add(renderMesh);
                    renderMesh.Parent = parent;
                }

                meshGroup.Meshes.Add(renderMesh);

                PostParseMeshes(ref meshGroup, mesh.Children, renderMesh);
            }
        }

        public static List<Vector3> JointsTransformGoDeeper(ModelParser.GLTFData.SkeletonData.Joint joint)
        {
            var transforms = new List<Vector3>();
            var boneTrans = joint.AssignedNode.LocalTransform.Translation;
            var boneGlobalTrans = new Vector4(boneTrans.X, boneTrans.Y, boneTrans.Z, 1.0f) * EliminationMathHelper.NumericsMatrixToMatrix(joint.AssignedNode.WorldMatrix);
            transforms.Add(new Vector3(boneTrans.X, boneTrans.Y, boneTrans.Z));
            foreach (var child in joint.Children)
            {
                transforms.AddRange(JointsTransformGoDeeper(child));
            }
            return transforms;
        }

        public static void AddGLTFMeshToObject(ModelParser.GLTFData data, ref GameObject obj)
        {
            var mesh = obj.AddComponent<MeshGroupComponent>();

            PostParseMeshes(ref mesh, data.Meshes);
        }

        private static List<Animation> JointsGoDeeper(ModelParser.GLTFData.SkeletonData.Joint joint)
        {
            var anims = new List<Animation>();
            if (joint.Animations != null)
            {
                foreach (var anim in joint.Animations)
                {
                    if (anims.Contains(anim)) continue;
                    anims.Add(anim);
                    foreach (var child in joint.Children)
                    {
                        anims.AddRange(JointsGoDeeper(child));
                    }
                }
            }
            return anims;
        }

        private static void SetAnimsDeep(ref EliminationEngine.Render.Mesh meshParent, Animation[] anims)
        {
            meshParent.Animations = anims;
            foreach (var mesh in meshParent.TechnicalChildren)
            {
                if (mesh.IsJoint || mesh.IsSkeletonChild)
                {
                    mesh.Animations = anims;
                }

                var meshVar = mesh;
                SetAnimsDeep(ref meshVar, anims);
            }
        }

        private static void PostParseAnim(ref MeshGroupComponent meshGroup, ModelParser.GLTFData.SkeletonData skeleton)
        {
            var anims = new List<Animation>();
            foreach (var joint in skeleton.Joints)
            {
                anims.AddRange(JointsGoDeeper(joint));
            }
            if (anims != null)
            {
                foreach (var mesh in meshGroup.Meshes)
                {
                    var meshVar = mesh;
                    SetAnimsDeep(ref meshVar, anims.ToArray());
                }
            }
        }

        private static ModelParser.GLTFData.SkeletonData? AnimGoDeeper(ModelParser.GLTFData.MeshData meshData)
        {
            if (meshData.SkeletonData != null)
            {
                return meshData.SkeletonData;
            }
            foreach (var mesh in meshData.Children)
            {
                if (mesh.SkeletonData != null)
                {
                    return mesh.SkeletonData;
                }
                var deeper = AnimGoDeeper(mesh);
                if (deeper != null)
                {
                    return deeper;
                }
            }
            return null;
        }

        public static void AddAnimationsToObject(ModelParser.GLTFData data, ref GameObject obj)
        {
            if (obj.TryGetComponent<MeshGroupComponent>(out var meshGroup))
            {
                foreach (var mesh in data.Meshes)
                {
                    var found = AnimGoDeeper(mesh);
                    if (found != null)
                    {
                        PostParseAnim(ref meshGroup, found);
                    }
                }
            }
            else
            {
                Logger.Warn("No mesh to add skeleton on");
            }
        }

        public static void LoadObjectAnims(ModelParser.GLTFData data, ref GameObject obj)
        {
            foreach (var mesh in data.Meshes)
            {
                if (mesh.SkeletonData != null)
                {
                    var meshGroup = obj.GetComponent<MeshGroupComponent>();
                    PostParseAnim(ref meshGroup, mesh.SkeletonData);
                }
            }
        }
    }
    public static class ModelParser
    {
        public class GLTFData
        {
            public class PrimitiveData
            {
                public Vector3Array Vertices { get; }
                public Vector2Array UVs { get; }
                public uint[] Indices { get; }
                public Vector3Array Normals { get; }
                public Vector4Array? Joints { get; set; }
                public Vector4Array? Weights { get; set; }

                public PrimitiveData(Vector3Array vertices, Vector2Array uvs, uint[] indices, Vector3Array normals)
                {
                    this.Vertices = vertices;
                    this.UVs = uvs;
                    this.Indices = indices;
                    this.Normals = normals;
                }
            }
            public class SkeletonData
            {
                public class Joint
                {
                    public List<Joint> Children = new();
                    public string Name = "unknown";
                    public int Id = 0;
                    public Animation[]? Animations;
                    public Node? AssignedNode;
                }

                public List<Joint> Joints = new();
                public string Name = "unknown";
            }
            public class MeshData
            {
                public string Name = "unknown";
                public List<float> Weights = new();
                public List<PrimitiveData> Primitives = new();
                public List<MeshData> Children = new();
                public Material? Mat;
                public SkeletonData? SkeletonData;
                public bool IsSkeleton = false;
                public bool IsSkeletonChild = false;
                public bool IsJoint = false;
                public Node? AssignedNode;
                public Vector3[]? BakedJointsTransform;
            }
            public List<MeshData> Meshes = new();
        }


        public static GLTFData ParseGLTFExternal(string path)
        {
            var modelData = new GLTFData();

            var model = ModelRoot.Load(path);
            var scene = model.DefaultScene;
            foreach (var node in scene.VisualChildren)
            {
                var meshData = ParseMesh(node, modelData);

                if (meshData != null)
                {
                    modelData.Meshes.Add(meshData);
                }
            }

            return modelData;
        }

        public static GLTFData.SkeletonData.Joint? ParseJoint(Node node, GLTFData modelData)
        {
            if (!node.IsSkinJoint) return null;
            var data = new GLTFData.SkeletonData.Joint();
            data.Name = node.Name;
            data.Animations = node.LogicalParent.LogicalAnimations.ToArray();
            data.AssignedNode = node;
            
            foreach (var child in node.VisualChildren)
            {
                var res = ParseJoint(child, modelData);
                if (res != null)
                {
                    data.Children.Add(res);
                }
            }
            return data;
        }

        public static GLTFData.MeshData? ParseMesh(Node node, GLTFData modelData, bool skeletonChild = false, Vector3[]? jointsTrans = null)
        {
            Console.WriteLine(node.Name);

            var meshData = new GLTFData.MeshData();
            meshData.IsSkeletonChild = skeletonChild;
            if (skeletonChild && jointsTrans != null)
            {
                meshData.BakedJointsTransform = jointsTrans;
            }
            meshData.IsJoint = node.IsSkinJoint;
            meshData.Name = node.Name;
            meshData.AssignedNode = node;
            var skelChild = false;
            List<Vector3>? transforms = null;
            foreach (var possibleJoint in node.VisualChildren) {
                if (possibleJoint.IsSkinJoint && !node.IsSkinJoint)
                {
                    meshData.IsSkeleton = true;
                    skelChild = true;
                    
                    var skeleton = new GLTFData.SkeletonData();
                    skeleton.Name = node.Name;
                    foreach (var child in node.VisualChildren)
                    {
                        var res = ParseJoint(child, modelData);
                        if (res != null)
                        {
                            skeleton.Joints.Add(res);
                        }
                    }
                    meshData.SkeletonData = skeleton;

                    transforms = new List<Vector3>();
                    foreach (var joint in meshData.SkeletonData.Joints)
                    {
                        transforms.AddRange(ModelHelper.JointsTransformGoDeeper(joint));
                    }
                }
                else
                {
                    foreach (var possJoint in possibleJoint.VisualChildren)
                    {
                        if (possJoint.IsSkinJoint && !possJoint.IsSkinJoint)
                        {
                            meshData.IsSkeleton = true;
                            var skeleton = new GLTFData.SkeletonData();
                            skeleton.Name = node.Name;
                            foreach (var child in node.VisualChildren)
                            {
                                var res = ParseJoint(child, modelData);
                                if (res != null)
                                {
                                    skeleton.Joints.Add(res);
                                }
                            }
                            meshData.SkeletonData = skeleton;
                        }
                    }
                }
            }


            if (node.Mesh != null)
            {
                foreach (var primitive in node.Mesh.Primitives)
                {
                    var vertices = primitive.GetVertices("POSITION").AsVector3Array();
                    var uvs = primitive.GetVertices("TEXCOORD_0").AsVector2Array();
                    var bakedIndices = primitive.GetIndices();
                    var normals = primitive.GetVertices("NORMAL").AsVector3Array();
                    var prim = new GLTFData.PrimitiveData(vertices, uvs,
                        bakedIndices?.ToArray() ?? Enumerable.Range(0, vertices.Count).Select(e => (uint)e).ToArray(), normals);
                    if (node.Mesh.LogicalParent.LogicalSkins.Count > 0)
                    {
                        var joints = primitive.GetVertices("JOINTS_0").AsVector4Array();
                        var weights = primitive.GetVertices("WEIGHTS_0").AsVector4Array();
                        prim.Joints = joints;
                        prim.Weights = weights;
                    }
                    meshData.Mat = primitive.Material;
                    meshData.Primitives.Add(prim);
                }
            }

            foreach (var child in node.VisualChildren)
            {
                var childData = ParseMesh(child, modelData, skelChild, transforms?.ToArray());
                if (childData != null)
                {
                    meshData.Children.Add(childData);
                }
            }

            return meshData;
        }
    }
}

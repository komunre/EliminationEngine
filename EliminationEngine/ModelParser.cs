using System.Numerics;
using SharpGLTF.Schema2;
using EliminationEngine.GameObjects;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

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
                    indexOffset += (uint)prim.Vertices.Length;
                }
                
                PostParseMesh(meshData.Children, ref indexOffset, data);
            }

            return data;
        }

        public static void PostParseMeshes(ref MeshGroupComponent meshGroup, List<ModelParser.GLTFData.MeshData> meshes)
        {
            foreach (var mesh in meshes)
            {
                var renderMesh = new Render.Mesh();
                var vertsData = new List<float>();
                var uvData = new List<float>();
                var indices = new List<uint>();
                foreach (var primitive in mesh.Primitives) {
                    vertsData.AddRange(primitive.Vertices.SelectMany(e => new[] { e.X, e.Y, e.Z }));
                    uvData.AddRange(primitive.UVs.SelectMany(e => new[] { e.X, e.Y }));
                    indices.AddRange(primitive.Indices.Select(e => e));
                }

                if (mesh.Mat != null)
                {
                    var color = mesh.Mat.Channels.ElementAt(0).Texture.PrimaryImage.Content.Content.ToArray();
                    renderMesh.Image = color.ToArray();
                }
                renderMesh.Width = 32;
                renderMesh.Height = 32;
                renderMesh.Vertices = vertsData.ToArray();
                renderMesh.TexCoords = uvData.ToArray();
                renderMesh.Indices = indices.ToArray();

                meshGroup.Meshes.Add(renderMesh);

                PostParseMeshes(ref meshGroup, mesh.Children);
            }
        }

        public static void AddGLTFMeshToObject(ModelParser.GLTFData data, ref GameObject obj)
        {
            uint io = 0;
            var vt = PostParseMesh(data.Meshes, ref io); // TODO: is that even needed here?
            var mesh = obj.AddComponent<GameObjects.MeshGroupComponent>();

            PostParseMeshes(ref mesh, data.Meshes);
        }
    }
    public static class ModelParser
    {
        public class GLTFData
        {
            public readonly struct PrimitiveData
            {
                public Vector3[] Vertices { get; }
                public Vector2[] UVs { get; }
                public uint[] Indices { get; }

                public PrimitiveData(Vector3[] vertices, Vector2[] uvs, uint[] indices)
                {
                    this.Vertices = vertices;
                    this.UVs = uvs;
                    this.Indices = indices;
                }
            }
            public class MeshData
            {
                public List<float> Weights = new();
                public List<PrimitiveData> Primitives = new();
                public List<MeshData> Children = new();
                public Material Mat;
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
                var meshData = ParseMesh(node);

                if (meshData != null)
                {
                    modelData.Meshes.Add(meshData);
                }
            }

            return modelData;
        }

        public static GLTFData.MeshData? ParseMesh(Node node)
        {
            Console.WriteLine(node.Name);

            var meshData = new GLTFData.MeshData();
            if (node.IsSkinSkeleton)
            {
                // Load skeleton here
            }
            if (node.Mesh != null)
            {
                var weights = node.Mesh.MorphWeights;
                meshData.Weights = weights.ToList();
                foreach (var primitive in node.Mesh.Primitives)
                {
                    var vertices = primitive.GetVertices("POSITION");
                    var uvs = primitive.GetVertices("TEXCOORD_0");
                    meshData.Primitives.Add(new GLTFData.PrimitiveData(
                        vertices.AsVector3Array().ToArray(),
                        uvs.AsVector2Array().ToArray(),
                        primitive.GetIndices().ToArray()));
                    meshData.Mat = primitive.Material;
                }
            }

            foreach (var child in node.VisualChildren)
            {
                var childData = ParseMesh(child);
                if (childData != null)
                {
                    meshData.Children.Add(childData);
                }
            }

            return meshData;
        }
    }
}

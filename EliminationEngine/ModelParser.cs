using System.Numerics;
using SharpGLTF.Schema2;
using EliminationEngine.GameObjects;

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

        public static void AddGLTFMeshToObject(ModelParser.GLTFData data, string texture, ref GameObject obj)
        {
            uint io = 0;
            var vt = PostParseMesh(data.Meshes, ref io);
            var mesh = obj.AddComponent<GameObjects.Mesh>();
            mesh.Vertices = vt.Vertices.ToArray();
            mesh.Indices = vt.Indices.ToArray();
            mesh.TexCoords = vt.UVs.ToArray();
            mesh.LoadMesh(texture);
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
            if (node.Mesh == null) return null;
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

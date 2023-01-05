using EliminationEngine.GameObjects;
using SharpGLTF.Schema2;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Numerics;

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
                    data.Vertices.AddRange(prim.Vertices.SelectMany(e => new[] { e.X, e.Y, e.Z }));
                    data.UVs.AddRange(prim.UVs.SelectMany(e => new[] { e.X, e.Y }));

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
                var verticesFull = new List<float>();
                var uvData = new List<float>();
                var indices = new List<uint>();
                var normals = new List<float>();
                foreach (var primitive in mesh.Primitives)
                {
                    vertsData.AddRange(primitive.Vertices.SelectMany(e => new[] { e.X + mesh.Center.X, e.Y + mesh.Center.Y, e.Z + mesh.Center.Z }));
                    var ind = primitive.Indices.Select(e => e);
                    var indFirst = ind.Min();
                    var indOrder = ind;
                    uvData.AddRange(primitive.UVs.SelectMany(e => new[] { e.X, e.Y }));
                    indices.AddRange(ind);
                    normals.AddRange(primitive.Normals.SelectMany(e => new[] { e.X, e.Y, e.Z }));
                }

                var final = new float[indices.Count() * 3];
                var index = 0;
                for (var i = 0; i < indices.Count(); i++)
                {
                    var indice = (int)indices.ElementAt(i);
                    final[index] = vertsData[indice * 3];
                    final[index + 1] = vertsData[indice * 3 + 1];
                    final[index + 2] = vertsData[indice * 3 + 2];
                    index += 3;
                }

                verticesFull.AddRange(final);

                // FLIPPING NORMALS HERE!!!
                var flippedIndices = new uint[indices.Count];
                for (var i = 0; i < indices.Count; i += 3)
                {
                    flippedIndices[i] = indices[i + 2];
                    flippedIndices[i + 1] = indices[i + 1];
                    flippedIndices[i + 2] = indices[i];
                }

                index = 0;
                var verticesFullFlipped = new float[verticesFull.Count()];

                for (var i = 0; i < flippedIndices.Count(); i++)
                {
                    var indice = (int)flippedIndices.ElementAt(i);
                    final[index] = vertsData[indice * 3];
                    final[index + 1] = vertsData[indice * 3 + 1];
                    final[index + 2] = vertsData[indice * 3 + 2];
                    index += 3;
                }

                verticesFullFlipped = final;

                if (mesh.Mat != null && mesh.Mat.Channels.ElementAt(0).Texture != null)
                {
                    var image = (Image<Rgba32>)SixLabors.ImageSharp.Image.Load(mesh.Mat.Channels.ElementAt(0).Texture.PrimaryImage.Content.Open());
                    renderMesh.Width = image.Width;
                    renderMesh.Height = image.Height;
                    renderMesh.Image = ImageLoader.LoadImageData(image).Pixels.ToArray();
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
                    renderMesh.Image = ImageLoader.LoadImageData(image).Pixels.ToArray();
                }
                renderMesh.Vertices = vertsData.ToArray();
                renderMesh.VerticesFull = verticesFull.ToArray();
                renderMesh.VerticesFullFlipped = verticesFullFlipped;
                renderMesh.TexCoords = uvData.ToArray();
                renderMesh.Indices = indices.ToArray();
                renderMesh.Normals = normals.ToArray();

                renderMesh.Name = mesh.Name;
                if (mesh.Mat != null)
                {
                    renderMesh.MaterialName = mesh.Mat.Name;
                }
                else
                {
                    renderMesh.MaterialName = "placeholder";
                }
                renderMesh.Position = new OpenTK.Mathematics.Vector3(mesh.Center.X, mesh.Center.Y, mesh.Center.Z);

                meshGroup.Meshes.Add(renderMesh);

                PostParseMeshes(ref meshGroup, mesh.Children);
            }
        }

        public static MeshGroupComponent AddGLTFMeshToObject(ModelParser.GLTFData data, ref GameObject obj)
        {
            var mesh = obj.AddComponent<GameObjects.MeshGroupComponent>();

            PostParseMeshes(ref mesh, data.Meshes);

            return mesh;
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
                public Vector3[] Normals { get; }

                public PrimitiveData(Vector3[] vertices, Vector2[] uvs, uint[] indices, Vector3[] normals)
                {
                    this.Vertices = vertices;
                    this.UVs = uvs;
                    this.Indices = indices;
                    this.Normals = normals;
                }
            }
            public class MeshData
            {
                public string Name = "Unknown";
                public List<float> Weights = new();
                public List<PrimitiveData> Primitives = new();
                public List<MeshData> Children = new();
                public Material? Mat;
                public Vector3 Center = Vector3.Zero;
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
            Console.WriteLine("Loading mesh: " + node.Name);

            var meshData = new GLTFData.MeshData();
            if (node.IsSkinSkeleton)
            {
                // Load skeleton here
            }
            if (node.Mesh != null)
            {
                var weights = node.Mesh.MorphWeights;
                meshData.Weights = weights.ToList();
                meshData.Name = node.Name;
                meshData.Center = node.LocalTransform.Translation;
                foreach (var primitive in node.Mesh.Primitives)
                {
                    var vertices = primitive.GetVertices("POSITION").AsVector3Array().ToArray();
                    var uvs = primitive.GetVertices("TEXCOORD_0").AsVector2Array().ToArray();
                    var bakedIndices = primitive.GetIndices();
                    var normals = primitive.GetVertices("NORMAL").AsVector3Array().ToArray();
                    meshData.Primitives.Add(new GLTFData.PrimitiveData(vertices, uvs,
                        bakedIndices?.ToArray() ?? Enumerable.Range(0, vertices.Length).Select(e => (uint)e).ToArray(), normals));
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

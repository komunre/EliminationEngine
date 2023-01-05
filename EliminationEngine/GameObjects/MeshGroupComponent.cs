using EliminationEngine.Physics;
using EliminationEngine.Render;
using OpenTK.Mathematics;

namespace EliminationEngine.GameObjects
{
    public class MeshGroupComponent : EntityComponent
    {
        public List<Mesh> Meshes = new();

        public MeshGroupComponent(GameObject o) : base(o)
        {

        }

        public Mesh? FindByName(string name)
        {
            foreach (var mesh in Meshes)
            {
                if (mesh.Name == name)
                {
                    return mesh;
                }
            }
            return null;
        }

        public void SetTextureForEvery(TextureData texture)
        {
            foreach (var mesh in Meshes)
            {
                mesh._tex = texture.TextureID;
            }
        }

        public List<Mesh> FindByMaterial(string materialName)
        {
            var found = new List<Mesh>();
            foreach (var mesh in Meshes)
            {
                if (mesh.MaterialName == materialName)
                {
                    found.Add(mesh);
                }
            }
            return found;
        }

        public Mesh CombineMeshes()
        {
            var vertices = new List<float>();
            var verticesFull = new List<float>();
            var verticesFullFlipped = new List<float>();
            var indices = new List<uint>();
            var normals = new List<float>();

            var texture = Meshes[0]._tex;

            foreach (var mesh in Meshes)
            {
                vertices.AddRange(mesh.Vertices);
                verticesFull.AddRange(mesh.VerticesFull);
                verticesFullFlipped.AddRange(mesh.VerticesFullFlipped);
                indices.AddRange(mesh.Indices);
                normals.AddRange(mesh.Normals);
            }

            var newMesh = new Mesh();
            newMesh.Vertices = vertices.ToArray();
            newMesh.VerticesFull = verticesFull.ToArray();
            newMesh.VerticesFullFlipped = verticesFullFlipped.ToArray();
            newMesh.Indices = indices.ToArray();
            newMesh.Normals = normals.ToArray();
            newMesh._tex = texture;
            
            return newMesh;
        }

        /// <summary>
        /// Creates collision box for an entire group of meshes
        /// </summary>
        public void CreateCollisionBox()
        {
            if (Owner.TryGetComponent<HitBox>(out var box))
            {
                Logger.Error("Hitbox already exists on the group of meshes.");
                return;
            }

            var min = new Vector3(999999f, 999999f, 999999f);
            var max = new Vector3(-999999f, -999999f, -999999f);
            foreach (var mesh in Meshes)
            {
                for (var i = 0; i < mesh.Vertices.Length; i += 3)
                {
                    var x = mesh.Vertices[i];
                    var y = mesh.Vertices[i + 1];
                    var z = mesh.Vertices[i + 2];

                    if (x < min.X)
                    {
                        min.X = x;
                    }
                    if (y < min.Y)
                    {
                        min.Y = y;
                    }
                    if (z < min.Z)
                    {
                        min.Z = z;
                    }

                    if (x > max.X)
                    {
                        max.X = x;
                    }
                    if (y > max.Y)
                    {
                        max.Y = y;
                    }
                    if (z > max.Z)
                    {
                        max.Z = z;
                    }
                }
            }
            
            var hitbox = Owner.AddComponent<HitBox>();
            hitbox.AddBox(new Box3(min, max));
        }
    }
}

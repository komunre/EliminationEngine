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

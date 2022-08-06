using EliminationEngine.Render;

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
    }
}

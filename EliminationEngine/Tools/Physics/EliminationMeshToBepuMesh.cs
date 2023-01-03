using EliminationEngine;
using EliminationEngine.Tools;
using EliminationEngine.GameObjects;
using BepuPhysics.Collidables;
using EliminationEngine.Physics;
using BepuUtilities.Memory;

namespace EliminationEngine.Tools.Physics
{
    public static class EliminationMeshToBepuMesh
    {
        public static BepuPhysics.Collidables.Mesh Convert(EliminationEngine.Render.Mesh mesh, BufferPool pool)
        {
            var triangleCount = mesh.Vertices.Length / (3*3);
            var triangles = new Triangle[triangleCount];
            for (var i = 0; i < mesh.Vertices.Length; i += 3*3)
            {
                var tr = new BepuPhysics.Collidables.Triangle();
                tr.A = new System.Numerics.Vector3(mesh.Vertices[i], mesh.Vertices[i + 1], mesh.Vertices[i + 2]);
                tr.B = new System.Numerics.Vector3(mesh.Vertices[i + 3], mesh.Vertices[i + 4], mesh.Vertices[i + 5]);
                tr.C = new System.Numerics.Vector3(mesh.Vertices[i + 6], mesh.Vertices[i + 7], mesh.Vertices[i + 8]);
                triangles[i / (3*3)] = tr;
            }
            unsafe
            {
                var buffer = new BepuUtilities.Memory.Buffer<BepuPhysics.Collidables.Triangle>(&triangles, triangleCount);
                var m = new Mesh(buffer, System.Numerics.Vector3.One, pool);
                return m;
            }
        }
    }
}

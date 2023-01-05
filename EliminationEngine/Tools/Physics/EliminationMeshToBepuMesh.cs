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
            var triangleCount = mesh.VerticesFullFlipped.Length / 9;
            pool.Take<Triangle>(triangleCount, out var triangles);
            for (var i = 0; i < mesh.VerticesFullFlipped.Length; i += 9)
            {
                ref var tr = ref triangles[i / 9];
                tr.A = new System.Numerics.Vector3(mesh.VerticesFullFlipped[i], mesh.VerticesFullFlipped[i + 1], mesh.VerticesFullFlipped[i + 2]);
                tr.B = new System.Numerics.Vector3(mesh.VerticesFullFlipped[i + 3], mesh.VerticesFullFlipped[i + 4], mesh.VerticesFullFlipped[i + 5]);
                tr.C = new System.Numerics.Vector3(mesh.VerticesFullFlipped[i + 6], mesh.VerticesFullFlipped[i + 7], mesh.VerticesFullFlipped[i + 8]);
            }

            var m = new Mesh(triangles, new System.Numerics.Vector3(1, 1, 1), pool);
            return m;
        }
    }
}

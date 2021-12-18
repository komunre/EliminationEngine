using EliminationEngine.GameObjects;
using EliminationEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttackGame
{
    public class GLTFTestSystem : EntitySystem
    {
        public override void OnLoad()
        {
            base.OnLoad();

            var data = ModelParser.ParseGLTFExternal("res/test.glb");

            var obj = new GameObject();
            ModelHelper.AddGLTFMeshToObject(data, "res/cube_test_texture.png", ref obj);

            var mesh = obj.GetComponent<Mesh>();
            for (var i = 0; i < mesh.Vertices.Count; i += 3)
            {
                Console.WriteLine(mesh.Vertices[i] + ":" + mesh.Vertices[i + 1] + ":" + mesh.Vertices[i + 2]);
            }

            obj.Position = new OpenTK.Mathematics.Vector3(0, 0, 0);
            obj.Rotation = OpenTK.Mathematics.Quaternion.FromEulerAngles(50, 30, 0);

            Engine.AddGameObject(obj);
        }
    }
}

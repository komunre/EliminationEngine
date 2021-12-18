using EliminationEngine.GameObjects;
using EliminationEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;

namespace AttackGame
{
    public class GLTFTestSystem : EntitySystem
    {
        public GameObject GltfObject;
        public override void OnLoad()
        {
            base.OnLoad();

            var data = ModelParser.ParseGLTFExternal("res/misshat.glb");
            var obj = new GameObject();
            var second = new GameObject();
            ModelHelper.AddGLTFMeshToObject(data, "res/cube_test_texture.png", ref obj);
            ModelHelper.AddGLTFMeshToObject(data, "res/cube_test_texture.png", ref second);

            obj.Position = new OpenTK.Mathematics.Vector3(0, 0, 0);
            obj.Rotation = OpenTK.Mathematics.Quaternion.FromEulerAngles(0.2f, 0.3f, 0);

            second.Position = new OpenTK.Mathematics.Vector3(0.5f, 0.5f, 2);
            second.Scale = new OpenTK.Mathematics.Vector3(1.5f, 1.5f, 1.5f);

            Engine.AddGameObject(obj);
            //Engine.AddGameObject(second);

            GltfObject = obj;
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            GltfObject.Rotation = EliminationMathHelper.QuaternionFromEuler(new Vector3(50, (float)Engine.Elapsed.TotalMilliseconds * 0.001f, 38));
        }
    }
}

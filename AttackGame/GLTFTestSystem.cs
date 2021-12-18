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
        public GameObject GltfObject;
        public override void OnLoad()
        {
            base.OnLoad();

            var data = ModelParser.ParseGLTFExternal("res/misshat.glb");

            var obj = new GameObject();
            ModelHelper.AddGLTFMeshToObject(data, "res/basic.png", ref obj);

            obj.Position = new OpenTK.Mathematics.Vector3(0, 0, 0);
            obj.Rotation = OpenTK.Mathematics.Quaternion.FromEulerAngles(50, 30, 0);

            Engine.AddGameObject(obj);

            GltfObject = obj;
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            GltfObject.Rotation = OpenTK.Mathematics.Quaternion.FromEulerAngles(34, (float)Engine.Elapsed.TotalMilliseconds * 0.001f, 38);
        }
    }
}

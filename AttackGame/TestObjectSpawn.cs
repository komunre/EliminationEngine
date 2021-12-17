using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EliminationEngine;
using EliminationEngine.GameObjects;

namespace AttackGame
{
    public class TestObjectSpawn : EntitySystem
    {
        GameObject test;
        public override void OnLoad()
        {
            base.OnLoad();
            if (Engine == null) return;

            var obj = new GameObject();
            var data = ModelParser.ParseObj("res/test.obj");

            test = obj;

            ModelHelper.AddObjMeshToObject(data, "res/basic.png", ref obj);

            Engine.AddGameObject(obj);
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            test.Position += new OpenTK.Mathematics.Vector3(0, 0.0005f, 0);
        }
    }
}

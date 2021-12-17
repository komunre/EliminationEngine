using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EliminationEngine;
using EliminationEngine.GameObjects;
using OpenTK.Mathematics;

namespace AttackGame
{
    public class TestObjectSpawn : EntitySystem
    {
        GameObject test;
        private float add = 0;
        public override void OnLoad()
        {
            base.OnLoad();
            if (Engine == null) return;

            var obj = new GameObject();
            var data = ModelParser.ParseObj("res/monkey.obj");

            obj.Position = new Vector3(0, 0, -1.5f);

            test = obj;

            ModelHelper.AddObjMeshToObject(data, "res/basic.png", ref obj);

            Engine.AddGameObject(obj);
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            add += 0.0005f;

            //test.Position += new Vector3(0, 0.0001f, 0);
            test.Rotation += Quaternion.FromEulerAngles(new Vector3(0, add, 0));
        }
    }
}

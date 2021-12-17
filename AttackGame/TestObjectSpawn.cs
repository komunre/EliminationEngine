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
        GameObject test2;
        private float add = 0;
        public override void OnLoad()
        {
            base.OnLoad();
            if (Engine == null) return;

            var obj = new GameObject();
            var data = ModelParser.ParseObj("res/cube.obj");

            obj.Position = new Vector3(-0.9f, -0.5f, 0);
            obj.Rotation = Quaternion.FromEulerAngles(15f, 0, 0);

            test = obj;

            ModelHelper.AddObjMeshToObject(data, "res/cube_test_texture.png", ref obj);

            Engine.AddGameObject(obj);
            
            var second = new GameObject();
            var sData = ModelParser.ParseObj("res/monkey.obj");

            test2 = second;

            ModelHelper.AddObjMeshToObject(sData, "res/basic.png", ref second);

            Engine.AddGameObject(test2);
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            add += 0.0005f;

            //test.Position += new Vector3(0, 0.0001f, 0);
            test.Rotation += Quaternion.FromEulerAngles(new Vector3(15, add, 0));
            test2.Rotation += Quaternion.FromEulerAngles(new Vector3(0, add, 0));
        }
    }
}

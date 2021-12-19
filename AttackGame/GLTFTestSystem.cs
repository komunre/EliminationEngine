﻿using EliminationEngine.GameObjects;
using EliminationEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace AttackGame
{
    public class GLTFTestSystem : EntitySystem
    {
        public GameObject GltfObject;
        private GameObject _camera;
        public override void OnLoad()
        {
            base.OnLoad();

            var camera = new GameObject();
            camera.AddComponent<CameraComponent>();
            camera.Position = new Vector3(0, 3, 0);
            camera.Rotation = EliminationMathHelper.QuaternionFromEuler(new Vector3(-35, 0, 0));

            Engine.AddGameObject(camera);
            _camera = camera;

            var data = ModelParser.ParseGLTFExternal("res/WaterBottle.glb");
            var obj = new GameObject();
            var second = new GameObject();
            var dummyBottle = new GameObject();
            dummyBottle.Position = new Vector3(0, 6, 0);
            ModelHelper.AddGLTFMeshToObject(data, ref obj);
            ModelHelper.AddGLTFMeshToObject(data, ref dummyBottle);

            var misshatData = ModelParser.ParseGLTFExternal("res/misshat.glb");
            ModelHelper.AddGLTFMeshToObject(misshatData, ref second);

            obj.Position = new OpenTK.Mathematics.Vector3(0, 0, 0);
            obj.Rotation = OpenTK.Mathematics.Quaternion.FromEulerAngles(0.2f, 0.3f, 0);
            obj.Scale = new Vector3(1f, 1f, 1f);

            second.Position = new OpenTK.Mathematics.Vector3(2, 0, 10);
            second.Scale = new OpenTK.Mathematics.Vector3(1f, 1f, 1f);

            Engine.AddGameObject(obj);
            Engine.AddGameObject(second);
            Engine.AddGameObject(dummyBottle);

            GltfObject = obj;
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            var dir = Vector3.Zero;
            if (Engine.KeyState.IsKeyDown(Keys.D)) {
                dir += Vector3.UnitZ;
            }
            if (Engine.KeyState.IsKeyDown(Keys.W))
            {
                dir += Vector3.UnitY;
            }
            if (Engine.KeyState.IsKeyDown(Keys.A))
            {
                dir += -Vector3.UnitZ;
            }
            if (Engine.KeyState.IsKeyDown(Keys.S))
            {
                dir += -Vector3.UnitY;
            }
            _camera.Position += dir * 2 * Engine.DeltaTime;
            //_camera.Rotation = EliminationMathHelper.QuaternionFromEuler(new Vector3(-90, 0, 0)); // WORKS!
            _camera.LookAt(new Vector3(0, 0, 0));
        }
    }
}

using EliminationEngine.GameObjects;
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
            camera.Position = new Vector3(0, 0, 2);
            camera.Rotation = EliminationMathHelper.QuaternionFromEuler(new Vector3(-35, 0, 0));

            Engine.AddGameObject(camera);
            _camera = camera;

            var data = ModelParser.ParseGLTFExternal("res/fox.glb");
            var obj = new GameObject();
            var second = new GameObject();
            ModelHelper.AddGLTFMeshToObject(data, ref obj);

            var misshatData = ModelParser.ParseGLTFExternal("res/misshat.glb");
            ModelHelper.AddGLTFMeshToObject(misshatData, ref second);

            obj.Position = new OpenTK.Mathematics.Vector3(0, 0, 0);
            obj.Rotation = OpenTK.Mathematics.Quaternion.FromEulerAngles(0.2f, 0.3f, 0);
            obj.Scale = new Vector3(0.01f, 0.01f, 0.01f);

            second.Position = new OpenTK.Mathematics.Vector3(0.5f, 0.5f, 2);
            second.Scale = new OpenTK.Mathematics.Vector3(1.5f, 1.5f, 1.5f);

            Engine.AddGameObject(obj);
            //Engine.AddGameObject(second);

            GltfObject = obj;
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            var dir = Vector3.Zero;
            if (Engine.KeyState.IsKeyDown(Keys.D)) {
                dir += new Vector3(1, 0, 0);
            }
            if (Engine.KeyState.IsKeyDown(Keys.W))
            {
                dir += new Vector3(0, 1, 0);
            }
            if (Engine.KeyState.IsKeyDown(Keys.A))
            {
                dir += new Vector3(-1, 0, 0);
            }
            if (Engine.KeyState.IsKeyDown(Keys.S))
            {
                dir += new Vector3(0, -1, 0);
            }
            _camera.Position += dir * 2 * Engine.DeltaTime;
            _camera.LookAt(Vector3.Zero);
        }
    }
}

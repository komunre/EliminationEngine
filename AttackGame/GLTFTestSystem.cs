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
using EliminationEngine.Render;
using EliminationEngine.Tools;
using EliminationEngine.Systems;

namespace AttackGame
{
    public class GLTFTestSystem : EntitySystem
    {
        public GameObject? GltfObject;
        private GameObject? _camera;
        private GameObject? redLight;
        private SoundSystem _soundSystem;

        public GLTFTestSystem(Elimination e) : base(e)
        {

        }
        public override void OnLoad()
        {
            base.OnLoad();

            _soundSystem = Engine.GetSystem<SoundSystem>();

            var camera = new GameObject();
            camera.AddComponent<CameraComponent>();
            camera.Position = new Vector3(0, 4.5f, -10);
            camera.Rotation = EliminationMathHelper.QuaternionFromEuler(new Vector3(-35, 0, 0));

            Engine.AddGameObject(camera);
            _camera = camera;

            var data = ModelParser.ParseGLTFExternal("res/ocean.glb");
            var obj = new GameObject();
            var second = new GameObject();
            //var dummyBottle = new GameObject();
            //dummyBottle.Position = new Vector3(0, 6, 0);
            ModelHelper.AddGLTFMeshToObject(data, ref obj);
            //ModelHelper.AddGLTFMeshToObject(data, ref dummyBottle);

            var misshatData = ModelParser.ParseGLTFExternal("res/misshat.glb");
            ModelHelper.AddGLTFMeshToObject(misshatData, ref second);

            obj.Position = new OpenTK.Mathematics.Vector3(0, 0, 0);
            //obj.Rotation = OpenTK.Mathematics.Quaternion.FromEulerAngles(0.2f, 0.3f, 0);
            obj.Scale = new Vector3(1f, 1f, 1f);

            second.Position = new OpenTK.Mathematics.Vector3(2, 0, 10);
            second.Scale = new OpenTK.Mathematics.Vector3(1f, 1f, 1f);

            Engine.AddGameObject(obj);
            Engine.AddGameObject(second);
            //Engine.AddGameObject(dummyBottle);

            var cubeData = ModelParser.ParseGLTFExternal("res/cube.glb");
            /*for (var i = 0; i < 15; i++)
            {
                var obj2 = new GameObject();
                var random = new Random();
                obj2.Position = new Vector3(random.Next(-10, 10), random.Next(-10, 10), random.Next(-10, 10));
                ModelHelper.AddGLTFMeshToObject(cubeData, ref obj2);
                //var l = obj2.AddComponent<LightComponent>();
                //l.Diffuse = 150;
                Engine.AddGameObject(obj2);
            }*/

            GltfObject = obj;

            var light = new GameObject();
            var lightComp = light.AddComponent<LightComponent>();
            light.Position = new Vector3(0, 5, 0);
            lightComp.Diffuse = 70f;

            var secLight = new GameObject();
            var secLightComp = secLight.AddComponent<LightComponent>();
            secLightComp.Diffuse = 20f;
            secLightComp.Color = new EliminationEngine.Tools.Color(255, 255, 255, 255);
            secLight.Position = new Vector3(0, 5, 0);
            //Engine.AddGameObject(light);
            //Engine.AddGameObject(secLight);
            redLight = secLight;

            var parent = new GameObject();
            var parented = new GameObject();
            parent.Position = new Vector3(0, 1, 0);

            //parent.AddComponent<RotateDemoComponent>();
            parent.Rotation = EliminationMathHelper.QuaternionFromEuler(new Vector3(123, 230, 0));
            parented.Position = new Vector3(0, 0, 0);
            parented.Scale = new Vector3(0.2f, 0.2f, 0.2f);

            //ModelHelper.AddGLTFMeshToObject(cubeData, ref parent);
            ModelHelper.AddGLTFMeshToObject(misshatData, ref parented);

            Engine.AddChildTo(parent, parented);
            Engine.AddGameObject(parent);

            Engine.AddChildTo(camera, redLight);
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            var dir = Vector3.Zero;
            if (Engine.KeyState.IsKeyDown(Keys.D)) {
                dir += Vector3.UnitX;
            }
            if (Engine.KeyState.IsKeyDown(Keys.W))
            {
                dir += Vector3.UnitZ;
            }
            if (Engine.KeyState.IsKeyDown(Keys.A))
            {
                dir += -Vector3.UnitX;
            }
            if (Engine.KeyState.IsKeyDown(Keys.S))
            {
                dir += -Vector3.UnitZ;
            }
            if (Engine.KeyState.IsKeyDown(Keys.Space))
            {
                dir += Vector3.UnitY;
            }
            if (Engine.KeyState.IsKeyDown(Keys.LeftShift))
            {
                dir += -Vector3.UnitY;
            }
            //_camera.Position += dir * 2f * Engine.DeltaTime;

            if (redLight != null)
            {
                redLight.Position.X = (float)MathHelper.Sin(Engine.Elapsed.TotalMilliseconds * 0.001f) * 10.5f;
                redLight.Position.Z = (float)MathHelper.Cos(Engine.Elapsed.TotalMilliseconds * 0.001f) * 10.5f;
            }


            //_camera.Rotation = EliminationMathHelper.QuaternionFromEuler(new Vector3(90, 0, 0)); // WORKS!
            if (_camera != null)
            {
                _camera.LookAt(new Vector3(0, 0, 0)); // works too
            }

            var rotateDemos = Engine.GetObjectsOfType<RotateDemoComponent>();
            if (rotateDemos == null) return;
            foreach (var rotate in rotateDemos)
            {
                rotate.Owner.Rotation *= EliminationMathHelper.QuaternionFromEuler(rotate.RotDir * Engine.DeltaTime);
            }
            //_camera.LookAt(rotateDemos[0].Owner.GlobalPosition);

            if (Engine.KeyState.IsKeyDown(Keys.O)) {
                _soundSystem.GenSound(1046, SoundType.Noise, 100, 1);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AttackGame.Game;
using EliminationEngine;
using EliminationEngine.GameObjects;
using OpenTK.Mathematics;

namespace AttackGame
{
    public enum GameState
    {
        Menu,
        Singleplayer,
    }
    public class GameStateSystem : EntitySystem
    {
        public GameState State = GameState.Menu;
        public List<GameObject> UIObjects = new();
        protected int GameRoundWorld;
        public bool Started = false;

        public GameStateSystem(Elimination e) : base(e)
        {
        }

        public override void OnLoad()
        {
            base.OnLoad();

            var camera = new GameObject();
            camera.AddComponent<CameraComponent>();
            Engine.AddGameObject(camera);
        }

        public override void PostLoad()
        {
            GameRoundWorld = Engine.CreateWorld();

            var start = new GameObject();
            var widget = start.AddComponent<UIWidget>();
            widget.Text = "Start";
            start.Position = new Vector3(0, 0, 0);
            start.Scale = new Vector3(0.3f, 0.1f, 1f);
            widget.Size = 60;
            widget.Font = new SharpFont.Face(new SharpFont.Library(), "res/Oswald-Regular.ttf");
            widget.OnClick += () =>
            {
                State = GameState.Singleplayer;
            };
            widget.OnScreen = true;
            Engine.AddGameObject(start);
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            if ((State == GameState.Singleplayer || Engine.KeyState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Enter)) && !Started)
            {
                Engine.SwitchWorld(GameRoundWorld);
                var cam = new GameObject();
                cam.AddComponent<CameraComponent>();
                cam.Position = new Vector3(5, 3, -10);
                cam.LookAt(new Vector3(0, 0, 0));

                var map = new GameObject();
                var mapData = ModelParser.ParseGLTFExternal("res/map.glb");
                var boxes = map.AddComponent<HitBox>();
                boxes.AddBox(new Box3(new Vector3(-1090, -1, -1090), new Vector3(1090, 1, 1090)));
                ModelHelper.AddGLTFMeshToObject(mapData, ref map);
                map.Position = new Vector3(0, 0, 0);

                var testAnim = new GameObject();
                var amily = ModelParser.ParseGLTFExternal("res/Amily.glb");
                ModelHelper.AddGLTFMeshToObject(amily, ref testAnim);
                //ModelHelper.LoadObjectAnims(amily, ref testAnim);
                var amilyAnims = ModelParser.ParseGLTFExternal("res/anims/SimpleCharacter.glb");
                ModelHelper.AddAnimationsToObject(amilyAnims, ref testAnim);
                testAnim.GetComponent<MeshGroupComponent>().Animator.ActiveAnimation = "Eww";
                Engine.AddGameObject(testAnim);

                var spawnerBlue = new GameObject();
                spawnerBlue.AddComponent<SpawnerComponent>();

                var spawnerRed = new GameObject();
                var spwn = spawnerRed.AddComponent<SpawnerComponent>();
                spwn.Red = true;

                spawnerBlue.Position = new Vector3(-15, 1.1f, -15);
                spawnerRed.Position = new Vector3(15, 1.1f, 15);

                var light = new GameObject();
                var lightComp = light.AddComponent<LightComponent>();
                lightComp.Diffuse = 8;
                lightComp.MaxAffectDstance = 1000;
                lightComp.Constant = 10;
                light.Position = new Vector3(0, 80, 0);

                //Engine.AddGameObject(map);
                Engine.AddGameObject(cam);
                Engine.AddGameObject(spawnerBlue);
                Engine.AddGameObject(spawnerRed);
                Engine.AddGameObject(light);

                Started = true;
            }
        }
    }
}

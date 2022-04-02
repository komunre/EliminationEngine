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

        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            if ((State == GameState.Singleplayer || Engine.KeyState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Enter)) && !Started)
            {
                Engine.SwitchWorld(GameRoundWorld);
                var cam = new GameObject();
                cam.AddComponent<CameraComponent>();
                cam.Position = new Vector3(0, 10, -10);
                cam.LookAt(new Vector3(0, 0, 0));

                var map = new GameObject();
                var mapData = ModelParser.ParseGLTFExternal("res/map.glb");
                var boxes = map.AddComponent<HitBox>();
                boxes.AddBox(new Box3(new Vector3(-1090, -1, -1090), new Vector3(1090, 1, 1090)));
                ModelHelper.AddGLTFMeshToObject(mapData, ref map);
                map.Position = new Vector3(0, 0, 0);

                var spawnerBlue = new GameObject();
                spawnerBlue.AddComponent<SpawnerComponent>();

                var spawnerRed = new GameObject();
                var spwn = spawnerRed.AddComponent<SpawnerComponent>();
                spwn.Red = true;

                spawnerBlue.Position = new Vector3(-15, 0, -15);
                spawnerRed.Position = new Vector3(15, 0, 15);

                var light = new GameObject();
                var lightComp = light.AddComponent<LightComponent>();
                lightComp.Diffuse = 8;
                lightComp.MaxAffectDstance = 1000;
                lightComp.Constant = 10;
                light.Position = new Vector3(0, 80, 0);

                Engine.AddGameObject(map);
                Engine.AddGameObject(cam);
                Engine.AddGameObject(spawnerBlue);
                Engine.AddGameObject(spawnerRed);
                Engine.AddGameObject(light);

                Started = true;
            }
        }
    }
}

using EliminationEngine.GameObjects;
using EliminationEngine;
using OpenTK.Mathematics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using EliminationEngine.Tools;

namespace AttackGame.Game
{
    public class SpawnSystem : EntitySystem
    {
        public SpawnSystem(Elimination e) : base(e)
        {

        }
        public override void OnUpdate()
        {
            base.OnUpdate();

            foreach (var spawner in Engine.GetObjectsOfType<SpawnerComponent>())
            {
                if (Engine.Elapsed.Seconds > spawner.NextSpawnTime)
                {
                    for (var i = 0; i < spawner.NextSpawnCount; i++) {
                        var howarh = new GameObject();
                        howarh.Position = spawner.Owner.Position;
                        var box = howarh.AddComponent<HitBox>();
                        box.AddBox(new Box3(new Vector3(-0.05f, -0.05f, -0.05f), new Vector3(0.05f, 0.05f, 0.05f)));
                        var hwc = howarh.AddComponent<HowarhComponent>();
                        hwc.SetDestination(howarh.Position);
                        var sprite = howarh.AddComponent<SpriteGenerator>();
                        sprite.GenerateMesh((Image<Rgba32>)Image.Load("res/howarh.png"), false);

                        if (spawner.Red)
                        {
                            howarh.BaseColor = new EliminationEngine.Tools.Color(255, 0, 0, 255);
                        }
                        else
                        {
                            howarh.BaseColor = new EliminationEngine.Tools.Color(0, 0, 255, 255);
                        }

                        Engine.AddGameObject(howarh);
                    }

                    spawner.NextSpawnTime = Engine.Elapsed.Seconds + new Random().Next(2, 6);
                    spawner.NextSpawnCount = new Random().Next(10, 25);
                }
            }
        }
    }
}

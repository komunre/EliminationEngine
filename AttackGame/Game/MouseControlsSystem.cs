using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EliminationEngine;
using EliminationEngine.GameObjects;
using EliminationEngine.Physics;
using EliminationEngine.Tools;
using OpenTK.Mathematics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace AttackGame.Game
{
    public class MouseControlsSystem : EntitySystem
    {
        public bool Pressed = true;
        public float Radius = 0.0f;
        public List<GameObject> Selected = new();
        protected GameObject SelectVisual;

        public MouseControlsSystem(Elimination e) : base(e)
        {

        }
        public override void OnLoad()
        {
            base.OnLoad();

            //Engine.LockCursor();
        }
        public override void OnUpdate()
        {
            base.OnUpdate();

            if (Engine.KeyState.IsKeyPressed(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Escape))
            {
                Engine.StopEngine();
            }

            if (Engine.MouseState.IsButtonDown(OpenTK.Windowing.GraphicsLibraryFramework.MouseButton.Left))
            {
                if (SelectVisual == null)
                {
                    SelectVisual = new GameObject();
                    var spriteGen = SelectVisual.AddComponent<SpriteGenerator>();
                    spriteGen.GenerateMesh(Image.Load<Rgba32>("res/select-spiral.png"));
                    SelectVisual.Rotation = EliminationMathHelper.QuaternionFromEuler(new Vector3(90, 0, 0));
                    Engine.AddGameObject(SelectVisual);
                }
                Selected.Clear();
                Radius += 5.0f * Engine.DeltaTime;
                Pressed = true;

                var hitPos = Engine.GetSystem<Raycast>().RaycastFromCameraCursor()[0].EndPos;
                //var hitPos = Engine.GetSystem<Raycast>().RaycastFromPos(new Vector3(0.1f, 15, 0.1f), EliminationMathHelper.QuaternionFromEuler(new Vector3(0, -1, 0)))[0].EndPos;
                Logger.Info(Radius.ToString());
                SelectVisual.Position = hitPos;
                SelectVisual.Scale = new Vector3(Math.Max(1, Radius), Math.Max(1, Radius), 1);
                foreach (var select in Engine.GetSystem<CollisionSystem>().ObjectsInSphere(hitPos, Radius))
                {
                    if (select.HasComponent<HowarhComponent>() && !Selected.Contains(select))
                    {
                        Selected.Add(select);
                    }
                }
            }
            else
            {
                if (Pressed)
                {
                    Logger.Info("Total selected: " + Selected.Count);
                }
                Radius = 0.0f;
                Pressed = false;
            }

            if (Engine.MouseState.IsButtonDown(OpenTK.Windowing.GraphicsLibraryFramework.MouseButton.Right))
            {
                var pos = Engine.GetSystem<Raycast>().RaycastFromCameraCursor()[0].EndPos;
                foreach (var select in Selected)
                {
                    if (select.TryGetComponent<HowarhComponent>(out var h))
                    {
                        h.SetDestination(pos);
                    }
                }
            }
        }
    }
}

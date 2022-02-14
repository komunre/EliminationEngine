using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EliminationEngine.GameObjects;
using AttackGame;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Mathematics;
using EliminationEngine;

namespace AttackGame.Game
{
    public class CameraSystem : EntitySystem
    {
        public CameraSystem(Elimination e) : base(e)
        {
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            var cameras = Engine.GetObjectsOfType<CameraComponent>()?.Select(e => { if (e.Active) return e; else return null; });
            if (cameras == null) return;
            var camera = cameras.ElementAt(0);
            if (camera == null) return;

            var dir = Vector3.Zero;
            if (Engine.KeyState.IsKeyDown(Keys.W))
            {
                dir.Z += 1;
            }
            if (Engine.KeyState.IsKeyDown(Keys.A))
            {
                dir.X += 1;
            }
            if (Engine.KeyState.IsKeyDown(Keys.S))
            {
                dir.Z -= 1;
            }
            if (Engine.KeyState.IsKeyDown(Keys.D))
            {
                dir.X -= 1;
            }

            camera.Owner.Position += dir * 2 * Engine.DeltaTime;
        }
    }
}

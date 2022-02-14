using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EliminationEngine.GameObjects;
using EliminationEngine;
using OpenTK.Mathematics;

namespace AttackGame.Game
{
    public class HowarhSystem : EntitySystem
    {
        public HowarhSystem(Elimination e) : base(e)
        {
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            var cameras = Engine.GetObjectsOfType<CameraComponent>().Select(x => { if (x.Active) return x; else return null; });
            var camera = cameras.FirstOrDefault();

            var rand = new Random();
            foreach (var howarh in Engine.GetObjectsOfType<HowarhComponent>())
            {
                if (howarh.State == HowarhState.Wandering && howarh.Owner.Position == howarh.GetDestination())
                {
                    howarh.SetDestination(howarh.Owner.Position + new Vector3((float)rand.NextDouble(), 0, (float)rand.NextDouble()));
                }

                howarh.Owner.Position += (howarh.GetDestination() + howarh.Owner.Position).Normalized() * 0.5f * Engine.DeltaTime;
                howarh.Owner.LookAt(camera.Owner.Position);
            }
        }
    }
}

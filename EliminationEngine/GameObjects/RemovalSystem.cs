using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EliminationEngine.GameObjects;
using EliminationEngine;

namespace EliminationEngine.GameObjects
{
    public class RemovalSystem : EntitySystem
    {
        public RemovalSystem(Elimination e) : base(e)
        {

        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            foreach (var removeTimer in Engine.GetObjectsOfType<RemovalTimer>())
            {
                removeTimer.Timer -= Engine.DeltaTime;
                if (removeTimer.Timer <= 0)
                {
                    Engine.RemoveGameObject(removeTimer.Owner);
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EliminationEngine.GameObjects;
using OpenTK.Mathematics;

namespace AttackGame.Game
{
    public enum HowarhState
    {
        Wandering,
        InWar,
        Moving,
    }
    public class HowarhComponent : EntityComponent
    {
        protected Vector3 Destination;
        public HowarhState State = HowarhState.Wandering;

        public HowarhComponent(GameObject owner) : base(owner)
        {
        }

        public void SetDestination(Vector3 dest)
        {
            var rand = new Random();
            Destination = dest + new Vector3((float)rand.NextDouble(), 0, (float)rand.NextDouble());
        }

        public Vector3 GetDestination()
        {
            return Destination;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EliminationEngine.GameObjects;
using OpenTK.Mathematics;

namespace AttackGame.Game
{
    public class SpawnerComponent : EntityComponent
    {
        public SpawnerComponent(GameObject o) : base(o)
        {

        }

        public float NextSpawnTime = 0;
        public int NextSpawnCount = 0;

        public bool Red = false;
    }
}

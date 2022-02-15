using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EliminationEngine.GameObjects;
using EliminationEngine;

namespace EliminationEngine.GameObjects
{
    public class RemovalTimer : EntityComponent
    {
        public float Timer = 2.0f;
        public RemovalTimer(GameObject owner) : base(owner)
        {

        }
    }
}

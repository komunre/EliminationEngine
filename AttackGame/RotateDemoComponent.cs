using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EliminationEngine.GameObjects;
using OpenTK.Mathematics;

namespace AttackGame
{
    public class RotateDemoComponent : EntityComponent
    {
        public RotateDemoComponent(GameObject o) : base(o)
        {

        }
        public Vector3 RotDir = new Vector3(30, 30, 0);
    }
}

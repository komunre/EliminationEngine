using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;
using EliminationEngine.GameObjects;

namespace EliminationEngine.GameObjects
{
    public class HitBox : EntityComponent
    {
        public HitBox(GameObject owner) : base(owner)
        {

        }

        protected List<Box3> Boxes = new();
        public bool CanHitRaycast = true;

        public void AddBox(Box3 box)
        {
            Boxes.Add(box);
        }

        public List<Box3> GetBoxes()
        {
            return Boxes;
        }
    }
}

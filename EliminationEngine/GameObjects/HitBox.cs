using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;
using EliminationEngine.GameObjects;

namespace EliminationEngine.GameObjects
{
    public class BoxData
    {
        public Box3 Bounds;
        public bool Static = false;

        public BoxData(Box3 bounds, bool stat)
        {
            Bounds = bounds;
            Static = stat;
        }
    }
    public class HitBox : EntityComponent
    {
        public HitBox(GameObject owner) : base(owner)
        {

        }

        protected List<BoxData> Boxes = new();
        public bool CanHitRaycast = true;

        public void AddBox(Box3 box, bool stat = false)
        {
            Boxes.Add(new BoxData(box, stat));
        }

        public List<BoxData> GetBoxes()
        {
            return Boxes;
        }
    }
}

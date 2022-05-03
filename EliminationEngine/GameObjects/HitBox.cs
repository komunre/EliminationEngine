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
            /*var boxes = Boxes;
            var boxesCopy = new List<BoxData>();
            foreach (var box in boxes) {
                var newBox = new BoxData(new Box3(), false);
                newBox.Bounds.Min = new Vector3(box.Bounds.Min.X + Owner.GlobalPosition.X, box.Bounds.Min.Y + Owner.GlobalPosition.Y, box.Bounds.Min.Z + Owner.GlobalPosition.Z);
                newBox.Bounds.Max = new Vector3(box.Bounds.Max.X + Owner.GlobalPosition.X, box.Bounds.Max.Y + Owner.GlobalPosition.Y, box.Bounds.Max.Z + Owner.GlobalPosition.Z);
                boxesCopy.Add(newBox);
            }
            return boxesCopy;*/
            return Boxes;
        }
    }
}

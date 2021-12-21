using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliminationEngine.GameObjects
{
    public class CameraComponent : EntityComponent
    {
        public CameraComponent(GameObject o) : base(o)
        {

        }
        public bool Active = true;
        public int Width = 800;
        public int Height = 600;
        public float ClipNear = 0.1f;
        public float ClipFar = 1000f;
    }
}

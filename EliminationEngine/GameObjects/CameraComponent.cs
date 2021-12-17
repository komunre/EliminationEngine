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
        public Vector3 Position;
        public Quaternion Rotation;
    }
}

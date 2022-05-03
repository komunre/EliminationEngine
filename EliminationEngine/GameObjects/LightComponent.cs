using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EliminationEngine.Tools;

namespace EliminationEngine.GameObjects
{
    public class LightComponent : EntityComponent
    {
        public LightComponent(GameObject o) : base(o)
        {

        }
        public float Constant = 0;
        public float Diffuse = 100f;
        public float Qudratic = 0;
        public Color Color = new Color(255, 255, 255, 255);
        public float MaxAffectDstance = 30f;
        public List<string> IgnoredLayers = new();
    }
}

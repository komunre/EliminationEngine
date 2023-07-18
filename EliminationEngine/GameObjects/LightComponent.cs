using EliminationEngine.Tools;

namespace EliminationEngine.GameObjects
{
    public class LightComponent : EntityComponent
    {
        public LightComponent(GameObject o) : base(o)
        {

        }
        public float Constant = 0;
        public float ConstantDiffuse = 0;
        public float Diffuse = 0.1f;
        public float Qudratic = 0;
        public Color Color = new Color(255, 255, 255, 255);
        public float MaxAffectDstance = 1000f;
        public List<string> IgnoredLayers = new();
        public bool Directional = false;
        public float DirectionalCutoffAngle = 15;
        public float DistanceFactor = 1.0f;
        public float MaxBrightness = 1.0f;
    }
}

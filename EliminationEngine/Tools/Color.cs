namespace EliminationEngine.Tools
{
    public struct Color
    {
        public float R = 255;
        public float G = 255;
        public float B = 255;
        public float A = 255;

        public Color(float r, float g, float b, float a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public void ConvertToFloat()
        {
            R /= 255;
            G /= 255;
            B /= 255;
            A /= 255;
        }
    }
}

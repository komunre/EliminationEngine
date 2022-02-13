using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpFont;
using System.Drawing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;

namespace EliminationEngine.GameObjects
{
    public class TextComponent : EntityComponent
    {
        public TextComponent(GameObject owner) : base(owner)
        {
            
        }

        public string Text = "";
        public uint Size = 13;
        public Face Font;
        public bool OnScreen = true;
        public bool Changed = true;
        public Bitmap? DrawBitmap = null;
        public Image<Rgba32>? DrawImage = null;

        public int VertBuff = 0;
        public int IndBuff = 0;
        public int TextureIdent = 0;
        public int TexCoordBuff = 0;
    }
}

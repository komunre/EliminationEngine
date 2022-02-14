using EliminationEngine.Render;
using OpenTK.Graphics.OpenGL4;
using SharpFont;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliminationEngine.GameObjects
{
    public class UIWidget : EntityComponent
    {
        public UIWidget(GameObject owner) : base(owner)
        {
            ImageFromColor(Rgba32.ParseHex("#7dbf9c"));
        }

        public string Text = String.Empty;
        public Face? Font = null;
        public uint Size = 30;
        public Image<Rgba32> DrawImage;
        public Image<Rgba32> TextImage; // Actually used for generation
        
        public float RelX = 0;
        public float RelY = 0;
        
        [Obsolete("Usse scale instead")]
        public float Width = 50;
        [Obsolete("Usse scale instead")]
        public float Height = 50;

        public int VertBuff = 0;
        public int IndBuff = 0;
        public int TexCoordBuff = 0;
        public int TextureIdent = 0;
        public int TextTextureIdent = 0;

        public bool Changed = false;
        public bool OnScreen = true;
        public bool Pressed = false;

        public delegate void OnWidgetClick();
        public event OnWidgetClick OnClick;

        public void ImageFromColor(Rgba32 color)
        {
            DrawImage = new Image<Rgba32>(1000, 1000);
            for (var i = 0; i < DrawImage.Height; i++)
            {
                var row = DrawImage.GetPixelRowSpan(i);
                for (var j = 0; j < DrawImage.Width; j++)
                {
                    row[j] = color;
                }
            }
        }

        public void RegenerateAll(CameraComponent camera)
        {
            GenerateText();

            SpriteGenerator sprGen = null;
            if (!Owner.TryGetComponent<SpriteGenerator>(out sprGen))
            {
                sprGen = Owner.AddComponent<SpriteGenerator>();
            }
            sprGen.GenerateMesh(TextImage, OnScreen);
            if (!OnScreen)
            {
                sprGen.Owner.Parent = camera.Owner;
            }
        }

        public void GenerateText()
        {
            if (Text == String.Empty || Text == null) return;
            if (Font == null) return;

            Font.SetPixelSizes(Size, Size);
            var bmp = FontService.RenderString(Text, Font);

            if (bmp == null) return;

            var memStream = new MemoryStream();
            bmp.Save(memStream, ImageFormat.Png);
            memStream.Position = 0;
            var loaded = SixLabors.ImageSharp.Image.Load<Rgb24>(memStream, new PngDecoder());
            TextImage = loaded.CloneAs<Rgba32>();
            TextImage = ImageLoader.MakeColorTransparent(TextImage, Rgba32.ParseHex("#000000"));
        }

        public void OnClickCall()
        {
            if (OnClick == null) return;
            OnClick.Invoke();
        }
    }
}

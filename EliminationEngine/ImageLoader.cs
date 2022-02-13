using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace EliminationEngine
{
    public class ImageData
    {
        public int Width = 0;
        public int Height = 0;
        public List<byte> Pixels = new();
    }
    public class ImageLoader
    {
        public static ImageData LoadTexture(string path)
        {
            Image<Rgba32> image = Image.Load<Rgba32>(path);

            image.Mutate(x => x.Flip(FlipMode.Vertical));

            var pixels = new List<byte>(4 * image.Width * image.Height);

            for (var y = 0; y < image.Height; y++)
            {
                var row = image.GetPixelRowSpan(y);

                for (var x = 0; x < image.Width; x++)
                {
                    pixels.Add(row[x].R);
                    pixels.Add(row[x].G);
                    pixels.Add(row[x].B);
                }
            }

            var data = new ImageData();
            data.Width = image.Width;
            data.Height = image.Height;
            data.Pixels = pixels;
            return data;
        }

        public static Image<Rgba32> MakeColorTransparent(Image<Rgba32> image, Rgba32 color)
        {
            var im = image;
            for (var y = 0; y < im.Height; y++)
            {
                var row = im.GetPixelRowSpan(y);

                for (var x = 0; x < im.Width; x++)
                {
                    var pixel = row[x];
                    if (pixel.Equals(color))
                    {
                        row[x] = Rgba32.ParseHex("#000000");
                        row[x].A = 0;
                    }
                }
            }
            return im;
        }

        public static ImageData LoadTextureFromImage(Image<Rgba32> image, bool flip = false)
        {
            if (flip)
            {
                image.Mutate(x => x.Flip(FlipMode.Vertical));
            }

            var pixels = new List<byte>(4 * image.Width * image.Height);

            for (var y = 0; y < image.Height; y++)
            {
                var row = image.GetPixelRowSpan(y);

                for (var x = 0; x < image.Width; x++)
                {
                    pixels.Add(row[x].R);
                    pixels.Add(row[x].G);
                    pixels.Add(row[x].B);
                    pixels.Add(row[x].A);
                }
            }

            var data = new ImageData();
            data.Width = image.Width;
            data.Height = image.Height;
            data.Pixels = pixels;
            return data;
        }
    }
}

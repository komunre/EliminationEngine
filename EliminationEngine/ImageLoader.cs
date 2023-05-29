
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

using OpenTK.Graphics.OpenGL4;
using EliminationEngine.Render;

namespace EliminationEngine
{
    public class ImageData
    {
        public int Width = 0;
        public int Height = 0;
        public List<byte> Pixels = new();
    }

    public enum ImageFilter
    {
        Nearest,
        Linear,
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

        public static ImageData LoadImageData(Image<Rgba32> image, bool flip = false)
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

        public static TextureData CreateTextureFromImage(string imagePath, ImageFilter filter, bool flip = false, bool invert = false)
        {
            return CreateTextureFromImage(Image.Load<Rgba32>(imagePath), filter, flip, invert);
        }

        public static TextureData CreateTextureFromImage(Image<Rgba32> image, ImageFilter filter, bool flip = false, bool invert = false)
        {
            if (flip)
            {
                image.Mutate(x => x.Flip(FlipMode.Vertical));
            }

            if (invert)
            {
                image.Mutate(x => x.Flip(FlipMode.Horizontal));
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

            TextureMinFilter filterMin;
            TextureMagFilter filterMag;

            switch (filter)
            {
                default:
                case ImageFilter.Nearest:
                    filterMin = TextureMinFilter.Nearest;
                    filterMag = TextureMagFilter.Nearest;
                    break;
                case ImageFilter.Linear:
                    filterMin = TextureMinFilter.Linear;
                    filterMag = TextureMagFilter.Linear;
                    break;
            }

            var texture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, texture);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pixels.ToArray());

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)filterMin);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)filterMag);

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            var tdata = new TextureData(texture);
            tdata.ImageData = data;

            return tdata;
        }

        public static TextureData CreateTextureFromImageData(ImageData image, ImageFilter filter, bool flip = false, bool invert = false)
        {
            if (flip)
            {
                List<byte> flipped = new();
                for (int y = image.Height-1; y >= 0; y--)
                {
                    for (int x = 0; x < (image.Width)*4; x++)
                    {
                        flipped.Add(image.Pixels[(y * (image.Width*4)) + x]);
                    }
                }
                image.Pixels = flipped;
            }

            if (invert)
            {
                List<byte> inverted = new();
                for (int y = 0; y < image.Height; y++)
                {
                    for (int x = (image.Width)*4-1; x >= 0; x -= 4)
                    {
                        var i = (y * (image.Width * 4)) + x;
                        inverted.AddRange(new byte[] { image.Pixels[i-3], image.Pixels[i-2], image.Pixels[i-1], image.Pixels[i] });
                    }
                }
                image.Pixels = inverted;
            }

            var data = image;

            TextureMinFilter filterMin;
            TextureMagFilter filterMag;

            switch (filter)
            {
                default:
                case ImageFilter.Nearest:
                    filterMin = TextureMinFilter.Nearest;
                    filterMag = TextureMagFilter.Nearest;
                    break;
                case ImageFilter.Linear:
                    filterMin = TextureMinFilter.Linear;
                    filterMag = TextureMagFilter.Linear;
                    break;
            }

            var texture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, texture);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, image.Pixels.ToArray());

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)filterMin);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)filterMag);

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            var tdata = new TextureData(texture);
            tdata.ImageData = data;

            return tdata;
        }
    }
}

using EliminationEngine.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpFont;
using System.Drawing;
using OpenTK.Graphics.OpenGL4;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Formats.Jpeg;
using System.Drawing.Imaging;
using SixLabors.ImageSharp.Formats.Png;

namespace EliminationEngine.Render
{
    public class TextSystem : EntitySystem
    {
        public string DefaultFontPath = "res/engfont.ttf";
        public Library Lib = new Library();

        private Shader TextShader = new Shader("Shaders/text.vert", "Shaders/text.frag");
        public TextSystem(Elimination e) : base(e)
        {
        }

        public override void OnLoad()
        {
            base.OnLoad();
        }

        public override void OnDraw()
        {
            base.OnDraw();

            var texts = Engine.GetObjectsOfType<TextComponent>();
            foreach (var text in texts)
            {
                if (text.Font != null)
                {
                    text.Font.SetPixelSizes(text.Size, text.Size);

                    if (text.Text == "" || text.Text == null) return;

                    if (text.Changed)
                    {
                        text.VertBuff = GL.GenBuffer();
                        text.IndBuff = GL.GenBuffer();
                        text.TexCoordBuff = GL.GenBuffer();

                        var bmp = FontService.RenderString(text.Text, text.Font);
                        text.DrawBitmap = bmp;

                        if (bmp == null) return;

                        var memStream = new MemoryStream();
                        bmp.Save(memStream, ImageFormat.Png);
                        memStream.Position = 0;
                        var loaded = SixLabors.ImageSharp.Image.Load<Rgb24>(memStream, new PngDecoder());
                        text.DrawImage = loaded.CloneAs<Rgba32>();
                        text.DrawImage = ImageLoader.MakeColorTransparent(text.DrawImage, Rgba32.ParseHex("#000000"));

                        text.DrawImage.SaveAsPng("testfile.png");

                        var cameras = Engine.GetObjectsOfType<CameraComponent>()?.Select(e => { if (e.Active) return e; else return null; });
                        var camera = cameras.ElementAt(0);

                        if (text.OnScreen)
                        {
                            var vertices = new float[4 * 3];

                            vertices[0] = text.Owner.Position.X / camera.Width;
                            vertices[1] = text.Owner.Position.Y / camera.Height;
                            vertices[2] = 0.0f;

                            vertices[3] = (float)((text.Owner.Position.X + text.DrawBitmap.Width) / camera.Width);
                            vertices[4] = text.Owner.Position.Y / camera.Height;
                            vertices[5] = 0.0f;

                            vertices[6] = (float)((text.Owner.Position.X + text.DrawBitmap.Width) / camera.Width);
                            vertices[7] = (float)((text.Owner.Position.Y + text.DrawBitmap.Height) / camera.Height);
                            vertices[8] = 0.0f;

                            vertices[9] = text.Owner.Position.X / camera.Width;
                            vertices[10] = (float)((text.Owner.Position.Y + text.DrawBitmap.Height) / camera.Height);
                            vertices[11] = 0.0f;

                            var indices = new uint[6];
                            indices[0] = 0;
                            indices[1] = 1;
                            indices[2] = 2;
                            indices[3] = 2;
                            indices[4] = 3;
                            indices[5] = 0;

                            GL.BindBuffer(BufferTarget.ArrayBuffer, text.VertBuff);
                            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

                            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
                            GL.EnableVertexAttribArray(0);

                            GL.BindBuffer(BufferTarget.ElementArrayBuffer, text.IndBuff);
                            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);
                        }

                        text.TextureIdent = GL.GenTexture();
                        GL.BindTexture(TextureTarget.Texture2D, text.TextureIdent);
                        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, text.DrawImage.Width, text.DrawImage.Height, 0, OpenTK.Graphics.OpenGL4.PixelFormat.Rgba, PixelType.UnsignedByte, ImageLoader.LoadTextureFromImage(text.DrawImage, true).Pixels.ToArray());
                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Nearest);

                        var texCoords = new float[4 * 2];
                        texCoords[0] = 0.0f;
                        texCoords[1] = 0.0f;

                        texCoords[2] = 1.0f;
                        texCoords[3] = 0.0f;

                        texCoords[4] = 1.0f;
                        texCoords[5] = 1.0f;

                        texCoords[6] = 0.0f;
                        texCoords[7] = 1.0f;

                        GL.BindBuffer(BufferTarget.ArrayBuffer, text.TexCoordBuff);
                        GL.BufferData(BufferTarget.ArrayBuffer, texCoords.Length * sizeof(float), texCoords.ToArray(), BufferUsageHint.StaticDraw);

                        GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
                        GL.EnableVertexAttribArray(1);


                        text.Changed = false;
                    }

                    if (text.OnScreen)
                    {
                        TextShader.Use();

                        GL.BindTexture(TextureTarget.Texture2D, text.TextureIdent);
                        GL.BindVertexArray(text.VertBuff);
                        GL.BindBuffer(BufferTarget.ElementArrayBuffer, text.IndBuff);

                        GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);
                    }
                }
            }
        }
    }
}

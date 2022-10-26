using OpenTK.Graphics.OpenGL4;

namespace EliminationEngine.GameObjects
{
    public static class DefaultCameraBuffers
    {
        public static int VertexBuff = 0;
        public static int TexCoordBuff = 0;
        public static int VertexArray = 0;
        public static int IndicesBuff = 0;
        public static Shader Shader = new Shader("Shaders/camera.vert", "Shaders/camera.frag");

        public static void InitValues()
        {
            VertexBuff = GL.GenBuffer();
            VertexArray = GL.GenVertexArray();
            IndicesBuff = GL.GenBuffer();

            GL.BindVertexArray(VertexArray);

            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBuff);
            GL.BufferData(BufferTarget.ArrayBuffer, 4 * 3 * sizeof(float), new float[] {
                -1.0f, -1.0f, 0.0f,
                1.0f, -1.0f, 0.0f,
                1.0f, 1.0f, 0.0f,
                -1.0f, 1.0f, 0.0f,
            }, BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, IndicesBuff);
            GL.BufferData(BufferTarget.ElementArrayBuffer, 6 * sizeof(uint), new uint[] { 0, 1, 2, 0, 2, 3 }, BufferUsageHint.StaticDraw);

            TexCoordBuff = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, TexCoordBuff);
            GL.BufferData(BufferTarget.ArrayBuffer, 4 * 2 * sizeof(float), new float[] {
                0.0f, 0.0f,
                1.0f, 0.0f,
                1.0f, 1.0f,
                0.0f, 1.0f,
            }, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
            GL.EnableVertexAttribArray(1);
        }
    }
    public class CameraComponent : EntityComponent
    {
        public CameraComponent(GameObject o) : base(o)
        {
            RunFullFrameBufferConfig();
        }

        public SimpleRotation Rotation = new SimpleRotation();

        public int FoV = 80;
        public bool Active = false;
        public int Width = 800;
        public int Height = 800;
        public float ClipNear = 0.1f;
        public float ClipFar = 1000f;
        protected int FrameBuffer = 0;
        protected int DepthFrameBuffer = 0;
        protected int RenderTexture = 0;
        public bool RenderToTexture = false;
        public Shader CameraShader = DefaultCameraBuffers.Shader;
        public bool UseDefaultShape = true;
        protected int RBO = 0;
        public bool Perspective = true;

        public bool Protected = true;

        private float _orthoWidth = 5;
        private float _orthoHeight = 5;
        private float _orthoVis = 5;

        public float OrthoWidth
        {
            get => _orthoWidth;
            private set => _orthoWidth = value;
        }

        public float OrthoHeight
        {
            get => _orthoHeight;
            private set => _orthoHeight = value;
        }

        public float OrthoVisibility
        {
            get => _orthoVis;
            private set => _orthoVis = value;
        }

        public void SetOrthoVisibility(float visibility)
        {
            _orthoWidth = 5;
            _orthoHeight = 5;
            //_orthoWidth = visibility;
            //_orthoHeight = visibility / ((float)Height / (float)Width);
            //_orthoVis = visibility;

            //Console.WriteLine("Window: " + Width + ":" + Height);
            //Console.WriteLine("Render:" + OrthoWidth + ":" + OrthoHeight);
        }

        protected void GenerateFrameBuffers()
        {
            FrameBuffer = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FrameBuffer);
            //GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Rgb32i, Width, Height);
            DepthFrameBuffer = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, DepthFrameBuffer);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent, Width, Height);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, DepthFrameBuffer);

            RBO = GL.GenRenderbuffer();
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, RBO);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Depth24Stencil8, Width, Height);
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FrameBuffer);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment, RenderbufferTarget.Renderbuffer, RBO);
        }

        public int GetFrameBuffer()
        {
            return FrameBuffer;
        }

        public void BindFrameBuffer()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FrameBuffer);
        }

        protected void GenerateRenderTexture()
        {
            RenderTexture = GL.GenTexture();
            BindFrameBuffer();
            GL.BindTexture(TextureTarget.Texture2D, RenderTexture);
            var data = new byte[Width * Height * 3];
            for (int y = 0; y < Height * 3; y++)
            {
                for (int x = 0; x < Width * 3; x++)
                {
                    var index = y * Width + x;
                    if (index == data.Length) break;
                    data[index] = (byte)(0);
                }
            }
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, Width, Height, 0, PixelFormat.Rgb, PixelType.UnsignedByte, data);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
        }

        protected void ConfigureFrameBuffers()
        {
            BindFrameBuffer();
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, RenderTexture, 0);
            GL.DrawBuffers(1, new[] { DrawBuffersEnum.ColorAttachment0 });
        }

        public void RunFullFrameBufferConfig()
        {
            GenerateFrameBuffers();
            GenerateRenderTexture();
            ConfigureFrameBuffers();
            Protected = false;
        }

        public void RemoveFrameBuffers()
        {
            GL.DeleteFramebuffer(FrameBuffer);
            GL.DeleteFramebuffer(DepthFrameBuffer);
        }

        public void ClearTextureTools()
        {
            GL.DeleteFramebuffer(FrameBuffer);
            GL.DeleteFramebuffer(DepthFrameBuffer);
            GL.DeleteTexture(RenderTexture);
            GL.DeleteRenderbuffer(RBO);
            Protected = true;
        }

        public int GetTexture()
        {
            return RenderTexture;
        }

        public int GetRBO()
        {
            return RBO;
        }
    }
}

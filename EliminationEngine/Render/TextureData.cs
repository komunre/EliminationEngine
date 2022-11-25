using OpenTK.Graphics.OpenGL4;

namespace EliminationEngine.Render
{
    public struct TextureData
    {
        public int TextureID = 0;
        public int FBO = 0;

        public ImageData? ImageData = null;

        public TextureData()
        {
        }

        public TextureData(int id)
        {
            TextureID = id;
        }

        public TextureData(int id, int fbo)
        {
            TextureID = id;
            FBO = fbo;
        }

        public void GenerateFBO()
        {
            FBO = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FBO);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, TextureID, 0);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }
    }
}

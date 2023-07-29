using EliminationEngine.Render;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using OpenTK.Graphics.OpenGL4;

namespace EliminationEngine.GameObjects
{
    public class SpriteGenerator : EntityComponent
    {
        public SpriteGenerator(GameObject owner) : base(owner)
        {
        }

        public static void AddMeshToObject(GameObject gameObject, Image<Rgba32> image, ImageFilter filter, bool unlit = true, bool onscreen = false)
        {
            var generator = gameObject.AddComponent<SpriteGenerator>();
            generator.GenerateMesh(image, filter, onscreen, unlit);
        }

        public static void AddMeshToObject(GameObject gameObject, ImageData image, ImageFilter filter, bool unlit = true, bool onscreen = false)
        {
            var generator = gameObject.AddComponent<SpriteGenerator>();
            generator.GenerateMesh(image, filter, onscreen, unlit);
        }

        public void GenerateMesh(Image<Rgba32> image, ImageFilter filter, bool onScreen = false, bool unlit = true)
        {
            GenerateMesh(ImageLoader.LoadImageData(image), filter, onScreen, unlit);
        }

        public void GenerateMesh(ImageData image, ImageFilter filter, bool onScreen = false, bool unlit = true)
        {
            MeshGroupComponent? meshGroup = null;
            if (!Owner.TryGetComponent<MeshGroupComponent>(out meshGroup))
            {
                meshGroup = Owner.AddComponent<MeshGroupComponent>();
            }

            var mesh = new Mesh();
            mesh.Vertices = EngineStatics.SpriteStatics.Vertices;
            mesh.Indices = EngineStatics.SpriteStatics.Indices;
            mesh.TexCoords = EngineStatics.SpriteStatics.TexCoords;

            mesh._tex = ImageLoader.CreateTextureFromImageData(image, filter, true, true).TextureID;

            if (unlit)
            {
                var vertShader = "Shaders/unlit.vert";
                if (onScreen)
                {
                    vertShader = "Shaders/onscreen.vert";
                }
                mesh._shader = new Shader(vertShader, "Shaders/text.frag");
            }
            else
            {
                mesh._shader = new Shader("Shaders/textured.vert", "Shaders/textured.frag", "Shaders/textured.geom");
            }

            meshGroup.Meshes.Add(mesh);
        }
    }
}

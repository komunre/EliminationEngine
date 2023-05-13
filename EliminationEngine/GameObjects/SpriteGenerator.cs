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

        public static void AddMeshToObject(GameObject gameObject, Image<Rgba32> image, ImageFilter filter)
        {
            var generator = gameObject.AddComponent<SpriteGenerator>();
            generator.GenerateMesh(image, filter, false);
        }

        public static void AddMeshToObject(GameObject gameObject, ImageData image, ImageFilter filter)
        {
            var generator = gameObject.AddComponent<SpriteGenerator>();
            generator.GenerateMesh(image, filter, false);
        }

        public void GenerateMesh(Image<Rgba32> image, ImageFilter filter, bool onScreen = false)
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

            mesh._tex = ImageLoader.CreateTextureFromImage(image, filter, true).TextureID;

            var vertShader = "Shaders/unlit.vert";
            if (onScreen)
            {
                vertShader = "Shaders/onscreen.vert";
            }
            mesh._shader = new Shader(vertShader, "Shaders/text.frag");

            meshGroup.Meshes.Add(mesh);
        }

        public void GenerateMesh(ImageData image, ImageFilter filter, bool onScreen = false)
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

            mesh._tex = ImageLoader.CreateTextureFromImageData(image, filter, true).TextureID;

            var vertShader = "Shaders/unlit.vert";
            if (onScreen)
            {
                vertShader = "Shaders/onscreen.vert";
            }
            mesh._shader = new Shader(vertShader, "Shaders/text.frag");

            meshGroup.Meshes.Add(mesh);
        }
    }
}

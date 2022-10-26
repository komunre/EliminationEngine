using EliminationEngine.Render;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

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

        public void GenerateMesh(Image<Rgba32> image, ImageFilter filter, bool onScreen = false)
        {
            MeshGroupComponent? meshGroup = null;
            if (!Owner.TryGetComponent<MeshGroupComponent>(out meshGroup))
            {
                meshGroup = Owner.AddComponent<MeshGroupComponent>();
            }

            var mesh = new Mesh();
            mesh.Vertices = new float[4 * 3]
            {
                0.0f, 0.0f, 0.0f,
                1.0f, 0.0f, 0.0f,
                1.0f, 1.0f, 0.0f,
                0.0f, 1.0f, 0.0f,
            };
            mesh.Indices = new uint[6 * 2]
            {
                0, 1, 2, 2, 3, 0,
                0, 3, 2, 2, 1, 0,
            };
            mesh.TexCoords = new float[4 * 2]
            {
                0.0f, 0.0f,
                1.0f, 0.0f,
                1.0f, 1.0f,
                0.0f, 1.0f,
            };

            mesh._tex = ImageLoader.CreateTextureFromImage(image, filter, true).Item1;

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

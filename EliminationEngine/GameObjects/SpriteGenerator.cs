using EliminationEngine.Render;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using OpenTK.Graphics.OpenGL4;

namespace EliminationEngine.GameObjects
{
    public class SpriteGenerator : EntityComponent
    {
        public static Dictionary<byte[], int> LoadedTextures = new();
        public static Shader UnlitShader = new Shader("Shaders/unlit.vert", "Shaders/text.frag");
        public static Shader OnScreenShader = new Shader("Shaders/onscreen.vert", "Shaders/text.frag");

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

        public void RegisterSprite(ImageData image, ImageFilter filter, bool onScreen = false, bool unlit = true)
        {

        }

        public void GenerateMesh(ImageData image, ImageFilter filter, bool onScreen = false, bool unlit = true)
        {
            MeshGroupComponent? meshGroup = null;
            if (!Owner.TryGetComponent<MeshGroupComponent>(out meshGroup))
            {
                meshGroup = Owner.AddComponent<MeshGroupComponent>();
            }

            var mesh = new Mesh();
            mesh._vertexArr = EngineStatics.SpriteStatics.VertexArray;
            mesh._buffer = EngineStatics.SpriteStatics.VertexBuffer;
            mesh._indicesBuffer = EngineStatics.SpriteStatics.IndicesBuffer;
            mesh._texCoordBuffer = EngineStatics.SpriteStatics.TexCoordBuffer;
            //mesh.Vertices = EngineStatics.SpriteStatics.Vertices;
            mesh.Indices = EngineStatics.SpriteStatics.Indices;
            //mesh.TexCoords = EngineStatics.SpriteStatics.TexCoords;

            foreach (var k in LoadedTextures.Keys)
            {
                if (k.SequenceEqual(image.Hash))
                {
                    mesh._tex = LoadedTextures[k];
                }
            }
            if (mesh._tex == 0) {
                mesh._tex = ImageLoader.CreateTextureFromImageData(image, filter, true, true).TextureID;
                LoadedTextures.Add(image.Hash, mesh._tex);
            }

            if (unlit)
            {
                mesh._shader = UnlitShader;
            }
            else if ((unlit && onScreen) || onScreen)
            {
                mesh._shader = OnScreenShader;
            }
            else
            {
                mesh._shader = MeshSystem.TexturedShader;
            }

            meshGroup.Meshes.Add(mesh);
        }
    }
}

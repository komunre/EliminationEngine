using EliminationEngine;
using EliminationEngine.GameObjects;
using OpenTK.Graphics.OpenGL4;

namespace EliminationEngine
{
    public static class EngineStatics
    {
        public static SpriteStatics SpriteStatics = new SpriteStatics();
        public static CameraStatics CameraStatics = new CameraStatics();

        public static void Init()
        {
            SpriteStatics.Init();
            CameraStatics.Init();
        }
    }

    public class SpriteStatics
    {
        public float[] Vertices = new float[4 * 3]
        {
                -1.0f, -1.0f, 0.0f,
                1.0f, -1.0f, 0.0f,
                1.0f, 1.0f, 0.0f,
                -1.0f, 1.0f, 0.0f,
        };

        public uint[] Indices = new uint[6 * 2]
        {
                0, 1, 2, 2, 3, 0,
                0, 3, 2, 2, 1, 0,
        };

        public float[] TexCoords = new float[4 * 2] // Horizontally flipped due to some bug. TODO: Figure out why
        {
                0.0f, 0.0f,
                1.0f, 0.0f,
                1.0f, 1.0f,
                0.0f, 1.0f,
        };

        public int VertexArray = 0;
        public int VertexBuffer = 0;
        public int IndicesBuffer = 0;
        public int TexCoordBuffer = 0;
        public Shader Shader = new Shader("Shaders/unlit.vert", "Shaders/text.frag");

        public void Init()
        {
            VertexBuffer = GL.GenBuffer();
            VertexArray = GL.GenVertexArray();
            IndicesBuffer = GL.GenBuffer();

            GL.BindVertexArray(VertexArray);

            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, 4 * 3 * sizeof(float), Vertices, BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, IndicesBuffer);
            GL.BufferData(BufferTarget.ElementArrayBuffer, 6 * sizeof(uint), Indices, BufferUsageHint.StaticDraw);

            TexCoordBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, TexCoordBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, 4 * 2 * sizeof(float), TexCoords, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
            GL.EnableVertexAttribArray(1);
        }
    }

    public class CameraStatics : SpriteStatics
    {
        public CameraStatics()
        {
            TexCoords = new float[] {
                0.0f, 0.0f,
                1.0f, 0.0f,
                1.0f, 1.0f,
                0.0f, 1.0f,
            };
            Indices = new uint[]
            {
                0, 1, 2, 0, 2, 3
            };
            Shader = new Shader("Shaders/camera.vert", "Shaders/camera.frag");
        }
    }

    public class EngineStaticsInitSystem : EntitySystem
    {
        public EngineStaticsInitSystem(Elimination e) : base(e)
        {

        }

        public override void OnLoad()
        {
            base.OnLoad();
            EngineStatics.Init();
        }
    }
}

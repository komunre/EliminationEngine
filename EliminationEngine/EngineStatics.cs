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

        public float[] Normals = new float[4 * 3];

        public int VertexArray = 0;
        public int VertexBuffer = 0;
        public int IndicesBuffer = 0;
        public int TexCoordBuffer = 0;
        public int NormalsBuffer = 0;
        public Shader Shader = new Shader("Shaders/unlit.vert", "Shaders/text.frag");

        public void Init()
        {
            var v1 = Vertices[1] - Vertices[3];
            var v2 = Vertices[0] - Vertices[2];
            var n = v1 * v2;
            for (int i = 0; i < 4; i++)
            {
                Normals[i * 3] = n;
                Normals[i * 3 + 1] = n;
                Normals[i * 3 + 2] = n;
            }

            VertexBuffer = GL.GenBuffer();
            VertexArray = GL.GenVertexArray();
            IndicesBuffer = GL.GenBuffer();
            NormalsBuffer = GL.GenBuffer();
            TexCoordBuffer = GL.GenBuffer();

            GL.BindVertexArray(VertexArray);

            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, 4 * 3 * sizeof(float), Vertices, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, IndicesBuffer);
            GL.BufferData(BufferTarget.ElementArrayBuffer, 6 * 2 * sizeof(uint), Indices, BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, TexCoordBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, 4 * 2 * sizeof(float), TexCoords, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
            GL.EnableVertexAttribArray(1);

            /*GL.BindBuffer(BufferTarget.ArrayBuffer, NormalsBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, 4 * 3 * sizeof(float), Normals, BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(2);*/
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

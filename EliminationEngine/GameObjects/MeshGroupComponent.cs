using EliminationEngine.Render;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliminationEngine.GameObjects
{
    public class MeshGroupComponent : EntityComponent
    {
        public List<Mesh> Meshes = new();
        public Animator Animator = new();

        public MeshGroupComponent(GameObject o) : base(o)
        {
            
        }
    }
}

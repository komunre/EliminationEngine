using EliminationEngine;
using EliminationEngine.GameObjects;
using EliminationEngine.Render;

namespace EliminationEngine.Systems.Tiles
{
    public class TiledObjectComponent : EntityComponent, TileData
    {
        public int X { get; set; }
        public int Y { get; set; }
        public TileChunk? CurrentChunk;
        public bool ChunkLoader = false;

        public TiledObjectComponent(GameObject owner) : base(owner)
        {

        }

        public int ID { get; set; }
        public TextureData Texture { get; set; }
        public bool Collision { get; set; }

        public void RoundFromPos()
        {
            var globalPos = Owner.GlobalPosition;
            X = (int)globalPos.X;
            Y = (int)globalPos.Y;
        }
    }
}

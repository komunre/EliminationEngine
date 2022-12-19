using EliminationEngine.Tools;
using EliminationEngine.GameObjects;
using EliminationEngine;
using EliminationEngine.Render;

namespace EliminationEngine.Systems.Tiles
{
    public interface TileData
    {
        public int ID { get; set; }
        public TextureData Texture { get; set; }
        public bool Collision { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
    }
}

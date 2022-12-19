

using EliminationEngine.Render;

namespace EliminationEngine.Systems.Tiles
{
    public class BackgroundTile : TileData
    {
        public int ID { get; set; }
        public TextureData Texture { get; set; }
        public bool Collision { get; set; }

        public int X { get; set; }
        public int Y { get; set; }

        public BackgroundTile(int iD, TextureData texture, bool collision)
        {
            ID = iD;
            Texture = texture;
            Collision = collision;
        }

        public BackgroundTile(int id)
        {
            this.ID = id;
        }

        public BackgroundTile(int id, int texID)
        {
            this.ID = id;
            Texture = new TextureData(texID);
        }
    }
}

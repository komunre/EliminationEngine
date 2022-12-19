
namespace EliminationEngine.Systems.Tiles
{
    public struct TileChunk
    {
        public int ID = -1;
        public int X = 0;
        public int Y = 0;

        public const int Width = 16;
        public const int Height = 16;

        public static TileChunk Invalid = new TileChunk(-1);

        public BackgroundTile[,] ChunkContent = new BackgroundTile[TileChunk.Width, TileChunk.Height];

        public TileChunk(int id)
        {
            ID = id;
        }

        public TileChunk(int id, int x, int y)
        {
            this.ID = id;
            this.X = x;
            this.Y = y;
        }
    }
}

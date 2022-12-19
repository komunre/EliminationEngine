using EliminationEngine.GameObjects;
using EliminationEngine.Render;
using OpenTK.Graphics.OpenGL;
using System.ComponentModel;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace EliminationEngine.Systems.Tiles
{
    public class TileSystem : EntitySystem
    {
        public bool TileMovement = true;
        public bool RoundFromPos = false;

        public string[] LoadedMapFiles;
        public List<TileChunk> LoadedChunks = new();

        public List<TextureData> RegisteredTextures = new();
        public Dictionary<TiledObjectComponent, (int, int)> LastPositions = new();

        public delegate TileChunk WorldGenerator(TileSystem system, int x, int y);

        protected WorldGenerator? WorldGen;
        public bool ProceduralGeneration = false;

        public int MaxTileID = 0;
        protected byte[] _objectStart = Encoding.ASCII.GetBytes("ObjectDATA[");

        public string CurrentMapPath = "res/map/";

        public TileSystem(Elimination e) : base(e)
        {

        }

        public int RegisterTileTexture(TextureData data)
        {
            RegisteredTextures.Add(data);
            return RegisteredTextures.IndexOf(data);
        }

        public int GetGridID(int x, int y)
        {
            return (x/16) * ((y/16) + 1);
        }

        public TileChunk ChunkLoaderGen(TileSystem sys, int x, int y)
        {
            var id = GetGridID(x, y);
            return LoadChunk(id);
        }

        public void SetWorldGenerator(WorldGenerator gen)
        {
            WorldGen = gen;
        }

        public void LoadMap(string mapPath)
        {
            var mapFiles = Directory.GetFiles(mapPath);
            LoadedMapFiles = mapFiles;

            CurrentMapPath = mapPath;

            if (!mapFiles.Contains("0map.ch"))
            {
                Logger.Error("No ID 0 chunk found in map files!");
                return;
            }
        }

        public void ReloadMapFilesList()
        {
            var mapFiles = Directory.GetFiles(CurrentMapPath);
            LoadedMapFiles = mapFiles;
        }

        public TileChunk LoadChunk(string filename)
        {
            if (!LoadedMapFiles.Contains(filename))
            {
                Logger.Error("No chunk with filename " + filename + " found in map files! Aborting chunk load.");
                return TileChunk.Invalid;
            }

            var chunk = new TileChunk();

            var fileData = File.ReadAllBytes(CurrentMapPath + filename);
            var IDBytes = new byte[] { fileData[0], fileData[1], fileData[2], fileData[3] };
            var chunkID = BitConverter.ToInt32(IDBytes);

            chunk.ID = chunkID;

            chunk.X = BitConverter.ToInt32(new byte[] { fileData[4], fileData[5], fileData[6], fileData[7] });
            chunk.Y = BitConverter.ToInt32(new byte[] { fileData[8], fileData[9], fileData[10], fileData[11] });

            var x = 0;
            var y = 0;
            for (var i = 16; i < 16 + (16*16 * 3); i += 2)
            {
                var texID = fileData[i];
                var tile = new BackgroundTile(MaxTileID, RegisteredTextures[texID], fileData[i + 1] == 1 ? true : false);
                tile.ID = MaxTileID;
                tile.X = x;
                tile.Y = y;
                chunk.ChunkContent[x, y] = tile;
                x++;
                if (x >= 16)
                {
                    x = 0;
                    y++;
                }
                MaxTileID++;
            }

            LoadedChunks.Add(chunk);

            return chunk;
        }

        public GameObject[] LoadAllTileObjects()
        {
            var formatter = new BinaryFormatter();

            var objectFiles = LoadedMapFiles.Where(x => x.EndsWith("map.ot"));

            var objects = new GameObject[objectFiles.Count()];

            var i = 0;
            foreach (var filename in objectFiles)
            {
                var file = File.OpenRead(filename);
                var o = (GameObject)formatter.Deserialize(file);
                if (o == null)
                {
                    Logger.Error("Deserialization error on " + filename);
                    continue;
                }
                objects[i] = o;
                i++;
            }

            return objects;
        }

        public GameObject[] LoadTiledObjects(TileChunk chunk)
        {
            var formatter = new BinaryFormatter();

            var objectFiles = LoadedMapFiles.Where(x => x.EndsWith(chunk.ID + "map.ot"));

            var objects = new GameObject[objectFiles.Count()];

            var i = 0;
            foreach (var filename in objectFiles)
            {
                var file = File.OpenRead(filename);
                var o = (GameObject)formatter.Deserialize(file);
                if (o == null)
                {
                    Logger.Error("Deserialization error on " + filename);
                    continue;
                }
                objects[i] = o;
                i++;
            }

            return objects;
        }

        public void SaveTiledObjects(TileChunk chunk, GameObject[] obj)
        {
            var id = chunk.ID;

            var formatter = new BinaryFormatter();

            foreach (var o in obj)
            {
                var filename = CurrentMapPath + o.Id + "_" + id + "map.ot";
                var file = File.Open(filename, FileMode.OpenOrCreate);
                formatter.Serialize(file, o);
                file.Flush();
                file.Close();
            }

            ReloadMapFilesList();
        }

        public void SaveChunk(TileChunk chunk)
        {
            var chunkID = BitConverter.GetBytes(chunk.ID);
            var chunkPosX = BitConverter.GetBytes(chunk.X);
            var chunkPosY = BitConverter.GetBytes(chunk.Y);
            var file = File.Open(chunk.ID + "map.ch", FileMode.OpenOrCreate);
            file.Write(chunkID, 0, 4);
            file.Write(chunkPosX, 0, 4);
            file.Write(chunkPosY, 0, 4);
            file.Flush();
            foreach (var tile in chunk.ChunkContent)
            {
                file.Write(new byte[] { (byte)RegisteredTextures.FindIndex(x => x.Equals(tile.Texture)) }, 0, 1);
                file.Write(new byte[] { (byte)(tile.Collision ? 1 : 0) }, 0, 1);
                file.Flush();
            }
            file.Close();

            ReloadMapFilesList();
        }

        public void AlignChunkPos(TileChunk chunk, int x, int y)
        {
            foreach (var tile in chunk.ChunkContent)
            {
                tile.X += x;
                tile.Y += y;
            }
        }

        public TileChunk LoadChunk(int id)
        {
            return LoadChunk(id + "map.ch");
        }

        public override void OnLoad()
        {
            base.OnLoad();
        }

        public override void PostLoad()
        {
            base.PostLoad();

            if (WorldGen == null)
            {
                WorldGen = ChunkLoaderGen;
                ProceduralGeneration = true;
            }
        }

        public override void OnDraw()
        {
            base.OnDraw();

            foreach (var camera in Engine.GetObjectsOfType<CameraComponent>())
            {
                if (!camera.Active) continue;
                if (!camera.Owner.TryGetComponent<TiledObjectComponent>(out var tiled)) return;

                tiled.RoundFromPos();

                var chunk = tiled.CurrentChunk;
                foreach (var tile in chunk.Value.ChunkContent)
                {
                    
                }
            }
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            foreach (var tiled in Engine.GetObjectsOfType<TiledObjectComponent>())
            {
                if (RoundFromPos)
                {
                    tiled.RoundFromPos();
                }

                // Check for current chunk, generate or set to null in case of lack of existance.
                if (tiled.CurrentChunk == null || tiled.CurrentChunk.Value.X + TileChunk.Width < tiled.X || tiled.CurrentChunk.Value.X > tiled.X || tiled.CurrentChunk.Value.Y + TileChunk.Height < tiled.Y || tiled.CurrentChunk.Value.Y > tiled.Y)
                {
                    var found = false;
                    foreach (var chunk in LoadedChunks)
                    {
                        if (chunk.X + TileChunk.Width > tiled.X && chunk.X < tiled.X && tiled.CurrentChunk.Value.Y + TileChunk.Height > tiled.Y && tiled.CurrentChunk.Value.Y < tiled.Y)
                        {
                            tiled.CurrentChunk = chunk;
                            found = true;
                            break;
                        }
                    }
                    if (!found && ProceduralGeneration && WorldGen != null)
                    {
                        WorldGen.Invoke(this, (int)(tiled.X / 16) * 16, (int)(tiled.Y / 16) * 16);
                    }
                    else
                    {
                        tiled.CurrentChunk = null;
                    }
                }

                // Prevent getting into collided items
                if (tiled.CurrentChunk == null) continue;
                foreach (var tileofch in tiled.CurrentChunk.Value.ChunkContent)
                {
                    if (tileofch.Collision && tileofch.X == tiled.X && tileofch.Y == tiled.Y)
                    {
                        var lastPos = LastPositions[tiled];
                        tiled.X = lastPos.Item1;
                        tiled.Y = lastPos.Item2;
                    }
                }

                // Save last pos
                LastPositions[tiled] = (tiled.X, tiled.Y);
            }
        }
    }
}

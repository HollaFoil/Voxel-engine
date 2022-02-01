using GlmSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Voxel_engine.World.Generation;

namespace Voxel_engine.World
{
    partial class World
    {
        public List<Chunk> loadedChunks = new List<Chunk>();
        private List<Tuple<int, int>> toRemove = new List<Tuple<int, int>>();
        private Dictionary<Tuple<int, int>, Chunk> loadedChunksBuffer = new Dictionary<Tuple<int, int>, Chunk>();
        public static IReadOnlyList<Tuple<int, int>> directions = new ReadOnlyCollection<Tuple<int, int>>(new List<Tuple<int, int>>
        {
            new Tuple<int, int>(-1, 0),
            new Tuple<int, int>(1, 0),
            new Tuple<int, int>(0, 1),
            new Tuple<int, int>(0, -1)
        });
        int renderDistance = 10;
        public int MaxNewChunksPerTick { get; private set; }

        private List<Tuple<int, int>> chunkUpdatePattern = null;

        public Position position = new Position();
        Thread thread = new Thread(RunThread);
        public bool terminate = false;
        public World(int renderDistance, int maxNewChunksPerTick)
        {
            chunkUpdatePattern = new List<Tuple<int, int>>();
            this.renderDistance = renderDistance;
            this.MaxNewChunksPerTick = maxNewChunksPerTick;
            UpdateChunkLoadingPattern();

            thread.Start(this);

        }
        public void TerminateThread()
        {
            terminate = true;
        }
        public void SetRenderDistance(int val)
        {
            renderDistance = val;
            UpdateChunkLoadingPattern();
        }

        private void UpdateChunkFaces()
        {
            Parallel.ForEach(loadedChunksBuffer.Values, (chunk) =>
            {
                if (chunk.updatedMesh) return;
                lock (chunk)
                {
                    chunk.UpdateExposedFaces();
                    chunk.GenerateMesh();
                    chunk.updatedMesh = true;
                }
            });
        }

        private void UpdateChunkLoadingPattern()
        {

            lock (chunkUpdatePattern)
            {
                chunkUpdatePattern = new List<Tuple<int, int>>();
                for (int i = -renderDistance; i <= renderDistance; i++)
                {
                    for (int j = -renderDistance; j <= renderDistance; j++)
                    {
                        if (!IsWithinDistance(0, 0, i, j)) continue;
                        chunkUpdatePattern.Add(new Tuple<int, int>(i, j));
                    }
                }

                chunkUpdatePattern.Sort(delegate (Tuple<int, int> x, Tuple<int, int> y)
                {
                    // find distances to origin
                    int d1 = x.Item1 * x.Item1 + x.Item2 * x.Item2;
                    int d2 = y.Item1 * y.Item1 + y.Item2 * y.Item2;

                    if (d1 == d2)
                    {
                        // slow, but idc because calculated only on render distance change
                        double a1 = Math.Atan2(x.Item1, x.Item2);
                        double a2 = Math.Atan2(y.Item1, y.Item2);
                        return a1.CompareTo(a2);
                    }
                    return d1.CompareTo(d2);
                });
            }
        }

        public void LoadAndUnloadChunks(int centerx, int centery)
        {
            toRemove.Clear();
            foreach((Tuple<int, int> key, Chunk c) in loadedChunksBuffer)
            {
                if (IsWithinDistance(centerx, centery, c.x, c.y)) continue;
                lock (c) c.UnloadChunk();
                toRemove.Add(key);
            }

            foreach (var key in toRemove) loadedChunksBuffer.Remove(key);
            lock (chunkUpdatePattern)
            {
                int loadedChunks = 0;
                foreach ((int cdx, int cdy) in chunkUpdatePattern)
                {
                    Tuple<int, int> pos = new Tuple<int, int>(cdx + centerx, cdy + centery);
                    if (loadedChunksBuffer.ContainsKey(pos)) continue;

                    Chunk newChunk = ChunkGenerator.GenerateChunk(pos.Item1, pos.Item2);
                    newChunk.SetNotUpdated();
                    loadedChunksBuffer.Add(pos, newChunk);

                    
                    foreach((int dx, int dy) in directions)
                    {
                        Chunk neighbour = GetChunk(dx + pos.Item1, dy + pos.Item2);
                        if (neighbour == null) continue;
                        neighbour.RegisterNeighbour(newChunk);
                        newChunk.RegisterNeighbour(neighbour);
                        lock (neighbour) neighbour.SetNotUpdated();

                    }
                    if (++loadedChunks == MaxNewChunksPerTick) break;
                }
            }
        }

        public Chunk GetChunk(int x, int y)
        {
            Tuple<int, int> pos = new Tuple<int, int>(x, y);
            if (!loadedChunksBuffer.TryGetValue(pos, out Chunk c)) return null;
            return c;
        }

        public List<Chunk> CloneList()
        {
            lock (loadedChunks)
            {
                List<Chunk> clone = new List<Chunk>(loadedChunks.Count);
                foreach (var chunk in loadedChunks) clone.Add(chunk);
                return clone;
            }
        }

        private void SwapBuffers()
        {
            loadedChunks.Clear();
            loadedChunks.AddRange(loadedChunksBuffer.Values);
        }
        public void UpdatePosition(vec3 pos)
        {
            lock (position)
            {
                position.Update(pos);
            }
        }

        private bool IsWithinDistance(int centerx, int centery, int chunkx, int chunky)
        {
            int distx = Math.Abs(centerx - chunkx);
            int disty = Math.Abs(centery - chunky);
            return (distx*distx + disty*disty) < renderDistance*renderDistance;
        }

        public byte? GetBlock(int x, int y, int z)
        {
            lock (loadedChunksBuffer)
            {
                int chunkx = (x >= 0 ? x / 16 : ((x - 15) / 16));
                int chunky = (z >= 0 ? z / 16 : ((z - 15) / 16));
                if (!loadedChunksBuffer.TryGetValue(new Tuple<int, int>(chunkx, chunky), out Chunk chunk)) return null;
                return chunk.blockType[mod(x, 16), mod(y, 256), mod(z, 16)];
            }
        }
        public void SetBlock(int x, int y, int z, byte block)
        {
            lock (loadedChunksBuffer)
            {
                int chunkx = (x >= 0 ? x / 16 : ((x - 15) / 16));
                int chunky = (z >= 0 ? z / 16 : ((z - 15) / 16));
                if (!loadedChunksBuffer.TryGetValue(new Tuple<int, int>(chunkx, chunky), out Chunk chunk)) return;
                chunk.blockType[mod(x, 16), mod(y, 256), mod(z, 16)] = block;
            }
        }
        public Chunk GetChunkFromBlockCoords(int x, int y, int z)
        {
            lock (loadedChunksBuffer)
            {
                int chunkx = (x >= 0 ? x / 16 : ((x - 15) / 16));
                int chunky = (z >= 0 ? z / 16 : ((z - 15) / 16));
                if (!loadedChunksBuffer.TryGetValue(new Tuple<int, int>(chunkx, chunky), out Chunk chunk)) return null;
                return chunk;
            }
        }
        private int mod(int x, int m)
        {
            return (x % m + m) % m;
        }
        public static void RunThread(object? obj)
        {
            World world = (World)obj;
            if (world == null) Console.WriteLine("BROKEN");

            while (!world.terminate)
            {

                int x, y;
                lock (world.position)
                {
                    x = world.position.GetChunkX();
                    y = world.position.GetChunkY();
                }
                world.LoadAndUnloadChunks(x, y);
                world.UpdateChunkFaces();
                lock (world.loadedChunks) lock (world.loadedChunksBuffer)
                {
                    world.SwapBuffers();
                }
                Thread.Sleep(1000 / 20);
            }

            
        }
    }
    class Position
    {
        private float x, y, z;
        public void Update(vec3 pos)
        {
            x = pos.x;
            y = pos.y;
            z = pos.z;
        }
        public int GetChunkX()
        {
            return (int)Math.Floor(x / 16.0f);
        }
        public int GetChunkY()
        {
            return (int)Math.Floor(z / 16.0f);
        }
    }
}

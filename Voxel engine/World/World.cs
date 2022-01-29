using GlmSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Voxel_engine.World.Generation;

namespace Voxel_engine.World
{
    internal class World
    {
        public List<Chunk> loadedChunks = new List<Chunk>();
        public List<Chunk> loadedChunksBuffer = new List<Chunk>();
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
            Parallel.For(0, loadedChunksBuffer.Count, (offset) =>
            {
                Chunk chunk = loadedChunksBuffer[offset];
                if (chunk.updatedMesh) return;
                Chunk left = loadedChunksBuffer.Find(c => ((c.x - 1 == chunk.x) && (c.y == chunk.y)));
                Chunk right = loadedChunksBuffer.Find(c => ((c.x + 1 == chunk.x) && (c.y == chunk.y)));
                Chunk up = loadedChunksBuffer.Find(c => ((c.x == chunk.x) && (c.y - 1 == chunk.y)));
                Chunk down = loadedChunksBuffer.Find(c => ((c.x == chunk.x) && (c.y + 1 == chunk.y)));
                lock (chunk) 
                {
                    chunk.UpdateExposedFaces(right, left, up, down);
                        //chunk.UpdateExposedFaces(null, null, null, null);
                    chunk.GenerateMesh();
                    chunk.updatedMesh = true;
                }
            });
            /*foreach (var chunk in loadedChunks)
            {
                if (chunk.updatedMesh) continue;
                Chunk left = loadedChunks.Find(c => (c.x - 1 == chunk.x) && (c.y == chunk.y));
                Chunk right = loadedChunks.Find(c => (c.x + 1 == chunk.x) && (c.y == chunk.y));
                Chunk up = loadedChunks.Find(c => (c.x == chunk.x) && (c.y - 1 == chunk.y));
                Chunk down = loadedChunks.Find(c => (c.x == chunk.x) && (c.y + 1== chunk.y));


                chunk.UpdateExposedFaces(right, left, up, down);
                chunk.updatedMesh = true;
            }*/
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
            for (int i = 0; i < loadedChunksBuffer.Count; i++)
            {
                if (!IsWithinDistance(centerx, centery, loadedChunksBuffer[i].x, loadedChunksBuffer[i].y))
                {
                    Chunk chunk = loadedChunksBuffer[i];
                    Chunk left = loadedChunksBuffer.Find(c => ((c.x - 1 == chunk.x) && (c.y == chunk.y)));
                    Chunk right = loadedChunksBuffer.Find(c => ((c.x + 1 == chunk.x) && (c.y == chunk.y)));
                    Chunk up = loadedChunksBuffer.Find(c => ((c.x == chunk.x) && (c.y - 1 == chunk.y)));
                    Chunk down = loadedChunksBuffer.Find(c => ((c.x == chunk.x) && (c.y + 1 == chunk.y)));
                    if (left != null) left.SetNotUpdated();
                    if (right != null) right.SetNotUpdated();
                    if (up != null) up.SetNotUpdated();
                    if (down != null) down.SetNotUpdated();
                    loadedChunksBuffer[i].FreeArrays();
                    loadedChunksBuffer.RemoveAt(i);
                    i--;
                }
            }
            lock (chunkUpdatePattern)
            {
                int loadedChunks = 0;
                foreach ((int cdx, int cdy) in chunkUpdatePattern)
                {
                    if (loadedChunksBuffer.Any(c => (c.x == centerx + cdx) && (c.y == centery + cdy))) continue;
                    loadedChunksBuffer.Add(ChunkGenerator.GenerateChunk(centerx + cdx, centery + cdy));

                    Chunk chunk = loadedChunksBuffer[loadedChunksBuffer.Count - 1];
                    chunk.SetNotUpdated();
                    Chunk left = loadedChunksBuffer.Find(c => ((c.x - 1 == chunk.x) && (c.y == chunk.y)));
                    Chunk right = loadedChunksBuffer.Find(c => ((c.x + 1 == chunk.x) && (c.y == chunk.y)));
                    Chunk up = loadedChunksBuffer.Find(c => ((c.x == chunk.x) && (c.y - 1 == chunk.y)));
                    Chunk down = loadedChunksBuffer.Find(c => ((c.x == chunk.x) && (c.y + 1 == chunk.y)));
                    if (left != null) left.SetNotUpdated();
                    if (right != null) right.SetNotUpdated();
                    if (up != null) up.SetNotUpdated();
                    if (down != null) down.SetNotUpdated();
                    if (++loadedChunks == MaxNewChunksPerTick) break;
                }
            }
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
            foreach (var chunk in loadedChunksBuffer) loadedChunks.Add(chunk);
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

        private static void RunThread(object? obj)
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

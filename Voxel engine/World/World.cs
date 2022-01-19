using GlmSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voxel_engine.World.Generation;

namespace Voxel_engine.World
{
    internal class World
    {
        public List<Chunk> loadedChunks = new List<Chunk>();
        int renderDistance = 10;
        public Position position = new Position();

        public World(int renderDistance)
        {
            this.renderDistance = renderDistance;
            Thread thread = new Thread(RunThread);
            thread.Start(this);

        }

        public void SetRenderDistance(int val)
        {
            renderDistance = val;
        }

        public void UpdateChunkFaces()
        {
            Parallel.For(0, loadedChunks.Count, (offset) =>
            {
                Chunk chunk = loadedChunks[offset];
                if (chunk.updatedMesh) return;
                Chunk left = loadedChunks.Find(c => (c.x - 1 == chunk.x) && (c.y == chunk.y));
                Chunk right = loadedChunks.Find(c => (c.x + 1 == chunk.x) && (c.y == chunk.y));
                Chunk up = loadedChunks.Find(c => (c.x == chunk.x) && (c.y - 1 == chunk.y));
                Chunk down = loadedChunks.Find(c => (c.x == chunk.x) && (c.y + 1 == chunk.y));

                lock (chunk)
                {
                    chunk.UpdateExposedFaces(right, left, up, down);
                    chunk.updatedMesh = true;
                    chunk.GenerateMesh();
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
        public void LoadAndUnloadChunks(int centerx, int centery)
        {
            for (int i = 0; i < loadedChunks.Count; i++)
            {
                if (!IsWithinDistance(centerx, centery, loadedChunks[i].x, loadedChunks[i].y))
                {
                    Chunk chunk = loadedChunks[i];
                    Chunk left = loadedChunks.Find(c => (c.x - 1 == chunk.x) && (c.y == chunk.y));
                    Chunk right = loadedChunks.Find(c => (c.x + 1 == chunk.x) && (c.y == chunk.y));
                    Chunk up = loadedChunks.Find(c => (c.x == chunk.x) && (c.y - 1 == chunk.y));
                    Chunk down = loadedChunks.Find(c => (c.x == chunk.x) && (c.y + 1 == chunk.y));
                    if (left != null) left.SetNotUpdated();
                    if (right != null) right.SetNotUpdated();
                    if (up != null) up.SetNotUpdated();
                    if (down != null) down.SetNotUpdated();
                    lock (loadedChunks)
                    {
                        loadedChunks.RemoveAt(i);
                    }
                }
            }
            for (int i = 0; i < renderDistance * 2; i++)
            {
                for (int j = 0; j < renderDistance * 2; j++)
                {
                    if (!IsWithinDistance(centerx, centery, centerx + i - renderDistance, centery + j - renderDistance) ||
                        loadedChunks.Any(c => (c.x == centerx + i - renderDistance) && (c.y == centery + j - renderDistance))) continue;
                    lock (loadedChunks)
                    {
                        loadedChunks.Add(ChunkGenerator.GenerateChunk(centerx + i - renderDistance, centery + j - renderDistance));
                    }
                    Chunk chunk = loadedChunks[loadedChunks.Count - 1];
                    Chunk left = loadedChunks.Find(c => (c.x - 1 == chunk.x) && (c.y == chunk.y));
                    Chunk right = loadedChunks.Find(c => (c.x + 1 == chunk.x) && (c.y == chunk.y));
                    Chunk up = loadedChunks.Find(c => (c.x == chunk.x) && (c.y - 1 == chunk.y));
                    Chunk down = loadedChunks.Find(c => (c.x == chunk.x) && (c.y + 1 == chunk.y));
                    if (left != null) left.SetNotUpdated();
                    if (right != null) right.SetNotUpdated();
                    if (up != null) up.SetNotUpdated();
                    if (down != null) down.SetNotUpdated();
                }
            }
            //UpdateChunkFaces();
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
            Console.WriteLine("OUTPUT");
            while (true)
            {
                int x, y;
                //Console.WriteLine("ABC");
                lock (world.position)
                {
                    x = world.position.GetChunkX();
                    y = world.position.GetChunkY();
                }
                //Console.WriteLine("{0} {1}", x, y);
                world.LoadAndUnloadChunks(x, y);
                //Console.WriteLine("AAAAAAAAAAAAAAAAAAAAAAA");
                world.UpdateChunkFaces();
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

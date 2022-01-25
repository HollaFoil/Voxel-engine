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
        public Position position = new Position();
        Thread thread = new Thread(RunThread);
        public bool terminate = false;
        public World(int renderDistance)
        {
            this.renderDistance = renderDistance;
            
            thread.Start(this);

        }
        public void TerminateThread()
        {
            terminate = true;
        }
        public void SetRenderDistance(int val)
        {
            renderDistance = val;
        }

        private void UpdateChunkFaces()
        {
            //Console.WriteLine();
            Parallel.For(0, loadedChunksBuffer.Count, (offset) =>
            {
                Chunk chunk = loadedChunksBuffer[offset];
                if (chunk.updatedMesh) return;
                Chunk left = loadedChunksBuffer.Find(c => ((c.x - 1 == chunk.x) && (c.y == chunk.y)));
                Chunk right = loadedChunksBuffer.Find(c => ((c.x + 1 == chunk.x) && (c.y == chunk.y)));
                Chunk up = loadedChunksBuffer.Find(c => ((c.x == chunk.x) && (c.y - 1 == chunk.y)));
                Chunk down = loadedChunksBuffer.Find(c => ((c.x == chunk.x) && (c.y + 1 == chunk.y)));
                Chunk clone = chunk.Clone();
                clone.UpdateExposedFaces(right, left, up, down);
                clone.GenerateMesh();
                clone.updatedMesh = true;
                lock (chunk) lock (clone)
                {
                    chunk = clone;
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
                    loadedChunksBuffer.RemoveAt(i);
                    i--;
                }
            }
            for (int i = 0; i < renderDistance * 2; i++)
            {
                for (int j = 0; j < renderDistance * 2; j++)
                {
                    if (!IsWithinDistance(centerx, centery, centerx + i - renderDistance, centery + j - renderDistance) ||
                    loadedChunksBuffer.Any(c => (c.x == centerx + i - renderDistance) && (c.y == centery + j - renderDistance))) continue;
                    loadedChunksBuffer.Add(ChunkGenerator.GenerateChunk(centerx + i - renderDistance, centery + j - renderDistance));

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
                }
            }
            //UpdateChunkFaces();
        }
        public List<Chunk> CloneList()
        {
            lock (loadedChunks)
            {
                List<Chunk> clone = new List<Chunk>();
                foreach (var chunk in loadedChunks) clone.Add(chunk);
                return clone;
            }
        }
        private void CopyChunksToBuffer()
        {
            foreach (var chunk in loadedChunks) loadedChunksBuffer.Add(chunk);
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
            while (!world.terminate)
            {

                int x, y;
                //Console.WriteLine("ABC");
                lock (world.position)
                {
                    x = world.position.GetChunkX();
                    y = world.position.GetChunkY();
                }
                world.CopyChunksToBuffer();
                //Console.WriteLine("{0} {1}", x, y);
                world.LoadAndUnloadChunks(x, y);
                //Console.WriteLine("AAAAAAAAAAAAAAAAAAAAAAA");
                world.UpdateChunkFaces();
                
                lock (world.loadedChunks) lock (world.loadedChunksBuffer)
                {
                    world.loadedChunks = world.loadedChunksBuffer;
                    world.loadedChunksBuffer = new List<Chunk>();
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

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

        public World(int renderDistance)
        {
            this.renderDistance = renderDistance;
            PopulateChunks(new vec3(0, 0, 0));
            UpdateChunkFaces();
        }

        public void SetRenderDistance(int val)
        {
            renderDistance = val;
        }
        public void PopulateChunks(vec3 location)
        {
            for (int i = 0; i < renderDistance*2; i++)
            {
                for (int j = 0; j < renderDistance*2; j++)
                {
                    if (!IsWithinDistance(0, 0, i - renderDistance, j - renderDistance)) continue;
                    loadedChunks.Add(ChunkGenerator.GenerateChunk(i - renderDistance, j - renderDistance));
                }
            }
            return;
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


                chunk.UpdateExposedFaces(right, left, up, down);
                chunk.updatedMesh = true;
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
                if (!IsWithinDistance(centerx, centery, loadedChunks[i].x, loadedChunks[i].y)) {
                    Chunk chunk = loadedChunks[i];
                    Chunk left = loadedChunks.Find(c => (c.x - 1 == chunk.x) && (c.y == chunk.y));
                    Chunk right = loadedChunks.Find(c => (c.x + 1 == chunk.x) && (c.y == chunk.y));
                    Chunk up = loadedChunks.Find(c => (c.x == chunk.x) && (c.y - 1 == chunk.y));
                    Chunk down = loadedChunks.Find(c => (c.x == chunk.x) && (c.y + 1 == chunk.y));
                    if (left != null) left.SetNotUpdated();
                    if (right != null) right.SetNotUpdated();
                    if (up != null) up.SetNotUpdated();
                    if (down != null) down.SetNotUpdated();
                    loadedChunks.RemoveAt(i);
                }
            }
            for (int i = 0; i < renderDistance*2; i++)
            {
                for (int j = 0; j < renderDistance *2; j++)
                {
                    if (!IsWithinDistance(centerx, centery, centerx+i-renderDistance, centery+j-renderDistance) ||
                        loadedChunks.Any(c => (c.x == centerx + i - renderDistance) && (c.y == centery + j - renderDistance))) continue;
                    loadedChunks.Add(ChunkGenerator.GenerateChunk(centerx + i - renderDistance, centery + j - renderDistance));
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


        private bool IsWithinDistance(int centerx, int centery, int chunkx, int chunky)
        {
            int distx = Math.Abs(centerx - chunkx);
            int disty = Math.Abs(centery - chunky);
            return (distx*distx + disty*disty) < renderDistance*renderDistance;
        }
    }

}

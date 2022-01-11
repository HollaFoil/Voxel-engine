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
                    loadedChunks.Add(ChunkGenerator.GenerateChunk(i - renderDistance, j - renderDistance));
                }
            }
            return;
        }

        public void UpdateChunkFaces()
        {
            foreach (var chunk in loadedChunks)
            {
                chunk.UpdateExposedFaces(null, null, null, null);
            }
        }
        public void LoadAndUnloadChunks(int centerx, int centery)
        {
            for (int i = 0; i < loadedChunks.Count; i++)
            {
                if (!IsWithinDistance(centerx, centery, loadedChunks[i].x, loadedChunks[i].y)) {
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
                    loadedChunks[loadedChunks.Count - 1].UpdateExposedFaces(null, null, null, null);
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

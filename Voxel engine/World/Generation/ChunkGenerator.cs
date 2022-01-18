using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voxel_engine.World.Generation
{
    internal static class ChunkGenerator
    {
        static int octaves = 3;
        public static Chunk GenerateChunk(int chunkx, int chunky)
        {
            var data = Noise2d.GenerateChunkNoiseMap(chunkx, chunky, octaves);
            byte[,,] blockTypes = new byte[32, 256, 32];
            for (int x = 0; x < 32; x++)
            {
                for (int z = 0; z < 32; z++)
                {
                    int height = (int)Math.Ceiling((1.0f +data[x*32 + z])/2 * 255);
                    blockTypes[x, height, z] = 4;
                    blockTypes[x, height-1, z] = 2;
                    blockTypes[x, height-2, z] = 2;
                    blockTypes[x, height-3, z] = 2;
                    for (int y = height-4; y >= 0; y--)
                    {
                        blockTypes[x, y, z] = 6;
                    }
                }
            }
            return new(chunkx, chunky, blockTypes);
        }

    }
}

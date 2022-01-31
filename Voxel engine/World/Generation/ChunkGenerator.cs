using System;
using System.Buffers;
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
            //var data = Noise2d.GenerateChunkNoiseMap(chunkx, chunky, octaves);
            int poolId = ChunkArrayPool.Rent3DArray();
            float min = 0, max = 1;
            var map = Perlin3D.GenerateChunkNoiseMap(chunkx, chunky, 1);
            byte[,,] blockTypes = ChunkArrayPool.arrayPool[poolId];
            for (int x = 0; x < 16; x++)
            {
                for (int y = 0; y < 256; y++)
                {
                    for (int z = 0; z < 16; z++)
                    {
                        //float val = Perlin3D.Noise(x + chunkx*16, y, z + chunky * 16, 4, ref min, ref max);
                        //if (val > 0.01f) Console.WriteLine("Biggah");
                        //int height = (int)Math.Ceiling((1.0f + data[x * 16 + z]) / 2 * 255);
                        blockTypes[x, y, z] = (map[x + z * 16 + y*16*16] > 0.5f ? (byte)1 : (byte)0);
                    }
                }
                
            }
            Console.WriteLine(min);
            Console.WriteLine(max);
            //ArrayPool<float>.Shared.Return(data, true);
            ArrayPool<float>.Shared.Return(map, true);
            return new Chunk(chunkx, chunky, blockTypes, poolId);
        }

    }
}

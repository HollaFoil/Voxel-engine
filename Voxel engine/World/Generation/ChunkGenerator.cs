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
        static Random rand = new Random();
        static int octaves = 3;
        public static Chunk GenerateChunk(int chunkx, int chunky)
        {
            //var data = Noise2d.GenerateChunkNoiseMap(chunkx, chunky, octaves);
            int poolId = ChunkArrayPool.Rent3DArray();
            double min = 0, max = 1;
            var map = Noise3D.GenerateChunkNoiseMap(chunkx, chunky, 4, ref min, ref max);
            byte[,,] blockTypes = ChunkArrayPool.arrayPool[poolId];
            for (int x = 0; x < 16; x++)
            {
                for (int y = 255; y >= 0; y--)
                {
                    for (int z = 0; z < 16; z++)
                    {
                        //float val = Perlin3D.Noise(x + chunkx*16, y, z + chunky * 16, 4, ref min, ref max);
                        //if (val > 0.01f) Console.WriteLine("Biggah");
                        //int height = (int)Math.Ceiling((1.0f + data[x * 16 + z]) / 2 * 255);

                        bool solid = (easeInOutCubic(1 - y / 255.0) * 1.5) - map[x * 16 * 256 + y * 16 + z] > 0.5;
                        byte blockID;
                        if (!solid) blockID = 0;
                        else if (y == 255 || blockTypes[x, y + 1, z] == 0) blockID = 4;
                        else if (blockTypes[x, y + 1, z] == 6) blockID = 6;
                        else if (blockTypes[x, y + 4, z] == 4) blockID = 6;
                        else blockID = 2;
                        blockTypes[x, y, z] = blockID;
                    }
                }
                
            }
            //ArrayPool<float>.Shared.Return(data, true);
            ArrayPool<double>.Shared.Return(map, true);
            return new Chunk(chunkx, chunky, blockTypes, poolId);
        }
        public static double easeInOutCubic(double x) {
            return x < 0.5 ? 4 * x * x * x : 1 - Math.Pow(-2 * x + 2, 3) / 2;
        }
}
}

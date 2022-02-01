using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voxel_engine
{
    internal static class ChunkMesh
    {
        

        public static byte[] GenerateMesh(byte[,,] block, byte[,,] exposedFaces, int chunkx, int chunky, out int length)
        {

            int faces = 0;
            for (int x = 0; x < 16; x++)
            {
                for (int y = 0; y < 256; y++)
                {
                    for (int z = 0; z < 16; z++)
                    {
                        if (block[x, y, z] == 0 || exposedFaces[x, y, z] == 0) continue;
                        if ((exposedFaces[x, y, z] & BlockMesh.FRONT) > 0) faces++;
                        if ((exposedFaces[x, y, z] & BlockMesh.RIGHT) > 0) faces++;
                        if ((exposedFaces[x, y, z] & BlockMesh.LEFT) > 0) faces++;
                        if ((exposedFaces[x, y, z] & BlockMesh.BACK) > 0) faces++;
                        if ((exposedFaces[x, y, z] & BlockMesh.TOP) > 0) faces++;
                        if ((exposedFaces[x, y, z] & BlockMesh.BOTTOM) > 0) faces++;
                    }
                }
            }
            length = faces * BlockMesh.sizeOfFace;
            byte[] mesh = ArrayPool<byte>.Shared.Rent(length);
            int index = 0;
            for (byte x = 0; x < 16; x++)
            {
                for (int y = 0; y < 256; y++)
                {
                    for (byte z = 0; z < 16; z++)
                    {
                        if (block[x, y, z] == 0) continue;
                        byte[] data = BlockMesh.GetVertices(exposedFaces[x, y, z], block[x, y, z], x, (byte)y, z, chunkx, chunky, out int size);
                        Buffer.BlockCopy(data, 0, mesh, index, size);
                        index += size;
                        ArrayPool<byte>.Shared.Return(data, true);
                    }
                }
            }
            return mesh;
        }
    }
}

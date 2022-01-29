using GlmSharp;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voxel_engine
{
    internal static class BlockMesh
    {
        static BlockMesh()
        {
            for (int i = 0; i < 64; i++)
            {
                faceCounts[i] = Convert.ToString(i, 2).ToCharArray().Count(c => c == '1');
            }
        }
        public static byte[] GetVertices(byte faces, byte tex, byte x, byte y, byte z, int chunkx, int chunky, out int length)
        {
            //block byte xyz, vertex id byte, tex id byte, chunk int x, y
            byte[] vertices = ArrayPool<byte>.Shared.Rent(faceCounts[faces] * sizeOfFace);
            length = faceCounts[faces] * sizeOfFace;
            int currentByte = 0;
            for (int i = 0, face = 1; i < 6; i++, face *= 2)
            {
                if ((faces & face) == 0) continue;
                for (int j = 0; j < 6; j++)
                {
                    vertices[currentByte] = x;
                    vertices[currentByte + 1] = y;
                    vertices[currentByte + 2] = z;
                    vertices[currentByte + 3] = indices[i * 6 + j];
                    vertices[currentByte + 4] = faceTexIds[tex - 1, i];
                    int[] chunkcoords = ArrayPool<int>.Shared.Rent(2);
                    chunkcoords[0] = chunkx;
                    chunkcoords[1] = chunky;
                    CopyToByteArray(currentByte + 5, ref vertices, ref chunkcoords);
                    ArrayPool<int>.Shared.Return(chunkcoords);
                    currentByte += sizeOfVertex;

                }
            }
            return vertices;
        }
        private static void CopyToByteArray(int offset, ref byte[] destination, ref int[] ToCopy)
        {
            Buffer.BlockCopy(ToCopy, 0, destination, offset, 2 * sizeof(int));
        }

        static byte[,] faceTexIds = new byte[,]
        {
            {1,1,1,1,1,1 },
            {2,2,2,2,2,2 },
            {5,5,5,5,5,5 },
            {3,3,3,3,2,4 },
            {6,6,6,6,6,6 },
            {7,7,7,7,7,7 },
        };
        static byte[] indices = {
                //Faces definition
            0,1,3, 0,3,2,           //Face front
            4,5,7, 4,7,6,           //Face right
            8,9,11, 8,11,10,        //...
            12,13,15, 12,15,14,
            16,17,19, 16,19,18,
            20,21,23, 20,23,22,
        };

        static int[] faceCounts = new int[64];
        public const int sizeOfFace = 6 * (sizeOfVertex);
        public const int sizeOfVertex = 2 * sizeof(int) + 5 * sizeof(byte);
        public const byte BACK = 1, LEFT = 2, FRONT = 4, RIGHT = 8, BOTTOM = 16, TOP = 32;
        // SIDE SIDE SIDE SIDE BOTTOM TOP
    }
}

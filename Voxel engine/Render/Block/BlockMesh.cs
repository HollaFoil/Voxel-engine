using GlmSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voxel_engine
{
    internal static class BlockMesh
    {
        public const int sizeOfFace = 6*(sizeOfVertex);
        public const int sizeOfVertexFloats = 5 * sizeof(float);
        public const int sizeOfVertex = 5 * sizeof(float) + sizeof(byte);
        public const byte BACK = 1, LEFT = 2, FRONT = 4, RIGHT = 8, BOTTOM = 16, TOP = 32;
        // SIDE SIDE SIDE SIDE BOTTOM TOP
        public static byte[] GetVertices(byte faces, byte tex, int x, int y, int z)
        {
            int countOfFaces = Convert.ToString(faces, 2).ToCharArray().Count(c => c == '1');
            byte[] vertices = new byte[countOfFaces*sizeOfFace];
            float[] reference = Translate(x, y, z);
            int currentByte = 0;
            for (int i = 0, face = 1; i < 6; i++, face *= 2)
            {
                if ((faces & face) == 0) continue;
                for (int j = 0; j < 6; j++)
                {
                    CopyToByteArray(indices[i * 6 + j], ref vertices, ref currentByte, ref reference);
                    currentByte += sizeOfVertex;
                    vertices[currentByte - 1] = faceTexIds[tex-1,i];
                    
                }
            }
            return vertices;
        }
        private static void CopyToByteArray(int vertex, ref byte[] destination, ref int currentByte, ref float[] vertices)
        {
            Buffer.BlockCopy(vertices, sizeOfVertexFloats*vertex, destination, currentByte, sizeOfVertexFloats);
        }
        private static float[] Translate(int x, int y, int z)
        {
            float[] reference = (float[])vertices.Clone();
            for (int i = 0; i < vertices.Length/5; i++)
            {
                reference[i * 5] += x;
                reference[i * 5 + 1] += y;
                reference[i * 5 + 2] += z;
            }
            return reference;
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

        static float[] vertices = {
        //Vertices according to faces + texture coordinates
            0f, 0f, 1.0f,             0.0f, 1.0f,
            1.0f, 0f, 1.0f,           1.0f, 1.0f,
            0f, 1.0f, 1.0f,           0.0f, 0.0f,
            1.0f, 1.0f, 1.0f,         1.0f, 0.0f,

            1.0f, 0f, 1.0f,           0.0f, 1.0f,
            1.0f, 0f, 0f,             1.0f, 1.0f,
            1.0f, 1.0f, 1.0f,         0.0f, 0.0f,
            1.0f, 1.0f, 0f,           1.0f, 0.0f,

            1.0f, 0f, 0f,             0.0f, 1.0f,
            0f, 0f, 0f,               1.0f, 1.0f,
            1.0f, 1.0f, 0f,           0.0f, 0.0f,
            0f, 1.0f, 0f,             1.0f, 0.0f,

            0f, 0f, 0f,               0.0f, 1.0f,
            0f, 0f, 1.0f,             1.0f, 1.0f,
            0f, 1.0f, 0f,             0.0f, 0.0f,
            0f, 1.0f, 1.0f,           1.0f, 0.0f,

            0f, 0f, 0f,               0.0f, 1.0f,
            1.0f, 0f, 0f,             1.0f, 1.0f,
            0f, 0f, 1.0f,             0.0f, 0.0f,
            1.0f, 0f, 1.0f,           1.0f, 0.0f,

            0f, 1.0f, 1.0f,           0.0f, 1.0f,
            1.0f, 1.0f, 1.0f,         1.0f, 1.0f,
            0f, 1.0f, 0f,             0.0f, 0.0f,
            1.0f, 1.0f, 0f,           1.0f, 0.0f,
        };
        static int[] indices = {
                //Faces definition
            0,1,3, 0,3,2,           //Face front
            4,5,7, 4,7,6,           //Face right
            8,9,11, 8,11,10,        //...
            12,13,15, 12,15,14,
            16,17,19, 16,19,18,
            20,21,23, 20,23,22,
        };
    }
}

﻿using GlmSharp;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voxel_engine.World;

namespace Voxel_engine
{
    internal static class BlockMesh
    {
        public static World.World world = null;
        static BlockMesh()
        {
            for (int i = 0; i < 64; i++)
            {
                faceCounts[i] = Convert.ToString(i, 2).ToCharArray().Count(c => c == '1');
            }
        }
        public static byte[] GetVertices(byte faces, byte tex, byte x, byte y, byte z, int chunkx, int chunky, out int length)
        {
            //block byte xyz, vertex id byte, tex id byte, byte AO, chunk int x, y
            int xworld = x + chunkx * 16, zworld = z + chunky * 16;
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
                    vertices[currentByte + 5] = GetAOOfVertex(xworld, y, zworld, indices[i * 6 + j]);
                    int[] chunkcoords = { chunkx, chunky };
                    CopyToByteArray(currentByte + 6, ref vertices, ref chunkcoords);
                    currentByte += sizeOfVertex;

                }
            }
            return vertices;
        }
        private static void CopyToByteArray(int offset, ref byte[] destination, ref int[] ToCopy)
        {
            Buffer.BlockCopy(ToCopy, 0, destination, offset, 2 * sizeof(int));
        }
        private static byte GetAOOfVertex (int x, int y, int z, int vertId)
        {

            byte? block;
            int side1, side2, corner;
            int val = 0;
            switch (vertId) {
                case 0:
                    block = Block.GetType(x - 1 , y, z + 1 );
                    side1 = (block == null || block == 0) ? 0 : 1;
                    block = Block.GetType(x , y - 1, z + 1 );
                    side2 = (block == null || block == 0) ? 0 : 1;
                    block = Block.GetType(x - 1 , y - 1, z + 1 );
                    corner = (block == null || block == 0) ? 0 : 1;
                    return CalcAO(side1, side2, corner);
                case 1:
                    block = Block.GetType(x + 1 , y, z + 1 );
                    side1 = (block == null || block == 0) ? 0 : 1;
                    block = Block.GetType(x , y - 1, z + 1 );
                    side2 = (block == null || block == 0) ? 0 : 1;
                    block = Block.GetType(x + 1 , y - 1, z + 1 );
                    corner = (block == null || block == 0) ? 0 : 1;
                    return CalcAO(side1, side2, corner);
                case 2:
                    block = Block.GetType(x - 1 , y, z + 1 );
                    side1 = (block == null || block == 0) ? 0 : 1;
                    block = Block.GetType(x , y + 1, z + 1 );
                    side2 = (block == null || block == 0) ? 0 : 1;
                    block = Block.GetType(x - 1 , y + 1, z + 1 );
                    corner = (block == null || block == 0) ? 0 : 1;
                    return CalcAO(side1, side2, corner);
                case 3:
                    block = Block.GetType(x + 1 , y, z + 1 );
                    side1 = (block == null || block == 0) ? 0 : 1;
                    block = Block.GetType(x , y + 1, z + 1 );
                    side2 = (block == null || block == 0) ? 0 : 1;
                    block = Block.GetType(x + 1 , y + 1, z + 1 );
                    corner = (block == null || block == 0) ? 0 : 1;
                    return CalcAO(side1, side2, corner);
                case 4:
                    block = Block.GetType(x + 1 , y, z + 1 );
                    side1 = (block == null || block == 0) ? 0 : 1;
                    block = Block.GetType(x + 1 , y - 1, z );
                    side2 = (block == null || block == 0) ? 0 : 1;
                    block = Block.GetType(x + 1 , y - 1, z + 1 );
                    corner = (block == null || block == 0) ? 0 : 1;
                    return CalcAO(side1, side2, corner);
                case 5:
                    block = Block.GetType(x + 1 , y, z - 1 );
                    side1 = (block == null || block == 0) ? 0 : 1;
                    block = Block.GetType(x + 1 , y - 1, z );
                    side2 = (block == null || block == 0) ? 0 : 1;
                    block = Block.GetType(x + 1 , y - 1, z - 1 );
                    corner = (block == null || block == 0) ? 0 : 1;
                    return CalcAO(side1, side2, corner);
                case 6:
                    block = Block.GetType(x + 1 , y, z + 1 );
                    side1 = (block == null || block == 0) ? 0 : 1;
                    block = Block.GetType(x + 1 , y + 1, z );
                    side2 = (block == null || block == 0) ? 0 : 1;
                    block = Block.GetType(x + 1 , y + 1, z + 1 );
                    corner = (block == null || block == 0) ? 0 : 1;
                    return CalcAO(side1, side2, corner);
                case 7:
                    block = Block.GetType(x + 1 , y, z - 1 );
                    side1 = (block == null || block == 0) ? 0 : 1;
                    block = Block.GetType(x + 1 , y + 1, z );
                    side2 = (block == null || block == 0) ? 0 : 1;
                    block = Block.GetType(x + 1 , y + 1, z - 1 );
                    corner = (block == null || block == 0) ? 0 : 1;
                    return CalcAO(side1, side2, corner);
                case 8:
                    block = Block.GetType(x + 1 , y, z - 1 );
                    side1 = (block == null || block == 0) ? 0 : 1;
                    block = Block.GetType(x , y - 1, z - 1 );
                    side2 = (block == null || block == 0) ? 0 : 1;
                    block = Block.GetType(x + 1 , y - 1, z - 1 );
                    corner = (block == null || block == 0) ? 0 : 1;
                    return CalcAO(side1, side2, corner);
                case 9:
                    block = Block.GetType(x - 1 , y, z - 1 );
                    side1 = (block == null || block == 0) ? 0 : 1;
                    block = Block.GetType(x , y - 1, z - 1 );
                    side2 = (block == null || block == 0) ? 0 : 1;
                    block = Block.GetType(x - 1 , y - 1, z - 1 );
                    corner = (block == null || block == 0) ? 0 : 1;
                    return CalcAO(side1, side2, corner);
                case 10:
                    block = Block.GetType(x + 1 , y, z - 1 );
                    side1 = (block == null || block == 0) ? 0 : 1;
                    block = Block.GetType(x , y + 1, z - 1 );
                    side2 = (block == null || block == 0) ? 0 : 1;
                    block = Block.GetType(x + 1 , y + 1, z - 1 );
                    corner = (block == null || block == 0) ? 0 : 1;
                    return CalcAO(side1, side2, corner);
                case 11:
                    block = Block.GetType(x - 1 , y, z - 1 );
                    side1 = (block == null || block == 0) ? 0 : 1;
                    block = Block.GetType(x , y + 1, z - 1 );
                    side2 = (block == null || block == 0) ? 0 : 1;
                    block = Block.GetType(x - 1 , y + 1, z - 1 );
                    corner = (block == null || block == 0) ? 0 : 1;
                    return CalcAO(side1, side2, corner);
                case 12:
                    block = Block.GetType(x - 1 , y - 1, z );
                    side1 = (block == null || block == 0) ? 0 : 1;
                    block = Block.GetType(x - 1 , y, z - 1 );
                    side2 = (block == null || block == 0) ? 0 : 1;
                    block = Block.GetType(x - 1 , y - 1, z - 1 );
                    corner = (block == null || block == 0) ? 0 : 1;
                    return CalcAO(side1, side2, corner);
                case 13:
                    block = Block.GetType(x - 1 , y - 1, z );
                    side1 = (block == null || block == 0) ? 0 : 1;
                    block = Block.GetType(x - 1 , y, z + 1 );
                    side2 = (block == null || block == 0) ? 0 : 1;
                    block = Block.GetType(x - 1 , y - 1, z + 1 );
                    corner = (block == null || block == 0) ? 0 : 1;
                    return CalcAO(side1, side2, corner);
                case 14:
                    block = Block.GetType(x - 1 , y + 1, z );
                    side1 = (block == null || block == 0) ? 0 : 1;
                    block = Block.GetType(x - 1 , y, z - 1 );
                    side2 = (block == null || block == 0) ? 0 : 1;
                    block = Block.GetType(x - 1 , y + 1, z - 1 );
                    corner = (block == null || block == 0) ? 0 : 1;
                    return CalcAO(side1, side2, corner);
                case 15:
                    block = Block.GetType(x - 1 , y + 1, z );
                    side1 = (block == null || block == 0) ? 0 : 1;
                    block = Block.GetType(x - 1 , y, z + 1 );
                    side2 = (block == null || block == 0) ? 0 : 1;
                    block = Block.GetType(x - 1 , y + 1, z + 1 );
                    corner = (block == null || block == 0) ? 0 : 1;
                    return CalcAO(side1, side2, corner);
                case 16:
                    block = Block.GetType(x - 1 , y - 1, z );
                    side1 = (block == null || block == 0) ? 0 : 1;
                    block = Block.GetType(x , y - 1, z - 1 );
                    side2 = (block == null || block == 0) ? 0 : 1;
                    block = Block.GetType(x - 1 , y - 1, z - 1 );
                    corner = (block == null || block == 0) ? 0 : 1;
                    return CalcAO(side1, side2, corner);
                case 17:
                    block = Block.GetType(x + 1 , y - 1, z );
                    side1 = (block == null || block == 0) ? 0 : 1;
                    block = Block.GetType(x , y - 1, z - 1 );
                    side2 = (block == null || block == 0) ? 0 : 1;
                    block = Block.GetType(x + 1 , y - 1, z - 1 );
                    corner = (block == null || block == 0) ? 0 : 1;
                    return CalcAO(side1, side2, corner);
                case 18:
                    block = Block.GetType(x - 1 , y - 1, z );
                    side1 = (block == null || block == 0) ? 0 : 1;
                    block = Block.GetType(x , y - 1, z + 1 );
                    side2 = (block == null || block == 0) ? 0 : 1;
                    block = Block.GetType(x - 1 , y - 1, z + 1 );
                    corner = (block == null || block == 0) ? 0 : 1;
                    return CalcAO(side1, side2, corner);
                case 19:
                    block = Block.GetType(x + 1 , y - 1, z );
                    side1 = (block == null || block == 0) ? 0 : 1;
                    block = Block.GetType(x , y - 1, z + 1 );
                    side2 = (block == null || block == 0) ? 0 : 1;
                    block = Block.GetType(x + 1 , y - 1, z + 1 );
                    corner = (block == null || block == 0) ? 0 : 1;
                    return CalcAO(side1, side2, corner);
                case 20:
                    block = Block.GetType(x - 1 , y + 1, z );
                    side1 = (block == null || block == 0) ? 0 : 1;
                    block = Block.GetType(x , y + 1, z + 1 );
                    side2 = (block == null || block == 0) ? 0 : 1;
                    block = Block.GetType(x - 1 , y + 1, z + 1 );
                    corner = (block == null || block == 0) ? 0 : 1;
                    return CalcAO(side1, side2, corner);
                case 21:
                    block = Block.GetType(x + 1 , y + 1, z );
                    side1 = (block == null || block == 0) ? 0 : 1;
                    block = Block.GetType(x , y + 1, z + 1 );
                    side2 = (block == null || block == 0) ? 0 : 1;
                    block = Block.GetType(x + 1 , y + 1, z + 1 );
                    corner = (block == null || block == 0) ? 0 : 1;
                    return CalcAO(side1, side2, corner);
                case 22:
                    block = Block.GetType(x - 1 , y + 1, z );
                    side1 = (block == null || block == 0) ? 0 : 1;
                    block = Block.GetType(x , y + 1, z - 1 );
                    side2 = (block == null || block == 0) ? 0 : 1;
                    block = Block.GetType(x - 1 , y + 1, z - 1 );
                    corner = (block == null || block == 0) ? 0 : 1;
                    return CalcAO(side1, side2, corner);
                case 23:
                    block = Block.GetType(x + 1 , y + 1, z );
                    side1 = (block == null || block == 0) ? 0 : 1;
                    block = Block.GetType(x , y + 1, z - 1 );
                    side2 = (block == null || block == 0) ? 0 : 1;
                    block = Block.GetType(x + 1 , y + 1, z - 1 );
                    corner = (block == null || block == 0) ? 0 : 1;
                    return CalcAO(side1, side2, corner);
                default:
                    return 0;
            }
        }
        private static byte CalcAO(int side1, int side2, int corner)
        {
            if (side1 == 1 && side2 == 1) return 0;
            return (byte)(3 - (side1 + side2 + corner));
        }

        static byte[,] faceTexIds = new byte[,]
        {
            {1,1,1,1,1,1 },
            {2,2,2,2,2,2 },
            {5,5,5,5,5,5 },
            {3,3,3,3,2,4 },
            {6,6,6,6,6,6 },
            {7,7,7,7,7,7 },
            {8,8,8,8,9,9 },
            {10,10,10,10,10,10 },
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
        public const int sizeOfVertex = 2 * sizeof(int) + 6 * sizeof(byte);
        public const byte BACK = 1, LEFT = 2, FRONT = 4, RIGHT = 8, BOTTOM = 16, TOP = 32;
        // SIDE SIDE SIDE SIDE BOTTOM TOP
    }
}

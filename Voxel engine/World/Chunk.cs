using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voxel_engine.World.Generation;

namespace Voxel_engine.World
{
    public class Chunk
    {
        private const byte BACK = 1, LEFT = 2, FRONT = 4, RIGHT = 8, BOTTOM = 16, TOP = 32;
        public byte[,,] blockType;
        public byte[,,] exposedFaces;
        public int x, y;
        public int bufferID = -1;
        public int blockTypePoolId, exposedFacesPoolId;
        public bool updatedMesh = false, bufferedMesh = false;
        public byte[] data;

        public Chunk(int x, int y, byte[,,] blockType, int poolId)
        {
            this.blockType = blockType;
            this.x = x;
            this.y = y;
            blockTypePoolId = poolId;
            exposedFacesPoolId = ChunkArrayPool.Rent3DArray();
            exposedFaces = ChunkArrayPool.arrayPool[exposedFacesPoolId];
        }
        public void FreeArrays()
        {
            ChunkArrayPool.Return3DArray(blockTypePoolId);
            ChunkArrayPool.Return3DArray(exposedFacesPoolId);
        }
        public void SetBufferId(int id)
        {
            bufferID = id;
        }
        public void GenerateMesh()
        {
            data = ChunkMesh.GenerateMesh(blockType, exposedFaces, x, y);
        }
        public void UpdateExposedFaces(Chunk right, Chunk left, Chunk down, Chunk up)
        {
            int index = 0;
            for (int x = 0; x < 16; x++)
            {
                for (int y = 0; y < 256; y++)
                {
                    for (int z = 0; z < 16; z++)
                    {
                        if (blockType[x, y, z] == 0)
                        {
                            exposedFaces[x, y, z] = 0;
                        }
                        byte directions = 0;
                        if (x == 15 && left == null) directions += BlockMesh.LEFT;
                        else if (x == 15 && left.blockType[0, y, z] == 0) directions += BlockMesh.LEFT;
                        else if (x < 15 && blockType[x + 1, y, z] == 0) directions += BlockMesh.LEFT;

                        if (z == 15 && down == null) directions += BlockMesh.BACK;
                        else if (z == 15 && down.blockType[x, y, 0] == 0) directions += BlockMesh.BACK;
                        else if (z < 15 && blockType[x, y, z + 1] == 0) directions += BlockMesh.BACK;

                        if (x == 0 && right == null) directions += BlockMesh.RIGHT;
                        else if (x == 0 && right.blockType[15, y, z] == 0) directions += BlockMesh.RIGHT;
                        else if (x != 0 && blockType[x - 1, y, z] == 0) directions += BlockMesh.RIGHT;

                        if (z == 0 && up == null) directions += BlockMesh.FRONT;
                        else if (z == 0 && up.blockType[x, y, 15] == 0) directions += BlockMesh.FRONT;
                        else if (z != 0 && blockType[x, y, z - 1] == 0) directions += BlockMesh.FRONT;

                        if (y == 0 || blockType[x, y - 1, z] == 0) directions += BlockMesh.BOTTOM;
                        if (y == 255 || blockType[x, y + 1, z] == 0) directions += BlockMesh.TOP;
                        exposedFaces[x, y, z] = directions;
                    }
                }
            }
        }
        public void SetNotUpdated()
        {
            updatedMesh = false;
            bufferedMesh = false;
        }
        public void SetBlock(int x, int y, int z, byte id)
        {
            blockType[x, y, z] = id;
        }
    }
}

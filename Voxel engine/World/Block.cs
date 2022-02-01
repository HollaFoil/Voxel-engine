using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voxel_engine.World
{
    internal class Block
    {
        public int x { get; private set; }
        public int y { get; private set; }
        public int z { get; private set; }
        public byte? Type
        {
            get { return GetType(x, y, z); }
            set { SetType(x, y, z, (byte)value); }
        }

        public Block(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static byte? GetType(int x, int y, int z)
        {
            return Program.world.GetBlockType(x, y, z);
        }

        public static void SetType(int x, int y, int z, byte type)
        {
            Program.world.SetBlockType(x, y, z, type);
        }
    }
}

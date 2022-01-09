using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voxel_engine
{
    internal class Texture
    {
        public int ID;
        public string name;
        public byte[] textureBytes;
        public Texture(int ID, string name, byte[] textureBytes)
        {
            this.ID = ID;
            this.name = name;
            this.textureBytes = textureBytes;
        }
    }
}

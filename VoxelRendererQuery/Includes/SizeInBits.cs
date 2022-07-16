using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoxelRendererQuery.Includes
{
    [System.AttributeUsage(System.AttributeTargets.Field)]
    public class SizeInBits : System.Attribute
    {
        internal int bits;
        public SizeInBits(int bits)
        {
            this.bits = bits;
        }
    }
}
